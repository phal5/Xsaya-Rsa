Shader "Hidden/Custom/VisionMask"
{
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline" = "UniversalPipeline" }
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            Name "VisionMask"

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            // Must match VolumetricLightingRenderFeature.kMaxRevealSources.
            #define MAX_REVEAL_SOURCES 4

            float4 _VisionMaskColor;   // rgb : multiplier for masked pixels, a : 1 when the sky is masked

            int    _VisionRevealCount;
            float4 _VisionRevealSphere[MAX_REVEAL_SOURCES]; // xyz : centre, w : outer radius
            float4 _VisionRevealParams[MAX_REVEAL_SOURCES]; // x : inner radius, y : strength

            // How far the reveal sources open the mask at this point. 1 = fully clear.
            float RevealAt(float3 positionWS)
            {
                float reveal = 0.0;

                [loop]
                for (int r = 0; r < _VisionRevealCount; r++)
                {
                    float4 sphere = _VisionRevealSphere[r];
                    float4 params = _VisionRevealParams[r];

                    float d = distance(positionWS, sphere.xyz);
                    float f = 1.0 - smoothstep(params.x, sphere.w, d);
                    reveal = max(reveal, f * params.y);
                }

                return saturate(reveal);
            }

            half4 Frag(Varyings input) : SV_Target
            {
                float2 uv = input.texcoord;
                half4 sourceColor = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv);

                float rawDepth = SampleSceneDepth(uv);

                #if UNITY_REVERSED_Z
                    bool isSky = rawDepth <= 1e-7;
                #else
                    bool isSky = rawDepth >= 1.0 - 1e-7;
                #endif

                if (isSky && _VisionMaskColor.a < 0.5)
                    return sourceColor;

                // The sky sits at infinity, so no reveal sphere ever reaches it.
                float reveal = 0.0;
                if (!isSky)
                {
                    float3 positionWS = ComputeWorldSpacePosition(uv, rawDepth, UNITY_MATRIX_I_VP);
                    reveal = RevealAt(positionWS);
                }

                float3 multiplier = lerp(_VisionMaskColor.rgb, float3(1.0, 1.0, 1.0), reveal);

                return half4(sourceColor.rgb * multiplier, sourceColor.a);
            }
            ENDHLSL
        }
    }
}
