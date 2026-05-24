#ifndef CUSTOM_LIGHTING_INCLUDED
#define CUSTOM_LIGHTING_INCLUDED

#ifndef SHADERGRAPH_PREVIEW
#ifndef MAX_VISIBLE_LIGHTS
#define MAX_VISIBLE_LIGHTS 32
#endif

#ifndef UNIVERSAL_LIGHTING_INCLUDED
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
#endif
#endif

// ------------------------------------------------------------------------
// NODE 1: MAIN LIGHT
// ------------------------------------------------------------------------
void GetMainLight_float(float3 PositionWS, float3 NormalWS, out float3 LightColor, out float NdotL)
{
    LightColor = float3(0, 0, 0);
    NdotL = 0.0;

#ifdef SHADERGRAPH_PREVIEW
    LightColor = float3(1, 1, 1);
    NdotL = 0.5;
#else
    float4 clipPos = TransformWorldToHClip(PositionWS);
#if defined(_MAIN_LIGHT_SHADOWS_SCREEN) && !defined(_SURFACE_TYPE_TRANSPARENT)
    float4 shadowCoord = ComputeScreenPos(clipPos);
#else
    float4 shadowCoord = TransformWorldToShadowCoord(PositionWS);
#endif

    Light mainLight = GetMainLight(shadowCoord, PositionWS, half4(1, 1, 1, 1));
    float attenuation = mainLight.distanceAttenuation * mainLight.shadowAttenuation;
    
    // Attenuation is now baked directly into the LightColor
    NdotL = saturate(dot(NormalWS, mainLight.direction));
    LightColor = mainLight.color * attenuation * NdotL;
#endif
}

// ------------------------------------------------------------------------
// NODE 2: ADDITIONAL DIRECTIONAL LIGHTS
// ------------------------------------------------------------------------
void GetAdditionalDirectionalLights_float(float3 PositionWS, float3 NormalWS, out float3 TotalLightColor, out float AccumulatedNdotL)
{
    TotalLightColor = float3(0, 0, 0);
    AccumulatedNdotL = 0.0;

#ifndef SHADERGRAPH_PREVIEW
#if defined(_FORWARD_PLUS)
    UNITY_LOOP
    for (uint dirLightIndex = 0; dirLightIndex < min(URP_FP_DIRECTIONAL_LIGHTS_COUNT, MAX_VISIBLE_LIGHTS); dirLightIndex++)
    {
        Light dirLight = GetAdditionalLight(dirLightIndex, PositionWS, half4(1, 1, 1, 1));
        float dirNdotL = saturate(dot(NormalWS, dirLight.direction));
        float dirAtten = dirLight.distanceAttenuation * dirLight.shadowAttenuation;
        
        AccumulatedNdotL += (dirNdotL * dirAtten);
        TotalLightColor += dirLight.color * (dirNdotL * dirAtten);
    }
#endif
#endif
}

// ------------------------------------------------------------------------
// NODE 3: ADDITIONAL POINT & SPOT LIGHTS
// ------------------------------------------------------------------------
void GetAdditionalPointSpotLights_float(float3 PositionWS, float3 NormalWS, out float3 TotalLightColor, out float AccumulatedNdotL)
{
    TotalLightColor = float3(0, 0, 0);
    AccumulatedNdotL = 0.0;

#ifndef SHADERGRAPH_PREVIEW
    InputData inputData = (InputData) 0;
    inputData.positionWS = PositionWS;
    inputData.normalWS = NormalWS;
    float4 clipPos = TransformWorldToHClip(PositionWS);
    inputData.normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(clipPos);

    uint pixelLightCount = GetAdditionalLightsCount();
    
    LIGHT_LOOP_BEGIN(pixelLightCount)

    Light addLight = GetAdditionalLight(lightIndex, PositionWS, half4(1, 1, 1, 1));
    float addNdotL = saturate(dot(NormalWS, addLight.direction));
    float addAtten = addLight.distanceAttenuation * addLight.shadowAttenuation;
        
    AccumulatedNdotL += (addNdotL * addAtten);
    TotalLightColor += addLight.color * (addNdotL * addAtten);
    LIGHT_LOOP_END
#endif
}



// Half-Precision Variants

void GetMainLight_half(half3 PositionWS, half3 NormalWS, out half3 LightColor, out half NdotL)
{
    LightColor = half3(0, 0, 0);
    NdotL = 0.0;

#ifdef SHADERGRAPH_PREVIEW
    LightColor = half3(1, 1, 1);
    NdotL = 0.5;
#else
    float4 clipPos = TransformWorldToHClip(PositionWS);
#if defined(_MAIN_LIGHT_SHADOWS_SCREEN) && !defined(_SURFACE_TYPE_TRANSPARENT)
    float4 shadowCoord = ComputeScreenPos(clipPos);
#else
    float4 shadowCoord = TransformWorldToShadowCoord(PositionWS);
#endif

    Light mainLight = GetMainLight(shadowCoord, PositionWS, float4(1, 1, 1, 1));
    float attenuation = mainLight.distanceAttenuation * mainLight.shadowAttenuation;
    
    // Attenuation is now baked directly into the LightColor
    NdotL = saturate(dot(NormalWS, mainLight.direction));
    LightColor = mainLight.color * attenuation * NdotL;
#endif
}

void GetAdditionalDirectionalLights_half(half3 PositionWS, half3 NormalWS, out half3 TotalLightColor, out half AccumulatedNdotL)
{
    TotalLightColor = half3(0, 0, 0);
    AccumulatedNdotL = 0.0;

#ifndef SHADERGRAPH_PREVIEW
#if defined(_FORWARD_PLUS)
    UNITY_LOOP
    for (uint dirLightIndex = 0; dirLightIndex < min(URP_FP_DIRECTIONAL_LIGHTS_COUNT, MAX_VISIBLE_LIGHTS); dirLightIndex++)
    {
        Light dirLight = GetAdditionalLight(dirLightIndex, PositionWS, half4(1, 1, 1, 1));
        float dirNdotL = saturate(dot(NormalWS, dirLight.direction));
        float dirAtten = dirLight.distanceAttenuation * dirLight.shadowAttenuation;
        
        AccumulatedNdotL += (dirNdotL * dirAtten);
        TotalLightColor += dirLight.color * (dirNdotL * dirAtten);
    }
#endif
#endif
}

void GetAdditionalPointSpotLights_half(half3 PositionWS, half3 NormalWS, out half3 TotalLightColor, out half AccumulatedNdotL)
{
    TotalLightColor = half3(0, 0, 0);
    AccumulatedNdotL = 0.0;

#ifndef SHADERGRAPH_PREVIEW
    InputData inputData = (InputData) 0;
    inputData.positionWS = PositionWS;
    inputData.normalWS = NormalWS;
    float4 clipPos = TransformWorldToHClip(PositionWS);
    inputData.normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(clipPos);

    uint pixelLightCount = GetAdditionalLightsCount();
    
    LIGHT_LOOP_BEGIN(pixelLightCount)

    Light addLight = GetAdditionalLight(lightIndex, PositionWS, half4(1, 1, 1, 1));
    float addNdotL = saturate(dot(NormalWS, addLight.direction));
    float addAtten = addLight.distanceAttenuation * addLight.shadowAttenuation;
        
    AccumulatedNdotL += (addNdotL * addAtten);
    TotalLightColor += addLight.color * (addNdotL * addAtten);
    LIGHT_LOOP_END
#endif
}
#endif