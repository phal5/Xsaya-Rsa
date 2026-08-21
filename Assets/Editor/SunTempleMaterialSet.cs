using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// Sun_Temple ships as a Built-in RP package, so its materials render magenta under URP.
/// Two restored sets exist side by side:
///
///   Materials/          - the project's cel look, on Shader Graphs/Cel_rev1
///   Materials_Original/ - the package's own look, on the Sun_Temple_URP shader ports
///
/// Both sets use the same material names, so switching is a matter of repointing every
/// FBX's external material remap at the other folder. The package folder is gitignored,
/// so re-importing Sun_Temple wipes both sets; the shader ports under
/// Assets/Art/Shaders/SunTemple_URP are version controlled and survive.
/// </summary>
static class SunTempleMaterialSet
{
    const string MeshRoot = "Assets/Art/External/Sun_Temple/Content/Meshes/Main";
    const string CelSet = MeshRoot + "/Materials";
    const string FaithfulSet = MeshRoot + "/Materials_Original";

    [MenuItem("Tools/Sun Temple/Use Cel Materials (Cel_rev1)")]
    static void UseCel() => Retarget(CelSet);

    [MenuItem("Tools/Sun Temple/Use Faithful Materials (URP ports)")]
    static void UseFaithful() => Retarget(FaithfulSet);

    [MenuItem("Tools/Sun Temple/Report Current Material Set")]
    static void Report()
    {
        var counts = new Dictionary<string, int>();
        foreach (var path in ModelPaths())
        {
            var importer = AssetImporter.GetAtPath(path) as ModelImporter;
            if (importer == null) continue;

            foreach (var pair in importer.GetExternalObjectMap())
            {
                if (pair.Value == null) continue;
                var dir = Path.GetDirectoryName(AssetDatabase.GetAssetPath(pair.Value))?.Replace('\\', '/');
                if (string.IsNullOrEmpty(dir)) continue;
                counts.TryGetValue(dir, out int n);
                counts[dir] = n + 1;
            }
        }

        if (counts.Count == 0)
        {
            Debug.Log("Sun Temple: no external material remaps found.");
            return;
        }

        foreach (var kv in counts.OrderByDescending(k => k.Value))
            Debug.Log($"Sun Temple: {kv.Value} remaps -> {kv.Key}");
    }

    static IEnumerable<string> ModelPaths()
    {
        return AssetDatabase.FindAssets("t:Model", new[] { MeshRoot })
                            .Select(AssetDatabase.GUIDToAssetPath)
                            .Distinct();
    }

    static void Retarget(string targetFolder)
    {
        if (!AssetDatabase.IsValidFolder(targetFolder))
        {
            Debug.LogError($"Sun Temple: material set folder not found: {targetFolder}");
            return;
        }

        // name -> material, for the set we are switching to
        var byName = AssetDatabase.FindAssets("t:Material", new[] { targetFolder })
                                  .Select(AssetDatabase.GUIDToAssetPath)
                                  .Select(AssetDatabase.LoadAssetAtPath<Material>)
                                  .Where(m => m != null)
                                  .GroupBy(m => m.name)
                                  .ToDictionary(g => g.Key, g => g.First());

        var paths = ModelPaths().ToList();
        int remapped = 0, reimported = 0;
        var missing = new HashSet<string>();

        try
        {
            AssetDatabase.StartAssetEditing();

            for (int i = 0; i < paths.Count; i++)
            {
                var path = paths[i];
                if (EditorUtility.DisplayCancelableProgressBar(
                        "Sun Temple", $"Retargeting {Path.GetFileName(path)}", (float)i / paths.Count))
                    break;

                var importer = AssetImporter.GetAtPath(path) as ModelImporter;
                if (importer == null) continue;

                bool changed = false;
                foreach (var pair in importer.GetExternalObjectMap().ToList())
                {
                    if (pair.Key.type != typeof(Material)) continue;

                    // Match on the material currently in the slot, not on the slot name:
                    // the FBX's internal name is often a different casing or an older name
                    // than the project material it was remapped to ("bark1" -> Fol_Bark1).
                    var current = pair.Value as Material;
                    string lookup = current != null ? current.name : pair.Key.name;

                    if (!byName.TryGetValue(lookup, out var replacement))
                    {
                        missing.Add(lookup);
                        continue;
                    }

                    if (pair.Value == replacement) continue;

                    importer.AddRemap(pair.Key, replacement);
                    remapped++;
                    changed = true;
                }

                if (changed)
                {
                    AssetDatabase.WriteImportSettingsIfDirty(path);
                    reimported++;
                }
            }
        }
        finally
        {
            AssetDatabase.StopAssetEditing();
            EditorUtility.ClearProgressBar();
        }

        AssetDatabase.Refresh();

        // FBX remaps only decide what a *newly instantiated* model gets. Prefabs and scene
        // objects carry their own serialized material references, so they need repointing
        // too or the switch is invisible in the scene.
        // Saving prefabs during play mode is unreliable, so that pass is skipped there.
        int prefabsFixed = EditorApplication.isPlaying ? -1 : RetargetPrefabs(byName, missing);
        int sceneRenderers = RetargetOpenScenes(byName, missing);

        foreach (var name in missing.OrderBy(n => n))
            Debug.LogWarning($"Sun Temple: no material named '{name}' in {targetFolder}; that slot was left alone.");

        string prefabPart = prefabsFixed < 0 ? "prefabs skipped (play mode)" : $"{prefabsFixed} prefabs";
        Debug.Log($"Sun Temple: repointed {remapped} import slots across {reimported} models, " +
                  $"{prefabPart} and {sceneRenderers} scene renderers to {targetFolder}. " +
                  $"Open scenes are modified but not saved.");
    }

