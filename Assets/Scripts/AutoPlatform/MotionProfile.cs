using UnityEngine;

/// <summary>
/// 캐릭터의 이동 성능을 레벨 디자인이 읽을 수 있는 수치로 옮긴 것.
///
/// 값을 여기에 따로 적어두지 않는다. <see cref="ReadFrom"/>로 CharacterManager에서 그대로 긁어온다 —
/// 튜닝이 한 곳에서만 일어나야 배치와 실제 움직임이 어긋나지 않는다.
/// 인스펙터의 필드는 캐릭터가 아직 씬에 없을 때 쓰는 기본값이자, 지금 무엇으로 계산 중인지 보여주는 창이다.
/// </summary>
[System.Serializable]
public class MotionProfile
{
    [Header("이동")]
    public float groundSpeed = 7.5f;
    public float airborneSpeed = 5f;
    public float jumpSpeed = 5f;

    [Header("공중 대시")]
    public float dashSpeed = 10f;
    public float dashTime = 0.2f;

    [Header("턱 잡기")]
    public float ledgeReachHigh = 1.85f;
    public float ledgeReachLow = 1.5f;
    public float ledgeReach = 0.4f;

    [Header("몸")]
    public float characterHeight = 1.5f;
    public float characterRadius = 0.25f;

    [Header("환경")]
    public float gravity = 9.81f;

    /// <summary>도약에서 정점까지 걸리는 시간.</summary>
    public float RiseTime => jumpSpeed / Mathf.Max(gravity, 0.01f);

    /// <summary>발밑 기준 정점 높이. 대시는 수평이라 이 값을 넘기지 못한다.</summary>
    public float ApexHeight => jumpSpeed * jumpSpeed / (2f * Mathf.Max(gravity, 0.01f));

    /// <summary>대시 한 장이 벌어주는 수평 거리. 중력이 꺼져 있어 궤도와 무관하게 일정하다.</summary>
    public float DashDistance => dashSpeed * dashTime;

    /// <summary>
    /// 발로 오를 수 있는 최대 높이. 지상 점프의 정점에서 공중 점프를 한 번 더 쓴 것이다 —
    /// 점프는 세로 속도를 <b>덮어쓰므로</b>, 높이만 놓고 보면 정점에서 거는 것이 최선이다.
    /// (거리를 놓고 보면 정반대로 가장 낮은 지점이 최선인데, 그 둘은 같이 얻지 못한다.)
    /// </summary>
    public float MaxClimb => ApexHeight * 2f;

    /// <summary>턱까지 써서 오를 수 있는 최대 높이.</summary>
    public float LedgeCeiling => MaxClimb + ledgeReachHigh;

    /// <summary>
    /// 도달 판정 캐시를 무르는 열쇠. 값이 하나라도 바뀌면 달라진다.
    ///
    /// 궤적 최적화는 프로파일마다 다시 풀어야 하는데, 튜닝은 자주 일어나고 계산은 싸지 않다.
    /// 값 자체로 열쇠를 만들면 "다시 계산하라"고 따로 일러줄 것이 없다.
    /// </summary>
    public long Signature
    {
        get
        {
            unchecked
            {
                long hash = 17;
                hash = hash * 31 + Mathf.RoundToInt(groundSpeed * 1000f);
                hash = hash * 31 + Mathf.RoundToInt(airborneSpeed * 1000f);
                hash = hash * 31 + Mathf.RoundToInt(jumpSpeed * 1000f);
                hash = hash * 31 + Mathf.RoundToInt(dashSpeed * 1000f);
                hash = hash * 31 + Mathf.RoundToInt(dashTime * 1000f);
                hash = hash * 31 + Mathf.RoundToInt(ledgeReachHigh * 1000f);
                hash = hash * 31 + Mathf.RoundToInt(ledgeReach * 1000f);
                hash = hash * 31 + Mathf.RoundToInt(gravity * 1000f);
                return hash;
            }
        }
    }

    /// <summary>
    /// 씬의 캐릭터에서 실제 값을 읽어온다. 하나라도 못 읽으면 건드리지 않고 false를 돌려준다 —
    /// 반쯤 갱신된 프로파일로 배치를 판정하면 어긋난 것을 알아채기 어렵다.
    /// </summary>
    public bool ReadFrom(CharacterManager character)
    {
        if (character == null) return false;

        groundSpeed = character.GroundSpeed;
        airborneSpeed = character.AirborneSpeed;
        jumpSpeed = character.JumpSpeed;
        dashSpeed = character.DashSpeed;
        dashTime = character.DashTime;

        if (character.Ledge != null)
        {
            ledgeReachHigh = character.Ledge.reachHigh;
            ledgeReachLow = character.Ledge.reachLow;
            ledgeReach = character.Ledge.reach;
        }

        if (character.CapsuleCollider != null)
        {
            characterHeight = character.CharacterHeight;
            characterRadius = character.CapsuleCollider.radius
                * character.CapsuleCollider.transform.lossyScale.x;
        }

        gravity = Mathf.Abs(Physics.gravity.y);

        return true;
    }
}
