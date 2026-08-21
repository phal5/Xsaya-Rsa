// URP port of Sun_Temple/Decal_Puddle.
// The puddle shape comes from the mask's R channel and the wet ripple from two
// counter-scrolling samples of one normal map, exactly as the original did.
Shader "Sun_Temple_URP/Decal_Puddle"
{
    Properties
    {
        _Color("Color", Color) = (0.5, 0.5, 0.5, 0)
        _Mask("Mask (R)", 2D) = "black" {}
        _MaskFade("Mask Fade", Range(0, 1)) = 0
        [Normal]_BumpMap("Normal (RGB)", 2D) = "bump" {}

        _Roughness("Roughness", Range(0, 1)) = 0
        _ScrollSpeed("ScrollSpeed", Range(0, 4)) = 2
    }

    SubShader
    {
        Tags { "RenderType" = "Transparent" "RenderPipeline" = "UniversalPipeline" "Queue" = "Transparent" }
        LOD 300

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"

        CBUFFER_START(UnityPerMaterial)
            float4 _Mask_ST;
            float4 _BumpMap_ST;
            half4  _Color;
            half   _MaskFade;
            half   _Roughness;
            half   _ScrollSpeed;
        CBUFFER_END
        ENDHLSL

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Back

            HLSLPROGRAM
            #pragma target 3.0
            #pragma vertex vert
            #pragma fragment frag

            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #pragma multi_compile_fragment _ _REFLECTION_PROBE_BLENDING
            #pragma multi_compile_fragment _ _REFLECTION_PROBE_BOX_PROJECTION
            #pragma multi_compile _ LIGHTMAP_ON
            #pragma multi_compile _ DYNAMICLIGHTMAP_ON
            #pragma multi_compile _ DIRLIGHTMAP_COMBINED
            #pragma multi_compile _ SHADOWS_SHADOWMASK
            #pragma multi_compile _ LIGHTMAP_SHADOW_MIXING
            #pragma multi_compile_fog
            #pragma multi_compile_instancing

            #include "SunTempleCommon.hlsl"

            TEXTURE2D(_Mask);     SAMPLER(sampler_Mask);
            TEXTURE2D(_BumpMap);  SAMPLER(sampler_BumpMap);

            STVaryings vert(STAttributes input)
            {
                return STVertex(input, float3(0, 0, 0), input.normalOS);
            }

            half4 frag(STVaryings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                float2 uvMask = input.uv * _Mask_ST.xy + _Mask_ST.zw;
                float2 uvBump = input.uv * _BumpMap_ST.xy + _BumpMap_ST.zw;

                half scroll = _ScrollSpeed * _Time.x;
                float2 uv1 = uvBump + float2(scroll, scroll);
                float2 uv2 = uvBump - float2(scroll, scroll);

                half3 normal_a = UnpackNormal(SAMPLE_TEXTURE2D(_BumpMap, sampler_BumpMap, uv1));
                half3 normal_b = UnpackNormal(SAMPLE_TEXTURE2D(_BumpMap, sampler_BumpMap, uv2));
                half3 normalCombined = normal_a + normal_b;

                half alpha = SAMPLE_TEXTURE2D(_Mask, sampler_Mask, uvMask).r;

                SurfaceData surf = STDefaultSurface();
                surf.albedo = _Color.rgb;
                surf.normalTS = SafeNormalize(normalCombined);
                surf.metallic = _Color.a;
                surf.smoothness = saturate(1 - _Roughness);
                surf.alpha = lerp(alpha, 0, _MaskFade);

                return STFragmentPBR(input, surf);
            }
            ENDHLSL
        }
    }

    FallBack Off
}
