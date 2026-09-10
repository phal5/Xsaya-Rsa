// 베기 호를 색이 아니라 왜곡으로 보여준다.
//
// 그림을 그리지 않고 뒤에 있던 화면을 밀어내기만 하므로, 배경이 어떤 색이든 묻히지 않는다.
// 반원 메시의 UV는 U가 호를 따라가는 각도, V가 날의 폭이라 텍스처 없이 마스크가 나온다.
//
// 화면을 읽으려면 URP 애셋의 Opaque Texture가 켜져 있어야 한다. PC_RPAsset은 켜져 있다.
//
// 순차로 드러나는 것은 파티클 나이를 받아서 한다. 파티클 시스템 렌더러의 Custom Vertex Streams에
// AgePercent가 UV 다음에 꽂혀 있어야 TEXCOORD0.z로 들어온다. 빠져 있으면 z가 0으로 고정되어
// 아무것도 드러나지 않는다 — 이펙트가 통째로 안 보이면 여기부터 본다.
//
// 나이 스트림이 없는 메시(트레일)는 _UseAge를 끈다. 그러면 처음부터 다 드러난 것으로 치고,
// 사라지는 것은 정점 알파가 맡는다 — 회피 트레일(M_DodgeDistortion)이 그렇게 쓴다.
Shader "Custom/SlashDistortion"
{
    Properties
    {
        _Strength ("왜곡 세기", Range(0, 2)) = 0.15
        _EdgeSoftness ("가장자리 무르기", Range(0.01, 0.5)) = 0.3
        _HeadBias ("머리 쪽 쏠림", Range(0, 1)) = 0.35
        _Chroma ("채도 강조 n", Range(-1, 5)) = 1.5
        _Shade ("밝기 치우침 (-검게 / +희게)", Range(-1, 1)) = 0
        _RevealTime ("드러나는 데 쓰는 수명 비율", Range(0.05, 1)) = 0.4
        _RevealSoftness ("드러나는 경계 무르기", Range(0.001, 0.5)) = 0.08
        _RevealFlip ("반대쪽부터 드러내기", Range(0, 1)) = 0
        [ToggleUI] _UseAge ("파티클 나이로 드러내기 (트레일은 끈다)", Float) = 1
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
                float4 uv : TEXCOORD0;   // xy 메시 UV, z 파티클 나이(AgePercent 스트림)
                half4 color : COLOR;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 uv : TEXCOORD0;
                float4 screenPos : TEXCOORD1;
                half4 color : COLOR;
            };

            CBUFFER_START(UnityPerMaterial)
                float _Strength;
                float _EdgeSoftness;
                float _HeadBias;
                float _Chroma;
                float _Shade;
                float _RevealTime;
                float _RevealSoftness;
                float _RevealFlip;
                float _UseAge;
            CBUFFER_END

            Varyings Vertex(Attributes input)
            {
                Varyings output;

                VertexPositionInputs positions = GetVertexPositionInputs(input.positionOS.xyz);
                output.positionCS = positions.positionCS;
                output.screenPos = positions.positionNDC;
                output.uv = input.uv.xyz;
                output.color = input.color;

                return output;
            }

            half4 Fragment(Varyings input) : SV_Target
            {
                // 얇은 끝(U=0)에서 넓은 끝(U=1)으로 쓸어 드러낸다. 아직 오지 않은 자리는
                // 반투명하게 두지 않고 아예 버린다 — 남겨두면 호 전체가 옅게 미리 보여서
                // 순차로 그어지는 것이 아니라 통째로 나타났다 진해지는 것으로 읽힌다.
                // 나이 스트림이 없으면 z가 0으로 들어와 아래 clip이 전부를 버린다. 그런 메시는
                // 나이를 쓰지 않고 다 드러난 것으로 친다 — 슬래시는 켜 둔 채라 전과 같다.
                float age = lerp(1.0, input.uv.z, _UseAge);
                float head = saturate(age / max(_RevealTime, 1e-4));
                float along = lerp(input.uv.x, 1.0 - input.uv.x, _RevealFlip);
                float edge = head * (1.0 + _RevealSoftness) - along;

                clip(edge);

                float reveal = saturate(edge / max(_RevealSoftness, 1e-4));

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
                float presence = alongArc * acrossBlade * tail * input.color.a * reveal;

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

                // 굴곡이 큰 자리일수록 색이 짙어진다. 굴곡은 가장자리에서 0이므로
                // 겉으로 갈수록 저절로 옅어지고, 따로 마스크를 곱해줄 필요가 없다.
                float bend = abs(lens) * presence;

                // 세 채널의 평균에서 얼마나 떨어져 있는지가 그 색의 치우침이다.
                // 그 치우침을 n배 더 얹으면 회색에서 멀어진다 — n이 음수면 반대로 회색에 가까워진다.
                float v = (scene.r + scene.g + scene.b) * (1.0 / 3.0);
                half3 tinted = scene + (scene - v) * (_Chroma * bend);

                // 굴곡이 큰 자리를 검거나 희게 민다. 채도를 올린 뒤에 얹으므로,
                // 짙어진 색이 그대로 밝아지거나 어두워진다. 0이면 아무 일도 일어나지 않는다.
                half3 target = saturate(sign(_Shade));          // 양수면 흰색, 음수면 검은색
                half3 shaded = lerp(tinted, target, saturate(abs(_Shade) * bend));

                // 아래로만 막는다. 위를 자르면 HDR 밝은 자리가 뭉개진다.
                return half4(max(shaded, 0.0), presence);
            }
            ENDHLSL
        }
    }

    Fallback Off
}
