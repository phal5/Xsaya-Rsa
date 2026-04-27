using UnityEngine;

public class CustomMath
{
    public static float smooth01(float t)
    {
        t = Mathf.Clamp01(t);
        return t * t * (3f - 2 * t);
    }
}
