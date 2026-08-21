// URP port of Sun_Temple/VertexBlend.
// Keeps the per-vertex blend between the base layer and layer B that the original
// used for plaster peeling off brickwork, plus the detail albedo/normal overlay.
Shader "Sun_Temple_URP/VertexBlend"
{
    Properties
    {
        _Color("BASE Tint (RGB), Tint Fade (A)", Color) = (0.5, 0.5, 0.5, 0)
        _MainTex("BASE Albedo (RGB) Tint Mask (A)", 2D) = "white" {}
        [Normal]_BumpMap("BASE Normal (RGB)", 2D) = "bump" {}
        _Roughness("BASE Roughness", Range(0,1)) = 1

        [NoScaleOffset] _layer1Tex("LAYER_B Albedo (RGB)", 2D) = "white" {}
        [Normal][NoScaleOffset] _layer1Norm("LAYER_B Normal (RGB)", 2D) = "bump" {}
        _layer1Tiling("LAYER_B Tiling", Float) = 1
        _layer1Rough("LAYER_B Roughness", Range(0, 1)) = 1

        [NoScaleOffset] _BlendMask("BLEND_Mask (R)", 2D) = "white" {}
        _BlendTile("BLEND_Tiling", Float) = 1
        _Choke("BLEND_Choke", Range(0, 60)) = 15
        _Crisp("BLEND_Crispyness", Range(1, 20)) = 5

        [NoScaleOffset] _DetailAlbedo("DETAIL_Albedo (R)", 2D) = "grey" {}
        [Normal][NoScaleOffset] _DetailNormal("DETAIL_Normal (RGB)", 2D) = "bump" {}
        _DetailNormalStrength("DETAIL_Normal Strength", Range(0,1)) = 0.4
        _DetailTiling("DETAIL_Tiling", Float) = 2
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" "Queue" = "Geometry" }
        LOD 500

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"

        CBUFFER_START(UnityPerMaterial)
            float4 _MainTex_ST;
            float4 _BumpMap_ST;
            half4  _Color;
            half   _Roughness;
            half   _layer1Tiling;
            half   _layer1Rough;
            half   _BlendTile;
            half   _Choke;
            half   _Crisp;
            half   _DetailNormalStrength;
            half   _DetailTiling;
        CBUFFER_END
        ENDHLSL

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma target 3.0
            #pragma vertex vert
            #pragma fragment frag

            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #pragma multi_compile_fragment _ _SCREEN_SPACE_OCCLUSION
            #pragma multi_compile_fragment _ _REFLECTION_PROBE_BLENDING
            #pragma multi_compile_fragment _ _REFLECTION_PROBE_BOX_PROJECTION
            #pragma multi_compile _ LIGHTMAP_ON
            #pragma multi_compile _ DYNAMICLIGHTMAP_ON
            #pragma multi_compile _ DIRLIGHTMAP_COMBINED
            #pragma multi_compile _ SHADOWS_SHADOWMASK
            #pragma multi_compile _ LIGHTMAP_SHADOW_MIXING
            #pragma multi_compile_fragment _ _LIGHT_LAYERS
            #pragma multi_compile_fog
            #pragma multi_compile_instancing

            #include "SunTempleCommon.hlsl"

            TEXTURE2D(_MainTex);       SAMPLER(sampler_MainTex);
            TEXTURE2D(_BumpMap);       SAMPLER(sampler_BumpMap);
            TEXTURE2D(_layer1Tex);     SAMPLER(sampler_layer1Tex);
            TEXTURE2D(_layer1Norm);    SAMPLER(sampler_layer1Norm);
            TEXTURE2D(_BlendMask);     SAMPLER(sampler_BlendMask);
            TEXTURE2D(_DetailAlbedo);  SAMPLER(sampler_DetailAlbedo);
            TEXTURE2D(_DetailNormal);  SAMPLER(sampler_DetailNormal);

            STVaryings vert(STAttributes input)
            {
                return STVertex(input, float3(0, 0, 0), input.normalOS);
            }

            half4 frag(STVaryings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                float2 uvMain = input.uv * _MainTex_ST.xy + _MainTex_ST.zw;
                float2 uvBump = input.uv * _BumpMap_ST.xy + _BumpMap_ST.zw;

                half4 main = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uvMain);
                main = lerp(main, _Color, main.a * _Color.a);

                half3 normal = UnpackNormal(SAMPLE_TEXTURE2D(_BumpMap, sampler_BumpMap, uvBump));

                half3 layer1Albedo = SAMPLE_TEXTURE2D(_layer1Tex, sampler_layer1Tex, uvMain * _layer1Tiling).rgb;
                half3 layer1Normal = UnpackNormal(SAMPLE_TEXTURE2D(_layer1Norm, sampler_layer1Norm, uvMain * _layer1Tiling));

                half blendMask = SAMPLE_TEXTURE2D(_BlendMask, sampler_BlendMask, uvMain * _BlendTile).r;
                blendMask = clamp(blendMask, 0.2, 0.9);

                half detailAlbedo = SAMPLE_TEXTURE2D(_DetailAlbedo, sampler_DetailAlbedo, uvMain * _DetailTiling).r;
                half3 detailNormal = UnpackNormal(SAMPLE_TEXTURE2D(_DetailNormal, sampler_DetailNormal, uvMain * _DetailTiling));

                // vertex colour red drives which layer shows through
                half blend = (input.color.r * blendMask) * _Choke;
                blend = pow(max(blend, 0), _Crisp);
                blend = saturate(blend);

                half3 blendedAlbedo = lerp(layer1Albedo, main.rgb, blend);
                // unity_ColorSpaceDouble was 2.0 in linear; LerpWhiteTo(x, 1) == x
                blendedAlbedo = blendedAlbedo * (detailAlbedo * 2.0);

                half3 blendedNormal = lerp(layer1Normal, normal, blend);
                blendedNormal = blendedNormal + (detailNormal * half3(_DetailNormalStrength, _DetailNormalStrength, 0));

                half blendedSmoothness = lerp(_layer1Rough, _Roughness, blend);

                SurfaceData surf = STDefaultSurface();
                surf.albedo = blendedAlbedo;
                surf.normalTS = SafeNormalize(blendedNormal);
                surf.smoothness = saturate(1 - blendedSmoothness);
                surf.metallic = 0;

                return STFragmentPBR(input, surf);
            }
            ENDHLSL
        }

        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }
            ZWrite On
            ZTest LEqual
            ColorMask 0

            HLSLPROGRAM
            #pragma target 3.0
            #pragma vertex STShadowVertex
            #pragma fragment STShadowFragment
            #pragma multi_compile_vertex _ _CASTING_PUNCTUAL_LIGHT_SHADOW
            #pragma multi_compile_instancing
            #define ST_SHADOW_PASS
            #include "SunTempleDepthPasses.hlsl"
            ENDHLSL
        }

        Pass
        {
            Name "DepthOnly"
            Tags { "LightMode" = "DepthOnly" }
            ZWrite On
            ColorMask R

            HLSLPROGRAM
            #pragma target 3.0
            #pragma vertex STDepthVertex
            #pragma fragment STDepthFragment
            #pragma multi_compile_instancing
            #define ST_DEPTH_PASS
            #include "SunTempleDepthPasses.hlsl"
            ENDHLSL
        }

        Pass
        {
            Name "DepthNormals"
            Tags { "LightMode" = "DepthNormals" }
            ZWrite On

            HLSLPROGRAM
            #pragma target 3.0
            #pragma vertex STDepthNormalsVertex
            #pragma fragment STDepthNormalsFragment
            #pragma multi_compile_instancing
            #define ST_DEPTHNORMALS_PASS
            #include "SunTempleDepthPasses.hlsl"
            ENDHLSL
        }
    }

    FallBack Off
}
