using UnityEngine;

/// <summary>
/// 수직 속도의 주인. 이동 컴포넌트가 매 스텝 <see cref="Resolve"/>를 부르고,
/// 돌려받은 <b>날것의 Y 속도</b>를 그대로 씁니다 — 이동 컴포넌트는 Y를 해석하지 않는다.
///
/// Rigidbody에 직접 쓰지 않는다. 쓰는 쪽은 이동 컴포넌트 하나뿐이고 여기는 값만 답하므로,
/// 두 FixedUpdate의 실행 순서가 결과를 바꾸지 않는다.
///
/// 입력은 두 종류를 받는다.
///   <see cref="SetVelocity"/> : 지정. 이전 속도를 지운다. 점프·내리꽂기처럼 궤도를 새로 여는 것.
///   <see cref="AddVelocity"/> : 가산. 기존 운동 위에 얹는다. 넉백·바람처럼 거들기만 하는 것.
/// 둘 다 한 스텝짜리다. Resolve가 소비하고 비운다.
///
/// 붙이면 중력도 이쪽이 전부 만든다 — Rigidbody.useGravity는 꺼야 이중으로 걸리지 않는다.
/// 붙이지 않으면 이동 컴포넌트는 Y에 손대지 않고 유니티 기본 중력에 맡긴다.
/// </summary>
public abstract class VerticalMotion : MonoBehaviour
{
    float _target;
    bool _hasTarget;
    float _delta;

    /// <summary>
    /// Y 속도를 지정한다. 이번 스텝의 <see cref="Step"/>을 건너뛰므로,
    /// 점프 초기 속도가 그 스텝의 중력에 깎이지 않는다.
    /// </summary>
    public void SetVelocity(float velocity)
    {
        _target = velocity;
        _hasTarget = true;
    }

    /// <summary>Y 속도에 더한다. 한 스텝 안에 여러 번 들어오면 누적된다.</summary>
    public void AddVelocity(float delta)
    {
        _delta += delta;
    }

    /// <summary>
    /// 아직 적용되지 않은 입력을 미리 반영해 본다.
    /// 제출과 적용 사이 한 스텝 동안 상승/하강 판정이 어긋나지 않게 한다.
    /// </summary>
    public float Preview(float currentY) => (_hasTarget ? _target : currentY) + _delta;

    /// <summary>이동 컴포넌트가 매 스텝 부른다. 이번 스텝에 쓸 Y 속도를 돌려준다.</summary>
    public float Resolve(float currentY, float dt)
    {
        // 지정이 있으면 거기서 새로 출발한다. 없으면 모델이 굴린다.
        float y = _hasTarget ? _target : Step(currentY, dt);

        // 가산은 어느 쪽 위에든 얹힌다. 점프 중에 맞은 넉백이 사라지지 않게.
        y += _delta;

        _hasTarget = false;
        _delta = 0f;

        return y;
    }

    /// <summary>
    /// 지정이 없는 스텝에 Y를 어떻게 굴릴지. 중력이든 부양이든 여기서 정한다.
    /// 소비와 비우기는 Resolve가 하므로 파생 쪽이 잊을 일이 없다.
    /// </summary>
    protected abstract float Step(float currentY, float dt);
}
