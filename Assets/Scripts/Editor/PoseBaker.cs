using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

/// <summary>
/// 자세를 잡아 둔 프리팹을 <b>한 프레임짜리 휴머노이드 클립</b>으로 굽고, 컨트롤러 상태에 물린다.
/// 누운 자세(Laid)와 대시 자세(Dash)가 이 한 길을 함께 쓴다. 자세가 늘면 아래 목록에 한 줄, 메뉴에 한 줄을 더한다.
///
/// 뼈에 자세를 직접 써 넣지 않는 이유. 애니메이터는 평가할 때마다 스켈레톤 전체를 덮어쓰므로,
/// 밖에서 얹은 자세는 다음 평가에 지워진다. 클립으로 구워 컨트롤러 상태에 넣으면
/// <b>자세의 주인이 계속 애니메이터 하나</b>이고, 다른 동작으로 넘어가는 것도 그냥 CrossFade다.
///
/// 트랜스폼 회전을 그대로 굽지 않는 이유. 주인공 아바타는 Humanoid라 트랜스폼 커브를
/// 리타기팅하지 않는다. 그래서 <see cref="HumanPoseHandler"/>로 자세를 <b>근육 값</b>으로 바꿔 굽는다.
///
/// <b>굽고 나서 스스로 검산한다.</b> 근육 커브의 이름은 유니티 버전에 따라 어긋난 적이 있어,
/// 구운 클립을 도로 샘플링해 원본 프리팹과 뼈 각도를 견주고 최대 오차를 찍는다.
/// 이름이 어긋났다면 그 뼈에서 큰 각도로 드러난다.
/// </summary>
public static class PoseBaker
{
    /// <summary>굽는 한 건. 어느 프리팹의 자세를, 어느 클립으로, 컨트롤러의 어느 상태에 물릴지.</summary>
    readonly struct Pose
    {
        /// <summary>자세를 잡아 둔 프리팹. 주인공과 뼈 이름이 같은 리그여야 한다.</summary>
        public readonly string source;

        /// <summary>구워 낼 클립.</summary>
        public readonly string output;

        /// <summary>물릴 컨트롤러 상태. CharacterManager에 적힌 같은 이름과 맞아야 한다.</summary>
        public readonly string state;

        public Pose(string source, string output, string state)
        {
            this.source = source;
            this.output = output;
            this.state = state;
        }
    }

    /// <summary>기상 전 누운 자세. CharacterManager.LaidState.</summary>
    static readonly Pose Laid = new Pose(
        "Assets/Prefabs/Props/character-laid.prefab",
        "Assets/Art/Animation/Altar/Laid.anim",
        "Laid");

    /// <summary>회피 동안의 자세. CharacterManager.DashState. 지상·공중이 함께 쓴다.</summary>
    static readonly Pose Dash = new Pose(
        "Assets/Prefabs/Props/character-dash.prefab",
        "Assets/Art/Animation/Dash/Dash.anim",
        "Dash");

    const string ProtagonistPath = "Assets/Prefabs/Props/Protagonist.prefab";
    const string ControllerPath = "Assets/Art/Animation/Protagonist.controller";

    /// <summary>구운 클립이 한 프레임만 있어도 길이가 0이 되지 않게 두 키를 이 간격으로 둔다.</summary>
    const float FrameRate = 30f;

    /// <summary>검산에서 이 각도를 넘으면 뼈가 어긋난 것으로 본다.</summary>
    const float ErrorTolerance = 1f;

    [MenuItem("Tools/Xsaya/누운 자세를 클립으로 굽기")]
    static void BakeLaid() => Bake(Laid);

    [MenuItem("Tools/Xsaya/대시 자세를 클립으로 굽기")]
    static void BakeDash() => Bake(Dash);

