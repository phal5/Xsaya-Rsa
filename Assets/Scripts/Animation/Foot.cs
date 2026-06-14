using UnityEngine;
using UnityEngine.Events;

public class Foot : MonoBehaviour
{
    [SerializeField] LayerMask _ground;
    [Space(10f)]
    [Tooltip("Stride when moving at 1m/s")]
    [SerializeField] float _baseStride = 0.6f;
    [SerializeField] float _lift = 0.2f;
    [SerializeField] float _distance = 1;
    [SerializeField] float _yDamp = 0.1f;
    [SerializeField] Vector3 _raycastOffset;
    [Space(10f)]
    [SerializeField] Transform _root;
    [SerializeField] Transform _target;
    [Space(10f)]
    [SerializeField] UnityEvent onFootChange;
    [field:SerializeField] public bool move { get; private set; }

    float _scaleCoefficient;

    Vector3 p0;
    Vector3 rootVelocity;
    Vector3 rootDirection;
    Vector3 raycastPosition;
    protected float rootSpeed;
    protected float t;
    float _baseStepFrequency;
    float _stepFrequency;

    Vector3 rootPositionT_fdt;
    float targetYT_dt;
    float targetYVelocity;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        InitializeValues();
    }

    // Update is called once per frame
    void Update()
    {
        if (move)
        {
            UpdateMotionBlock();
        }
        else
        {

        }
    }

    private void FixedUpdate()
    {
        if (move)
        {
            FixedUpdateMotionBlock();
        }
        else
        {

        }
    }

    protected void UpdateMotionBlock()
    {
        Step();
        if (t >= 1) WrapUp();
    }

    protected void FixedUpdateMotionBlock()
    {
        raycastPosition = _root.position + _root.TransformDirection(_raycastOffset);
        ProcessRootVelocity(out rootVelocity, out rootDirection, out rootSpeed);
        rootPositionT_fdt = raycastPosition;
    }

    protected void InitializeValues()
    {
        raycastPosition = _root.position + _root.TransformDirection(_raycastOffset);
        _baseStepFrequency = 1 / _baseStride * 2;
        if(Physics.Raycast(raycastPosition, Vector3.down, out RaycastHit hit, _distance, _ground)) _target.position = hit.point;
        else _target.position = raycastPosition + Vector3.down * _distance;
        ResetValues();
    }

    protected void ResetValues()
    {
        t = 0;
        p0 = _target.position;
        raycastPosition = _root.position + _root.TransformDirection(_raycastOffset);
        rootPositionT_fdt = raycastPosition;
        targetYT_dt = 0;
        targetYVelocity = 0;
    }

    protected void Step()
    {
        UpdateBaseValues(out _scaleCoefficient, out float stride, out _stepFrequency, ref t);

        Vector3 nextStep = EstimatedNextStep(stride);
        _target.position = CalculateTargetPosition(nextStep);
    }

    protected void ProcessRootVelocity(out Vector3 velocity, out Vector3 direction, out float speed)
    {
        Vector3 movement = raycastPosition - rootPositionT_fdt;
        velocity = movement / Time.fixedDeltaTime;
        direction = velocity.normalized;
        speed = velocity.magnitude;
    }

    protected void UpdateBaseValues(out float scaleCoefficient, out float stride, out float stepFrequency, ref float t)
    {
        scaleCoefficient = Mathf.Sqrt(rootSpeed);
        stride = _baseStride * scaleCoefficient;
        stepFrequency = _baseStepFrequency * scaleCoefficient;
        if ((t += stepFrequency * Time.deltaTime) > 1) t = 1;
    }

    protected Vector3 EstimatedNextStep(float stride)
    {
        Vector3 p1 = _stepFrequency == 0 ? raycastPosition : p1Estimate(stride);
        if (Physics.Raycast(p1, Vector3.down, out RaycastHit hit, _distance, 1<<3)) return hit.point;
        else return p1 + Vector3.down * _distance;
    }

    protected Vector3 p1Estimate(float stride)
    {
        float timeLeft = (1 - t) / _stepFrequency;
        Vector3 r1 = timeLeft * rootVelocity + raycastPosition;
        return r1 + 0.5f * stride * rootDirection;
    }

    protected Vector3 CalculateTargetPosition(Vector3 p1)
    {
        Vector3 interpolatedFootPosition = Vector3.Lerp(p0, p1, t);
        float footOffsetCoefficient = YAdditive(t);
        float footYLevitation = footOffsetCoefficient * _lift * Mathf.Clamp01(_scaleCoefficient);
        float footDampAllowance = footOffsetCoefficient * _yDamp;
        float DampedYAdditive = Mathf.SmoothDamp(targetYT_dt, footYLevitation, ref targetYVelocity, footDampAllowance);
        targetYT_dt = DampedYAdditive;
        return interpolatedFootPosition + DampedYAdditive * Vector3.up;
    }

    protected float YAdditive(float x)
    {
        return 4 * x * (1 - x);
    }

    protected void WrapUp()
    {
        onFootChange.Invoke();
        move = false;
    }
    
    public void StartMoving()
    {
        if (!move)
        {
            ResetValues();
            move = true;
        }
    }
}
