using UnityEngine;

public class PauseGame : MonoBehaviour
{
    private void OnEnable()
    {
        TimeManager.Pause();
    }

    private void OnDisable()
    {
        TimeManager.Resume();
    }
}
