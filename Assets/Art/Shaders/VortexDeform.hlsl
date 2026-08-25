#ifndef VORTEX_DEFORM_INCLUDED
#define VORTEX_DEFORM_INCLUDED

#ifndef UNIVERSAL_LIGHTING_INCLUDED
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#endif

// ----------------------------------------------------------------------------
// Smooth ReLU function:
// Returns 0 for z <= n, smoothly accelerates for n < z < n + width,
// and continues linearly with slope 1 for z >= n + width.
// ----------------------------------------------------------------------------
float SmoothReLU(float z, float n, float width)
{
    float x = z - n;
    if (x <= 0.0)
    {
        return 0.0;
    }
    float w = max(width, 0.0001);
    if (x >= w)
    {
        return x - 0.5 * w;
    }
    return (x * x) / (2.0 * w);
}

// Rotate a 3D vector around Z axis by angle theta
float3 RotateVectorZ(float3 vWS, float theta)
{
    float cosT = cos(theta);
    float sinT = sin(theta);
    return float3(
        vWS.x * cosT - vWS.y * sinT,
        vWS.x * sinT + vWS.y * cosT,
        vWS.z
    );
}

// Rotate a 3D vector around an arbitrary unit axis by angle theta (Rodrigues formula)
float3 RotateVectorAxis(float3 vWS, float3 axisDir, float theta)
{
    float cosT = cos(theta);
    float sinT = sin(theta);
    float3 vParallel = dot(vWS, axisDir) * axisDir;
    float3 vPerp = vWS - vParallel;
    return vParallel + vPerp * cosT + cross(axisDir, vPerp) * sinT;
}

// ----------------------------------------------------------------------------
// 1. World Z Axis Vortex Deform (Calculates deformed position & rotation angle theta)
// ----------------------------------------------------------------------------
void VortexDeform_float(
    float3 PositionOS,
    float T,
    float N,
    float SmoothWidth,
    float AxisX,
    float AxisY,
    float UseSquaredLimit,
    out float3 OutPositionOS,
    out float OutTheta
)
{
#ifdef SHADERGRAPH_PREVIEW
    float3 PositionWS = PositionOS;
#else
    float3 PositionWS = TransformObjectToWorld(PositionOS);
#endif

    // 1. Calculate z_smooth using Smooth ReLU (0 for z <= N, slope 1 for z >= N + SmoothWidth)
    float zSmooth = SmoothReLU(PositionWS.z, N, SmoothWidth);
    float v = zSmooth * T;

    // 2. Rotation angle theta = v
    float theta = v;
    OutTheta = theta;
    float cosT = cos(theta);
    float sinT = sin(theta);

    float dx = PositionWS.x - AxisX;
    float dy = PositionWS.y - AxisY;

    float dxRot = dx * cosT - dy * sinT;
    float dyRot = dx * sinT + dy * cosT;

    // 3. Scale: inversely proportional to v
    float denom = (UseSquaredLimit > 0.5) ? (v * v + 1.0) : max(abs(v), 1.0);
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
    half N,
    half SmoothWidth,
    half AxisX,
    half AxisY,
    half UseSquaredLimit,
    out half3 OutPositionOS,
    out half OutTheta
)
{
    float3 posOS = (float3)PositionOS;
    float3 outPosOS;
    float outTheta;
    VortexDeform_float(posOS, (float)T, (float)N, (float)SmoothWidth, (float)AxisX, (float)AxisY, (float)UseSquaredLimit, outPosOS, outTheta);
    OutPositionOS = (half3)outPosOS;
    OutTheta = (half)outTheta;
}

// 7 args overload (without theta output)
void VortexDeform_float(
    float3 PositionOS,
    float T,
    float N,
    float SmoothWidth,
    float AxisX,
    float AxisY,
    float UseSquaredLimit,
    out float3 OutPositionOS
)
{
    float dummyTheta;
    VortexDeform_float(PositionOS, T, N, SmoothWidth, AxisX, AxisY, UseSquaredLimit, OutPositionOS, dummyTheta);
}

void VortexDeform_half(
    half3 PositionOS,
    half T,
    half N,
    half SmoothWidth,
    half AxisX,
    half AxisY,
    half UseSquaredLimit,
    out half3 OutPositionOS
)
{
    half dummyTheta;
    VortexDeform_half(PositionOS, T, N, SmoothWidth, AxisX, AxisY, UseSquaredLimit, OutPositionOS, dummyTheta);
}

// 6 args overload (Backward compatibility with ShaderGraph)
void VortexDeform_float(
    float3 PositionOS,
    float T,
    float N,
    float AxisX,
    float AxisY,
    float UseSquaredLimit,
    out float3 OutPositionOS
)
{
    float dummyTheta;
    VortexDeform_float(PositionOS, T, N, 2.0, AxisX, AxisY, UseSquaredLimit, OutPositionOS, dummyTheta);
}

