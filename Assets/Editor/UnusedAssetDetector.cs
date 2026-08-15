using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class UnusedAssetDetector : EditorWindow
{
    private List<string> unusedAssets = new List<string>();
    private Vector2 scrollPos;

    [MenuItem("Tools/Find Unused Assets")]
    public static void ShowWindow()
    {
        GetWindow<UnusedAssetDetector>("Unused Assets Scanner");
    }

    void OnGUI()
    {
        GUILayout.Label("Unused Asset Detector", EditorStyles.boldLabel);
        GUILayout.Space(5);
        GUILayout.Label("This tool finds assets not referenced by any Scene, \nResources, or StreamingAssets.", EditorStyles.wordWrappedLabel);
        GUILayout.Space(10);

        if (GUILayout.Button("Scan Project (May take a minute)", GUILayout.Height(30)))
        {
            ScanForUnusedAssets();
        }

        if (unusedAssets.Count > 0)
        {
            GUILayout.Space(10);
            GUILayout.Label($"Found {unusedAssets.Count} potentially unused assets:", EditorStyles.boldLabel);

            scrollPos = GUILayout.BeginScrollView(scrollPos, "box");
            foreach (var asset in unusedAssets)
            {
                EditorGUILayout.SelectableLabel(asset, EditorStyles.textField, GUILayout.Height(EditorGUIUtility.singleLineHeight));
            }
            GUILayout.EndScrollView();

            GUILayout.Space(10);
            EditorGUILayout.HelpBox("Verify these assets before deleting them! Some assets might be loaded dynamically via scripts using string paths.", MessageType.Warning);
        }
        else if (unusedAssets != null && unusedAssets.Count == 0 && Event.current.type == EventType.Repaint)
        {
            GUILayout.Space(10);
            GUILayout.Label("No unused assets found, or scan hasn't been run yet.");
        }
    }

    private void ScanForUnusedAssets()
    {
        // 1. Find all assets in the project (excluding folders)
        string[] allAssets = AssetDatabase.GetAllAssetPaths()
            .Where(path => path.StartsWith("Assets/") && !AssetDatabase.IsValidFolder(path))
            .ToArray();

        // 2. Find all scenes in the project
        string[] allScenes = AssetDatabase.FindAssets("t:Scene")
            .Select(AssetDatabase.GUIDToAssetPath)
            .ToArray();

        // 3. Find all assets in special folders that Unity always includes
        string[] specialFolders = allAssets.Where(path => path.Contains("/Resources/") || path.Contains("/StreamingAssets/")).ToArray();

        // 4. Combine root assets to trace dependencies from
        List<string> rootAssets = new List<string>();
        rootAssets.AddRange(allScenes);
        rootAssets.AddRange(specialFolders);

        // 5. Ask Unity to find every single file referenced by the root assets
        string[] usedAssets = AssetDatabase.GetDependencies(rootAssets.ToArray(), true);

        // 6. The unused assets are the total assets minus the used assets
        unusedAssets = allAssets.Except(usedAssets).ToList();

        // Exclude scripts from this list. Manual code stripping breaks things!
        unusedAssets.RemoveAll(path => path.EndsWith(".cs") || path.EndsWith(".dll") || path.EndsWith(".asmdef"));

        // Sort alphabetically for easier reading
        unusedAssets.Sort();
    }
}