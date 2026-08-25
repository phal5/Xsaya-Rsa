#ifndef CEL_TESSELLATION_INCLUDED
#define CEL_TESSELLATION_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

struct TessellationFactors
{
    float edge[3] : SV_TessFactor;
    float inside  : SV_InsideTessFactor;
};

// Distance-based inverse interpolation for edge factor
float CalcDistanceTessFactor(float3 worldPos0, float3 worldPos1, float minDist, float maxDist, float maxFactor)
{
    float3 edgeMid = (worldPos0 + worldPos1) * 0.5;
    float dist = distance(edgeMid, GetCameraPositionWS());
    
    // Inverse distance lerp: closer -> maxFactor, further -> 1.0
    float f = saturate((maxDist - dist) / max(maxDist - minDist, 0.0001));
    return lerp(1.0, maxFactor, f);
}

TessellationFactors CalcDistanceTessFactors(
    float3 worldPos0, float3 worldPos1, float3 worldPos2,
    float minDist, float maxDist, float maxFactor)
{
    TessellationFactors factors;
    factors.edge[0] = CalcDistanceTessFactor(worldPos1, worldPos2, minDist, maxDist, maxFactor);
    factors.edge[1] = CalcDistanceTessFactor(worldPos2, worldPos0, minDist, maxDist, maxFactor);
    factors.edge[2] = CalcDistanceTessFactor(worldPos0, worldPos1, minDist, maxDist, maxFactor);
    factors.inside  = (factors.edge[0] + factors.edge[1] + factors.edge[2]) / 3.0;
    return factors;
}

#endif
