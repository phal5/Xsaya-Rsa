using System.Collections.Generic;
using System.Text;
using UnityEngine;

/// <summary>
/// <b>임시 진단용.</b> 원인을 잡으면 이 파일을 지운다.
///
/// 턱에 붙는 <b>순간부터</b> 기록한다. 떨어지는 속도로 잡으려던 앞의 판은 틀렸다 —
/// 자유낙하가 한 스텝에 눈에 띌 만큼 커지려면 몇 초가 걸리고, 그때쯤이면 정작 봐야 할
/// 붙잡는 순간이 버퍼에서 밀려나 있다. 사건은 낙하가 아니라 <b>붙잡기</b>다.
///
/// 씬 전환의 순간이동은 세지 않는다 — 그건 director가 자리를 옮기는 것이지 떨어지는 것이 아니다.
/// </summary>
public class LedgeDropTrace : MonoBehaviour
{
    /// <summary>사건이 잡힌 뒤 이만큼 더 담고 쏟는다.</summary>
    const int Tail = 200;

    /// <summary>
    /// 붙잡은 높이보다 이만큼 아래로 내려가면 사건으로 본다.
    ///
    /// <b>속도로 보면 안 된다.</b> 붙어 있는 동안 두 몸은 키네마틱이라 linearVelocity가 늘 0이다 —
    /// Pin이 위치를 직접 쓰므로, 가라앉아도 속도에는 아무것도 나타나지 않는다.
    /// 실제로 그 판으로 한 번 놓쳤다.
    ///
    /// 붙잡고 나서 자리를 모아 가는 동안 조금 내려가는 것은 정상이라(실측 1.982 → 1.939),
    /// 그보다 넉넉히 크게 잡는다.
    /// </summary>
    const float SinkDepth = 0.35f;

    /// <summary>붙기 전 얼마나 거슬러 남길지. 대시가 들어오는 구간을 담아야 한다.</summary>
    const int Lead = 60;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Install()
    {
        GameObject go = new GameObject("~LedgeDropTrace");
        DontDestroyOnLoad(go);
        go.AddComponent<LedgeDropTrace>();
    }

    readonly Queue<string> _lead = new Queue<string>();
    readonly List<string> _episode = new List<string>();

    CharacterManager _character;
    CharacterRoot _root;
    Character_Ledge _ledge;
    Rigidbody _body;
    Rigidbody _external;

    Vector3 _last;
    bool _hasLast;

    bool _recording;
    int _tail;
    int _episodes;
    string _path;

    /// <summary>이번 episode에서 가라앉는 것이 이미 잡혔는지. 표시는 한 번만 남긴다.</summary>
    bool _sinking;

    /// <summary>붙잡은 순간의 높이. 여기서 얼마나 내려갔는지로 판정한다.</summary>
    float _caughtY;
    bool _caught;

    // 한 프레임에 FixedUpdate가 몇 번 도는지. 프레임 드랍의 실제 크기가 이것이다.
    int _frame = -1;
    int _stepsThisFrame;

    void FixedUpdate()
    {
        if (!Ready()) return;

        if (Time.frameCount != _frame) { _frame = Time.frameCount; _stepsThisFrame = 1; }
        else _stepsThisFrame++;

        string path = StatePath();
        bool onLedge = path.Contains("Ledge");

        string line = Line(path);

        if (!_recording)
        {
            _lead.Enqueue(line);
            while (_lead.Count > Lead) _lead.Dequeue();

            if (onLedge) Begin();
        }
        else
        {
            _episode.Add(line);

            // 붙잡은 높이를 기록해 둔다. 그 높이가 이 episode의 기준이 된다.
            if (onLedge && !_caught) { _caught = true; _caughtY = _body.position.y; }

            // <b>매달린 채 내려간다</b> — 있어서는 안 되는 일이고, 이 순간이 곧 원인이 선 자리다.
            // 놓기는 내려가는 것이 정상이다. 그것까지 세면 매번 오탐이 뜬다.
            bool releasing = path.Contains("Ledge_Release");

            if (onLedge && !releasing && _caught && !_sinking && _body.position.y < _caughtY - SinkDepth)
            {
                _sinking = true;
                _episode.Add("        ^^^^^^ 매달린 채 " + (_caughtY - _body.position.y).ToString("F3")
                             + "m 내려왔다 (붙잡은 높이 " + _caughtY.ToString("F3") + ") ^^^^^^");
                _tail = Tail;
            }

            if (_sinking) { if (--_tail <= 0) End(); }
            else if (onLedge) _tail = Tail;
            else if (--_tail <= 0) End();
        }

        _last = _body.position;
        _hasLast = true;
    }

