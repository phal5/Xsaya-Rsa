using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Punches a soft spherical hole through the <see cref="VisionMask"/>.
///
/// With <see cref="followPlayer"/> the hole tracks the protagonist through
/// <see cref="PlayerManager.instance"/>, so this can be placed in a background scene without
/// referencing anything across the scene boundary. Otherwise it sits on its own transform,
/// which is what a lantern or a torch wants.
/// </summary>
[ExecuteAlways]
[DisallowMultipleComponent]
[AddComponentMenu("Rendering/Vision Reveal Source")]
public class VisionRevealSource : MonoBehaviour
{
    private static readonly List<VisionRevealSource> s_ActiveSources = new List<VisionRevealSource>();

    public static IReadOnlyList<VisionRevealSource> ActiveSources => s_ActiveSources;

    [Header("Anchor")]
    [Tooltip("Track the protagonist through PlayerManager instead of this transform.")]
    public bool followPlayer = true;

    [Tooltip("World space offset from the anchor. Raise it to sit at chest height rather than at the feet.")]
    public Vector3 offset = Vector3.zero;

    [Header("Shape")]
    [Tooltip("World radius at which the reveal has faded out completely.")]
    [Min(0f)]
    public float radius = 6f;

    [Tooltip("Width of the soft edge, measured inwards from the radius.")]
    [Min(0f)]
    public float falloff = 3f;

    [Tooltip("How far the hole opens. 1 restores the original lighting, 0.7 only lifts the mask partway.")]
    [Range(0f, 1f)]
    public float strength = 1f;

    private void OnEnable()
    {
        s_ActiveSources.Add(this);
    }

    private void OnDisable()
    {
        s_ActiveSources.Remove(this);
    }

    /// <summary>
    /// Resolved every time the mask renders, so the hole never lags a frame behind the
    /// protagonist and nothing has to be moved in Update.
    ///
    /// Falls back to this transform when no player is reachable — PlayerManager fills its
    /// instance in Awake, so in edit mode there is nothing to follow and the object's own
    /// position is what the artist is positioning anyway.
    /// </summary>
    public Vector3 PositionWS
    {
        get
        {
            Transform anchor = ResolveAnchor();
            return (anchor != null ? anchor.position : transform.position) + offset;
        }
    }

    private Transform ResolveAnchor()
    {
        if (!followPlayer)
            return transform;

        PlayerManager manager = PlayerManager.instance;
        if (manager == null || manager.player == null)
            return transform;

        return manager.player;
    }

    /// <summary>Inside this radius the reveal is at full strength.</summary>
    public float InnerRadius => Mathf.Max(radius - falloff, 0f);

    public bool IsValid() => radius > 1e-4f && strength > 0f;

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Vector3 centre = PositionWS;

        Gizmos.color = new Color(1f, 0.9f, 0.5f, 0.9f);
        Gizmos.DrawWireSphere(centre, radius);

        if (falloff > 0f)
        {
            Gizmos.color = new Color(1f, 0.9f, 0.5f, 0.35f);
            Gizmos.DrawWireSphere(centre, InnerRadius);
        }

        // Show what it is tracking when that is not this object.
        if (followPlayer && centre != transform.position + offset)
        {
            Gizmos.color = new Color(1f, 0.9f, 0.5f, 0.5f);
            Gizmos.DrawLine(transform.position, centre);
        }
    }
#endif
}
