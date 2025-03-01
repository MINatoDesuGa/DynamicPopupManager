using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;
/// <summary>
/// Utility class to handle multiple dynamic popups/prompts in game using one UI element
/// Feel free to extend this as per your requirements 
/// Special thanks to Kenney or www.kenney.nl for UI assets
/// </summary>
public class DynamicPopupManager : UtilityFunctions {
    public static DynamicPopupManager Instance { get; private set; }

    private const float BG_ADJUST_xOFFSET = 100f;
    private const float POPUP_HEIGHT = 70f;
    private const float POPUP_SCALE_DURATION = 0.2f;

    [Header("References")]
    [SerializeField]
    private Image _popupTypeImage;
    [SerializeField,]
    private Image _popupBG;
    [SerializeField]
    private TMPro.TMP_Text _popupText;
    [SerializeField]
    private RectTransform _layoutParentTransform;
    [SerializeField]
    private Transform _popupHolder;

    [SerializeField]
    private List<PopupImage> _popupImagesRefs;
    private Dictionary<PopupType, Sprite> PopupImages = new();

    [Space(10)]
    [Header("Properties")]
    [SerializeField]
    private Color _errorBgColor = Color.red;
    [SerializeField]
    private Color _successBgColor = Color.green;
    [SerializeField]
    private float _popupDuration = 2f;

    private WaitForSeconds _popupDelay;

    //=======================================================================================//
    private void Awake() {
        if(Instance == null) {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        } else {
            Destroy(this);
        }
    }
    private void Start() {
        Init();
    }
    //testing purpose
    private void Update() {
        if(Input.GetKeyDown(KeyCode.T)) { 
            TriggerPopup(new Popup(PopupType.Error, "You dont have enough coins!"));
        }
        if (Input.GetKeyDown(KeyCode.R)) {
            TriggerPopup(new Popup(PopupType.Success, "Weapon upgraded to Lvl 2!"));
        }
        if (Input.GetKeyDown(KeyCode.Y)) {
            TriggerPopup(new Popup("Welcome to the world!", Color.blue));
        }
    }
    //=======================================================================================//
    public void TriggerPopup(Popup popup) {
        ResetPopup();
        
        ActivatePopupTypeImage(true);
        if(PopupImages.ContainsKey(popup.Type)) {
            _popupTypeImage.sprite = PopupImages[popup.Type];
        }
        switch (popup.Type) {
            case PopupType.Error:
                _popupBG.color = _errorBgColor;
                break;
            case PopupType.Success:
                _popupBG.color = _successBgColor;
                break;
            case PopupType.OnlyText:
                ActivatePopupTypeImage(false);
                _popupBG.color = popup.BgColor;
                break;
        }

        _popupText.text = popup.TextToDisplay;
        _currentRunningCoroutine = StartCoroutine(PopupInit());
    }
    private void ResetPopup() {
        ResetCoroutine();
        _popupHolder.localScale = Vector3.zero;
    }
    /// <summary>
    /// Adjust BG Image width based on prompt text length (imageWidth + textWidth + BG_ADJUST_xOFFSET)
    /// </summary>
    private void AdjustBGImageWidth() {
        var offsetFactor = _popupText.rectTransform.sizeDelta.x + BG_ADJUST_xOFFSET;
        if (_popupTypeImage.gameObject.activeSelf) {
            offsetFactor += _popupTypeImage.rectTransform.sizeDelta.x; 
        }
        
        _popupBG.rectTransform.sizeDelta = (Vector2.right * offsetFactor) + (Vector2.up * POPUP_HEIGHT);
    }
    private void ActivatePopupTypeImage(bool active) => _popupTypeImage.gameObject.SetActive(active);
    private void Init() {
        _popupDelay = new(_popupDuration);
        ResetPopup();

        foreach(var popupImageRefs in _popupImagesRefs) {
            PopupImages[popupImageRefs.PopupType] = popupImageRefs.PopupTypeImage;    
        }
        _popupImagesRefs.Clear();
    }
    private IEnumerator PopupInit() {
        StartCoroutine( ScalePopup(true) );
        yield return null;
        //as popupType icon is dynamically adjusted based on prompt text length via layoutGroup
        //refreshing layout after text update to reflect the change
        LayoutRebuilder.ForceRebuildLayoutImmediate(_layoutParentTransform);
        AdjustBGImageWidth();
        
        yield return _popupDelay;
        StartCoroutine( ScalePopup(false) );
    }
    private IEnumerator ScalePopup(bool active) {
        Vector3 initialScale = _popupHolder.localScale;
        Vector3 targetScale = active ? Vector3.one : Vector3.zero;

        float timeElapsed = 0f;

        while (timeElapsed < POPUP_SCALE_DURATION) {
            timeElapsed += Time.deltaTime;
            float scaleValue = Mathf.Lerp(0f, 1f, timeElapsed / POPUP_SCALE_DURATION);
            _popupHolder.localScale = Vector3.Lerp(initialScale, targetScale, scaleValue);

            yield return null; 
        }
        _popupHolder.localScale = targetScale;
    }
}

//########################################################################//
[Serializable]
public struct PopupImage {
    public PopupType PopupType;
    public Sprite PopupTypeImage;
}
public struct Popup {
    public PopupType Type { get; private set; }
    public Color BgColor { get; private set; }
    public string TextToDisplay { get; private set; }
    public Popup(string text, Color bgColor) { //this constructor is for just text based popups
        this.Type = PopupType.OnlyText;
        this.BgColor = bgColor;
        this.TextToDisplay = text;
    }
    public Popup(PopupType type, string text) {
        this.Type = type;
        this.BgColor = Color.red;
        this.TextToDisplay = text;
    }
}
public enum PopupType {
    Error, Success, OnlyText
}