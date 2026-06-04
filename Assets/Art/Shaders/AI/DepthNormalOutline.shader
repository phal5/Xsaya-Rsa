Shader "Hidden/Custom/LaplacianDepthOutline"
{
    Properties
    {
        [Toggle] _UseOverlay ("Use Overlay Blend", Float) = 1
        _OutlineColor ("Outline Color", Color) = (0, 0, 0, 1)
        _BaseThickness ("Base Sampling Radius", Range(0.5, 5.0)) = 1.0
        _Threshold ("Edge Threshold", Float) = 0.01
        _LineIntensity ("Line Intensity Multiplier", Float) = 50.0
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline" = "UniversalPipeline" }
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            Name "LaplacianDepthOutline"

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            float _UseOverlay;
            half4 _OutlineColor;
            float _BaseThickness;
            float _Threshold;
            float _LineIntensity;

            half3 BlendOverlay(half3 base, half3 blend)
            {
                // Overlay logic: If base < 0.5, multiply. If base >= 0.5, screen.
                half3 branch = step(base, half3(0.5, 0.5, 0.5));
                half3 case1 = 2.0 * base * blend;
                half3 case2 = 1.0 - 2.0 * (1.0 - base) * (1.0 - blend);
                return lerp(case2, case1, branch);
            }

            half4 Frag(Varyings input) : SV_Target
            {
                float2 uv = input.texcoord;
                half4 sourceColor = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv);

                // Calculate the offset size based on screen resolution
                float2 texel = _ScreenSize.zw * _BaseThickness;

                // 1. Read depth of center pixel
                float centerDepth = LinearEyeDepth(SampleSceneDepth(uv), _ZBufferParams);

                // Ensure we don't divide by zero later (near clipping plane protection)
                centerDepth = max(centerDepth, 0.0001); 

                // 2. Read depth from 8 adjacent pixels
                float sumDepth = 0;
                
                float2 offsets[8] = {
                    float2(-1, -1), float2(0, -1), float2(1, -1),
                    float2(-1,  0),                float2(1,  0),
                    float2(-1,  1), float2(0,  1), float2(1,  1)
                };

                [unroll]
                for(int i = 0; i < 8; i++)
                {
                    float2 sampleUV = uv + (offsets[i] * texel);
                    sumDepth += LinearEyeDepth(SampleSceneDepth(sampleUV), _ZBufferParams);
                }

                float avgDepth = sumDepth * 0.125;

                // 3. Calculate difference and apply your ratio formula
                float absoluteDifference = abs(centerDepth - avgDepth);
                
                // depth - difference divided by depth
                float depthRatio = absoluteDifference / centerDepth;

                // 4. Threshold and Thickness Mapping
                // We multiply by _LineIntensity to give you control over how dark/thick the line appears
                float edge = saturate((depthRatio - _Threshold)) * _LineIntensity;

                // --- 4. Alpha & Overlay Blending Composite ---
                
                // Combine your calculated edge intensity with the material's Alpha slider
                float finalAlpha = saturate(edge * _OutlineColor.a);

                // Calculate the Photoshop Overlay color
                half3 overlayColor = BlendOverlay(sourceColor.rgb, _OutlineColor.rgb);

                // Choose between normal color and overlay color based on your toggle
                half3 finalOutlineColor = lerp(_OutlineColor.rgb, overlayColor, _UseOverlay);

                // Blend the chosen outline color over the scene using the final alpha
                return half4(lerp(sourceColor.rgb, finalOutlineColor, finalAlpha), sourceColor.a);
            }
            ENDHLSL
        }
    }
}