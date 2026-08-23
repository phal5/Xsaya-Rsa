using UnityEngine;

/// <summary>
/// 상태기계가 애니메이터에 말을 거는 유일한 창구.
///
/// 가짜 애니메이터는 매 프레임 속도와 체력을 보고 "지금 무슨 상태겠지"를 <b>추측</b>했다.
/// FSM이 이미 정답을 들고 있으므로 그 추측은 군더더기다.
/// 그래서 여기엔 판단이 없다. 상태가 Enter에서 이름을 부르면 그대로 튼다.
///
/// 진행도를 되돌려주는 것까지가 여기의 일이고, 그 숫자로 무엇을 할지는 상태가 정한다.
/// </summary>
public class Character_Animation : MonoBehaviour
{
    [SerializeField] Animator _animator;

    [Tooltip("상태 전환 시 섞는 시간(초).")]
    [SerializeField, Min(0f)] float _fade = 0.1f;

    string _current;

    bool _claimed;

    /// <summary>
    /// 방금 건 한 컷을 <b>다음 축이 덮지 않게</b> 표시한다.
    ///
    /// 축을 넘기며 자세를 이미 정해 놓은 쪽이 부르고, 받는 쪽은 <see cref="Claimed"/>를 보고
    /// 제 자세를 걸지 말지 정한다. CrossFade는 섞이는 데 시간이 걸려서, 표시가 없으면
    /// 받는 쪽이 같은 프레임에 덮어써 앞의 클립이 현재 상태가 되어 보지도 못한다.
    /// </summary>
    public void Claim() => _claimed = true;

    /// <summary>표시가 서 있었는지. <b>읽으면 풀린다</b> — 다음 전환까지 남아 다른 자세를 막지 않도록.</summary>
    public bool Claimed
    {
        get
        {
            bool claimed = _claimed;
            _claimed = false;
            return claimed;
        }
    }

    /// <summary>컨트롤러의 상태 이름을 그대로 넘긴다. 같은 이름이면 다시 걸지 않는다.</summary>
    public void Play(string state) => Play(state, -1f);

    /// <param name="fade">섞이는 시간. 자세가 크게 다른 두 동작을 잇는 곳이 기본값보다 길게 준다. 음수면 기본값.</param>
    public void Play(string state, float fade)
    {
        if (_animator == null || string.IsNullOrEmpty(state)) return;
        if (_current == state) return;

        _current = state;

        Drop();

        Warn(state);
        _animator.CrossFadeInFixedTime(state, Fade(fade));
    }

    float _once = -1f;

    /// <summary>
    /// <b>다음 한 번의 전환만</b> 이 시간으로 섞는다.
    ///
    /// 짧은 동작을 걸고 나가는 쪽이 쓴다. 나가는 블렌드는 다음 자세를 거는 쪽이 정하는데,
    /// 그쪽은 자기가 무엇을 밀어내는지 모른다. 0.1초짜리 기본값이 0.11초짜리 동작을 덮으면
    /// 그 동작은 온전한 자세를 한 번도 못 보여주고 사라진다.
    ///
    /// 명시적으로 넘긴 값이 언제나 이긴다. 걸어두고 아무도 안 쓰면 다음 전환에서 그냥 사라진다.
    /// </summary>
    public void FadeOnce(float fade) => _once = fade;

    /// <summary>이번 전환에 쓸 시간. 넘긴 값 &gt; 한 번짜리 &gt; 기본값 순이고, 읽으면 한 번짜리는 풀린다.</summary>
    float Fade(float requested)
    {
        float once = _once;
        _once = -1f;

        if (requested >= 0f) return requested;

        return once >= 0f ? once : _fade;
    }

