using UnityEngine;

public class HideInGame : MonoBehaviour
{
    void Awake()
    {
        if (!enabled) return;
        Renderer rend = GetComponent<Renderer>();
        if (rend != null)
        {
            rend.enabled = false;
        }
    }
}