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
        // 멈춰 둔 동안에는 아무것도 셈하지 않는다. 몸은 떼어져 있어 실리지도 않지만, 셈 자체가 돌면
        // Mutual의 걸음 속도가 목표를 향해 줄어들어 풀었을 때 이어받을 것이 새어 나간다.
        if (_frozen) return;

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

        SetSampler(_external.linearVelocity + magnitude * accel);

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
    private void SetBodyVelocity(Vector3 velocity) => SetVelocity(_own, velocity);

    /// <summary>외력 몸도 같은 이유로 거쳐 간다. 붙어 있는 동안은 이쪽도 키네마틱이다.</summary>
    private void SetSampler(Vector3 velocity) => SetVelocity(_external, velocity);

    private static void SetVelocity(Rigidbody body, Vector3 velocity)
    {
        if (body.isKinematic) return;

        body.linearVelocity = velocity;
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
        SetSampler(v);
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
    /// 두 몸을 물리에서 떼어낸다. <b>자리의 주인이 하나여야 하는 구간</b>이 쓴다.
    ///
    /// 한쪽만 떼면 주인이 둘이 된다. 몸만 키네마틱으로 두면 외력 몸은 여전히 물리 바디라
    /// 지오메트리에 겹칠 때 솔버가 밀어내는데, 다음 프레임 Singular이 그 밀려난 자리를
    /// 몸에 그대로 복사한다 — 우리가 정한 자리가 조용히 덮인다. 겹치는 정도에 따라
    /// 일어나거나 말거나 하므로 재현도 잘 되지 않는다.
    ///
    /// 떼어낸 동안에는 속도를 실을 수 없다. 다시 붙인 <b>뒤에</b> 실어야 한다.
    /// </summary>
    public void Detach(bool detached)
    {
        _own.isKinematic = detached;
        _external.isKinematic = detached;
    }

    /// <summary>
    /// 몸을 한 지점에 못박는다. <b>중력을 끈 상태에서만 쓴다.</b>
    ///
    /// 켜둔 채 위치만 되돌리면 보이는 자리는 멎어 있어도 sampler의 아래 방향 속도는 계속 쌓이고,
    /// 놓는 순간 그동안 쌓인 만큼이 한꺼번에 터진다. 매달림·오르기가 중력부터 끄는 이유다.
    ///
    /// 두 몸에 모두 적는다. Singular이 매 FixedUpdate마다 sampler를 rigidbody로 옮기지만,
    /// 그 순서에 기대면 부르는 시점에 따라 한 프레임 어긋난다.
    ///
    /// Rigidbody와 Transform 둘 다에 적는다. Rigidbody.position은 <b>물리 자세만</b> 바꾸고,
    /// 그것이 Transform에 실리는 시점은 다음 시뮬레이션 스텝이다 — 프로젝트가 Auto Sync Transforms를 꺼둔 탓이다.
    /// 그러니 시간이 멎은 구간에서는 그 스텝이 오지 않아, 몸은 옮겨졌는데 그림과 카메라만 옛 자리에 남는다.
    /// 부활 직후가 그렇다 — Character_Rest가 들어오면서 상한을 0으로 누르므로, 일어날 때까지 스텝이 한 번도 돌지 않는다.
    /// </summary>
    public void Pin(Vector3 position)
    {
        _targetVelocity = Vector3.zero;
        _localVelocity = Vector3.zero;

        _external.position = position;
        _external.transform.position = position;
        SetSampler(Vector3.zero);

        _own.position = position;
        _own.transform.position = position;
        SetBodyVelocity(Vector3.zero);

        // 멈춰 둔 동안이라면 적어 둔 속도도 버린다. 자리를 직접 정한 쪽이 있으면 멈추기 전의 속도는
        // 더 이상 그 자리의 것이 아니다 — 씬 전환이 새 자리에 놓은 몸에 떠나온 자리의 낙하 속도를 실으면
        // 도착하자마자 튕겨 나간다.
        _frozenOwn = Vector3.zero;
        _frozenExternal = Vector3.zero;
    }

    bool _frozen;
    Vector3 _frozenOwn;
    Vector3 _frozenExternal;

    /// <summary>
    /// 몸을 그 자리에 세운다. 두 몸을 떼어내고(<see cref="Detach"/>) 멈춘 순간의 속도를 적어 둔다.
    /// 풀면 다시 붙인 <b>뒤에</b> 그 속도를 싣는다 — 떼어낸 동안에는 속도를 실을 수 없다.
    /// 공중에서 멈췄다면 떨어지던 그대로 이어서 떨어진다.
    ///
    /// 떼어내는 것이라 밀려나지도 떨어지지도 않는다. 트리거는 그대로 겹치므로 낙사 볼륨 같은 것은
    /// 멈춘 동안에도 닿는다 — 대사 중에도 죽을 수 있어야 하는 연출이 있다.
    ///
    /// 같은 쪽으로 두 번 불러도 한 번만 먹는다. 멈춘 채 다시 멈추면 0을 "멈춘 순간의 속도"로 적는다.
    /// </summary>
    public void Freeze(bool frozen)
    {
        if (_frozen == frozen) return;

        _frozen = frozen;

        if (frozen)
        {
            _frozenOwn = _own.linearVelocity;
            _frozenExternal = _external.linearVelocity;
            Detach(true);
            return;
        }

        Detach(false);
        SetBodyVelocity(_frozenOwn);
        SetSampler(_frozenExternal);
    }

    public void SetSamplerYVelocity(float velocity)
    {
        Vector3 v = _external.linearVelocity;
        v.y = velocity;
        SetSampler(v);
    }

    public void IncrementSamplerVelocity(Vector3 incrementation)
    {
        SetSampler(_external.linearVelocity + incrementation);
    }

    public void SetSamplerVelocity(Vector3 velocity)
    {
        SetSampler(velocity);
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
        SetSampler(_own.linearVelocity);
        _targetVelocity = Vector3.zero;
        _localVelocity = Vector3.zero;
    }

    private void IntegrateSingularToMutual()
    {
        _targetVelocity = _external.linearVelocity;
        _localVelocity = _external.linearVelocity;
        _external.position = _own.position;
        SetSampler(Vector3.zero);
    }

    #endregion
}