    /// <summary>
    /// 컨트롤러에 그 이름의 상태가 있는지.
    ///
    /// 유니티는 없는 이름으로 CrossFade를 걸면 <b>아무 말 없이 무시한다.</b>
    /// 상태가 안 만들어졌는지, FSM이 안 불렀는지, 이름이 틀렸는지가 화면상 똑같이 보여
    /// 원인을 가릴 수가 없다. 그 침묵을 여기서 깬다.
    ///
    /// 같은 이름으로 두 번 나무라지는 않는다. 매 프레임 부르는 자리가 있어 로그가 잠긴다.
    ///
    /// <b>재생을 막지는 않는다.</b> 이 조회가 틀릴 수도 있는데 그걸 근거로 호출을 끊으면
    /// 알려주려던 고장 대신 새 고장을 만든다. 말만 하고 걸어보는 것이 맞다.
    /// </summary>
    void Warn(string state)
    {
        if (_animator.HasState(0, Animator.StringToHash(state))) return;

        if (_missing.Add(state))
            Debug.LogWarning($"[{name}] 애니메이터에 '{state}' 상태가 없는 것으로 읽힙니다. " +
                             $"이름이 틀렸거나 컨트롤러에 그 상태가 없습니다.", this);
    }

    readonly System.Collections.Generic.HashSet<string> _missing = new System.Collections.Generic.HashSet<string>();

    #region Root Motion

    Vector3 _rootMotion;

    /// <summary>
    /// 루트 모션을 유니티가 스스로 적용하지 못하게 가로챈다.
    ///
    /// Animator가 몸이 아니라 자식 오브젝트에 붙어 있어, 그대로 두면 <b>메시만 몸에서 떨어져 나간다.</b>
    /// 피격 판정은 몸에 있으므로 그건 판정과 그림이 어긋난다는 뜻이다.
    /// 여기서 받아두고, 쓰겠다는 상태가 가져가 몸에 싣는다.
    ///
    /// 이 함수가 있는 것만으로 유니티는 자동 적용을 그만둔다. 아무도 가져가지 않으면 그냥 버려진다.
    /// </summary>
    void OnAnimatorMove()
    {
        if (_animator == null) return;

        // 한 번의 FixedUpdate 사이에 애니메이터가 여러 번 돌 수 있어 더한다.
        _rootMotion += _animator.deltaPosition;
    }

    /// <summary>
    /// 루트 모션을 <b>받아둘지</b>. 가져갈 쪽이 있는 구간만 켠다.
    ///
    /// 꺼두면 유니티가 <see cref="OnAnimatorMove"/>를 아예 부르지 않아 쌓일 것이 없다.
    /// 켜둔 채 아무도 안 가져가면 그 몫이 계속 쌓이고, 다음에 가져가는 구간에 들어서는 순간
    /// 그동안의 것이 한꺼번에 몸에 실린다 — 한 번 매달렸다 나온 뒤 다시 매달릴 때 몸이 홱 도는 이유였다.
    ///
    /// 몸에 그때그때 옮기는 방법도 있지만 그럴 수 없다. 지상·공중은 속도로 움직이는 설계라
    /// 클립의 이동까지 실으면 달리기가 7.5에서 10.4로 빨라진다. 클립은 그쪽에서 자세만 준다.
    /// </summary>
    public void CaptureRootMotion(bool capture)
    {
        if (_animator == null) return;

        _animator.applyRootMotion = capture;
        Drop();
    }

    /// <summary>쌓인 몫을 통째로 버린다.</summary>
    void Drop()
    {
        _rootMotion = Vector3.zero;
    }

    Transform _leftHand, _rightHand;
    bool _handsLooked;

    /// <summary>
    /// 두 손의 가운데. <b>묻기만 한다</b> — 여기서 읽은 값으로 몸을 옮기지 않는다.
    ///
    /// 클립마다 손이 제자리에 놓이는 프레임이 다르고, 그것을 프레임 수로 적어두면
    /// 클립이 바뀔 때마다 다시 재야 한다. 뼈에 직접 물으면 어느 클립이든 저절로 맞는다.
    /// </summary>
    public bool TryHandCenter(out Vector3 center)
    {
        center = Vector3.zero;

        if (!_handsLooked)
        {
            _handsLooked = true;

            if (_animator != null && _animator.isHuman)
            {
                _leftHand = _animator.GetBoneTransform(HumanBodyBones.LeftHand);
                _rightHand = _animator.GetBoneTransform(HumanBodyBones.RightHand);
            }
        }

        if (_leftHand == null || _rightHand == null) return false;

        center = (_leftHand.position + _rightHand.position) * 0.5f;
        return true;
    }

