Shader "Hidden/Custom/VolumetricRaymarch"
{
    Properties
    {
        _JitterIntensity ("Jitter Intensity", Range(0, 1)) = 1.0
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline" = "UniversalPipeline" }
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            Name "VolumetricRaymarch"

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _ADDITIONAL_LIGHTS
            #pragma multi_compile _ _CLUSTER_LIGHT_LOOP
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile_fragment _ _SHADOWS_SOFT

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            // Must match VolumetricLightingRenderFeature.kMaxVolumes.
            #define MAX_FOG_VOLUMES 8

            float _JitterIntensity;

            int      _VolumeCount;
            float4x4 _VolumeWorldToLocal[MAX_FOG_VOLUMES]; // maps the box onto the unit cube [-1, 1]
            float4   _VolumeColor[MAX_FOG_VOLUMES];        // rgb : HDR tint
            float4   _VolumeParams[MAX_FOG_VOLUMES];       // x : density, y : anisotropy, z : step count, w : edge fade
            float4   _VolumeParams2[MAX_FOG_VOLUMES];      // x : main light shadows, y : additional lights

            float HenyeyGreenstein(float cosAngle, float g)
            {
                float g2 = g * g;
                float denom = 1.0 + g2 - 2.0 * g * cosAngle;
                return (1.0 - g2) / (4.0 * PI * pow(max(denom, 1e-4), 1.5));
            }

            // Slab test against the unit cube, in local space of the volume.
            // The ray parameter t stays in world units because the transform is affine.
            bool IntersectUnitBox(float3 ro, float3 rd, out float tNear, out float tFar)
            {
                float3 invDir = rcp(rd + (abs(rd) < 1e-6) * 1e-6);
                float3 t0 = (-1.0 - ro) * invDir;
                float3 t1 = ( 1.0 - ro) * invDir;
                float3 tMin = min(t0, t1);
                float3 tMax = max(t0, t1);

                tNear = max(max(tMin.x, tMin.y), tMin.z);
                tFar  = min(min(tMax.x, tMax.y), tMax.z);
                return tFar > max(tNear, 0.0);
            }

            // 1 at the core of the box, 0 at its faces.
            float EdgeFade(float3 localPos, float fadeWidth)
            {
                float3 a = abs(localPos);
                float edge = 1.0 - max(max(a.x, a.y), a.z);
                return saturate(edge / max(fadeWidth, 1e-4));
            }

            float3 EvaluateAdditionalLights(InputData inputData, float3 rayDir, float anisotropy)
            {
                float3 addedLighting = 0;
            #if defined(_ADDITIONAL_LIGHTS) || USE_CLUSTER_LIGHT_LOOP
                float3 positionWS = inputData.positionWS;

                #if USE_CLUSTER_LIGHT_LOOP
                // Additional directional lights live outside the cluster list.
                for (uint di = 0; di < min(URP_FP_DIRECTIONAL_LIGHTS_COUNT, MAX_VISIBLE_LIGHTS); di++)
                {
                    Light dirLight = GetAdditionalLight(di, positionWS, half4(1, 1, 1, 1));
                    float dirPhase = HenyeyGreenstein(dot(rayDir, dirLight.direction), anisotropy);
                    addedLighting += dirLight.color * dirLight.distanceAttenuation * dirLight.shadowAttenuation * dirPhase;
                }
                #endif

                uint lightCount = GetAdditionalLightsCount();
                LIGHT_LOOP_BEGIN(lightCount)
                    Light light = GetAdditionalLight(lightIndex, positionWS, half4(1, 1, 1, 1));
                    float phase = HenyeyGreenstein(dot(rayDir, light.direction), anisotropy);
                    addedLighting += light.color * light.distanceAttenuation * light.shadowAttenuation * phase;
                LIGHT_LOOP_END
            #endif
                return addedLighting;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                float2 uv = input.texcoord;
                half4 sourceColor = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv);

                if (_VolumeCount <= 0)
                    return sourceColor;

                float rawDepth = SampleSceneDepth(uv);
                float3 scenePosWS = ComputeWorldSpacePosition(uv, rawDepth, UNITY_MATRIX_I_VP);

                float3 rayOrigin = GetCameraPositionWS();
                float3 toScene = scenePosWS - rayOrigin;
                float sceneDist = length(toScene);
                float3 rayDir = toScene / max(sceneDist, 1e-5);

                #if UNITY_REVERSED_Z
                    bool isSky = rawDepth <= 1e-7;
                #else
                    bool isSky = rawDepth >= 1.0 - 1e-7;
                #endif
                if (isSky)
                    sceneDist = _ProjectionParams.z;

                float jitter = InterleavedGradientNoise(input.positionCS.xy, 0) * _JitterIntensity;

                InputData inputData = (InputData)0;
                inputData.normalizedScreenSpaceUV = uv;

                Light mainLight = GetMainLight();

                float3 totalVolumetricLight = 0;

                [loop]
                for (int v = 0; v < _VolumeCount; v++)
                {
                    float4x4 worldToLocal = _VolumeWorldToLocal[v];
                    float3 localOrigin = mul(worldToLocal, float4(rayOrigin, 1.0)).xyz;
                    float3 localDir    = mul((float3x3)worldToLocal, rayDir);

                    float tNear, tFar;
                    if (!IntersectUnitBox(localOrigin, localDir, tNear, tFar))
                        continue;

                    tNear = max(tNear, 0.0);
                    tFar  = min(tFar, sceneDist);
                    if (tFar <= tNear)
                        continue;

                    float4 params  = _VolumeParams[v];
                    float4 params2 = _VolumeParams2[v];

                    int steps = (int)params.z;
                    float stepSize = (tFar - tNear) / steps;
                    float t = tNear + stepSize * jitter;

                    // light.direction points from the sample towards the light, so this is the
                    // angle between the incoming photon and the one scattered into the camera.
                    // Positive anisotropy therefore peaks when looking towards the light.
                    float mainPhase = HenyeyGreenstein(dot(rayDir, mainLight.direction), params.y);
                    float3 volumeColor = _VolumeColor[v].rgb;

                    [loop]
                    for (int i = 0; i < steps; i++)
                    {
                        float3 localPos = localOrigin + localDir * t;
                        float fade = EdgeFade(localPos, params.w);

                        if (fade > 0.0)
                        {
                            float3 posWS = rayOrigin + rayDir * t;
                            inputData.positionWS = posWS;

                            float shadow = 1.0;
                            if (params2.x > 0.5)
                                shadow = MainLightRealtimeShadow(TransformWorldToShadowCoord(posWS));

                            float3 stepLighting = mainLight.color * shadow * mainPhase;

                            if (params2.y > 0.5)
                                stepLighting += EvaluateAdditionalLights(inputData, rayDir, params.y);

                            totalVolumetricLight += stepLighting * volumeColor * (params.x * fade * stepSize);
                        }

                        t += stepSize;
                    }
                }

                return sourceColor + half4(totalVolumetricLight, 0);
            }
            ENDHLSL
        }
    }
}
