// URP port of Sun_Temple/Foliage.
// Alpha-cut cards with the original per-vertex wind, driven by vertex colour red.
// The wind runs in the shadow and depth passes too, so shadows track the motion.
Shader "Sun_Temple_URP/Foliage"
{
    Properties
    {
        _Color("Main Color", Color) = (1,1,1,1)
        _MainTex("Layer_A Albedo (RGB)", 2D) = "black" {}
        _Cutoff("Alpha cutoff", Range(0,1)) = 0.5
        _SelfIllum("Self Illumination", Range(0, 1)) = 0
        _NormalModification("Normal Modification", Range(0, 1)) = 1

        _WaveFreq("Wave Frequency", Float) = 20
        _WaveHeight("Wave Height", Float) = 0.1
        _WaveScale("Wave Scale", Float) = 1
    }

    SubShader
    {
        Tags { "RenderType" = "TransparentCutout" "RenderPipeline" = "UniversalPipeline" "Queue" = "AlphaTest" "IgnoreProjector" = "True" }
        LOD 200

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"

        CBUFFER_START(UnityPerMaterial)
            float4 _MainTex_ST;
            half4  _Color;
            half   _Cutoff;
            half   _SelfIllum;
            half   _NormalModification;
            half   _WaveFreq;
            half   _WaveHeight;
            half   _WaveScale;
        CBUFFER_END

        TEXTURE2D(_MainTex);  SAMPLER(sampler_MainTex);
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
            #include "SunTempleWind.hlsl"

            STVaryings vert(STAttributes input)
            {
                float3 windOS = ST_WindSimplified(input.positionOS.xyz, input.color, _WaveFreq, _WaveHeight, _WaveScale);
                float3 leaningNormal = ST_FoliageNormal(input.normalOS, _NormalModification);
                return STVertex(input, windOS, leaningNormal);
            }

            half4 frag(STVaryings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                float2 uv = input.uv * _MainTex_ST.xy + _MainTex_ST.zw;
                half4 albedo = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv);

                clip(albedo.a - _Cutoff);

                SurfaceData surf = STDefaultSurface();
                surf.albedo = albedo.rgb * _Color.rgb;
                surf.emission = albedo.rgb * _SelfIllum;
                surf.alpha = albedo.a;
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
            Cull Back

            HLSLPROGRAM
            #pragma target 3.0
            #pragma vertex STShadowVertex
            #pragma fragment STShadowFragment
            #pragma multi_compile_vertex _ _CASTING_PUNCTUAL_LIGHT_SHADOW
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "SunTempleWind.hlsl"
            #define ST_VERTEX_OFFSET(positionOS, color) ST_WindSimplified(positionOS, color, _WaveFreq, _WaveHeight, _WaveScale)
            #define ST_DEPTH_CLIP(uv) clip(SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, (uv) * _MainTex_ST.xy + _MainTex_ST.zw).a - _Cutoff);
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
            Cull Back

            HLSLPROGRAM
            #pragma target 3.0
            #pragma vertex STDepthVertex
            #pragma fragment STDepthFragment
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "SunTempleWind.hlsl"
            #define ST_VERTEX_OFFSET(positionOS, color) ST_WindSimplified(positionOS, color, _WaveFreq, _WaveHeight, _WaveScale)
            #define ST_DEPTH_CLIP(uv) clip(SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, (uv) * _MainTex_ST.xy + _MainTex_ST.zw).a - _Cutoff);
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

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "SunTempleWind.hlsl"
            #define ST_VERTEX_OFFSET(positionOS, color) ST_WindSimplified(positionOS, color, _WaveFreq, _WaveHeight, _WaveScale)
            #define ST_DEPTH_CLIP(uv) clip(SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, (uv) * _MainTex_ST.xy + _MainTex_ST.zw).a - _Cutoff);
            #define ST_DEPTHNORMALS_PASS
            #include "SunTempleDepthPasses.hlsl"
            ENDHLSL
        }
    }

    FallBack Off
}
