using UnityEditor;
using UnityEngine;
using System.Reflection;
using System;

public class PlaySoundEditorWindow : EditorWindow
{
    private AudioClip exitPlayModeClip;
    private AudioClip lightmapBakeClip;

    [MenuItem("Tools/Editor Sound Settings")]
    public static void ShowWindow()
    {
        GetWindow<PlaySoundEditorWindow>("Editor Sound Settings");
    }

    private void OnEnable()
    {
        // Load Exit Play Mode Sound
        string exitPath = EditorPrefs.GetString("ExitPlayModeSoundPath", "");
        if (!string.IsNullOrEmpty(exitPath))
        {
            exitPlayModeClip = AssetDatabase.LoadAssetAtPath<AudioClip>(exitPath);
        }

        // Load Lightmap Bake Sound
        string bakePath = EditorPrefs.GetString("LightmapBakeSoundPath", "");
        if (!string.IsNullOrEmpty(bakePath))
        {
            lightmapBakeClip = AssetDatabase.LoadAssetAtPath<AudioClip>(bakePath);
        }

        // Unsubscribe first to avoid duplicate registrations
        EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;

        Lightmapping.bakeCompleted -= OnLightmapBakeCompleted;
        Lightmapping.bakeCompleted += OnLightmapBakeCompleted;
    }

    private void OnDisable()
    {
        // Clean up callbacks when the window is destroyed or disabled
        EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
        Lightmapping.bakeCompleted -= OnLightmapBakeCompleted;
    }

    private void OnGUI()
    {
        GUILayout.Label("Editor Event Sound Settings", EditorStyles.boldLabel);
        GUILayout.Space(10);

        // Exit Play Mode Sound Slot
        EditorGUI.BeginChangeCheck();
        exitPlayModeClip = (AudioClip)EditorGUILayout.ObjectField("Exit Play Mode Sound", exitPlayModeClip, typeof(AudioClip), false);
        if (EditorGUI.EndChangeCheck())
        {
            string path = AssetDatabase.GetAssetPath(exitPlayModeClip);
            EditorPrefs.SetString("ExitPlayModeSoundPath", path);
        }

        GUILayout.Space(10);

        // Lightmap Bake Complete Sound Slot
        EditorGUI.BeginChangeCheck();
        lightmapBakeClip = (AudioClip)EditorGUILayout.ObjectField("Lightmap Bake Finished", lightmapBakeClip, typeof(AudioClip), false);
        if (EditorGUI.EndChangeCheck())
        {
            string path = AssetDatabase.GetAssetPath(lightmapBakeClip);
            EditorPrefs.SetString("LightmapBakeSoundPath", path);
        }
    }

    private static void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.ExitingPlayMode)
        {
            PlaySavedSound("ExitPlayModeSoundPath");
        }
    }

    private static void OnLightmapBakeCompleted()
    {
        PlaySavedSound("LightmapBakeSoundPath");
    }

    private static void PlaySavedSound(string editorPrefKey)
    {
        string path = EditorPrefs.GetString(editorPrefKey, "");
        if (!string.IsNullOrEmpty(path))
        {
            AudioClip clip = AssetDatabase.LoadAssetAtPath<AudioClip>(path);
            if (clip != null)
            {
                PlayEditorSound(clip);
            }
        }
    }

    private static void PlayEditorSound(AudioClip clip)
    {
        Assembly unityEditorAssembly = typeof(AudioImporter).Assembly;
        Type audioUtilClass = unityEditorAssembly.GetType("UnityEditor.AudioUtil");

        MethodInfo method = audioUtilClass.GetMethod("PlayPreviewClip",
            BindingFlags.Static | BindingFlags.Public, null, new Type[] { typeof(AudioClip), typeof(int), typeof(bool) }, null);

        if (method != null)
        {
            method.Invoke(null, new object[] { clip, 0, false });
        }
        else
        {
            method = audioUtilClass.GetMethod("PlayClip",
                BindingFlags.Static | BindingFlags.Public, null, new Type[] { typeof(AudioClip) }, null);
            method?.Invoke(null, new object[] { clip });
        }
    }
}
