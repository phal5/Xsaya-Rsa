#ifndef SUNTEMPLE_URP_WIND_INCLUDED
#define SUNTEMPLE_URP_WIND_INCLUDED

// Port of Sun_Temple/Content/Shaders/VertexWind.cginc.
// The original read `_Time` (a float4) into a `half`, which CG silently truncated
// to `.x`; that truncation is reproduced explicitly here.

float3 ST_WindSimplified(float3 positionOS, half4 color, half waveFreq, half waveHeight, half waveScale)
{
    half phase_slow = _Time.x * waveFreq;
    half phase_med  = _Time.x * 3 * waveFreq;
    half phase_fast = _Time.x * 5 * waveFreq;

    half offset  = (positionOS.x + (positionOS.z * waveScale)) * waveScale;
    half offset2 = (positionOS.x + (positionOS.z * waveScale * 3)) * waveScale * 3;
    half offset3 = (positionOS.x + (positionOS.z * waveScale * 5)) * waveScale * 5;

    half sin1 = sin(phase_slow + offset);
    half sin2 = sin(phase_med + offset2);
    half sin3 = sin(phase_fast + offset3);

    half sin_combined = (sin1 * 4) + sin2 + (sin3 * 0.5);

    half wind_factor = sin_combined * waveHeight * 0.1;
    wind_factor = wind_factor * color.r;

    float3 wind_xyz = float3(wind_factor, wind_factor * 0.2, wind_factor);
    wind_xyz = mul((float3x3)GetWorldToObjectMatrix(), wind_xyz);

    return wind_xyz;
}

// Cloth.shader carried its own `windanim`, which differs from the include's version:
// two octaves instead of three, twice the vertical throw, and no world-to-object mul.
float3 ST_WindCloth(float3 positionOS, half4 color, half waveFreq, half waveHeight, half waveScale)
{
    half phase_slow = _Time.x * waveFreq;
    half phase_med  = _Time.x * 4 * waveFreq;

    half offset  = (positionOS.x + (positionOS.z * waveScale)) * waveScale;
    half offset2 = (positionOS.x + (positionOS.z * waveScale * 2)) * waveScale * 2;

    half sin1 = sin(phase_slow + offset);
    half sin2 = sin(phase_med + offset2);

    half sin_combined = (sin1 * 4) + sin2;

    half wind_x = sin_combined * waveHeight * 0.1;
    float3 wind_xyz = float3(wind_x, wind_x * 2, wind_x);
    wind_xyz = wind_xyz * pow(max(color.r, 0), 2);

    return wind_xyz;
}

// The original leaned the normal upwards to soften the lighting on foliage cards.
float3 ST_FoliageNormal(float3 normalOS, half normalModification)
{
    float3 modified = float3(0, 2, 0);
    return lerp(normalOS, normalOS + modified, normalModification);
}

#endif
