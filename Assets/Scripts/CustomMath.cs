using UnityEngine;

public static class CustomMath
{
    public static float smooth01(float t)
    {
        t = Mathf.Clamp01(t);
        return t * t * (3f - 2 * t);
    }

    public static Vector3 PreservativeRemove(Vector3 remove, Vector3 from)
    {
        float mag = from.magnitude;
        remove = remove.normalized;

        from -= remove * Vector3.Dot(from, remove);
        return from;
    }

    public static float ReLU(float t)
    {
        return t > 0 ? t : 0;
    }
}
