using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[Serializable, VolumeComponentMenu("Custom/Outline")]
public class OutlineVolumeComponent : VolumeComponent, IPostProcessComponent
{
    [Tooltip("Enable Outline effect for this volume.")]
    public BoolParameter enableOutline = new BoolParameter(false);

    [Tooltip("Base sampling radius / line thickness.")]
    public ClampedFloatParameter baseThickness = new ClampedFloatParameter(1.0f, 0.5f, 5.0f);

    [Tooltip("Edge detection depth threshold.")]
    public FloatParameter threshold = new FloatParameter(0.01f);

    [Tooltip("Line intensity multiplier.")]
    public FloatParameter lineIntensity = new FloatParameter(50.0f);

    [Tooltip("Outline color.")]
    public ColorParameter outlineColor = new ColorParameter(Color.black);

    [Tooltip("Use Overlay blend mode instead of solid color.")]
    public BoolParameter useOverlay = new BoolParameter(true);

    public bool IsActive() => enableOutline.value && active;

    public bool IsTileCompatible() => false;
}
