using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A box shaped region that darkens whatever is rendered inside it. Every surface whose
/// depth falls inside the box has its shaded colour multiplied by <see cref="lightMultiplier"/>,
/// so the region reads as being in shade without needing a real occluder.
///
/// This runs in screen space after shading, so it scales the final colour rather than the
/// lighting term alone. Albedo and emission are dimmed along with the light.
/// </summary>
[ExecuteAlways]
[DisallowMultipleComponent]
[AddComponentMenu("Rendering/Shadow Volume")]
public class ShadowVolume : MonoBehaviour
{
    private static readonly List<ShadowVolume> s_ActiveVolumes = new List<ShadowVolume>();

    public static IReadOnlyList<ShadowVolume> ActiveVolumes => s_ActiveVolumes;

    [Header("Shape")]
    [Tooltip("Box size in local units, multiplied by the transform scale.")]
    public Vector3 size = new Vector3(10f, 10f, 10f);

    [Tooltip("How far the shading fades in from the box faces, as a fraction of the half size. 0 gives hard edges.")]
    [Range(0f, 1f)]
    public float edgeFade = 0.15f;

    [Header("Shading")]
    [Tooltip("Everything inside the box is multiplied by this. 1 leaves the lighting untouched, 0 makes it black.")]
    [Range(0f, 1f)]
    public float lightMultiplier = 0.35f;

    [Tooltip("Optional colour of the shade. White keeps the original hue and only darkens.")]
    [ColorUsage(false, false)]
    public Color tint = Color.white;

    [Header("Culling")]
    [Tooltip("Distance from the camera at which the volume fades out completely. 0 disables the fade.")]
    [Min(0f)]
    public float maxDistance = 100f;

    private void OnEnable()
    {
        s_ActiveVolumes.Add(this);
    }

    private void OnDisable()
    {
        s_ActiveVolumes.Remove(this);
    }

    /// <summary>Maps the unit cube [-1, 1] onto this volume.</summary>
    public Matrix4x4 LocalToWorld
    {
        get
        {
            Vector3 halfExtents = Vector3.Scale(transform.lossyScale, size) * 0.5f;
            return Matrix4x4.TRS(transform.position, transform.rotation, halfExtents);
        }
    }

    /// <summary>World space AABB around the (possibly rotated) box, used for frustum culling.</summary>
    public Bounds WorldBounds
    {
        get
        {
            Matrix4x4 m = LocalToWorld;
            Vector3 extents = new Vector3(
                Mathf.Abs(m.m00) + Mathf.Abs(m.m01) + Mathf.Abs(m.m02),
                Mathf.Abs(m.m10) + Mathf.Abs(m.m11) + Mathf.Abs(m.m12),
                Mathf.Abs(m.m20) + Mathf.Abs(m.m21) + Mathf.Abs(m.m22));
            return new Bounds(transform.position, extents * 2f);
        }
    }

    public bool IsValid()
    {
        Vector3 fullSize = Vector3.Scale(transform.lossyScale, size);
        return lightMultiplier < 1f
            && fullSize.x > 1e-4f && fullSize.y > 1e-4f && fullSize.z > 1e-4f;
    }

    /// <summary>Fades the volume out over the last quarter of <see cref="maxDistance"/>.</summary>
    public float DistanceFade(Vector3 cameraPositionWS)
    {
        if (maxDistance <= 0f)
            return 1f;

        float distance = Mathf.Sqrt(WorldBounds.SqrDistance(cameraPositionWS));
        float fadeStart = maxDistance * 0.75f;
        return 1f - Mathf.Clamp01((distance - fadeStart) / Mathf.Max(maxDistance - fadeStart, 1e-4f));
    }

    /// <summary>The colour surfaces inside the box are multiplied by, faded by camera distance.</summary>
    public Color ResolveMultiplier(Vector3 cameraPositionWS)
    {
        float fade = DistanceFade(cameraPositionWS);
        Color target = tint * lightMultiplier;
        return Color.Lerp(Color.white, target, fade);
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        DrawGizmo(0.25f);
    }

    private void OnDrawGizmosSelected()
    {
        DrawGizmo(0.8f);
    }

    private void DrawGizmo(float alpha)
    {
        Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.Scale(transform.lossyScale, size));

        Gizmos.color = new Color(0.1f, 0.1f, 0.25f, alpha);
        Gizmos.DrawWireCube(Vector3.zero, Vector3.one);

        if (edgeFade > 0f)
        {
            Gizmos.color = new Color(0.1f, 0.1f, 0.25f, alpha * 0.35f);
            Gizmos.DrawWireCube(Vector3.zero, Vector3.one * (1f - edgeFade));
        }
    }
#endif
}
