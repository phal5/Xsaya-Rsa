// 베기 호를 색이 아니라 왜곡으로 보여준다.
//
// 그림을 그리지 않고 뒤에 있던 화면을 밀어내기만 하므로, 배경이 어떤 색이든 묻히지 않는다.
// 반원 메시의 UV는 U가 호를 따라가는 각도, V가 날의 폭이라 텍스처 없이 마스크가 나온다.
//
// 화면을 읽으려면 URP 애셋의 Opaque Texture가 켜져 있어야 한다. PC_RPAsset은 켜져 있다.
Shader "Custom/SlashDistortion"
{
    Properties
    {
        _Strength ("왜곡 세기", Range(0, 0.5)) = 0.15
        _EdgeSoftness ("가장자리 무르기", Range(0.01, 0.5)) = 0.3
        _HeadBias ("머리 쪽 쏠림", Range(0, 1)) = 0.35
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Transparent"
            "Queue" = "Transparent+10"
            "RenderPipeline" = "UniversalPipeline"
            "IgnoreProjector" = "True"
        }

        Pass
        {
            Name "SlashDistortion"

            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Off

            HLSLPROGRAM
            #pragma vertex Vertex
            #pragma fragment Fragment

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareOpaqueTexture.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                half4 color : COLOR;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 screenPos : TEXCOORD1;
                half4 color : COLOR;
            };

            CBUFFER_START(UnityPerMaterial)
                float _Strength;
                float _EdgeSoftness;
                float _HeadBias;
            CBUFFER_END

            Varyings Vertex(Attributes input)
            {
                Varyings output;

                VertexPositionInputs positions = GetVertexPositionInputs(input.positionOS.xyz);
                output.positionCS = positions.positionCS;
                output.screenPos = positions.positionNDC;
                output.uv = input.uv;
                output.color = input.color;

                return output;
            }

            half4 Fragment(Varyings input) : SV_Target
            {
                float2 screenUV = input.screenPos.xy / input.screenPos.w;

                // 네 가장자리를 모두 무르게 만든다. 딱 끊으면 왜곡된 자리와 아닌 자리 사이에
                // 메시 모양 그대로의 경계선이 생겨, 호가 아니라 잘린 판때기로 보인다.
                float alongArc = smoothstep(0.0, _EdgeSoftness, input.uv.x)
                               * smoothstep(0.0, _EdgeSoftness, 1.0 - input.uv.x);
                float acrossBlade = smoothstep(0.0, _EdgeSoftness, input.uv.y)
                                  * smoothstep(0.0, _EdgeSoftness, 1.0 - input.uv.y);

                // 벤 자리가 가장 세고 지나온 쪽으로 갈수록 잦아든다.
                float tail = lerp(1.0, 1.0 - input.uv.x, _HeadBias);

                // 파티클 알파를 그대로 받는다. Color over Lifetime으로 사라지게 할 수 있다.
                float presence = alongArc * acrossBlade * tail * input.color.a;

                // 날의 폭 방향이 화면에서 어느 쪽인지는 UV의 화면 미분이 알려준다.
                // 메시가 어떤 각도로 놓이든, 심지어 회전 중이어도 따라온다.
                float2 gradient = float2(ddx(input.uv.y), ddy(input.uv.y));
                float2 direction = gradient / max(length(gradient), 1e-5);

                // 밀어내는 양은 알파와 따로 잡는다. 가장자리를 무르게 만든 마스크를 여기에도
                // 곱하면 정작 가장 많이 밀어야 할 자리에서 0이 되어, 세기를 올려도 굴절이 늘지 않는다.
                //
                // 프로파일은 양 끝에서 0이고 그 사이에서 ±1까지 간다. 끝에서 0인 덕분에
                // 마스크 없이도 경계에 밀린 자국이 남지 않는다. 2.598은 x(1-x^2)의 최대값 역수다.
                float across = input.uv.y * 2.0 - 1.0;
                float lens = across * (1.0 - across * across) * 2.598;
                float2 offset = direction * lens * _Strength * saturate(tail * input.color.a);

                half3 scene = SampleSceneColor(saturate(screenUV + offset));

                return half4(scene, presence);
            }
            ENDHLSL
        }
    }

    Fallback Off
}
