using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class WeightedTransform
{
    public Transform target;
    [Range(0f, 1f)] public float weight;
}

public class TransformWeightBlender : MonoBehaviour
{
    [Tooltip("The sum of all weights will be forced to 1.")]
    [SerializeField] List<WeightedTransform> targets = new List<WeightedTransform>();

    private Coroutine activeTransition;
    private float _invDuration;

    private void LateUpdate()
    {
        transform.position = BlendedTransformation().position;
    }

    private (Vector3 position, Quaternion rotation) BlendedTransformation()
    {
        Vector3 blendedPosition = Vector3.zero;
        Quaternion blendedRotation = Quaternion.identity;

        bool firstRotation = true;
        float accumulatedWeight = 0;
        foreach (var t in targets)
        {
            if (t.target != null && t.weight > 0f)
            {
                if (t.weight > 0)
                {
                    blendedPosition += t.target.position * t.weight;

                    accumulatedWeight += t.weight;
                    if (firstRotation) { blendedRotation = t.target.rotation; firstRotation = false; }
                    else { blendedRotation = Quaternion.Slerp(blendedRotation, t.target.rotation, t.weight / accumulatedWeight); }
                }
            }
        }
        return (blendedPosition, blendedRotation);
    }

    /// <summary>
    /// Transitions the given target's weight to 1 over time, scaling others down proportionally.
    /// </summary>
    /// <param name="target">The Transform to prioritize.</param>
    /// <param name="duration">How long the transition should take in seconds.</param>
    /// <param name="easingFunction">Lambda taking a linear t (0 to 1) and returning a smoothed t.</param>
    /// <param name="onComplete">Callback triggered when the transition finishes.</param>
    public void Transit(Transform target, float duration, Func<float, float> easingFunction, Action onComplete)
    {
        if (activeTransition != null)
        {
            StopCoroutine(activeTransition);
        }

        if(duration > 0f)
        {
            _invDuration = 1 / duration;
            activeTransition = StartCoroutine(TransitionRoutine(target, duration, easingFunction, onComplete));
        }
        else
        {
            FinalizeWeights(targets.FirstOrDefault(t => t.target == target));
        }
    }

    private IEnumerator TransitionRoutine(Transform targetTransform, float duration, Func<float, float> easeFunc, Action onComplete)
    {
        // 1. Find the target in the list, or add it if it doesn't exist
        WeightedTransform mainTarget = targets.FirstOrDefault(t => t.target == targetTransform);
        if (mainTarget == null)
        {
            mainTarget = new WeightedTransform { target = targetTransform, weight = 0f };
            targets.Add(mainTarget);
        }

        // 2. Snapshot the initial state
        float startMainWeight = mainTarget.weight;
        Dictionary<WeightedTransform, float> initialOtherWeights = new Dictionary<WeightedTransform, float>();
        float initialOtherSum = 0f;

        foreach (var t in targets)
        {
            if (t != mainTarget)
            {
                initialOtherWeights[t] = t.weight;
                initialOtherSum += t.weight;
            }
        }

        // If the target is already at 1, snap to finish
        if (Mathf.Approximately(startMainWeight, 1f))
        {
            FinalizeWeights(mainTarget);
            onComplete?.Invoke();
            yield break;
        }

        // 3. Process the transition over time
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            // Apply the lambda smoothing function (fallback to linear if null)
            float smoothedT = easeFunc != null ? easeFunc(t) : t;

            // Interpolate the main target's weight
            float currentMainWeight = Mathf.Lerp(startMainWeight, 1f, smoothedT);
            mainTarget.weight = currentMainWeight;

            // Scale the other weights proportionally to share the remaining budget
            float remainingBudget = 1f - currentMainWeight;

            foreach (var other in initialOtherWeights.Keys)
            {
                if (initialOtherSum > 0f)
                {
                    other.weight = initialOtherWeights[other] * (remainingBudget / initialOtherSum);
                }
                else
                {
                    other.weight = 0f;
                }
            }

            yield return null;
        }

        // 4. Guarantee exact final values to prevent floating point inaccuracies
        FinalizeWeights(mainTarget);
        onComplete?.Invoke();
    }

    /// <summary>
    /// Forces the target to 1 and all others to 0.
    /// </summary>
    private void FinalizeWeights(WeightedTransform absoluteTarget)
    {
        foreach (var t in targets)
        {
            t.weight = (t == absoluteTarget) ? 1f : 0f;
        }
    }

    // --- Editor Helper ---
    // This ensures weights stay normalized if you tweak them in the Unity Inspector
    private void OnValidate()
    {
        if (targets == null || targets.Count == 0) return;

        float sum = targets.Sum(t => t.weight);

        if (sum == 0)
        {
            // If everything is 0, give the first one full weight
            targets[0].weight = 1f;
            return;
        }

        if (!Mathf.Approximately(sum, 1f))
        {
            foreach (var t in targets)
            {
                t.weight /= sum; // Normalize so they sum to 1
            }
        }
    }
}