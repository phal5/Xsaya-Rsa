using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

/// <summary>
/// 스킬이 들고 있는 클립을 보스의 애니메이터 컨트롤러에 반영한다.
///
/// 이 컨트롤러의 관례는 하나뿐이다 — <b>상태 이름 = 트리거 이름</b>, 그리고 AnyState에서 그 트리거로 들어온다.
/// 그래서 스킬 하나가 필요로 하는 것도 셋(트리거 · 상태 · 전이)뿐이고, 여기서 없는 것만 만든다.
///
/// <para>
/// <b>손으로 맞춘 것을 지우지 않는 것이 이 도구의 유일한 계약이다.</b>
/// 스킬이 Animator Trigger로 <i>이름을 댄</i> 상태만 건드리고, 그 상태에서도 모션만 바꾼다.
/// 배속·오프셋·전이 설정은 읽지도 쓰지도 않는다. 이미 있는 AnyState 전이는 새로 만들지 않는다.
/// 그래서 JumpLaunch / JumpSlam처럼 어떤 스킬도 이름을 대지 않는 상태에는 닿을 수 없다 —
/// 도약 4단계의 SlamSpeed와 25프레임 오프셋은 이 도구를 몇 번 돌리든 그대로 남는다.
/// </para>
///
/// OnValidate에 걸지 않은 이유: OnValidate는 인스펙터를 건드릴 때마다, 도메인 리로드마다,
/// 프리팹 임포트마다 돈다. 거기서 공유 애셋인 컨트롤러에 쓰기 시작하면 애셋이 끝없이 더러워지고
/// 머지 충돌이 난다. 그래서 사람이 누르는 버튼으로 둔다.
/// </summary>
public static class BossSkillAnimatorSync
{
    /// <summary>새로 만드는 전이의 길이. 컨트롤러의 나머지 전이와 같은 값이다.</summary>
    const float TransitionDuration = 0.12f;

    public static void Sync(BossManager boss)
    {
        AnimatorController controller = ResolveController(boss, out string problem);
        if (controller == null)
        {
            Debug.LogError($"[{boss.name}] 동기화할 수 없습니다 — {problem}", boss);
            return;
        }

        if (controller.layers.Length == 0)
        {
            Debug.LogError($"[{boss.name}] 컨트롤러에 레이어가 없습니다.", controller);
            return;
        }

        AnimatorStateMachine machine = controller.layers[0].stateMachine;
        Undo.RegisterCompleteObjectUndo(controller, "스킬 애니메이션 동기화");

        StringBuilder log = new StringBuilder();
        HashSet<string> claimed = new HashSet<string>();
        int changes = 0;

        foreach (Boss_SkillBase skill in boss.GetComponentsInChildren<Boss_SkillBase>(true))
        {
            if (skill == null) continue;

            string label = skill.GetType().Name;
            string stateName = skill.Profile.animatorTrigger;

            if (skill.Clip == null)
            {
                log.AppendLine($"  · {label} — 클립이 비어 있어 건너뜁니다 (손 배선).");
                continue;
            }

            if (string.IsNullOrEmpty(stateName))
            {
                Debug.LogError($"[{label}] Clip은 들어 있는데 Animator Trigger가 비어 있어 " +
                               $"상태 이름을 정할 수 없습니다.", skill);
                continue;
            }

            // 두 스킬이 같은 이름을 대면 나중 것이 앞의 클립을 덮는다. 덮기 전에 멈춘다.
            if (!claimed.Add(stateName))
            {
                Debug.LogError($"[{label}] 상태 이름 '{stateName}'을 다른 스킬이 이미 쓰고 있습니다.", skill);
                continue;
            }

            changes += SyncSkill(controller, machine, stateName, skill.Clip, label, log, boss.skillSpeedParameter);
        }

        if (changes > 0)
        {
            EditorUtility.SetDirty(controller);
            AssetDatabase.SaveAssets();
        }

        Debug.Log($"[{boss.name}] 스킬 애니메이션 동기화 — {controller.name}, 변경 {changes}건\n{log}", boss);
    }

