using UnityEngine;

public class PauseGame : MonoBehaviour
{
    private void OnEnable()
    {
        TimeManager.Stop();
    }

    private void OnDisable()
    {
        TimeManager.Resume();
    }
}
