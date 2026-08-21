// URP port of Sun_Temple/WindowGlass.
// Opaque despite the name. Keeps the vertex-colour emission mask, so only the windows
// the artist painted glow, and the fresnel cubemap reflection.
Shader "Sun_Temple_URP/WindowGlass"
{
    Properties
    {
        _MainTex("Albedo (RGB) Glass Mask(A)", 2D) = "white" {}
        [NoScaleOffset] _RoughnessTexture("Roughness (R)", 2D) = "white" {}
        [Normal][NoScaleOffset] _BumpMap("Normal", 2D) = "bump" {}
        [NoScaleOffset] _Emission("Emission(RGB)", 2D) = "black" {}

        _EmissionIntensity("Emission Intensity", Range(0, 8)) = 0
        _EmissionVertexMask("Emission Vertex Mask", Range(0, 1)) = 0
        _Reflection("Reflection (CUBE)", Cube) = "" {}
        _SkyColor("Sky Color (RGB)", Color) = (1, 1, 1, 1)
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" "Queue" = "Geometry" "ForceNoShadowCasting" = "True" }
        LOD 300

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"

        CBUFFER_START(UnityPerMaterial)
            float4 _MainTex_ST;
            half4  _SkyColor;
            half   _EmissionIntensity;
            half   _EmissionVertexMask;
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
            #pragma multi_compile_fog
            #pragma multi_compile_instancing

            #include "SunTempleCommon.hlsl"

            TEXTURE2D(_MainTex);           SAMPLER(sampler_MainTex);
            TEXTURE2D(_BumpMap);           SAMPLER(sampler_BumpMap);
            TEXTURE2D(_Emission);          SAMPLER(sampler_Emission);
            TEXTURE2D(_RoughnessTexture);  SAMPLER(sampler_RoughnessTexture);
            TEXTURECUBE(_Reflection);      SAMPLER(sampler_Reflection);

            STVaryings vert(STAttributes input)
            {
                return STVertex(input, float3(0, 0, 0), input.normalOS);
            }

            half4 frag(STVaryings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                float2 uv = input.uv * _MainTex_ST.xy + _MainTex_ST.zw;

                half4 color = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv);
                half3 normalTS = UnpackNormal(SAMPLE_TEXTURE2D(_BumpMap, sampler_BumpMap, uv));
                half roughness = SAMPLE_TEXTURE2D(_RoughnessTexture, sampler_RoughnessTexture, uv).r;

                // only the windows painted with vertex red light up
                half emissionMask = pow(max(input.color.r, 0), 4);
                half3 emission = SAMPLE_TEXTURE2D(_Emission, sampler_Emission, uv).rgb
                               * emissionMask * _EmissionIntensity * _SkyColor.rgb;

                half3 normalWS = SafeNormalize(input.normalWS.xyz);
                half3 viewDirWS = SafeNormalize(half3(input.normalWS.w, input.tangentWS.w, input.bitangentWS.w));

                half fresnel = 1.0 - saturate(dot(viewDirWS, normalWS));
                half3 reflectVector = reflect(-viewDirWS, normalWS);
                half3 reflection = SAMPLE_TEXTURECUBE(_Reflection, sampler_Reflection, reflectVector).rgb;
                reflection = reflection * (1 - roughness * 2) * pow(fresnel, 2);

                SurfaceData surf = STDefaultSurface();
                surf.albedo = color.rgb;
                surf.normalTS = normalTS;
                surf.emission = emission + saturate(reflection);
                surf.metallic = 0;
                surf.smoothness = roughness;

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
