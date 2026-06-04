Shader "Hidden/Custom/VolumetricRaymarch"
{
    Properties
    {
        _StepCount ("Raymarch Steps", Float) = 32
        _Density ("Fog Density", Float) = 0.1
        [HDR] _FogColor ("Fog Color", Color) = (1, 1, 1, 1)
        _Anisotropy ("Anisotropy (Forward Scattering)", Range(-1, 1)) = 0.5
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
            #pragma multi_compile _ _ADDITIONAL_LIGHT_SHADOWS
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            float _StepCount;
            float _Density;
            half4 _FogColor;
            float _Anisotropy;
            float _JitterIntensity;

            float HenyeyGreenstein(float cosAngle, float g) 
            {
                float g2 = g * g;
                float denom = 1.0 + g2 - 2.0 * g * cosAngle;
                return (1.0 - g2) / (4.0 * PI * pow(denom, 1.5));
            }
            
            float Rand(float2 uv)
            {
                return frac(sin(dot(uv, float2(12.9898, 78.233))) * 43758.5453);
            }

            float3 EvaluateAdditionalLights(float3 currentPosWS, float3 viewDirection, float anisotropy)
            {
                float3 addedLighting = 0;
                #if defined(_ADDITIONAL_LIGHTS)
                uint lightCount = GetAdditionalLightsCount();
                [loop]
                for (uint i = 0; i < lightCount; ++i)
                {
                    Light light = GetAdditionalLight(i, currentPosWS);
                    
                    #if defined(_ADDITIONAL_LIGHT_SHADOWS)
                    light.shadowAttenuation = AdditionalLightRealtimeShadow(i, currentPosWS, light.direction);
                    #endif
                    
                    float totalAttenuation = light.distanceAttenuation * light.shadowAttenuation;
                    float cosAngle = dot(viewDirection, light.direction);
                    float phase = HenyeyGreenstein(cosAngle, anisotropy);
                    
                    addedLighting += light.color * totalAttenuation * phase;
                }
                #endif
                return addedLighting;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                float2 uv = input.texcoord;
                half4 sourceColor = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv);
                
                float rawDepth = SampleSceneDepth(uv);
                float3 currentPosWS = ComputeWorldSpacePosition(uv, rawDepth, UNITY_MATRIX_I_VP);
                
                float3 rayStart = _WorldSpaceCameraPos;
                float3 rayDir = normalize(currentPosWS - rayStart);
                float rayLength = length(currentPosWS - rayStart);
                
                #if !UNITY_REVERSED_Z
                if(rawDepth == 1.0) rayLength = 100.0;
                #else
                if(rawDepth == 0.0) rayLength = 100.0;
                #endif
                
                float stepSize = rayLength / _StepCount;
                float3 stepVector = rayDir * stepSize;
                
                float jitter = Rand(uv) * _JitterIntensity;
                float3 currentPos = rayStart + (stepVector * jitter);
                
                float3 totalVolumetricLight = 0;
                
                [loop]
                for (int i = 0; i < _StepCount; i++)
                {
                    float3 stepLighting = 0;
                    
                    Light mainLight = GetMainLight();
                    float4 shadowCoord = TransformWorldToShadowCoord(currentPos);
                    mainLight.shadowAttenuation = MainLightRealtimeShadow(shadowCoord);
                    
                    float mainCosAngle = dot(rayDir, mainLight.direction);
                    float mainPhase = HenyeyGreenstein(mainCosAngle, _Anisotropy);
                    stepLighting += mainLight.color * mainLight.shadowAttenuation * mainPhase;
                    
                    stepLighting += EvaluateAdditionalLights(currentPos, rayDir, _Anisotropy);
                    
                    // Multiply the light by our new _FogColor!
                    totalVolumetricLight += stepLighting * _FogColor.rgb * _Density * stepSize;
                    
                    currentPos += stepVector;
                }
                
                return sourceColor + half4(totalVolumetricLight, 0);
            }
            ENDHLSL
        }
    }
}