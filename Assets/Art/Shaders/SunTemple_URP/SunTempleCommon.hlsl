#ifndef SUNTEMPLE_URP_COMMON_INCLUDED
#define SUNTEMPLE_URP_COMMON_INCLUDED

// Shared forward-lit plumbing for the URP ports of the Sun_Temple shaders.
// Modelled on URP's own SimpleLitForwardPass so the macro usage matches this
// package version exactly. Every port here samples a normal map, so the tangent
// frame is always carried rather than sitting behind _NORMALMAP.

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
#if defined(LOD_FADE_CROSSFADE)
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/LODCrossFade.hlsl"
#endif

struct STAttributes
{
    float4 positionOS           : POSITION;
    float3 normalOS             : NORMAL;
    float4 tangentOS            : TANGENT;
    float2 texcoord             : TEXCOORD0;
    float2 staticLightmapUV     : TEXCOORD1;
    float2 dynamicLightmapUV    : TEXCOORD2;
    half4  color                : COLOR;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct STVaryings
{
    float2 uv                   : TEXCOORD0;
    float3 positionWS           : TEXCOORD1;
    half4  normalWS             : TEXCOORD2;    // w: viewDir.x
    half4  tangentWS            : TEXCOORD3;    // w: viewDir.y
    half4  bitangentWS          : TEXCOORD4;    // w: viewDir.z
    half4  color                : TEXCOORD5;
    half   fogFactor            : TEXCOORD6;

    #if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
        float4 shadowCoord      : TEXCOORD7;
    #endif

    DECLARE_LIGHTMAP_OR_SH(staticLightmapUV, vertexSH, 8);

    #ifdef DYNAMICLIGHTMAP_ON
        float2 dynamicLightmapUV : TEXCOORD9;
    #endif

    #ifdef USE_APV_PROBE_OCCLUSION
        float4 probeOcclusion   : TEXCOORD10;
    #endif

    float4 positionCS           : SV_POSITION;
    UNITY_VERTEX_INPUT_INSTANCE_ID
    UNITY_VERTEX_OUTPUT_STEREO
};

// offsetOS lets the wind shaders displace the vertex without duplicating this block
STVaryings STVertex(STAttributes input, float3 offsetOS, float3 normalOS)
{
    STVaryings output = (STVaryings)0;

    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_TRANSFER_INSTANCE_ID(input, output);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

    float3 positionOS = input.positionOS.xyz + offsetOS;

    VertexPositionInputs vertexInput = GetVertexPositionInputs(positionOS);
    VertexNormalInputs normalInput = GetVertexNormalInputs(normalOS, input.tangentOS);

#if defined(_FOG_FRAGMENT)
    half fogFactor = 0;
#else
    half fogFactor = ComputeFogFactor(vertexInput.positionCS.z);
#endif

    output.uv = input.texcoord;
    output.positionWS = vertexInput.positionWS;
    output.positionCS = vertexInput.positionCS;
    output.color = input.color;

    half3 viewDirWS = GetWorldSpaceViewDir(vertexInput.positionWS);
    output.normalWS = half4(normalInput.normalWS, viewDirWS.x);
    output.tangentWS = half4(normalInput.tangentWS, viewDirWS.y);
    output.bitangentWS = half4(normalInput.bitangentWS, viewDirWS.z);

    OUTPUT_LIGHTMAP_UV(input.staticLightmapUV, unity_LightmapST, output.staticLightmapUV);
#ifdef DYNAMICLIGHTMAP_ON
    output.dynamicLightmapUV = input.dynamicLightmapUV.xy * unity_DynamicLightmapST.xy + unity_DynamicLightmapST.zw;
#endif
    OUTPUT_SH4(vertexInput.positionWS, output.normalWS.xyz,
               GetWorldSpaceNormalizeViewDir(vertexInput.positionWS),
               output.vertexSH, output.probeOcclusion);

    output.fogFactor = fogFactor;

#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
    output.shadowCoord = GetShadowCoord(vertexInput);
#endif

    return output;
}

void STInitializeInputData(STVaryings input, half3 normalTS, out InputData inputData)
{
    inputData = (InputData)0;

    inputData.positionWS = input.positionWS;
#if defined(DEBUG_DISPLAY)
    inputData.positionCS = input.positionCS;
#endif

    half3 viewDirWS = half3(input.normalWS.w, input.tangentWS.w, input.bitangentWS.w);
    inputData.tangentToWorld = half3x3(input.tangentWS.xyz, input.bitangentWS.xyz, input.normalWS.xyz);
    inputData.normalWS = TransformTangentToWorld(normalTS, inputData.tangentToWorld);

    inputData.normalWS = NormalizeNormalPerPixel(inputData.normalWS);
    viewDirWS = SafeNormalize(viewDirWS);
    inputData.viewDirectionWS = viewDirWS;

#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
    inputData.shadowCoord = input.shadowCoord;
#elif defined(MAIN_LIGHT_CALCULATE_SHADOWS)
    inputData.shadowCoord = TransformWorldToShadowCoord(inputData.positionWS);
#else
    inputData.shadowCoord = float4(0, 0, 0, 0);
#endif

    inputData.fogCoord = InitializeInputDataFog(float4(inputData.positionWS, 1.0), input.fogFactor);
    inputData.vertexLighting = half3(0, 0, 0);
    inputData.normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(input.positionCS);

#if defined(DEBUG_DISPLAY)
    #if defined(DYNAMICLIGHTMAP_ON)
        inputData.dynamicLightmapUV = input.dynamicLightmapUV.xy;
    #endif
    #if defined(LIGHTMAP_ON)
        inputData.staticLightmapUV = input.staticLightmapUV;
    #else
        inputData.vertexSH = input.vertexSH;
    #endif
    #if defined(USE_APV_PROBE_OCCLUSION)
        inputData.probeOcclusion = input.probeOcclusion;
    #endif
#endif
}

void STInitializeBakedGIData(STVaryings input, inout InputData inputData)
{
#if defined(_SCREEN_SPACE_IRRADIANCE)
    inputData.bakedGI = SAMPLE_GI(_ScreenSpaceIrradiance, input.positionCS.xy);
#elif defined(DYNAMICLIGHTMAP_ON)
    inputData.bakedGI = SAMPLE_GI(input.staticLightmapUV, input.dynamicLightmapUV, input.vertexSH, inputData.normalWS);
    inputData.shadowMask = SAMPLE_SHADOWMASK(input.staticLightmapUV);
#elif !defined(LIGHTMAP_ON) && (defined(PROBE_VOLUMES_L1) || defined(PROBE_VOLUMES_L2))
    inputData.bakedGI = SAMPLE_GI(input.vertexSH,
        GetAbsolutePositionWS(inputData.positionWS),
        inputData.normalWS,
        inputData.viewDirectionWS,
        input.positionCS.xy,
        input.probeOcclusion,
        inputData.shadowMask);
#else
    inputData.bakedGI = SAMPLE_GI(input.staticLightmapUV, input.vertexSH, inputData.normalWS);
    inputData.shadowMask = SAMPLE_SHADOWMASK(input.staticLightmapUV);
#endif
}

// Builds the lit result from a surface the caller has already filled in.
half4 STFragmentPBR(STVaryings input, SurfaceData surfaceData)
{
    InputData inputData;
    STInitializeInputData(input, surfaceData.normalTS, inputData);
    STInitializeBakedGIData(input, inputData);

    half4 color = UniversalFragmentPBR(inputData, surfaceData);
    color.rgb = MixFog(color.rgb, inputData.fogCoord);
    return color;
}

SurfaceData STDefaultSurface()
{
    SurfaceData s = (SurfaceData)0;
    s.albedo = half3(1, 1, 1);
    s.specular = half3(0, 0, 0);
    s.metallic = 0;
    s.smoothness = 0;
    s.normalTS = half3(0, 0, 1);
    s.emission = half3(0, 0, 0);
    s.occlusion = 1;
    s.alpha = 1;
    s.clearCoatMask = 0;
    s.clearCoatSmoothness = 0;
    return s;
}

#endif