    static int SyncSkill(AnimatorController controller, AnimatorStateMachine machine,
                         string stateName, AnimationClip clip, string label, StringBuilder log,
                         string speedParameter)
    {
        int changes = 0;
        StringBuilder detail = new StringBuilder();

        if (AddTriggerIfMissing(controller, stateName, label))
        {
            detail.AppendLine($"      + 트리거 '{stateName}'");
            changes++;
        }

        AnimatorState state = FindState(machine, stateName);
        if (state == null)
        {
            state = machine.AddState(stateName);
            detail.AppendLine($"      + 상태 '{stateName}'");
            changes++;
        }

        // 배속은 스킬이 정한다. 상태는 그 값을 받아만 준다.
        if (!string.IsNullOrEmpty(speedParameter) && state.speedParameter != speedParameter)
        {
            AddFloatIfMissing(controller, speedParameter);
            state.speedParameterActive = true;
            state.speedParameter = speedParameter;
            detail.AppendLine($"      ~ 배속 파라미터 → [{speedParameter}]");
            changes++;
        }

        // 이 상태에서 우리가 쓰는 것은 모션 하나뿐이다.
        if (state.motion != clip)
        {
            string before = state.motion == null ? "<없음>" : state.motion.name;
            state.motion = clip;
            detail.AppendLine($"      ~ 클립  {before}  →  {clip.name}");
            changes++;
        }

        if (AddAnyStateTransitionIfMissing(machine, state, stateName))
        {
            detail.AppendLine($"      + AnyState → '{stateName}' 전이");
            changes++;
        }

        if (changes == 0) log.AppendLine($"  · {label} → '{stateName}' — 이미 맞습니다.");
        else log.AppendLine($"  ▶ {label} → '{stateName}'\n{detail.ToString().TrimEnd()}");

        return changes;
    }

    #region Controller Pieces

    static bool AddTriggerIfMissing(AnimatorController controller, string name, string label)
    {
        foreach (AnimatorControllerParameter parameter in controller.parameters)
        {
            if (parameter.name != name) continue;

            if (parameter.type != AnimatorControllerParameterType.Trigger)
                Debug.LogWarning($"[{label}] 파라미터 '{name}'이 Trigger가 아니라 {parameter.type}입니다. " +
                                 $"SetTrigger가 먹지 않아 이 스킬의 애니메이션이 재생되지 않습니다.", controller);

            return false;
        }

        controller.AddParameter(name, AnimatorControllerParameterType.Trigger);
        return true;
    }

    /// <summary>배속 파라미터. 없으면 기본값 1로 만든다 — 0으로 태어나면 스킬이 멈춘 채 나온다.</summary>
    static void AddFloatIfMissing(AnimatorController controller, string name)
    {
        foreach (AnimatorControllerParameter parameter in controller.parameters)
            if (parameter.name == name) return;

        controller.AddParameter(new AnimatorControllerParameter
        {
            name = name,
            type = AnimatorControllerParameterType.Float,
            defaultFloat = 1f,
        });
    }

    static AnimatorState FindState(AnimatorStateMachine machine, string name)
    {
        foreach (ChildAnimatorState child in machine.states)
            if (child.state != null && child.state.name == name) return child.state;

        return null;
    }

    /// <summary>
    /// 이미 이 상태로 들어오는 AnyState 전이가 있으면 그대로 둔다.
    /// 조건이 무엇이든 손으로 맞춰둔 것일 수 있으므로 손대지 않는다.
    /// </summary>
    static bool AddAnyStateTransitionIfMissing(AnimatorStateMachine machine, AnimatorState state, string trigger)
    {
        foreach (AnimatorStateTransition existing in machine.anyStateTransitions)
            if (existing.destinationState == state) return false;

        AnimatorStateTransition transition = machine.AddAnyStateTransition(state);

        transition.hasExitTime = false;
        transition.hasFixedDuration = true;
        transition.duration = TransitionDuration;
        transition.offset = 0f;
        transition.canTransitionToSelf = false;   // 켜두면 트리거 한 번에 자기 자신으로 되감긴다
        transition.interruptionSource = TransitionInterruptionSource.None;
        transition.AddCondition(AnimatorConditionMode.If, 0f, trigger);

        return true;
    }

    #endregion

    /// <summary>오버라이드 컨트롤러가 물려 있어도 원본까지 따라 내려간다.</summary>
    static AnimatorController ResolveController(BossManager boss, out string problem)
    {
        problem = null;

        if (boss.animator == null)
        {
            problem = "BossManager의 Animator 칸이 비어 있습니다.";
            return null;
        }

        RuntimeAnimatorController runtime = boss.animator.runtimeAnimatorController;
        while (runtime is AnimatorOverrideController over) runtime = over.runtimeAnimatorController;

        AnimatorController controller = runtime as AnimatorController;
        if (controller == null) problem = "Animator에 AnimatorController가 물려 있지 않습니다.";

        return controller;
    }
}

/// <summary>기본 인스펙터 아래에 동기화 버튼만 붙인다.</summary>
[CustomEditor(typeof(BossManager))]
public class BossManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        EditorGUILayout.Space(10f);

        if (GUILayout.Button("스킬 애니메이션 동기화", GUILayout.Height(26f)))
            BossSkillAnimatorSync.Sync((BossManager)target);

        EditorGUILayout.HelpBox(
            "스킬에 넣은 Clip을 애니메이터에 반영합니다.\n" +
            "트리거 · 상태 · AnyState 전이 중 없는 것만 만들고, 이미 있는 상태는 클립만 바꿉니다. " +
            "손으로 맞춘 배속 · 오프셋 · 전이 설정은 건드리지 않습니다.",
            MessageType.None);
    }
}
