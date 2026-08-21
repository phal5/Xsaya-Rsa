// URP port of Sun_Temple/Decal — alpha-cut mesh decals for murals and wall ornaments.
Shader "Sun_Temple_URP/Decal"
{
    Properties
    {
        _Color("Color Tint (RGB), Fade (A)", Color) = (0.5, 0.5, 0.5, 0)
        _MainTex("Albedo (RGB), Alpha (A)", 2D) = "white" {}
        _Cutoff("Alpha Cutoff", Range(0, 1)) = 0.7

        [NoScaleOffset] _DetailAlbedo("DETAIL_Albedo", 2D) = "grey" {}
        _DetailTiling("DETAIL_Tiling", Float) = 2
    }

    SubShader
    {
        Tags { "RenderType" = "TransparentCutout" "RenderPipeline" = "UniversalPipeline" "Queue" = "AlphaTest" "ForceNoShadowCasting" = "True" }
        LOD 200

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"

        CBUFFER_START(UnityPerMaterial)
            float4 _MainTex_ST;
            half4  _Color;
            half   _Cutoff;
            half   _DetailTiling;
        CBUFFER_END

        TEXTURE2D(_MainTex);       SAMPLER(sampler_MainTex);
        TEXTURE2D(_DetailAlbedo);  SAMPLER(sampler_DetailAlbedo);
        ENDHLSL

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }
            Cull Back

            HLSLPROGRAM
            #pragma target 3.0
            #pragma vertex vert
            #pragma fragment frag

            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #pragma multi_compile_fragment _ _SCREEN_SPACE_OCCLUSION
            #pragma multi_compile _ LIGHTMAP_ON
            #pragma multi_compile _ DYNAMICLIGHTMAP_ON
            #pragma multi_compile _ DIRLIGHTMAP_COMBINED
            #pragma multi_compile _ SHADOWS_SHADOWMASK
            #pragma multi_compile _ LIGHTMAP_SHADOW_MIXING
            #pragma multi_compile_fog
            #pragma multi_compile_instancing

            #include "SunTempleCommon.hlsl"

            STVaryings vert(STAttributes input)
            {
                return STVertex(input, float3(0, 0, 0), input.normalOS);
            }

            half4 frag(STVaryings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                float2 uv = input.uv * _MainTex_ST.xy + _MainTex_ST.zw;

                half4 albedo = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv);
                half detailAlbedo = SAMPLE_TEXTURE2D(_DetailAlbedo, sampler_DetailAlbedo, uv * _DetailTiling).r * 2.0;

                albedo.rgb = albedo.rgb * _Color.rgb * detailAlbedo;
                half alphaMask = albedo.a * detailAlbedo * _Color.a;

                clip(alphaMask - _Cutoff);

                SurfaceData surf = STDefaultSurface();
                surf.albedo = lerp(_Color.rgb, albedo.rgb, alphaMask);
                surf.alpha = alphaMask;
                surf.metallic = 0;
                surf.smoothness = 0;

                return STFragmentPBR(input, surf);
            }
            ENDHLSL
        }

        Pass
        {
            Name "DepthOnly"
            Tags { "LightMode" = "DepthOnly" }
            ZWrite On
            ColorMask R
            Cull Back

            HLSLPROGRAM
            #pragma target 3.0
            #pragma vertex STDepthVertex
            #pragma fragment STDepthFragment
            #pragma multi_compile_instancing
            #define ST_DEPTH_CLIP(uv) clip(SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, (uv) * _MainTex_ST.xy + _MainTex_ST.zw).a * _Color.a - _Cutoff);
            #define ST_DEPTH_PASS
            #include "SunTempleDepthPasses.hlsl"
            ENDHLSL
        }

        Pass
        {
            Name "DepthNormals"
            Tags { "LightMode" = "DepthNormals" }
            ZWrite On
            Cull Back

            HLSLPROGRAM
            #pragma target 3.0
            #pragma vertex STDepthNormalsVertex
            #pragma fragment STDepthNormalsFragment
            #pragma multi_compile_instancing
            #define ST_DEPTH_CLIP(uv) clip(SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, (uv) * _MainTex_ST.xy + _MainTex_ST.zw).a * _Color.a - _Cutoff);
            #define ST_DEPTHNORMALS_PASS
            #include "SunTempleDepthPasses.hlsl"
            ENDHLSL
        }
    }

    FallBack Off
}
