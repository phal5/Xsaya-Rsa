#ifndef SUNTEMPLE_URP_DEPTHPASSES_INCLUDED
#define SUNTEMPLE_URP_DEPTHPASSES_INCLUDED

// ShadowCaster / DepthOnly / DepthNormals bodies shared by the Sun_Temple URP ports.
// Borrowing URP Lit's passes with UsePass is not an option: that pass declares its own
// UnityPerMaterial layout, which clashes with ours, and its cutout path keys off
// _BaseMap/_Cutoff rather than the _MainTex these shaders actually use.
//
// A shader that displaces vertices or clips defines these before including:
//   #define ST_VERTEX_OFFSET(positionOS, color)  <float3 offset>
//   #define ST_DEPTH_CLIP(uv)                    <clip(...) statement>

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"

#ifndef ST_VERTEX_OFFSET
    #define ST_VERTEX_OFFSET(positionOS, color) float3(0, 0, 0)
#endif

#ifndef ST_DEPTH_CLIP
    #define ST_DEPTH_CLIP(uv)
#endif

struct STDepthAttributes
{
    float4 positionOS : POSITION;
    float3 normalOS   : NORMAL;
    float2 texcoord   : TEXCOORD0;
    half4  color      : COLOR;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct STDepthVaryings
{
    float2 uv         : TEXCOORD0;
    float3 normalWS   : TEXCOORD1;
    float4 positionCS : SV_POSITION;
    UNITY_VERTEX_INPUT_INSTANCE_ID
    UNITY_VERTEX_OUTPUT_STEREO
};

// ---------------------------------------------------------------- ShadowCaster

#if defined(ST_SHADOW_PASS)
float3 _LightDirection;
float3 _LightPosition;

float4 STGetShadowPositionHClip(float3 positionOS, float3 normalOS)
{
    float3 positionWS = TransformObjectToWorld(positionOS);
    float3 normalWS = TransformObjectToWorldNormal(normalOS);

#if _CASTING_PUNCTUAL_LIGHT_SHADOW
    float3 lightDirectionWS = normalize(_LightPosition - positionWS);
#else
    float3 lightDirectionWS = _LightDirection;
#endif

    float4 positionCS = TransformWorldToHClip(ApplyShadowBias(positionWS, normalWS, lightDirectionWS));

#if UNITY_REVERSED_Z
    positionCS.z = min(positionCS.z, positionCS.w * UNITY_NEAR_CLIP_VALUE);
#else
    positionCS.z = max(positionCS.z, positionCS.w * UNITY_NEAR_CLIP_VALUE);
#endif

    return positionCS;
}

STDepthVaryings STShadowVertex(STDepthAttributes input)
{
    STDepthVaryings output = (STDepthVaryings)0;
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

    float3 positionOS = input.positionOS.xyz + ST_VERTEX_OFFSET(input.positionOS.xyz, input.color);
    output.uv = input.texcoord;
    output.positionCS = STGetShadowPositionHClip(positionOS, input.normalOS);
    return output;
}

half4 STShadowFragment(STDepthVaryings input) : SV_Target
{
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
    ST_DEPTH_CLIP(input.uv);
    return 0;
}
#endif

// ------------------------------------------------------------------ DepthOnly

#if defined(ST_DEPTH_PASS)
STDepthVaryings STDepthVertex(STDepthAttributes input)
{
    STDepthVaryings output = (STDepthVaryings)0;
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

    float3 positionOS = input.positionOS.xyz + ST_VERTEX_OFFSET(input.positionOS.xyz, input.color);
    output.uv = input.texcoord;
    output.positionCS = TransformObjectToHClip(positionOS);
    return output;
}

half4 STDepthFragment(STDepthVaryings input) : SV_Target
{
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
    ST_DEPTH_CLIP(input.uv);
    return 0;
}
#endif

// --------------------------------------------------------------- DepthNormals

#if defined(ST_DEPTHNORMALS_PASS)
STDepthVaryings STDepthNormalsVertex(STDepthAttributes input)
{
    STDepthVaryings output = (STDepthVaryings)0;
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_TRANSFER_INSTANCE_ID(input, output);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

    float3 positionOS = input.positionOS.xyz + ST_VERTEX_OFFSET(input.positionOS.xyz, input.color);
    output.uv = input.texcoord;
    output.positionCS = TransformObjectToHClip(positionOS);
    output.normalWS = TransformObjectToWorldNormal(input.normalOS);
    return output;
}

half4 STDepthNormalsFragment(STDepthVaryings input) : SV_Target
{
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
    ST_DEPTH_CLIP(input.uv);
    return half4(NormalizeNormalPerPixel(input.normalWS), 0.0);
}
#endif

#endif
