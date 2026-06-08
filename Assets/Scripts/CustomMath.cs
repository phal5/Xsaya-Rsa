using UnityEngine;

public static class CustomMath
{
    public static float smooth01(float t)
    {
        t = Mathf.Clamp01(t);
        return t * t * (3f - 2 * t);
    }

    public static Vector3 PreservativeRemove(Vector3 component, Vector3 from)
    {
        float mag = from.magnitude;

        Vector3 Clean = CleanRemove(component, from);
        Vector3 normalizedClean = Clean.normalized;
        Clean *= mag;

        return Clean;
    }

    public static Vector3 CleanRemove(Vector3 component, Vector3 from)
    {
        component = GetComponentFrom(component, from);
        return from - component;
    }

    public static Vector3 GetComponentFrom(Vector3 component, Vector3 from)
    {
        Vector3 normalizedComponent = component.normalized;
        return Vector3.Dot(from, normalizedComponent) * normalizedComponent;
    }

    public static float GetComponentSizeFrom(Vector3 component, Vector3 from)
    {
        Vector3 normalizedComponent = component.normalized;
        return Vector3.Dot(from, normalizedComponent);
    }

    public static float ReLU(float t)
    {
        return t > 0 ? t : 0;
    }

    public static Vector3 Multiply(Vector3 v1, Vector3 v2)
    {
        return new(v1.x * v2.x, v1.y * v2.y, v1.z * v2.z);
    }
}
