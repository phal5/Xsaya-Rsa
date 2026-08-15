using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[Serializable, VolumeComponentMenu("Custom/Volumetric Lighting")]
public class VolumetricLightingVolumeComponent : VolumeComponent, IPostProcessComponent
{
    [Tooltip("Enable Volumetric Lighting effect for this volume.")]
    public BoolParameter enableVolumetricLighting = new BoolParameter(false);

    [Tooltip("Raymarch step count.")]
    public ClampedFloatParameter stepCount = new ClampedFloatParameter(32f, 1f, 128f);

    [Tooltip("Fog density multiplier.")]
    public FloatParameter density = new FloatParameter(0.1f);

    [Tooltip("Fog color.")]
    public ColorParameter fogColor = new ColorParameter(Color.white, true, true, true);

    [Tooltip("Anisotropy (Forward Scattering).")]
    public ClampedFloatParameter anisotropy = new ClampedFloatParameter(0.5f, -1.0f, 1.0f);

    [Tooltip("Jitter intensity.")]
    public ClampedFloatParameter jitterIntensity = new ClampedFloatParameter(1.0f, 0.0f, 1.0f);

    public bool IsActive() => enableVolumetricLighting.value && active && density.value > 0f;

    public bool IsTileCompatible() => false;
}