    static void Bake(Pose pose)
    {
        string tag = $"[PoseBaker:{pose.state}]";

        GameObject sourceAsset = AssetDatabase.LoadAssetAtPath<GameObject>(pose.source);
        GameObject protagonistAsset = AssetDatabase.LoadAssetAtPath<GameObject>(ProtagonistPath);

        if (sourceAsset == null)
        {
            // 처음 굽는 자세라면 원본이 아직 없는 게 보통이다. 무엇을 하면 되는지까지 적어 준다.
            Debug.LogError($"{tag} {pose.source} 를 찾지 못했습니다. " +
                           "character-laid.prefab 을 복제해 이 이름으로 저장하고, 뼈를 돌려 자세를 잡은 뒤 다시 굽으세요.");
            return;
        }

        if (protagonistAsset == null) { Debug.LogError($"{tag} {ProtagonistPath} 를 찾지 못했습니다."); return; }

        GameObject rig = null;
        GameObject source = null;

        try
        {
            rig = (GameObject)PrefabUtility.InstantiatePrefab(protagonistAsset);
            source = (GameObject)PrefabUtility.InstantiatePrefab(sourceAsset);

            Animator animator = rig.GetComponentInChildren<Animator>(true);

            if (animator == null || animator.avatar == null || !animator.avatar.isHuman)
            {
                Debug.LogError($"{tag} 주인공에게 휴머노이드 아바타가 없습니다.");
                return;
            }

            // 애니메이터가 도는 동안에는 우리가 얹은 자세가 바로 덮인다. 재는 동안만 세워 둔다.
            animator.enabled = false;

            StringBuilder report = new StringBuilder();

            int copied = Apply(source.transform, animator.transform, report);

            if (copied == 0)
            {
                Debug.LogError($"{tag} 이름이 맞는 뼈가 하나도 없습니다. 두 리그가 다른 것 같습니다.");
                return;
            }

            AnimationClip clip = Capture(animator);

            Write(clip, pose.output);

            report.Insert(0, $"{tag} 뼈 {copied}개를 옮겨 {pose.output} 로 구웠습니다.\n");

            Wire(pose, report);

            Verify(pose, animator, source.transform, report);

            Debug.Log(report.ToString());
        }
        finally
        {
            if (source != null) Object.DestroyImmediate(source);
            if (rig != null) Object.DestroyImmediate(rig);
        }
    }

    /// <summary>
    /// 자세 프리팹의 자세를 리그에 얹는다. <b>회전만 옮긴다.</b>
    ///
    /// 위치는 리그의 본디 값이라 두 프리팹이 같아야 하고, 다르다면 그건 자세가 아니라
    /// 리그가 어긋난 것이다. 그래서 옮기지 않고 <b>다르면 보고만</b> 한다 —
    /// 조용히 덮으면 어긋난 리그를 어긋난 채로 구워 넣게 된다.
    /// </summary>
    static int Apply(Transform source, Transform target, StringBuilder report)
    {
        Dictionary<string, Transform> pose = new Dictionary<string, Transform>();

        foreach (Transform t in source.GetComponentsInChildren<Transform>(true))
            pose[t.name] = t;

        int copied = 0;
        int drifted = 0;

        foreach (Transform bone in target.GetComponentsInChildren<Transform>(true))
        {
            if (!pose.TryGetValue(bone.name, out Transform from)) continue;

            // 아바타 루트는 자세가 아니라 자리다. 여기를 돌리면 몸 전체가 함께 돈다.
            if (bone == target) continue;

            if ((from.localPosition - bone.localPosition).sqrMagnitude > 0.000001f)
            {
                drifted++;
                if (drifted <= 5)
                    report.AppendLine($"    위치가 다른 뼈: {bone.name}  자세 {from.localPosition:F4} / 리그 {bone.localPosition:F4}");
            }

            bone.localRotation = from.localRotation;
            copied++;
        }

        if (drifted > 0) report.AppendLine($"    위치가 다른 뼈 {drifted}개 (옮기지 않았습니다)");

        return copied;
    }

    /// <summary>얹어둔 자세를 근육 값으로 읽어 한 프레임짜리 클립으로 만든다.</summary>
    static AnimationClip Capture(Animator animator)
    {
        HumanPose pose = new HumanPose();

        HumanPoseHandler handler = new HumanPoseHandler(animator.avatar, animator.transform);
        handler.GetHumanPose(ref pose);
        handler.Dispose();

        AnimationClip clip = new AnimationClip { frameRate = FrameRate };

        Constant(clip, "RootT.x", pose.bodyPosition.x);
        Constant(clip, "RootT.y", pose.bodyPosition.y);
        Constant(clip, "RootT.z", pose.bodyPosition.z);

        Constant(clip, "RootQ.x", pose.bodyRotation.x);
        Constant(clip, "RootQ.y", pose.bodyRotation.y);
        Constant(clip, "RootQ.z", pose.bodyRotation.z);
        Constant(clip, "RootQ.w", pose.bodyRotation.w);

        for (int i = 0; i < HumanTrait.MuscleCount && i < pose.muscles.Length; i++)
            Constant(clip, HumanTrait.MuscleName[i], pose.muscles[i]);

        return clip;
    }

    /// <summary>한 값으로 고정된 커브 하나. 키가 둘이라야 길이가 0이 되지 않는다.</summary>
    static void Constant(AnimationClip clip, string property, float value)
    {
        EditorCurveBinding binding = EditorCurveBinding.FloatCurve(string.Empty, typeof(Animator), property);

        AnimationUtility.SetEditorCurve(clip, binding, AnimationCurve.Linear(0f, value, 1f / FrameRate, value));
    }