    void Begin()
    {
        _recording = true;
        _tail = Tail;
        _sinking = false;
        _caught = false;

        _episode.Clear();
        _episode.AddRange(_lead);
        _lead.Clear();
    }

    void End()
    {
        _recording = false;
        _episodes++;

        StringBuilder sb = new StringBuilder();
        sb.AppendLine();
        sb.AppendLine("=========== 턱 episode " + _episodes + " (" + _episode.Count + "스텝) "
                    + (_sinking ? "★ 가라앉음 잡힘" : "정상") + " ===========");
        foreach (string l in _episode) sb.AppendLine(l);

        System.IO.File.AppendAllText(_path, sb.ToString());
        Debug.LogWarning($"[LedgeDropTrace] episode {_episodes}: {_episode.Count}줄 → {_path}");

        _episode.Clear();
    }

    /// <summary>
    /// 판이 끝날 때 담고 있던 것을 쏟는다.
    ///
    /// 이게 없으면 <b>사건 한가운데서 정지한 판의 기록이 통째로 사라진다</b> —
    /// End()는 턱을 떠나고 한참 뒤에야 파일에 쓰기 때문이다. 실제로 그렇게 한 번 놓쳤다.
    /// </summary>
    void OnApplicationQuit()
    {
        if (_recording && _episode.Count > 0) End();
    }

    string Line(string path)
    {
        Vector3 p = _body.position;
        float step = _hasLast ? p.y - _last.y : 0f;

        StringBuilder sb = new StringBuilder();

        sb.Append("f").Append(Time.frameCount).Append("#").Append(_stepsThisFrame)
          .Append(" t=").Append(Time.fixedTime.ToString("F3"))
          .Append(" | rb ").Append(p.ToString("F3"))
          .Append(" tr ").Append(_body.transform.position.ToString("F3"))
          .Append(" dy ").Append(step.ToString("F3"))
          .Append(" | v ").Append(_body.linearVelocity.ToString("F2"));

        if (_external != null)
            sb.Append(" | ext ").Append(_external.position.ToString("F3"))
              .Append(" vExt ").Append(_external.linearVelocity.ToString("F2"))
              .Append(" grav ").Append(_external.useGravity);

        sb.Append(" | kin ").Append(_body.isKinematic)
          .Append("/").Append(_external == null ? "-" : _external.isKinematic.ToString());

        // <b>붙잡은 뒤 목적지가 어디로 잡혀 있는지.</b> 이것이 아래로 잡혀 있으면 측정이 원인이고,
        // 위에 그대로 있으면 원인은 늘리기 배율 — 즉 어느 클립의 travel을 읽었느냐다.
        if (_ledge != null)
            sb.Append(" | hang ").Append(_ledge.Anchor.hang.y.ToString("F3"))
              .Append(" stand ").Append(_ledge.Anchor.stand.y.ToString("F3"))
              .Append(" (stand-rb ").Append((_ledge.Anchor.stand.y - p.y).ToString("F3")).Append(")");

        sb.Append(" | ").Append(path);

        return sb.ToString();
    }

    /// <summary>루트부터 안쪽까지의 상태 이름. 어느 축의 어느 상태인지가 요점이다.</summary>
    string StatePath()
    {
        if (_root == null) return "루트 없음";

        StringBuilder sb = new StringBuilder();
        FiniteStateMachine machine = _root;

        for (int depth = 0; depth < 5; depth++)
        {
            System.Type type = machine._currentStateType;
            if (type == null) break;

            if (depth > 0) sb.Append(" > ");
            sb.Append(type.Name);

            FiniteStateMachine next = null;
            foreach (FiniteStateMachine m in _character.transform.root.GetComponentsInChildren<FiniteStateMachine>(true))
                if (m.GetType() == type) { next = m; break; }

            if (next == null) break;
            machine = next;
        }

        return sb.ToString();
    }

    bool Ready()
    {
        if (_body != null) return true;

        if (PlayerManager.instance == null || PlayerManager.instance.player == null) return false;

        _character = PlayerManager.instance.player.root.GetComponentInChildren<CharacterManager>(true);
        if (_character == null || _character.Body == null) return false;

        _root = PlayerManager.instance.Root;
        _body = _character.Body;
        _ledge = _character.transform.root.GetComponentInChildren<Character_Ledge>(true);

        foreach (Rigidbody rb in _character.transform.root.GetComponentsInChildren<Rigidbody>(true))
            if (rb != _body) _external = rb;

        _path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "ledge-drop-trace3.txt");
        Debug.Log($"[LedgeDropTrace] 준비됨 → {_path}");

        return true;
    }
}
