#ifndef VORTEX_DEFORM_INCLUDED
#define VORTEX_DEFORM_INCLUDED

#ifndef UNIVERSAL_LIGHTING_INCLUDED
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#endif

void VortexDeform_float(
    float3 PositionOS,
    float T,
    float AxisX,
    float AxisY,
    float UseSquaredLimit,
    out float3 OutPositionOS
)
{
#ifdef SHADERGRAPH_PREVIEW
    float3 PositionWS = PositionOS;
#else
    float3 PositionWS = TransformObjectToWorld(PositionOS);
#endif

    // 1. Calculate v = World Z + t
    float v = PositionWS.z + T;

    // 2. Base term = v * t
    float vt = v * T;

    // 3. Rotation angle: exactly v * t
    // When t = 0 or vt = 0, theta is strictly 0 (no initial tilt or rotation)
    float theta = vt;
    float cosT = cos(theta);
    float sinT = sin(theta);

    float dx = PositionWS.x - AxisX;
    float dy = PositionWS.y - AxisY;

    float dxRot = dx * cosT - dy * sinT;
    float dyRot = dx * sinT + dy * cosT;

    // 4. Multiply distance from axis by 1 / (v * t) with lower bound 1 on denominator
    // If UseSquaredLimit: denom = (v*t)^2 + 1.0 (smooth everywhere, >= 1, scale = 1 when vt = 0)
    // Else: denom = max(abs(vt), 1.0) (lower bound 1, scale = 1 when vt = 0)
    float denom = (UseSquaredLimit > 0.5) ? (vt * vt + 1.0) : max(abs(vt), 1.0);
    float scaleFactor = 1.0 / denom;

    float dxFinal = dxRot * scaleFactor;
    float dyFinal = dyRot * scaleFactor;

    float3 finalPositionWS = float3(AxisX + dxFinal, AxisY + dyFinal, PositionWS.z);

#ifdef SHADERGRAPH_PREVIEW
    OutPositionOS = finalPositionWS;
#else
    OutPositionOS = TransformWorldToObject(finalPositionWS);
#endif
}

void VortexDeform_half(
    half3 PositionOS,
    half T,
    half AxisX,
    half AxisY,
    half UseSquaredLimit,
    out half3 OutPositionOS
)
{
    float3 posOS = (float3)PositionOS;
    float3 outPosOS;
    VortexDeform_float(posOS, (float)T, (float)AxisX, (float)AxisY, (float)UseSquaredLimit, outPosOS);
    OutPositionOS = (half3)outPosOS;
}

#endif
