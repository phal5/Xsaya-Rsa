using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 한 번 닿으면 표시되고, 플레이어가 떨어지는 순간 사라지는 발판.
///
/// <b>닿는 즉시가 아니라 떠나는 순간에 사라진다.</b> 밟자마자 없애면 딛고 뛸 것이 없어져
/// 그냥 구덩이가 된다. 딛고 서 있는 동안은 멀쩡하고, 그 자리를 떠나면 돌아갈 길이 없어지는 것이
/// 이 발판이 만들려는 긴장이다.
///
/// 판정은 <see cref="PlayerManager.IsPlayer"/> 하나로 한다 — 태그나 레이어로 다시 물으면
/// 외력 몸과 캡슐이 갈린 구성에서 어느 쪽이 걸리느냐에 따라 답이 달라진다.
///
/// 콜라이더가 <b>이 오브젝트에 붙어 있어야</b> 한다. 물리 충돌 신호는 콜라이더가 달린
/// 게임오브젝트(와 그 위의 리지드바디)로만 가므로, 자식만 콜라이더인 통에 붙이면
/// 아무 신호도 오지 않는다. 감추는 것은 자식까지 함께 감춘다.
/// </summary>
public class Crumble : MonoBehaviour
{
    [Tooltip("접촉이 끊긴 뒤 이만큼 지나야 무너진다. 물리가 한두 스텝 접촉을 놓치는 것과 " +
             "정말로 떠난 것을 가른다. 캐릭터의 코요테 타임보다 짧게 두면, 발판이 없어진 뒤에 " +
             "그 발판에서 점프하는 순간이 생긴다.")]
    [SerializeField, Min(0f)] float _grace = 0.1f;

    [Tooltip("사라진 뒤 다시 나타나기까지의 시간. 0이면 돌아오지 않는다.")]
    [SerializeField, Min(0f)] float _return = 0f;

    [Tooltip("무너지는 순간. 소리나 파티클을 여기 문다.")]
    [SerializeField] UnityEvent _onCrumble;

    Collider[] _colliders;
    Renderer[] _renderers;

    /// <summary>한 번이라도 닿았는가. 닿기 전에는 떠날 일도 없으므로 아무것도 세지 않는다.</summary>
    bool _touched;

    bool _gone;

    /// <summary>마지막으로 접촉을 본 물리 시각.</summary>
    float _lastTouch;

    float _returnAt;

    void Awake()
    {
        // 꺼진 것까지 담는다. 처음부터 꺼 둔 장식을 되살리며 켜버리면 안 되므로
        // 각자의 상태를 건드리는 대신 통째로 껐다 켠다.
        _colliders = GetComponentsInChildren<Collider>(true);
        _renderers = GetComponentsInChildren<Renderer>(true);
    }

    void OnCollisionEnter(Collision collision) => Touch(collision.collider);

    /// <summary>
    /// 머무는 동안 계속 본다. 떠났는지를 <c>OnCollisionExit</c>로 묻지 않는 이유다 —
    /// 그 신호는 발판 위를 걷는 도중에도 한두 스텝씩 튀어나오고, 그때마다 발판이 무너지면
    /// 서 있기만 해도 바닥이 빠진다. 마지막으로 <b>닿아 있던 시각</b>을 적어두고
    /// 그것이 충분히 오래되었을 때 떠난 것으로 본다.
    /// </summary>
    void OnCollisionStay(Collision collision) => Touch(collision.collider);

    void OnTriggerEnter(Collider other) => Touch(other);

    void OnTriggerStay(Collider other) => Touch(other);

    /// <summary>
    /// 플레이어의 <b>몸</b>이 닿았을 때만 센다.
    ///
    /// 몸에는 공격 판정처럼 트리거인 콜라이더도 함께 달려 있고, 그것들도
    /// <see cref="PlayerManager.IsPlayer"/>에는 플레이어로 잡힌다. 걸러내지 않으면
    /// 발판 옆에서 칼을 한 번 휘두른 것만으로 밟은 것이 되어, 딛기도 전에 무너진다.
    /// </summary>
    void Touch(Collider other)
    {
        if (_gone || other.isTrigger || !PlayerManager.IsPlayer(other)) return;

        _touched = true;
        _lastTouch = Time.fixedTime;
    }

    void FixedUpdate()
    {
        if (_gone)
        {
            if (_return > 0f && Time.fixedTime >= _returnAt) Restore();
            return;
        }

        if (!_touched) return;
        if (Time.fixedTime - _lastTouch <= _grace) return;

        Vanish();
    }

    /// <summary>
    /// 오브젝트를 끄지 않고 콜라이더와 렌더러만 끈다.
    ///
    /// 끄면 이 컴포넌트도 함께 멎어 <see cref="_return"/>을 셀 것이 없어지고,
    /// 밖에서 <see cref="Restore"/>를 부르는 길도 막힌다.
    /// </summary>
    void Vanish()
    {
        _gone = true;
        _returnAt = Time.fixedTime + _return;

        Show(false);
        _onCrumble.Invoke();
    }

    /// <summary>
    /// 처음 상태로 되돌린다. 다시 닿기 전까지는 무너지지 않는다.
    ///
    /// <b>부활은 이것을 저절로 불러주지 않는다.</b> 같은 스테이지에서 죽으면 씬을 다시 올리지 않고
    /// 자리만 옮기므로, 무너진 발판은 무너진 채로 남는다. 되돌아와야 하는 자리라면
    /// <see cref="_return"/>을 주거나 체크포인트 쪽에서 이것을 부를 것.
    /// </summary>
    public void Restore()
    {
        _gone = false;
        _touched = false;

        Show(true);
    }

    void Show(bool visible)
    {
        foreach (Collider part in _colliders) part.enabled = visible;
        foreach (Renderer part in _renderers) part.enabled = visible;
    }

#if UNITY_EDITOR
    /// <summary>
    /// 신호가 올 수 없는 자리에 붙었으면 알린다.
    ///
    /// 조용히 아무 일도 일어나지 않는 것이 이 컴포넌트의 유일한 오작동 방식이라,
    /// 플레이해 보기 전에 눈에 띄어야 한다.
    /// </summary>
    void OnValidate()
    {
        if (GetComponent<Collider>() == null && GetComponent<Rigidbody>() == null)
            Debug.LogWarning($"[Crumble] '{name}'에 콜라이더가 없다. 충돌 신호는 콜라이더가 달린 " +
                             "오브젝트로만 가므로, 자식이 아니라 발판 자신에게 붙여야 한다.", this);
    }
#endif
}
