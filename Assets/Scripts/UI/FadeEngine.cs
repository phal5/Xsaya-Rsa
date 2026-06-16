using System;
using System.Collections;
using UnityEngine;

public class FadeEngine
{
    private MonoBehaviour _coroutineRunner;
    private Coroutine _currentFade;

    public FadeEngine(MonoBehaviour coroutineRunner)
    {
        _coroutineRunner = coroutineRunner;
    }

    /// <summary>
    /// Starts the fade routine, returning a 0-to-1 progress float via the onUpdate callback.
    /// </summary>
    public void PlayFade(float duration, bool fadingIn, Action<float> onUpdate, Action onComplete = null)
    {
        if (_currentFade != null)
        {
            _coroutineRunner.StopCoroutine(_currentFade);
        }

        _currentFade = _coroutineRunner.StartCoroutine(FadeRoutine(duration, fadingIn, onUpdate, onComplete));
    }

    private IEnumerator FadeRoutine(float duration, bool fadingIn, Action<float> onUpdate, Action onComplete)
    {
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsedTime / duration);

            float blendProgress = fadingIn ? t : 1f - t;

            // Send the calculated progress back to whatever script called this engine
            onUpdate?.Invoke(blendProgress);

            yield return null;
        }

        // Snap to exact final state to fix float imprecision
        onUpdate?.Invoke(fadingIn ? 1f : 0f);

        // Fire the completion event if one was provided
        onComplete?.Invoke();
    }
}