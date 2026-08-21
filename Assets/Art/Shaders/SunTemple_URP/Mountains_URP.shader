// URP port of Sun_Temple/Mountains.
// Two texture layers blended by a mask map, laid over one large terrain normal.
Shader "Sun_Temple_URP/Mountains"
{
    Properties
    {
        _TerrainNormal("Terrain Normal map (overall)", 2D) = "bump" {}

        _MainTex("Layer_A Albedo (RGB)", 2D) = "white" {}
        _BumpMap("LAYER_A Normal", 2D) = "bump" {}
        _baseTiling("LAYER_A Tiling", Float) = 1

        _layer1Tex("LAYER_B Albedo (RGB) Smoothness (A)", 2D) = "white" {}
        _layer1Norm("LAYER_B Normal", 2D) = "bump" {}
        _layer1Tiling("LAYER_B Tiling", Float) = 1

        _BlendMask("BLEND_Mask", 2D) = "white" {}
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
            float4 _BlendMask_ST;
            float4 _TerrainNormal_ST;
            float4 _layer1Tex_ST;
            float4 _layer1Norm_ST;
            half   _baseTiling;
            half   _layer1Tiling;
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

            TEXTURE2D(_MainTex);        SAMPLER(sampler_MainTex);
            TEXTURE2D(_BumpMap);        SAMPLER(sampler_BumpMap);
            TEXTURE2D(_layer1Tex);      SAMPLER(sampler_layer1Tex);
            TEXTURE2D(_layer1Norm);     SAMPLER(sampler_layer1Norm);
            TEXTURE2D(_BlendMask);      SAMPLER(sampler_BlendMask);
            TEXTURE2D(_TerrainNormal);  SAMPLER(sampler_TerrainNormal);

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

                half3 layerA_albedo = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uvMain * _baseTiling).rgb;
                half3 layerA_normal = UnpackNormal(SAMPLE_TEXTURE2D(_BumpMap, sampler_BumpMap, uvBump * _baseTiling));

                half3 terrainNormal = UnpackNormal(SAMPLE_TEXTURE2D(_TerrainNormal, sampler_TerrainNormal, uvBump));

                half3 layerB_albedo = SAMPLE_TEXTURE2D(_layer1Tex, sampler_layer1Tex, uvMain * _layer1Tiling).rgb;
                half3 layerB_normal = UnpackNormal(SAMPLE_TEXTURE2D(_layer1Norm, sampler_layer1Norm, uvMain * _layer1Tiling));

                half blendMask = SAMPLE_TEXTURE2D(_BlendMask, sampler_BlendMask, uvMain).r;

                half3 blendedAlbedo = lerp(layerB_albedo, layerA_albedo, blendMask);
                half3 blendedNormal = lerp(layerB_normal, layerA_normal, blendMask);
                half3 finalNormal = terrainNormal + half3(blendedNormal.r, blendedNormal.g, 0);

                SurfaceData surf = STDefaultSurface();
                surf.albedo = blendedAlbedo;
                surf.normalTS = SafeNormalize(finalNormal);
                surf.metallic = 0;
                surf.smoothness = 0;

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
