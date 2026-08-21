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
    [Tooltip("몸통. 흔들림이 닿지 않는 곳이어야 한다 — 걸음 속도를 여기서 잰다.")]
    [SerializeField] Transform _body;
    [SerializeField] Transform _target;
    [Space(10f)]
    [SerializeField] UnityEvent onFootChange;
    [field:SerializeField] public bool move { get; private set; }

    /// <summary>
    /// 지금 발이 들려 있는 높이. 딛고 있으면 0이고, 몸이 서면 0으로 잦아든다.
    ///
    /// 몸통 흔들림은 발의 절대 높이가 아니라 이 값을 봐야 한다. 높이에는 지형의
    /// 높낮이와, 걸음에서 이탈해 뒤에 남겨진 발까지 섞여 들어와 흔들림이 아니라
    /// 풀리지 않는 기울기가 된다.
    /// </summary>
    public float Lift { get; private set; }

    float _scaleCoefficient;

    Vector3 p0;
    Vector3 rootVelocity;
    Vector3 rootDirection;
    Vector3 raycastPosition;
    protected float rootSpeed;
    protected float t;
    float _baseStepFrequency;
    float _stepFrequency;

    Vector3 stableRoot;
    Vector3 stableRootT_fdt;
    Vector3 _bodyLocalRoot;
    float targetYT_dt;
    float targetYVelocity;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // 다리 뿌리를 몸 기준으로 한 번 적어둔다. 이후 속도는 이 점의 이동으로 잰다.
        // 실제 뿌리는 몸통과 함께 흔들리므로 그대로 재면 흔들림이 속도로 둔갑하고,
        // 빨라진 걸음이 흔들림을 더 키우는 되먹임이 닫힌다. 흔들림은 걸음을 따라오되
        // 걸음은 흔들림을 따라가지 않아야 한다.
        _bodyLocalRoot = _body.InverseTransformPoint(_root.position);
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
        stableRoot = _body.TransformPoint(_bodyLocalRoot);
        ProcessRootVelocity(out rootVelocity, out rootDirection, out rootSpeed);
        stableRootT_fdt = stableRoot;
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
        stableRoot = _body.TransformPoint(_bodyLocalRoot);
        stableRootT_fdt = stableRoot;
        targetYT_dt = 0;
        targetYVelocity = 0;
        Lift = 0;
    }

    protected void Step()
    {
        UpdateBaseValues(out _scaleCoefficient, out float stride, out _stepFrequency, ref t);

        Vector3 nextStep = EstimatedNextStep(stride);
        _target.position = CalculateTargetPosition(nextStep);
    }

    protected void ProcessRootVelocity(out Vector3 velocity, out Vector3 direction, out float speed)
    {
        Vector3 movement = stableRoot - stableRootT_fdt;
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
        if (Physics.Raycast(p1, Vector3.down, out RaycastHit hit, _distance, _ground)) return hit.point;
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
        Lift = DampedYAdditive;
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