void VortexDeform_half(
    half3 PositionOS,
    half T,
    half N,
    half AxisX,
    half AxisY,
    half UseSquaredLimit,
    out half3 OutPositionOS
)
{
    half dummyTheta;
    VortexDeform_half(PositionOS, T, N, 2.0, AxisX, AxisY, UseSquaredLimit, OutPositionOS, dummyTheta);
}

// ----------------------------------------------------------------------------
// 2. View Axis (Camera Forward) Vortex Deform
// ----------------------------------------------------------------------------
void ViewVortexDeform_float(
    float3 PositionOS,
    float T,
    float N,
    float SmoothWidth,
    float AxisOffsetX,
    float AxisOffsetY,
    float UseSquaredLimit,
    out float3 OutPositionOS,
    out float OutTheta,
    out float3 OutAxisDir
)
{
#ifdef SHADERGRAPH_PREVIEW
    float3 PositionWS = PositionOS;
    float3 camPosWS = float3(0, 0, -10);
    float3 camForwardWS = float3(0, 0, 1);
    float3 camRightWS = float3(1, 0, 0);
    float3 camUpWS = float3(0, 1, 0);
#else
    float3 PositionWS = TransformObjectToWorld(PositionOS);
    float3 camPosWS = GetCameraPositionWS();
    float3 camRightWS   = normalize(UNITY_MATRIX_V[0].xyz);
    float3 camUpWS      = normalize(UNITY_MATRIX_V[1].xyz);
    float3 camForwardWS = normalize(-UNITY_MATRIX_V[2].xyz);
#endif

    // 1. Calculate z_smooth using Smooth ReLU based on World Z depth
    float zSmooth = SmoothReLU(PositionWS.z, N, SmoothWidth);
    float v = zSmooth * T;

    float theta = v;
    OutTheta = theta;
    float denom = (UseSquaredLimit > 0.5) ? (v * v + 1.0) : max(abs(v), 1.0);
    float scaleFactor = 1.0 / denom;

    // 2. Define View Axis ray
    float3 axisOrigin = camPosWS + camRightWS * AxisOffsetX + camUpWS * AxisOffsetY;
    float3 axisDir = camForwardWS;
    OutAxisDir = axisDir;

    // 3. Decompose PositionWS relative to View Axis
    float3 r = PositionWS - axisOrigin;
    float3 rParallel = dot(r, axisDir) * axisDir;
    float3 rPerp = r - rParallel;

    // 4. 3D Rodrigues' Rotation around view axisDir by angle theta
    float cosT = cos(theta);
    float sinT = sin(theta);
    float3 rPerpRot = rPerp * cosT + cross(axisDir, rPerp) * sinT;

    // 5. Scale distance perpendicular to the view axis
    float3 rPerpFinal = rPerpRot * scaleFactor;

    // 6. Reconstruct final world position
    float3 finalPositionWS = axisOrigin + rParallel + rPerpFinal;

#ifdef SHADERGRAPH_PREVIEW
    OutPositionOS = finalPositionWS;
#else
    OutPositionOS = TransformWorldToObject(finalPositionWS);
#endif
}

void ViewVortexDeform_half(
    half3 PositionOS,
    half T,
    half N,
    half SmoothWidth,
    half AxisOffsetX,
    half AxisOffsetY,
    half UseSquaredLimit,
    out half3 OutPositionOS,
    out half OutTheta,
    out half3 OutAxisDir
)
{
    float3 posOS = (float3)PositionOS;
    float3 outPosOS;
    float outTheta;
    float3 outAxisDir;
    ViewVortexDeform_float(posOS, (float)T, (float)N, (float)SmoothWidth, (float)AxisOffsetX, (float)AxisOffsetY, (float)UseSquaredLimit, outPosOS, outTheta, outAxisDir);
    OutPositionOS = (half3)outPosOS;
    OutTheta = (half)outTheta;
    OutAxisDir = (half3)outAxisDir;
}

// 7 args overload (without theta/axis output)
void ViewVortexDeform_float(
    float3 PositionOS,
    float T,
    float N,
    float SmoothWidth,
    float AxisOffsetX,
    float AxisOffsetY,
    float UseSquaredLimit,
    out float3 OutPositionOS
)
{
    float dummyTheta;
    float3 dummyAxis;
    ViewVortexDeform_float(PositionOS, T, N, SmoothWidth, AxisOffsetX, AxisOffsetY, UseSquaredLimit, OutPositionOS, dummyTheta, dummyAxis);
}

void ViewVortexDeform_half(
    half3 PositionOS,
    half T,
    half N,
    half SmoothWidth,
    half AxisOffsetX,
    half AxisOffsetY,
    half UseSquaredLimit,
    out half3 OutPositionOS
)
{
    half dummyTheta;
    half3 dummyAxis;
    ViewVortexDeform_half(PositionOS, T, N, SmoothWidth, AxisOffsetX, AxisOffsetY, UseSquaredLimit, OutPositionOS, dummyTheta, dummyAxis);
}

#endif
