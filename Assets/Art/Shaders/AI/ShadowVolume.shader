Shader "Hidden/Custom/ShadowVolume"
{
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline" = "UniversalPipeline" }
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            Name "ShadowVolume"

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            // Must match VolumetricLightingRenderFeature.kMaxVolumes.
            #define MAX_SHADOW_VOLUMES 8

            int      _ShadowVolumeCount;
            float4x4 _ShadowVolumeWorldToLocal[MAX_SHADOW_VOLUMES]; // maps the box onto the unit cube [-1, 1]
            float4   _ShadowVolumeTint[MAX_SHADOW_VOLUMES];         // rgb : light multiplier, a : edge fade

            // 1 in the core of the box, 0 at its faces and outside.
            float ShadeWeight(float3 localPos, float fadeWidth)
            {
                float3 a = abs(localPos);
                float furthest = max(max(a.x, a.y), a.z);
                if (furthest > 1.0)
                    return 0.0;

                return saturate((1.0 - furthest) / max(fadeWidth, 1e-4));
            }

            half4 Frag(Varyings input) : SV_Target
            {
                float2 uv = input.texcoord;
                half4 sourceColor = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv);

                if (_ShadowVolumeCount <= 0)
                    return sourceColor;

                float rawDepth = SampleSceneDepth(uv);

                // The sky has no surface to shade, so it is never darkened.
                #if UNITY_REVERSED_Z
                    if (rawDepth <= 1e-7)
                        return sourceColor;
                #else
                    if (rawDepth >= 1.0 - 1e-7)
                        return sourceColor;
                #endif

                float3 positionWS = ComputeWorldSpacePosition(uv, rawDepth, UNITY_MATRIX_I_VP);

                float3 multiplier = 1.0;

                [loop]
                for (int v = 0; v < _ShadowVolumeCount; v++)
                {
                    float3 localPos = mul(_ShadowVolumeWorldToLocal[v], float4(positionWS, 1.0)).xyz;
                    float4 tint = _ShadowVolumeTint[v];

                    float weight = ShadeWeight(localPos, tint.a);
                    if (weight > 0.0)
                        multiplier *= lerp(float3(1.0, 1.0, 1.0), tint.rgb, weight);
                }

                return half4(sourceColor.rgb * multiplier, sourceColor.a);
            }
            ENDHLSL
        }
    }
}
