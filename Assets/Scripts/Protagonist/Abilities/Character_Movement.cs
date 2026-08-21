using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 몸이 둘이다. 이름이 그 둘을 가른다.
///
///   _external  <b>외력이 사는 몸.</b> 중력·발판·넉백이 여기 실린다. 캐릭터 콜라이더는 없다.
///   _own       <b>제 힘이 사는 몸.</b> 캡슐·피격 판정·메시가 여기 있고, 세상이 부딪히는 것도 이쪽이다.
///
/// Mutual에서는 둘이 더해진다 — 발판이 미는 만큼(external) 위에 제가 걷는 만큼(_localVelocity)을 얹는다.
/// Singular에서는 external이 전부를 들고 _own은 위치만 따라간다. 그래서 <see cref="Velocity"/>가
/// 모드에 따라 다른 쪽을 본다 — 속도의 원본이 어디인지가 모드마다 다르기 때문이다.
/// </summary>
public class Character_Movement : MonoBehaviour, IMovement
{
    [Tooltip("외력이 사는 몸. 중력과 발판 속도가 여기 실린다.")]
    [SerializeField] private Rigidbody _external;

    [Tooltip("제 힘이 사는 몸. 캡슐과 피격 판정이 여기 있다.")]
    [SerializeField] private Rigidbody _own;
    [Space(10f)]
    [SerializeField] private float _groundAcceleration;
    [SerializeField] private float _aerialAcceleration;

    Vector3 _localVelocity;
    Vector3 _targetVelocity;

    [SerializeField] bool _singular = false;

    private void FixedUpdate()
    {
        if (_singular) Singular();
        else Mutual();
    }

    private void Singular()
    {
        // singular mode does NOT utilize the concept of separated velocities
        // thus, only the SAMPLER will be utilized.

        Vector3 velocity = _external.linearVelocity;
        Vector3 inputDirection = _targetVelocity.normalized;
        Vector3 rawAccel = _targetVelocity - velocity;
        rawAccel.y = 0;

        // 입력이 없으면 방향이 없어 alignment가 0으로 죽고, _aerialAcceleration이 곱해질 자리가 통째로 사라진다.
        // 그때는 되돌려야 할 양 자체가 필요량이다. rawAccel이 이미 현재 수평 속도의 반대를 가리킨다.
        float alignment = inputDirection == Vector3.zero
            ? rawAccel.magnitude
            : Vector3.Dot(rawAccel, inputDirection);

        float disparity = CustomMath.ReLU(-alignment);
        Vector3 negativeAlignment = -disparity * inputDirection;
        Vector3 accel = (rawAccel - negativeAlignment).normalized;   // remove negative alignment factor from the acceleration vector. in short, accelerates in input direction only if character is slower than input.

        // 남은 차이보다 크게 밟지 않는다. 없으면 영점을 두고 진동한다.
        float magnitude = Mathf.Min(
            _aerialAcceleration * Mathf.Clamp(alignment, 0, 1) * Time.fixedDeltaTime,
            rawAccel.magnitude);

        _external.linearVelocity += magnitude * accel;

        // integrate velocity spaces
        _own.position = _external.position;
        SetBodyVelocity(_external.linearVelocity);
    }

    /// <summary>
    /// 몸에 속도를 싣는다. <b>키네마틱인 동안에는 쓰지 않는다.</b>
    ///
    /// 키네마틱 바디는 속도로 움직이지 않아 대입해도 무시되고 경고만 남는데,
    /// 여기는 매 FixedUpdate 도는 자리라 그 경고가 프레임마다 쌓인다.
    /// 위치 대입은 키네마틱에서도 그대로 먹으므로 몸을 옮기는 데는 지장이 없다.
    /// </summary>
    private void SetBodyVelocity(Vector3 velocity)
    {
        if (_own.isKinematic) return;

        _own.linearVelocity = velocity;
    }

    private void Mutual()
    {
        //0. set local velocity
        AccelerateTo(_targetVelocity);

        //1. set linear velocity as local + environmental velocity
        SetBodyVelocity(_external.linearVelocity + _localVelocity);

        //2. set sample position
        _external.position = _own.position;
    }

    private void AccelerateTo(Vector3 targetVelocity)
    {
        if (targetVelocity == _localVelocity) return;
        
        Vector3 direction = (targetVelocity - _localVelocity).normalized;
        float difference = (targetVelocity - _localVelocity).magnitude;
        float step = _groundAcceleration * Time.fixedDeltaTime;
        float incremence = Mathf.Min(step, difference);
        Vector3 acceleration = incremence * direction;
        Accelerate(acceleration);
    }

