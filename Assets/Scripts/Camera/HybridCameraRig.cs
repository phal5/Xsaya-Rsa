using UnityEngine;
using System.Collections;
using UnityEngine.Rendering;

public class HybridCameraRig : MonoBehaviour
{
    [Header("Core References")]
    [SerializeField] private Transform character;
    [SerializeField] private Transform target;

    [Header("State")]
    [SerializeField] private bool is2DMode = true;

    [Header("2D Mode Settings")]
    [SerializeField] private Vector3 offset2D = new Vector3(3f, 2f, -10f);
    [SerializeField] private float targetGravity = 0.1f;
    [SerializeField] private float positionDamping2D = 0.2f;
    private Vector3 currentVelocity2D;
    private Quaternion targetRotation2D = Quaternion.identity;

    [Header("3D Mode Settings")]
    [SerializeField] private Vector3 offset3DWorld = new Vector3(0f, 1.5f, 0f);
    [SerializeField] private Vector3 offset3DLocal = new Vector3(0f, 0f, -5f);
    [SerializeField] private float rotationDamping3D = 10f;
    private Quaternion targetRotation3D = Quaternion.identity;

    [Header("Transition Settings")]
    [SerializeField] private float transitionSpeed = 1.2f;
    [SerializeField] private AnimationCurve transitionCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private float transitionRate = 0f; // 0.0 = 2D, 1.0 = 3D

    void Start()
    {
        targetRotation2D = transform.rotation;
        targetRotation3D = transform.rotation;
        transitionRate = is2DMode ? 0f : 1f;
    }

    private void Update()
    {
        if (!is2DMode)
        {

        }
    }

    void LateUpdate()
    {
        if (character == null) return;

        if (TargetTransitionWeight() != transitionRate)
        {
            UpdateTransitionRate();
            HandleTransition();
        }
        else if (is2DMode)
        {
            Apply2DState();
        }
        else
        {
            Apply3DState();
        }
    }

    #region --- Transition Logic ---

    private float TargetTransitionWeight() =>
        is2DMode? 0 : 1;

    private void UpdateTransitionRate()
    {
        float direction = is2DMode ? -1 : 1;
        transitionRate += direction * transitionSpeed * Time.deltaTime;
        transitionRate = Mathf.Clamp01(transitionRate);
    }

    private void HandleTransition()
    {
        // Get both ideal mathematical states simultaneously
        var state2D = GetIdeal2DState();
        var state3D = GetIdeal3DState();

        float t = transitionCurve.Evaluate(transitionRate);

        // Blend between them using the animated transition weight
        transform.position = Vector3.Lerp(state2D.pos, state3D.pos, t);
        transform.rotation = Quaternion.Slerp(state2D.rot, state3D.rot, t);

        // Keep internal tracking variables updated to prevent snapping when the transition ends
        targetRotation3D = transform.rotation;
        currentVelocity2D = Vector3.zero;
    }

    #endregion

    #region --- State Calculations (The Math) ---

    private (Vector3 pos, Quaternion rot) GetIdeal2DState()
    {
        // Direction
        Quaternion idealRot = targetRotation2D;

        // Extract axes
        Vector3 camRight = idealRot * Vector3.right;
        Vector3 camUp = idealRot * Vector3.up;
        Vector3 camForward = idealRot * Vector3.forward;
        Vector3 charForward = character.forward;
        float dynamicX = offset2D.x * Vector3.Dot(charForward, camRight);

        Vector3 idealPos;

        // Dynamic offset
        Vector3 flatOffset = (dynamicX * camRight) + (offset2D.y * camUp);
        Vector3 zOffset = offset2D.z * camForward;
        Vector3 flatOffsetPos = character.position + flatOffset;

        if(target != null)
        {
            Vector3 disparity = (character.position - target.position);
            idealPos = Vector3.Lerp(target.position, flatOffsetPos, Mathf.Clamp01(disparity.sqrMagnitude / targetGravity)) + zOffset;
        }
        else
        {
            idealPos = flatOffsetPos + zOffset;
        }

            return (idealPos, idealRot);
    }

    private (Vector3 pos, Quaternion rot) GetIdeal3DState()
    {
        Vector3 rotationCenter = character.position + offset3DWorld;
        Vector3 direction = target != null ? (target.position - rotationCenter) : Vector3.zero;

        // Calculate ideal rotation without applying damping yet, 
        // because during a transition we need the absolute mathematical target
        Quaternion idealRot = (direction != Vector3.zero) ? 
            Quaternion.LookRotation(direction) : targetRotation3D;

        Vector3 idealPos = rotationCenter + (idealRot * offset3DLocal);

        return (idealPos, idealRot);
    }

    #endregion

    #region --- State Applications (The Movement) ---

    private void Apply2DState()
    {
        var (position, rotation) = GetIdeal2DState();
        position = Vector3.SmoothDamp(transform.position, position, ref currentVelocity2D, positionDamping2D);
        transform.SetPositionAndRotation(position, rotation);
    }

    private void Apply3DState()
    {
        var state = GetIdeal3DState();

        // Apply rotation damping natively in 3D mode
        targetRotation3D = state.rot;
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation3D, Time.deltaTime * rotationDamping3D);

        // Orbit position based on the currently damped rotation
        Vector3 rotationCenter = character.position + offset3DWorld;
        transform.position = rotationCenter + (transform.rotation * offset3DLocal);
    }

    #endregion

    // --- Public API ---

    public void ToggleMode(bool to2DMode) { is2DMode = to2DMode; }

    public void SetTarget(Transform _target) { target = _target; }

    // (Include the previously written SetTarget, Set2DDirection, and Apply3DManualRotation methods here)
}