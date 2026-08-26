using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// <see cref="Begin"/>을 부르면 자손들을 정해진 프레임 수에 걸쳐 통째로 흩어 없앤다.
///
/// <b>지울 순서는 시작할 때 한 번만 정한다.</b>
/// <see cref="_randomize"/>가 켜져 있으면 자손 계층을 뿌리 직속으로 평탄화(reparent)한 뒤
/// 순서를 무작위로 섞어 전체 영역에 걸쳐 자연스럽게 흩어지듯 지운다.
/// (평탄화해 두어야 중간 부모가 먼저 지워지면서 하위 자식들이 덩어리째 사라지는 일이 없다.)
///
/// 뿌리(이 오브젝트)는 남는다. 코루틴을 돌리는 것이 자신이기도 하고, 껍데기만 남겨두면
/// 이펙트나 사운드를 마저 낼 자리가 되기 때문이다. 통째로 없애야 한다면 밖에서 지울 것.
///
/// 도중에 이 오브젝트가 꺼지면 코루틴이 멎어 계층은 <b>반쯤 먹힌 채로</b> 남는다.
/// 다시 <see cref="Begin"/>을 부르면 남은 것들을 대상으로 처음부터 다시 센다.
/// </summary>
public class Disintegrate : MonoBehaviour
{
    [Tooltip("이 프레임 수에 나누어 지운다. 개수와 무관하게 걸리는 시간은 언제나 이만큼이다 — " +
             "자손이 많으면 한 프레임에 여러 개씩, 적으면 몇 프레임에 하나씩 사라진다.")]
    [SerializeField, Min(1)] int _frames = 60;

    [Tooltip("지우는 순서를 무작위로 섞는다. 켜면 계층 순서 대신 전체 영역에서 고르게 흩어지며 사라진다.")]
    [SerializeField] bool _randomize = true;

    [Tooltip("고정 시드를 사용할지 여부. 켜면 지정한 시드값에 따라 매번 동일한 무작위 패턴으로 사라집니다.")]
    [SerializeField] bool _useFixedSeed = false;

    [Tooltip("고정 시드 값.")]
    [SerializeField] int _seed = 0;

    /// <summary>지울 순서.</summary>
    readonly List<Transform> _order = new List<Transform>();

    Coroutine _sweep;

    /// <summary>흩어 없애기를 시작한다. 이미 진행 중이면 아무 일도 하지 않는다.</summary>
    public void Begin()
    {
        if (_sweep != null) return;

        _sweep = StartCoroutine(Sweep());
    }

    /// <summary>
    /// 코루틴은 오브젝트가 꺼지면서 함께 멎지만 핸들은 남는다. 지워주지 않으면
    /// 다시 켠 뒤 <see cref="Begin"/>이 "진행 중"으로 잘못 읽고 영영 시작하지 못한다.
    /// </summary>
    void OnDisable() => _sweep = null;

    IEnumerator Sweep()
    {
        _order.Clear();
        Collect(transform);

        int total = _order.Count;
        int done = 0;

        for (int frame = 1; frame <= _frames; frame++)
        {
            // 몫(total / _frames)을 프레임마다 똑같이 지우면 나머지가 통째로 남는다 —
            // 100개를 60프레임에 나누면 프레임당 1개씩, 40개가 그대로 살아남는다.
            // 대신 "이 프레임까지 지워져 있어야 할 누적 개수"를 잡으면 나머지가 자연히
            // 흩어지고, 마지막 프레임의 끝은 정확히 total이 된다.
            int upTo = (int)((long)total * frame / _frames);

            while (done < upTo) Erase(_order[done++]);

            if (frame < _frames) yield return null;
        }

        _order.Clear();
        _sweep = null;
    }

    /// <summary>
    /// 자손들을 수집한다. 무작위 모드일 때는 자식들을 뿌리로 평탄화한 뒤 셔플한다.
    /// </summary>
    void Collect(Transform root)
    {
        if (_randomize)
        {
            Transform[] all = root.GetComponentsInChildren<Transform>(true);
            _order.Capacity = Mathf.Max(_order.Capacity, all.Length);

            // 자손들을 뿌리로 끌어올려 독립적인 잎으로 만든다 (중간 부모 삭제 시 자식 일괄 삭제 방지)
            for (int i = 1; i < all.Length; i++)
            {
                Transform t = all[i];
                if (t != null && t != root)
                {
                    t.SetParent(root, true);
                    _order.Add(t);
                }
            }

            // Fisher-Yates 셔플 (고정 시드가 켜져 있으면 결정론적으로 재현)
            System.Random rng = _useFixedSeed ? new System.Random(_seed) : new System.Random();
            for (int i = _order.Count - 1; i > 0; i--)
            {
                int j = rng.Next(i + 1);
                Transform temp = _order[i];
                _order[i] = _order[j];
                _order[j] = temp;
            }
        }
        else
        {
            CollectPostOrder(root);
        }
    }

    /// <summary>
    /// 자손을 후위 순회로 담는다. 뿌리 자신은 담지 않는다.
    /// </summary>
    void CollectPostOrder(Transform node)
    {
        foreach (Transform child in node)
        {
            CollectPostOrder(child);
            _order.Add(child);
        }
    }

    /// <summary>
    /// 밖에서 먼저 지워졌을 수 있다. <see cref="Object.Destroy"/>는 프레임 끝에야 실제로
    /// 치우므로 목록을 만든 뒤에도 계층은 얼마든지 바뀔 수 있고, 그때마다 이 자리는
    /// 빈 칸이 된다. 빈 칸도 제 몫의 순서를 쓰고 지나간다.
    /// </summary>
    void Erase(Transform node)
    {
        if (node) Destroy(node.gameObject);
    }
}
