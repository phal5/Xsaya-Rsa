// URP port of Sun_Temple/Clouds.
// Unlit soft-additive backdrop clouds with the original scrolling UV distortion.
Shader "Sun_Temple_URP/Clouds"
{
    Properties
    {
        _Color("Main Color", Color) = (1,1,1,1)
        _MainTex("Base (RGB) Gloss (A)", 2D) = "white" {}

        _DistortionTexture("Distortion Texture", 2D) = "black" {}
        _DistortionIntensity("Distortion Intensity", Range(0, 1)) = 0.5
        _ScrollSpeed("Scroll Speed", Float) = 0.5
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" "Queue" = "Overlay" "IgnoreProjector" = "True" }
        LOD 200

        Pass
        {
            Name "Unlit"
            Tags { "LightMode" = "UniversalForward" }

            Cull Off
            ZWrite Off
            Blend OneMinusDstColor One

            HLSLPROGRAM
            #pragma target 3.0
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _DistortionTexture_ST;
                half4  _Color;
                half   _DistortionIntensity;
                half   _ScrollSpeed;
            CBUFFER_END

            TEXTURE2D(_MainTex);            SAMPLER(sampler_MainTex);
            TEXTURE2D(_DistortionTexture);  SAMPLER(sampler_DistortionTexture);

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv         : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float2 uv         : TEXCOORD0;
                float4 positionCS : SV_POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            Varyings vert(Attributes input)
            {
                Varyings output = (Varyings)0;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                float2 uv = input.uv * _MainTex_ST.xy + _MainTex_ST.zw;

                half scrollX = _ScrollSpeed * _Time.x;
                float2 uv_scrolled = uv + float2(scrollX, 0);

                half distortion = SAMPLE_TEXTURE2D(_DistortionTexture, sampler_DistortionTexture, uv_scrolled).r;
                half uv_distorted_x = (distortion * _DistortionIntensity * 0.1) - 0.05;

                float2 uv_distorted_xy = uv + float2(uv_distorted_x, 0);

                half3 col = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv_distorted_xy).rgb;
                half3 finalAlbedo = col * _Color.rgb;

                return half4(saturate(finalAlbedo), 1);
            }
            ENDHLSL
        }
    }

    FallBack Off
}
