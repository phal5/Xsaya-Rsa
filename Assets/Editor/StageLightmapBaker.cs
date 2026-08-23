using System;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 배경 씬들의 라이트맵을 <b>차례로</b> 굽는다. 밤새 세워두고 아침에 결과만 읽는 용도다.
///
/// <b>씬 하나씩 연다.</b> 여러 씬을 함께 열고 구우면 라이팅 데이터가 그 조합에 묶여,
/// 나중에 배경을 갈아끼우며 다른 조합으로 애디티브 로드할 때 어긋난다.
/// 스테이지마다 따로 구워야 각자의 데이터가 제 씬에 붙는다.
///
/// <b>동기 베이크다.</b> Lightmapping.Bake()가 끝날 때까지 에디터가 멈춘다.
/// 지켜보는 사람이 없는 시간에 돌리는 것이 전제이므로 그 편이 예측 가능하다.
///
/// 진행 기록은 콘솔과 <see cref="LogPath"/> 양쪽에 남긴다. 콘솔은 지워지거나 넘칠 수 있고,
/// 아침에 어디까지 갔는지 확인하려면 남아 있는 파일이 필요하다.
/// </summary>
public static class StageLightmapBaker
{
    const string SettingsPath = "Assets/Settings/StageLighting.lighting";

    /// <summary>버전 관리에 들어가지 않는 자리에 남긴다. 산출물이 아니라 기록이다.</summary>
    const string LogPath = "Library/StageLightmapBake.log";

    static readonly string[] Scenes =
    {
        "Assets/Scenes/Parts/Backgrounds/1. Temple of Awakening/Temple of Awakening.unity",
        "Assets/Scenes/Parts/Backgrounds/2. the Shrine/the Shrine.unity",
        "Assets/Scenes/Parts/Backgrounds/3. Sky Temple/Floating shrine.unity",
        "Assets/Scenes/Parts/Backgrounds/4. the Depths/Cave.unity",
    };

    [MenuItem("Tools/Lightmaps/Bake Stage Lightmaps (4 scenes)")]
    public static void BakeAll()
    {
        // 열려 있는 씬에 저장 안 된 변경이 있으면 시작하지 않는다.
        // 씬을 Single로 갈아끼우며 도는 구조라, 여기서 멈추지 않으면 그 변경이 조용히 사라진다.
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            Scene s = SceneManager.GetSceneAt(i);
            if (!s.isDirty) continue;

            Debug.LogError($"[StageLightmapBaker] '{s.name}'에 저장하지 않은 변경이 있습니다. 저장한 뒤 다시 실행하세요.");
            return;
        }

        LightingSettings settings = AssetDatabase.LoadAssetAtPath<LightingSettings>(SettingsPath);
        if (settings == null)
        {
            Debug.LogError($"[StageLightmapBaker] 베이크 설정을 찾지 못했습니다: {SettingsPath}");
            return;
        }

        StringBuilder log = new StringBuilder();
        Write(log, $"=== 시작 {Now()} — 씬 {Scenes.Length}개, 설정 '{settings.name}' ===");

        DateTime began = DateTime.Now;
        int done = 0;

        foreach (string path in Scenes)
        {
            DateTime sceneBegan = DateTime.Now;

            try
            {
                Write(log, $"[{Now()}] 열기: {path}");
                Scene scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);

                Lightmapping.SetLightingSettingsForScene(scene, settings);

                Write(log, $"[{Now()}] 굽기 시작: {scene.name}");
                bool ok = Lightmapping.Bake();

                if (!ok)
                {
                    Write(log, $"[{Now()}] !! 실패: {scene.name} — 다음 씬으로 넘어갑니다");
                    continue;
                }

                EditorSceneManager.SaveScene(scene);
                done++;

                Write(log, $"[{Now()}] 완료: {scene.name}  ({Elapsed(sceneBegan)})  라이트맵 {LightmapSettings.lightmaps.Length}장");
            }
            catch (Exception e)
            {
                Write(log, $"[{Now()}] !! 예외: {path}\n{e}");
            }
        }

        Write(log, $"=== 끝 {Now()} — {done}/{Scenes.Length}개 성공, 총 {Elapsed(began)} ===");
    }

    static string Now() => DateTime.Now.ToString("MM-dd HH:mm:ss");

    static string Elapsed(DateTime from)
    {
        TimeSpan t = DateTime.Now - from;
        return $"{(int)t.TotalHours:00}:{t.Minutes:00}:{t.Seconds:00}";
    }

    /// <summary>콘솔과 파일 양쪽에 남긴다. 매번 흘려 쓴다 — 도중에 멈춰도 거기까지는 남아야 한다.</summary>
    static void Write(StringBuilder log, string line)
    {
        Debug.Log($"[StageLightmapBaker] {line}");
        log.AppendLine(line);

        try { File.WriteAllText(LogPath, log.ToString()); }
        catch (Exception e) { Debug.LogWarning($"[StageLightmapBaker] 기록 실패: {e.Message}"); }
    }
}