    /// <summary>
    /// 지금 걸린 클립이 <b>통틀어</b> 몸을 얼마나 옮기는지. 클립의 제 좌표계로 돌려준다(z가 앞, y가 위).
    ///
    /// 믹사모 클립은 제 배우의 팔 길이와 제 턱 높이에 맞춰 찍힌 것이라 우리 몸과 우리 턱에
    /// 맞을 이유가 없다. 얼마나 어긋나는지를 알아야 그만큼 늘려 걸 수 있다.
    ///
    /// 그 값을 손으로 재어 적어두지 않는다 — 임포트 설정을 건드리는 순간 조용히 틀려지고,
    /// 틀린 줄도 모르게 된다. 자산에 직접 묻는 편이 언제나 맞다.
    /// </summary>
    public bool TryClipTravel(out Vector3 travel)
    {
        travel = Vector3.zero;
        if (_animator == null) return false;

        // 섞이는 동안 현재는 아직 떠나는 쪽을 가리킨다. 알고 싶은 것은 들어오는 쪽이다.
        AnimatorClipInfo[] info = _animator.IsInTransition(0)
            ? _animator.GetNextAnimatorClipInfo(0)
            : _animator.GetCurrentAnimatorClipInfo(0);

        if (info.Length == 0 || info[0].clip == null) return false;

        travel = info[0].clip.averageSpeed * info[0].clip.length;
        return true;
    }

    /// <summary>쌓인 루트 모션을 가져가고 비운다. 가져간 쪽이 몸에 싣는 책임을 진다.</summary>
    public Vector3 ConsumeRootMotion()
    {
        Vector3 delta = _rootMotion;
        _rootMotion = Vector3.zero;
        return delta;
    }

    #endregion


    /// <summary>
    /// 클립의 첫 프레임이 아니라 지정한 지점으로 섞어 들어간다.
    ///
    /// 같은 이름이어도 다시 건다 — 어디서부터 트느냐가 인자의 일부이므로,
    /// 이름만 보고 걸러내면 "같은 클립의 다른 지점"을 부를 방법이 없어진다.
    /// </summary>
    /// <param name="fade">섞이는 시간. 음수면 기본값을 쓴다.</param>
    public void PlayFrom(string state, float offsetSeconds, float fade = -1f)
    {
        if (_animator == null || string.IsNullOrEmpty(state)) return;

        _current = state;

        Drop();

        Warn(state);
        _animator.CrossFadeInFixedTime(state, Fade(fade), 0, offsetSeconds);
    }

    public void SetFloat(string parameter, float value)
    {
        if (_animator == null || string.IsNullOrEmpty(parameter)) return;
        _animator.SetFloat(parameter, value);
    }

    /// <summary>
    /// 지정한 상태가 지금 재생 중인지. 맞으면 진행도(0~1)를 함께 돌려준다.
    ///
    /// 전환 중에는 GetCurrentAnimatorStateInfo가 아직 <b>떠나는 쪽</b>을 가리키므로,
    /// 들어오는 쪽도 같이 본다. 이게 없으면 섞이는 동안 "재생 중이 아니다"로 읽힌다.
    /// </summary>
    public bool IsPlaying(string state, out float normalizedTime)
    {
        normalizedTime = 0f;
        if (_animator == null || string.IsNullOrEmpty(state)) return false;

        AnimatorStateInfo current = _animator.GetCurrentAnimatorStateInfo(0);
        if (current.IsName(state))
        {
            normalizedTime = current.normalizedTime;
            return true;
        }

        if (!_animator.IsInTransition(0)) return false;

        AnimatorStateInfo next = _animator.GetNextAnimatorStateInfo(0);
        if (!next.IsName(state)) return false;

        normalizedTime = next.normalizedTime;
        return true;
    }
}
