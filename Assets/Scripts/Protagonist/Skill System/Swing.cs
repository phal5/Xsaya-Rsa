using UnityEngine;

/// <summary>
/// 한 번의 휘두름. 클립 하나와 그 안의 <b>세 지점</b>으로 이루어진다.
///
/// 길이를 프레임으로 적는 것은 <see cref="CharacterManager.LedgeClip"/>과 같은 이유다.
/// 클립을 프레임 단위로 들여다보며 고르므로, 초로 환산해 적어두면 볼 때마다 다시 나눠야 한다.
///
/// 여기 적는 프레임은 <b>클립의 프레임</b>이지 벽시계가 아니다. 배속으로 트는 동안에도
/// 이 숫자들은 그대로 맞는다 — 배속은 같은 구간을 더 빨리 지나가게 할 뿐이다.
///
/// <see cref="start"/>와 <see cref="end"/>로 클립의 가운데만 남긴다. 믹사모 공격 클립은
/// 앞뒤가 대기 자세를 오가는 데 쓰여 실제 베는 구간이 3분의 1도 되지 않는데,
/// 그 앞뒤가 곧 "누르고 기다리는 시간"이다. 배속만 올려 해결하려 들면 베는 순간까지 뭉개진다.
///
/// <b>양 끝은 세우고 가운데만 뭉갠다.</b> 고르게 빠른 동작은 아무 자세도 서지 않아
/// 짧을수록 "뭔가 움찔했다"로만 읽힌다. 시작 자세를 몇 프레임 세워 무엇을 하려는지 보이고,
/// 사이를 지나치게 빠르게 지나가고, 끝 자세를 세워 무엇을 했는지 보인다.
/// 눈에 남는 것은 지나가는 도중이 아니라 서 있는 두 자세다.
///
/// 끝 자세를 얼마나 세울지는 여기 없다. <b>쿨다운이 끝날 때까지</b> 세운다 —
/// 그래야 "이 스킬은 0.5초에 한 번"이 화면에서도 그대로 보인다.
/// </summary>
[System.Serializable]
public struct Swing
{
    [Tooltip("컨트롤러의 상태 이름.")]
    public string state;

    [Tooltip("클립의 이 프레임부터 튼다. 앞의 준비 동작을 잘라내는 자리다.")]
    [Min(0f)] public float start;

    [Tooltip("이 프레임에 무기 판정을 연다.")]
    [Min(0f)] public float hitOpen;

    [Tooltip("이 프레임에 무기 판정을 닫는다. end보다 크게 두면 동작이 끝날 때까지 열려 있다.")]
    [Min(0f)] public float hitClose;

    [Tooltip("이 프레임에서 동작을 끝내고 이동으로 돌아간다.")]
    [Min(0f)] public float end;

    [Tooltip("시작 자세를 이만큼(프레임) 세워 두고 나서 움직인다.")]
    [Min(0f)] public float holdStart;

    public bool IsSet => !string.IsNullOrEmpty(state);
}
