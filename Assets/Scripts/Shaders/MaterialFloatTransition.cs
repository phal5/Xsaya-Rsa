using System.Collections;
using UnityEngine;

/// <summary>
/// Controls smooth transition of a shader float property (such as vortex distortion '_T')
/// by mapping a normalized 0~1 input to configurable min~max range over a specified duration.
/// </summary>
[AddComponentMenu("Shaders/Material Float Transition")]
public class MaterialFloatTransition : MonoBehaviour
{
    [Header("Target Material Settings")]
    [Tooltip("Target material to modify. If left empty, will attempt to find from Renderer on this GameObject.")]
    [SerializeField] private Material targetMaterial;

    [Tooltip("Use sharedMaterial (affects all instances) or material (creates instance).")]
    [SerializeField] private bool useSharedMaterial = true;

    [Header("0. Shader Property Settings")]
    [Tooltip("The exact string name of the shader float property to manipulate (e.g. '_T', '_N', '_SmoothWidth').")]
    [SerializeField] private string propertyName = "_T";

    [Header("2. Remapping Range (Min ~ Max)")]
    [Tooltip("Lower bound value mapped to input 0.0.")]
    [SerializeField] private float minValue = 0.0f;

    [Tooltip("Upper bound value mapped to input 1.0.")]
    [SerializeField] private float maxValue = 0.05f;

    [Header("3. Transition Duration & Curve")]
    [Tooltip("Time in seconds to smoothly transition from current value to target value.")]
    [Min(0.0f)]
    [SerializeField] private float transitionDuration = 1.0f;

    [Tooltip("Interpolation easing curve for the transition.")]
    [SerializeField] private AnimationCurve transitionCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Tooltip("Use unscaled time (ignores Time.timeScale).")]
    [SerializeField] private bool useUnscaledTime = false;

    private int propertyId;
    private Coroutine transitionCoroutine;
    private float currentValue;

    private float authoredValue;
    private bool hasAuthored;

    public Material TargetMaterial
    {
        get => targetMaterial;
        set
        {
            targetMaterial = value;
            InitProperty();
        }
    }

    public string PropertyName
    {
        get => propertyName;
        set
        {
            propertyName = value;
            InitProperty();
        }
    }

    public float MinValue { get => minValue; set => minValue = value; }
    public float MaxValue { get => maxValue; set => maxValue = value; }
    public float TransitionDuration { get => transitionDuration; set => transitionDuration = Mathf.Max(0f, value); }
    public float CurrentValue => currentValue;

    private void Awake()
    {
        ResolveTargetMaterial();
        InitProperty();
        Capture();
    }

    /// <summary>
    /// Captures current value the matrial holds.
    /// </summary>
    private void Capture()
    {
        if (targetMaterial == null || !targetMaterial.HasProperty(propertyId)) return;

        authoredValue = targetMaterial.GetFloat(propertyId);
        hasAuthored = true;
    }

    /// <summary>
    /// Returns manipulated value to value captured by Capture().
    /// </summary>
    private void OnDestroy()
    {
        if (!hasAuthored || targetMaterial == null) return;

        targetMaterial.SetFloat(propertyId, authoredValue);
    }

    private void OnValidate()
    {
        InitProperty();
    }

    private void ResolveTargetMaterial()
    {
        if (targetMaterial == null)
        {
            if (TryGetComponent<Renderer>(out var rend))
            {
                targetMaterial = useSharedMaterial ? rend.sharedMaterial : rend.material;
            }
        }
    }

    private void InitProperty()
    {
        if (!string.IsNullOrEmpty(propertyName))
        {
            propertyId = Shader.PropertyToID(propertyName);
            if (targetMaterial != null && targetMaterial.HasProperty(propertyId))
            {
                currentValue = targetMaterial.GetFloat(propertyId);
            }
        }
    }

    /// <summary>
    /// 1. Public function receiving a normalized input between 0.0 and 1.0.
    /// Maps the input to [minValue, maxValue] and smoothly transitions over transitionDuration.
    /// </summary>
    /// <param name="normalizedInput">Value between 0.0 and 1.0</param>
    public void SetNormalizedValue(float normalizedInput)
    {
        float clamped = Mathf.Clamp01(normalizedInput);
        float targetVal = Mathf.Lerp(minValue, maxValue, clamped);
        SetTargetValue(targetVal);
    }

    /// <summary>
    /// Alias for SetNormalizedValue (0.0 ~ 1.0).
    /// </summary>
    public void SetValue(float normalizedInput)
    {
        SetNormalizedValue(normalizedInput);
    }

    /// <summary>
    /// Smoothly transitions directly to the specified target value over transitionDuration.
    /// </summary>
    public void SetTargetValue(float targetVal)
    {
        ResolveTargetMaterial();
        if (targetMaterial == null)
        {
            Debug.LogWarning($"[MaterialFloatTransition] Target Material is null on {gameObject.name}!", this);
            return;
        }

        if (transitionDuration <= 0.0001f)
        {
            SetTargetValueInstant(targetVal);
            return;
        }

        if (transitionCoroutine != null)
        {
            StopCoroutine(transitionCoroutine);
        }

        transitionCoroutine = StartCoroutine(TransitionRoutine(targetVal));
    }

    /// <summary>
    /// Instantly sets the normalized value (0.0 ~ 1.0) without smooth transition.
    /// </summary>
    public void SetNormalizedValueInstant(float normalizedInput)
    {
        float clamped = Mathf.Clamp01(normalizedInput);
        float targetVal = Mathf.Lerp(minValue, maxValue, clamped);
        SetTargetValueInstant(targetVal);
    }

    /// <summary>
    /// Instantly sets the raw value without smooth transition.
    /// </summary>
    public void SetTargetValueInstant(float targetVal)
    {
        ResolveTargetMaterial();
        if (targetMaterial == null) return;

        if (transitionCoroutine != null)
        {
            StopCoroutine(transitionCoroutine);
            transitionCoroutine = null;
        }

        currentValue = targetVal;
        targetMaterial.SetFloat(propertyId, targetVal);
    }

    private IEnumerator TransitionRoutine(float targetVal)
    {
        if (targetMaterial.HasProperty(propertyId))
        {
            currentValue = targetMaterial.GetFloat(propertyId);
        }

        float startVal = currentValue;
        float elapsed = 0f;

        while (elapsed < transitionDuration)
        {
            elapsed += useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / transitionDuration);
            float curvedT = transitionCurve != null ? transitionCurve.Evaluate(t) : t;

            currentValue = Mathf.Lerp(startVal, targetVal, curvedT);
            targetMaterial.SetFloat(propertyId, currentValue);

            yield return null;
        }

        currentValue = targetVal;
        targetMaterial.SetFloat(propertyId, targetVal);
        transitionCoroutine = null;
    }

#if UNITY_EDITOR
    [ContextMenu("Test Input: 0.0 (Min)")]
    private void TestInputZero() => SetNormalizedValue(0.0f);

    [ContextMenu("Test Input: 0.5 (Mid)")]
    private void TestInputHalf() => SetNormalizedValue(0.5f);

    [ContextMenu("Test Input: 1.0 (Max)")]
    private void TestInputOne() => SetNormalizedValue(1.0f);
#endif
}