    static void Write(AnimationClip clip, string path)
    {
        string folder = System.IO.Path.GetDirectoryName(path).Replace('\\', '/');

        if (!AssetDatabase.IsValidFolder(folder))
        {
            string parent = System.IO.Path.GetDirectoryName(folder).Replace('\\', '/');
            AssetDatabase.CreateFolder(parent, System.IO.Path.GetFileName(folder));
        }

        AnimationClip existing = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);

        if (existing != null)
        {
            // 덮어쓴다. 지우고 새로 만들면 컨트롤러에 꽂아둔 참조가 끊긴다.
            EditorUtility.CopySerialized(clip, existing);
            Object.DestroyImmediate(clip);
        }
        else AssetDatabase.CreateAsset(clip, path);

        AssetDatabase.SaveAssets();
    }

    /// <summary>
    /// 구운 클립을 컨트롤러의 상태에 물린다.
    ///
    /// <b>여기서 함께 하는 이유.</b> 굽기와 배선을 따로 두면 구워놓고 꽂는 것을 잊은 판이 생기는데,
    /// 그건 화면에 "자세가 안 나온다"로만 드러나 원인을 되짚기 어렵다.
    ///
    /// 이미 있으면 클립만 갈아 끼운다 - 상태를 지우고 새로 만들면 그 자리를 가리키던 것이 끊긴다.
    /// 전이는 만들지 않는다. 이 컨트롤러의 상태들은 코드에서 CrossFade로만 걸린다.
    /// </summary>
    static void Wire(Pose pose, StringBuilder report)
    {
        AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath);

        if (controller == null) { report.AppendLine("    ! " + ControllerPath + " 를 찾지 못해 상태를 만들지 못했습니다."); return; }

        AnimationClip baked = AssetDatabase.LoadAssetAtPath<AnimationClip>(pose.output);
        AnimatorStateMachine machine = controller.layers[0].stateMachine;

        AnimatorState state = null;
        foreach (ChildAnimatorState child in machine.states)
            if (child.state.name == pose.state) state = child.state;

        bool made = state == null;

        if (made) state = machine.AddState(pose.state);

        state.motion = baked;
        state.speed = 1f;

        // 기본값을 덮어쓰지 않는다. 이 컨트롤러의 코드 구동 상태들과 같은 규칙이다.
        state.writeDefaultValues = false;

        EditorUtility.SetDirty(controller);
        AssetDatabase.SaveAssets();

        report.AppendLine("    상태 '" + pose.state + "' " + (made ? "새로 만듦" : "이미 있어 클립만 갱신")
                        + " | 전이 " + state.transitions.Length + "개 | writeDefaults " + state.writeDefaultValues);
    }

    /// <summary>
    /// 구운 클립을 도로 샘플링해 원본과 견준다.
    ///
    /// 근육 커브의 이름이 어긋나면 그 관절만 조용히 기본 자세로 남는다 — 손가락이 특히 그렇다.
    /// 각도로 재어 찍으면 어느 뼈가 그런지 바로 드러난다.
    /// </summary>
    static void Verify(Pose pose, Animator animator, Transform source, StringBuilder report)
    {
        AnimationClip baked = AssetDatabase.LoadAssetAtPath<AnimationClip>(pose.output);

        if (baked == null) { report.AppendLine("    검산: 구운 클립을 다시 읽지 못했습니다."); return; }

        report.AppendLine($"    humanMotion = {baked.humanMotion} | 길이 {baked.length:F4}초 "
                        + $"| 커브 {AnimationUtility.GetCurveBindings(baked).Length}개");

        if (!baked.humanMotion)
            report.AppendLine("    ! humanMotion이 거짓입니다 — 휴머노이드 클립으로 인식되지 않았습니다.");

        // 샘플링은 애니메이터가 있어야 돈다. 재는 동안만 켠다.
        animator.enabled = true;
        baked.SampleAnimation(animator.gameObject, 0f);
        animator.enabled = false;

        Dictionary<string, Transform> reference = new Dictionary<string, Transform>();
        foreach (Transform t in source.GetComponentsInChildren<Transform>(true)) reference[t.name] = t;

        float worst = 0f;
        string worstBone = "-";
        int off = 0;

        foreach (Transform bone in animator.transform.GetComponentsInChildren<Transform>(true))
        {
            if (bone == animator.transform) continue;
            if (!reference.TryGetValue(bone.name, out Transform from)) continue;

            float angle = Quaternion.Angle(from.localRotation, bone.localRotation);

            if (angle > ErrorTolerance) off++;
            if (angle > worst) { worst = angle; worstBone = bone.name; }
        }

        report.AppendLine($"    검산: 최대 오차 {worst:F2}° ({worstBone}), {ErrorTolerance:F0}° 넘는 뼈 {off}개");

        if (off > 0)
            report.AppendLine("    ! 어긋난 뼈가 있습니다. 근육 이름이 이 유니티 버전과 맞지 않을 수 있습니다.");
    }
}
