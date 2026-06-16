using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIGroupFader : MonoBehaviour
{
    #region Inspector Fields
    
    [Header("Fade Settings")]
    [SerializeField, Min(0f)] private float _fadeDuration = 0.5f;

    [Header("Startup Behavior")]
    [Tooltip("If true, the UI will instantly snap to invisible when the game starts, then smoothly fade in.")]
    [SerializeField] private bool _fadedOnStart = false;

    [Header("UI Elements")]
    [SerializeField] private List<Graphic> _uiElements = new List<Graphic>();
    
    #endregion

    #region Internal State
    
    private FadeEngine _fadeEngine;
    private Dictionary<Graphic, float> _originalAlphas = new Dictionary<Graphic, float>();
    
    #endregion

    #region Unity Lifecycle
    
    private void Awake()
    {
        _fadeEngine = new FadeEngine(this);

        // Cache the starting alphas exactly as they look in the Editor
        foreach (Graphic graphic in _uiElements)
        {
            if (graphic != null)
            {
                _originalAlphas.Add(graphic, graphic.color.a);
            }
        }
    }

    private void Start()
    {
        // If we want this to appear right as the scene loads:
        if (_fadedOnStart)
        {
            // 1. Instantly snap all alpha values to 0 before the player sees frame 1
            ApplyAlphaToAll(0f);
        }
    }
    
    #endregion

    #region Public API
    
    public void FadeIn()
    {
        gameObject.SetActive(true);
        
        _fadeEngine.PlayFade(
            duration: _fadeDuration, 
            fadingIn: true, 
            onUpdate: ApplyAlphaToAll
        );
    }

    public void FadeOut()
    {
        if (!gameObject.activeInHierarchy) return;
        
        _fadeEngine.PlayFade(
            duration: _fadeDuration, 
            fadingIn: false, 
            onUpdate: ApplyAlphaToAll, 
            onComplete: () => gameObject.SetActive(false)
        );
    }
    
    #endregion

    #region Core Logic
    
    private void ApplyAlphaToAll(float progress)
    {
        foreach (Graphic graphic in _uiElements)
        {
            if (graphic != null && _originalAlphas.TryGetValue(graphic, out float originalAlpha))
            {
                Color c = graphic.color;
                c.a = originalAlpha * progress;
                graphic.color = c;
            }
        }
    }
    
    #endregion
}