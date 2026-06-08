using UnityEditor;
using UnityEngine;
using System.Reflection;
using System;

[InitializeOnLoad]
public class PlaySoundEditorWindow : EditorWindow
{
    private AudioClip exitPlayModeClip;
    private AudioClip lightmapBakeClip;

    private bool exitPlayModeMuted;
    private bool lightmapBakeMuted;

    static PlaySoundEditorWindow()
    {
        EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;

        Lightmapping.bakeCompleted -= OnLightmapBakeCompleted;
        Lightmapping.bakeCompleted += OnLightmapBakeCompleted;
    }

    [MenuItem("Tools/Editor Sound Settings")]
    public static void ShowWindow()
    {
        GetWindow<PlaySoundEditorWindow>("Editor Sound Settings");
    }

    private void OnEnable()
    {
        LoadSettings();
    }

    private void LoadSettings()
    {
        string exitPath = EditorPrefs.GetString("ExitPlayModeSoundPath", "");
        if (!string.IsNullOrEmpty(exitPath)) exitPlayModeClip = AssetDatabase.LoadAssetAtPath<AudioClip>(exitPath);

        string bakePath = EditorPrefs.GetString("LightmapBakeSoundPath", "");
        if (!string.IsNullOrEmpty(bakePath)) lightmapBakeClip = AssetDatabase.LoadAssetAtPath<AudioClip>(bakePath);

        exitPlayModeMuted = EditorPrefs.GetBool("ExitPlayModeMuted", false);
        lightmapBakeMuted = EditorPrefs.GetBool("LightmapBakeMuted", false);
    }

    private void OnGUI()
    {
        GUILayout.Label("Editor Event Sound Settings", EditorStyles.boldLabel);
        GUILayout.Space(10);

        // --- 긴급 정지 버튼 ---
        GUI.backgroundColor = new Color(0.9f, 0.3f, 0.3f); // 버튼 색상을 붉은색 계열로 지정
        if (GUILayout.Button("🛑 Stop All Sounds", GUILayout.Height(30)))
        {
            StopAllEditorSounds();
        }
        GUI.backgroundColor = Color.white; // 색상 초기화

        GUILayout.Space(15);

        // --- Exit Play Mode Sound Section ---
        GUILayout.Label("Play Mode Settings", EditorStyles.miniBoldLabel);
        EditorGUI.BeginChangeCheck();
        exitPlayModeClip = (AudioClip)EditorGUILayout.ObjectField("Exit Play Mode Sound", exitPlayModeClip, typeof(AudioClip), false);
        if (EditorGUI.EndChangeCheck())
        {
            EditorPrefs.SetString("ExitPlayModeSoundPath", AssetDatabase.GetAssetPath(exitPlayModeClip));
        }

        EditorGUI.BeginChangeCheck();
        exitPlayModeMuted = EditorGUILayout.Toggle("Mute Exit Sound", exitPlayModeMuted);
        if (EditorGUI.EndChangeCheck())
        {
            EditorPrefs.SetBool("ExitPlayModeMuted", exitPlayModeMuted);
        }

        GUILayout.Space(15);

        // --- Lightmap Bake Section ---
        GUILayout.Label("Lightmap Settings", EditorStyles.miniBoldLabel);
        EditorGUI.BeginChangeCheck();
        lightmapBakeClip = (AudioClip)EditorGUILayout.ObjectField("Bake Finished Sound", lightmapBakeClip, typeof(AudioClip), false);
        if (EditorGUI.EndChangeCheck())
        {
            EditorPrefs.SetString("LightmapBakeSoundPath", AssetDatabase.GetAssetPath(lightmapBakeClip));
        }

        EditorGUI.BeginChangeCheck();
        lightmapBakeMuted = EditorGUILayout.Toggle("Mute Bake Sound", lightmapBakeMuted);
        if (EditorGUI.EndChangeCheck())
        {
            EditorPrefs.SetBool("LightmapBakeMuted", lightmapBakeMuted);
        }
    }

    private static void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.EnteredEditMode)
        {
            bool isMuted = EditorPrefs.GetBool("ExitPlayModeMuted", false);
            if (!isMuted) PlaySavedSound("ExitPlayModeSoundPath");
        }
    }

    private static void OnLightmapBakeCompleted()
    {
        bool isMuted = EditorPrefs.GetBool("LightmapBakeMuted", false);
        if (!isMuted) PlaySavedSound("LightmapBakeSoundPath");
    }

    private static void PlaySavedSound(string editorPrefKey)
    {
        string path = EditorPrefs.GetString(editorPrefKey, "");
        if (!string.IsNullOrEmpty(path))
        {
            AudioClip clip = AssetDatabase.LoadAssetAtPath<AudioClip>(path);
            if (clip != null) PlayEditorSound(clip);
        }
    }

    private static void PlayEditorSound(AudioClip clip)
    {
        Assembly unityEditorAssembly = typeof(AudioImporter).Assembly;
        Type audioUtilClass = unityEditorAssembly.GetType("UnityEditor.AudioUtil");

        MethodInfo method = audioUtilClass.GetMethod("PlayPreviewClip",
            BindingFlags.Static | BindingFlags.Public, null, new Type[] { typeof(AudioClip), typeof(int), typeof(bool) }, null);

        if (method != null) method.Invoke(null, new object[] { clip, 0, false });
    }

    // 재생 중인 모든 에디터 프리뷰 사운드를 즉시 정지시키는 메서드
    public static void StopAllEditorSounds()
    {
        Assembly unityEditorAssembly = typeof(AudioImporter).Assembly;
        Type audioUtilClass = unityEditorAssembly.GetType("UnityEditor.AudioUtil");

        MethodInfo method = audioUtilClass.GetMethod("StopAllPreviewClips",
            BindingFlags.Static | BindingFlags.Public);

        if (method != null)
        {
            method.Invoke(null, null);
        }
    }
}
