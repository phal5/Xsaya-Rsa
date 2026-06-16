using UnityEngine;

public class PauseGame : MonoBehaviour
{
    float timeScale;

    private void OnEnable()
    {
        timeScale = Time.timeScale;
        Time.timeScale = 0;
    }

    private void OnDisable()
    {
        Time.timeScale = timeScale;
    }
}
