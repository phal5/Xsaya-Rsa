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

    /// <summary>
    /// 지금 보이는 상태인지. 같은 상태로 다시 페이드하는 것을 막는 데 쓴다.
    ///
    /// 초기값이 true인 이유: 이 컴포넌트가 꺼져 있으면 Start가 돌지 않고,
    /// 그 경우 요소들은 에디터에서 authoring한 그대로 — 즉 보이는 채로 — 남는다.
    /// </summary>
    private bool _visible = true;

    public bool Visible => _visible;

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
            SetActivity(false);
            _visible = false;
        }
        else
        {
            SetActivity(true);
            ApplyAlphaToAll(1);
            _visible = true;
        }
    }
    
    #endregion

    #region Public API
    
    public void FadeIn()
    {
        // 이미 보이는 것을 다시 페이드하면 알파가 0으로 튕겼다가 올라온다. 깜빡임이 된다.
        if (_visible) return;
        _visible = true;

        SetActivity(true);


        _fadeEngine.PlayFade(
            duration: _fadeDuration,
            fadingIn: true,
            onUpdate: ApplyAlphaToAll
        );
    }

    public void FadeOut()
    {
        if (!gameObject.activeInHierarchy) return;
        if (!_visible) return;
        _visible = false;

        _fadeEngine.PlayFade(
            duration: _fadeDuration,
            fadingIn: false,
            onUpdate: ApplyAlphaToAll,
            onComplete: () => SetActivity(false)
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

    private void SetActivity(bool active)
    {
        foreach (Graphic graphic in _uiElements)
        {
            graphic.gameObject.SetActive(active);
        }
    }
    
    #endregion
}