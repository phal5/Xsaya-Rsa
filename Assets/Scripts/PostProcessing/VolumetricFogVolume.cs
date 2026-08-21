using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A box shaped region of volumetric fog. The raymarch only runs inside the boxes
/// placed in the scene, so the effect is local instead of covering the whole view.
/// The box is centered on the transform, sized by <see cref="size"/> times the transform scale,
/// and follows the transform rotation.
/// </summary>
[ExecuteAlways]
[DisallowMultipleComponent]
[AddComponentMenu("Rendering/Volumetric Fog Volume")]
public class VolumetricFogVolume : MonoBehaviour
{
    private static readonly List<VolumetricFogVolume> s_ActiveVolumes = new List<VolumetricFogVolume>();

    public static IReadOnlyList<VolumetricFogVolume> ActiveVolumes => s_ActiveVolumes;

    [Header("Shape")]
    [Tooltip("Box size in local units, multiplied by the transform scale.")]
    public Vector3 size = new Vector3(10f, 10f, 10f);

    [Tooltip("How far the fog fades in from the box faces, as a fraction of the half size. 0 gives hard edges.")]
    [Range(0f, 1f)]
    public float edgeFade = 0.2f;

    [Header("Scattering")]
    [Tooltip("Fog density multiplier.")]
    [Min(0f)]
    public float density = 0.02f;

    [Tooltip("Fog color.")]
    [ColorUsage(false, true)]
    public Color color = Color.white;

    [Tooltip("Anisotropy. Positive values scatter forward, towards the light.")]
    [Range(-1f, 1f)]
    public float anisotropy = 0.7f;

    [Header("Quality")]
    [Tooltip("Raymarch steps taken inside this volume.")]
    [Range(2, 128)]
    public int stepCount = 24;

    [Tooltip("Sample the main light shadow map at every step. The god ray look, but the expensive part.")]
    public bool mainLightShadows = true;

    [Tooltip("Accumulate point and spot lights at every step.")]
    public bool additionalLights = true;

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
            Vector3 center = transform.position;
            Vector3 extents = new Vector3(
                Mathf.Abs(m.m00) + Mathf.Abs(m.m01) + Mathf.Abs(m.m02),
                Mathf.Abs(m.m10) + Mathf.Abs(m.m11) + Mathf.Abs(m.m12),
                Mathf.Abs(m.m20) + Mathf.Abs(m.m21) + Mathf.Abs(m.m22));
            return new Bounds(center, extents * 2f);
        }
    }

    public bool IsValid()
    {
        Vector3 halfExtents = Vector3.Scale(transform.lossyScale, size);
        return density > 0f
            && halfExtents.x > 1e-4f && halfExtents.y > 1e-4f && halfExtents.z > 1e-4f;
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

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        DrawGizmo(new Color(color.r, color.g, color.b, 0.25f));
    }

    private void OnDrawGizmosSelected()
    {
        DrawGizmo(new Color(color.r, color.g, color.b, 0.8f));
    }

    private void DrawGizmo(Color wireColor)
    {
        Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.Scale(transform.lossyScale, size));
        Gizmos.color = wireColor;
        Gizmos.DrawWireCube(Vector3.zero, Vector3.one);

        if (edgeFade > 0f)
        {
            Gizmos.color = new Color(wireColor.r, wireColor.g, wireColor.b, wireColor.a * 0.35f);
            Gizmos.DrawWireCube(Vector3.zero, Vector3.one * (1f - edgeFade));
        }
    }
#endif
}