    static bool Swap(Renderer r, Dictionary<string, Material> byName, HashSet<string> missing)
    {
        var mats = r.sharedMaterials;
        bool changed = false;

        for (int i = 0; i < mats.Length; i++)
        {
            var m = mats[i];
            if (m == null) continue;

            // only touch materials that belong to one of the Sun Temple sets
            var path = AssetDatabase.GetAssetPath(m);
            if (string.IsNullOrEmpty(path) || !path.StartsWith(MeshRoot)) continue;

            if (!byName.TryGetValue(m.name, out var replacement)) { missing.Add(m.name); continue; }
            if (replacement == m) continue;

            mats[i] = replacement;
            changed = true;
        }

        if (changed) r.sharedMaterials = mats;
        return changed;
    }

    static int RetargetPrefabs(Dictionary<string, Material> byName, HashSet<string> missing)
    {
        const string prefabRoot = "Assets/Art/External/Sun_Temple";
        var paths = AssetDatabase.FindAssets("t:Prefab", new[] { prefabRoot })
                                 .Select(AssetDatabase.GUIDToAssetPath).Distinct().ToList();
        int touched = 0;

        for (int i = 0; i < paths.Count; i++)
        {
            if (EditorUtility.DisplayCancelableProgressBar(
                    "Sun Temple", $"Prefab {i + 1}/{paths.Count}", (float)i / paths.Count))
                break;

            var root = PrefabUtility.LoadPrefabContents(paths[i]);
            if (root == null) continue;

            bool changed = root.GetComponentsInChildren<Renderer>(true)
                               .Aggregate(false, (acc, r) => Swap(r, byName, missing) || acc);

            if (changed)
            {
                PrefabUtility.SaveAsPrefabAsset(root, paths[i]);
                touched++;
            }
            PrefabUtility.UnloadPrefabContents(root);
        }
        EditorUtility.ClearProgressBar();
        return touched;
    }

    static int RetargetOpenScenes(Dictionary<string, Material> byName, HashSet<string> missing)
    {
        int touched = 0;
        for (int s = 0; s < UnityEngine.SceneManagement.SceneManager.sceneCount; s++)
        {
            var scene = UnityEngine.SceneManagement.SceneManager.GetSceneAt(s);
            if (!scene.isLoaded) continue;

            foreach (var go in scene.GetRootGameObjects())
                foreach (var r in go.GetComponentsInChildren<Renderer>(true))
                    if (Swap(r, byName, missing))
                    {
                        EditorUtility.SetDirty(r);
                        touched++;
                    }

            // In play mode the scene is a runtime copy: the swap shows up immediately but
            // is thrown away on exit, and marking it dirty is illegal.
            if (touched > 0 && !EditorApplication.isPlaying)
                EditorSceneManager.MarkSceneDirty(scene);
        }
        return touched;
    }
}