    private void Accelerate(Vector3 acceleration)
    {
        _localVelocity += acceleration;
    }

    #region Public

    /// <summary>
    /// 몸이 지금 실제로 가진 속도.
    ///
    /// Singular에서는 sampler가 원본이고 rigidbody는 그것을 한 프레임 늦게 받는다.
    /// 게다가 몸이 키네마틱이면 아예 실리지 않아 0으로 읽힌다 — 물어보는 쪽은 원본을 봐야 한다.
    /// </summary>
    public Vector3 Velocity => _singular ? _external.linearVelocity : _own.linearVelocity;

    public void SetMovementMode(bool singular)
    {
        if (singular != _singular)
        {
            _singular = singular;
            if (singular) IntegrateMutualToSingular();
            else IntegrateSingularToMutual();
        }
    }

    public void ClearMovement()
    {
        _targetVelocity = Vector3.zero;
        _localVelocity = Vector3.zero;
        SetBodyVelocity(_external.linearVelocity);
        if (!_own.isKinematic) _own.angularVelocity = _external.angularVelocity;
    }

    public void Move(Vector3 targetLocalVelocity)
    {
        _targetVelocity = targetLocalVelocity;
    }

    /// <summary>
    /// 조향을 놓는다. 목표를 지금 속도에 맞춰 가속이 걸릴 여지를 없앤다.
    /// Move(Vector3.zero)가 "0까지 감속하라"인 것과 달리 이쪽은 "아무것도 시키지 않는다"이다.
    /// 외력이 수평을 바꿔도 다음 프레임에 다시 맞추도록 FixedUpdate마다 부른다.
    /// </summary>
    public void Coast()
    {
        // Singular은 수평이 sampler에 실려 있고, Mutual은 _localVelocity가 경사면 성분까지 들고 있다.
        // 그래서 전자만 수직을 턴다.
        _targetVelocity = _singular
            ? CustomMath.RemoveY(_external.linearVelocity)
            : _localVelocity;
    }

    public void Drop()
    {
        Vector3 v = _external.linearVelocity;
        v.y = -CustomMath.ReLU(-v.y);
        _external.linearVelocity = v;
    }

    /// <summary>
    /// 중력을 끄고 켠다. 수직 운동은 두 모드 모두 sampler가 소유하므로 sampler만 건드린다.
    /// 대시처럼 지속 동안 궤도를 무시해야 하는 상태가 쓴다.
    /// </summary>
    public void SetGravity(bool enabled)
    {
        _external.useGravity = enabled;
    }

    /// <summary>
    /// 몸을 한 지점에 못박는다. <b>중력을 끈 상태에서만 쓴다.</b>
    ///
    /// 켜둔 채 위치만 되돌리면 보이는 자리는 멎어 있어도 sampler의 아래 방향 속도는 계속 쌓이고,
    /// 놓는 순간 그동안 쌓인 만큼이 한꺼번에 터진다. 매달림·오르기가 중력부터 끄는 이유다.
    ///
    /// 두 몸에 모두 적는다. Singular이 매 FixedUpdate마다 sampler를 rigidbody로 옮기지만,
    /// 그 순서에 기대면 부르는 시점에 따라 한 프레임 어긋난다.
    /// </summary>
    public void Pin(Vector3 position)
    {
        _targetVelocity = Vector3.zero;
        _localVelocity = Vector3.zero;

        _external.position = position;
        _external.linearVelocity = Vector3.zero;

        _own.position = position;
        SetBodyVelocity(Vector3.zero);
    }

    public void SetSamplerYVelocity(float velocity)
    {
        Vector3 v = _external.linearVelocity;
        v.y = velocity;
        _external.linearVelocity = v;
    }

    public void IncrementSamplerVelocity(Vector3 incrementation)
    {
        _external.linearVelocity += incrementation;
    }

    public void SetSamplerVelocity(Vector3 velocity)
    {
        _external.linearVelocity = velocity;
    }

    public void SetLocalVelocity(Vector3 velocity)
    {
        _localVelocity = velocity;
    }

    #endregion

    #region VelocitySpace Integration

    private void IntegrateMutualToSingular()
    {
        _external.position = _own.position;
        _external.linearVelocity = _own.linearVelocity;
        _targetVelocity = Vector3.zero;
        _localVelocity = Vector3.zero;
    }

    private void IntegrateSingularToMutual()
    {
        _targetVelocity = _external.linearVelocity;
        _localVelocity = _external.linearVelocity;
        _external.position = _own.position;
        _external.linearVelocity = Vector3.zero;
    }

    #endregion
}
