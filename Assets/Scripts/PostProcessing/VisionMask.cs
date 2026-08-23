using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Darkens the whole view, and only <see cref="VisionRevealSource"/> spheres open it back up.
/// Use it to limit what the player can see; place the reveal source on the protagonist.
///
/// Unlike <see cref="ShadowVolume"/> this has no shape of its own. It covers everything the
/// camera renders, so exactly one mask should be active at a time. If several end up loaded,
/// the one with the highest <see cref="priority"/> wins.
///
/// This runs in screen space after shading, so it scales the final colour rather than the
/// lighting term alone.
/// </summary>
[ExecuteAlways]
[DisallowMultipleComponent]
[AddComponentMenu("Rendering/Vision Mask")]
public class VisionMask : MonoBehaviour
{
    private static readonly List<VisionMask> s_ActiveMasks = new List<VisionMask>();

    public static IReadOnlyList<VisionMask> ActiveMasks => s_ActiveMasks;

    [Header("Mask")]
    [Tooltip("Masked pixels are multiplied by this. 0 is pitch black, 0.05 keeps a readable silhouette.")]
    [Range(0f, 1f)]
    public float lightMultiplier = 0.05f;

    [Tooltip("Colour of the masked area. White only darkens; a cool tint reads as night.")]
    [ColorUsage(false, false)]
    public Color tint = Color.white;

    [Tooltip("Overall blend, for fading the mask in and out at runtime. 0 disables it entirely.")]
    [Range(0f, 1f)]
    public float weight = 1f;

    [Tooltip("Mask the sky too. Reveal spheres never reach it, so the sky stays uniformly masked.")]
    public bool affectSky = true;

    [Tooltip("When more than one mask is loaded, the highest priority is the one that renders.")]
    public int priority = 0;

    private void OnEnable()
    {
        s_ActiveMasks.Add(this);
    }

    private void OnDisable()
    {
        s_ActiveMasks.Remove(this);
    }

    public bool IsValid() => weight > 0f && lightMultiplier < 1f;

    /// <summary>The colour masked pixels are multiplied by, with <see cref="weight"/> folded in.</summary>
    public Color ResolveMultiplier()
    {
        return Color.Lerp(Color.white, tint * lightMultiplier, weight);
    }

    /// <summary>The mask that should render this frame, or null when none is usable.</summary>
    public static VisionMask Resolve()
    {
        VisionMask best = null;

        for (int i = 0; i < s_ActiveMasks.Count; i++)
        {
            VisionMask mask = s_ActiveMasks[i];
            if (mask == null || !mask.IsValid())
                continue;

            if (best == null || mask.priority > best.priority)
                best = mask;
        }

        return best;
    }
}
