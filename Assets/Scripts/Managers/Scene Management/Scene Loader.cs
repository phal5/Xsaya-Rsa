using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    // This string is what the final standalone build will actually use to load
    [HideInInspector]
    public string scenePath;

    // This block only runs inside the Unity Editor, so the build won't crash
#if UNITY_EDITOR
    [SerializeField] private UnityEditor.SceneAsset sceneToAsset;

    private void OnValidate()
    {
        if (sceneToAsset != null)
        {
            // Safely extracts the internal project path and stores it in our string
            scenePath = UnityEditor.AssetDatabase.GetAssetPath(sceneToAsset);
        }
    }
#endif

    // This public method is completely safe to call in your final game build!
    public void LoadScene()
    {
        if (!string.IsNullOrEmpty(scenePath))
        {
            SceneManager.LoadScene(scenePath);
        }
        else
        {
            Debug.LogError("No scene has been assigned to this loader!");
        }
    }
}