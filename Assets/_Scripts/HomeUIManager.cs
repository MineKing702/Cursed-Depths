using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CursedDepths.Core.Events;
using CursedDepths.Core.Settings;

public class HomeUIManager : MonoBehaviour
{
    public Image FadeInCover;
    public float fadeInDuration = 1f;
    public float fadeInDelay = 0.25f;

    public Animator HomeUIParentAnim;
    public Animator SettingsUIParentAnim;

    public Slider MasterVol;
    public Slider MusicVol;
    public Slider EffectsVol;

    public Button WalkLeftButton;
    public Button WalkRightButton;
    public Button JumpButton;
    public Button AttackButton;

    public TextMeshProUGUI WalkLeftKeyTxt;
    public TextMeshProUGUI WalkRightKeyTxt;
    public TextMeshProUGUI JumpKeyTxt;
    public TextMeshProUGUI AttackKeyTxt;

    private PlayerSettings currentSettings;

    private enum BindTarget { None, WalkLeft, WalkRight, Jump, Attack }

    private BindTarget waitingForBind = BindTarget.None;
    private KeyCode previousBind;
    private TextMeshProUGUI activeBindText;

    private bool ignoreNextInput;
    private float rebindStartTime;
    private const float inputIgnoreTime = 0.1f;

    private void OnEnable()
    {
        GameEvents.GameStartupRequested += FadeInHomescreen;
        GameEvents.SettingsLoaded += OnSettingsLoaded;
        GameEvents.SettingsSaved += OnSettingsSaved;
        GameEvents.OpenSettingsMenu += OpenSettings;
        GameEvents.ClosedSettingsMenu += CloseSettings;
    }

    private void OnDisable()
    {
        GameEvents.GameStartupRequested -= FadeInHomescreen;
        GameEvents.SettingsLoaded -= OnSettingsLoaded;
        GameEvents.SettingsSaved -= OnSettingsSaved;
        GameEvents.OpenSettingsMenu -= OpenSettings;
        GameEvents.ClosedSettingsMenu -= CloseSettings;
    }

    private void Update()
    {
        if (waitingForBind == BindTarget.None || currentSettings == null)
            return;

        if (ignoreNextInput)
        {
            if (Time.time - rebindStartTime < inputIgnoreTime)
                return;

            if (Input.GetMouseButton(0))
                return;

            ignoreNextInput = false;
        }

        KeyCode pressedKey = GetPressedKey();
        if (pressedKey == KeyCode.None)
            return;

        if (IsKeyAlreadyTaken(pressedKey, waitingForBind, currentSettings))
        {
            SetBind(waitingForBind, previousBind, currentSettings);
            activeBindText.text = previousBind.ToString();
        }
        else
        {
            SetBind(waitingForBind, pressedKey, currentSettings);
            activeBindText.text = pressedKey.ToString();
            GameEvents.SaveSettings(new SettingsSavedEventArgs(currentSettings));
        }

        FinishRebind();
    }

    private void FadeInHomescreen()
    {
        StartCoroutine(FadeInHome());
    }

    private IEnumerator FadeInHome()
    {
        if (FadeInCover == null)
            yield break;

        FadeInCover.gameObject.SetActive(true);

        yield return new WaitForSeconds(fadeInDelay);

        float startAlpha = FadeInCover.color.a;
        float time = 0f;

        while (time < fadeInDuration)
        {
            time += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, time / fadeInDuration);

            Color color = FadeInCover.color;
            color.a = Mathf.Lerp(startAlpha, 0f, t);
            FadeInCover.color = color;

            yield return null;
        }

        Color finalColor = FadeInCover.color;
        finalColor.a = 0f;
        FadeInCover.color = finalColor;
        FadeInCover.gameObject.SetActive(false);

        GameEvents.FinishGameStartup();
    }

    public void OpenSettings()
    {
        StartCoroutine(OpenSettingsMenu());
    }

    private IEnumerator OpenSettingsMenu()
    {
        SettingsUIParentAnim.gameObject.SetActive(true);
        HomeUIParentAnim.Play("UiSlideFadeOut");
        SettingsUIParentAnim.Play("UiSlideFadeIn");

        yield return new WaitForSeconds(.5f);

        HomeUIParentAnim.gameObject.SetActive(false);
    }

    public void CloseSettings(ClosedSettingsMenuEventArgs arg)
    {
        StartCoroutine(CloseSettingsMenu());
    }

    private IEnumerator CloseSettingsMenu()
    {
        HomeUIParentAnim.gameObject.SetActive(true);
        SettingsUIParentAnim.Play("UiSlideFadeOut");
        HomeUIParentAnim.Play("UiSlideFadeIn");

        yield return new WaitForSeconds(.2f);

        SettingsUIParentAnim.gameObject.SetActive(false);
    }

    private void OnSettingsLoaded(SettingsLoadedEventArgs arg)
    {
        currentSettings = arg.playerSettings;
        RefreshSettingsUI();
    }

    private void OnSettingsSaved(SettingsSavedEventArgs arg)
    {
        currentSettings = arg.playerSettings;
    }

    private void RefreshSettingsUI()
    {
        if (currentSettings == null)
            return;

        UpdateVolumeSliders(currentSettings);
        UpdateBindTexts(currentSettings);
        SetUIInteractable(true);
    }

    private void UpdateVolumeSliders(PlayerSettings settings)
    {
        MasterVol?.SetValueWithoutNotify(settings.MasterVolume);
        MusicVol?.SetValueWithoutNotify(settings.MusicVolume);
        EffectsVol?.SetValueWithoutNotify(settings.SoundEffects);
    }

    private void UpdateBindTexts(PlayerSettings settings)
    {
        if (WalkLeftKeyTxt != null) WalkLeftKeyTxt.text = settings.WalkLeft.ToString();
        if (WalkRightKeyTxt != null) WalkRightKeyTxt.text = settings.WalkRight.ToString();
        if (JumpKeyTxt != null) JumpKeyTxt.text = settings.Jump.ToString();
        if (AttackKeyTxt != null) AttackKeyTxt.text = settings.Attack.ToString();
    }

    public void UpdateMasterVol()
    {
        if (!CanApplySliderChange()) return;

        currentSettings.MasterVolume = MasterVol.value;
        GameEvents.SaveSettings(new SettingsSavedEventArgs(currentSettings));
    }

    public void UpdateMusicVol()
    {
        if (!CanApplySliderChange()) return;

        currentSettings.MusicVolume = MusicVol.value;
        GameEvents.SaveSettings(new SettingsSavedEventArgs(currentSettings));
    }

    public void UpdateEffectVol()
    {
        if (!CanApplySliderChange()) return;

        currentSettings.SoundEffects = EffectsVol.value;
        GameEvents.SaveSettings(new SettingsSavedEventArgs(currentSettings));
    }

    private bool CanApplySliderChange()
    {
        return waitingForBind == BindTarget.None && currentSettings != null;
    }

    public void RebindWalkLeft() => StartRebind(BindTarget.WalkLeft, WalkLeftKeyTxt);
    public void RebindWalkRight() => StartRebind(BindTarget.WalkRight, WalkRightKeyTxt);
    public void RebindJump() => StartRebind(BindTarget.Jump, JumpKeyTxt);
    public void RebindAttack() => StartRebind(BindTarget.Attack, AttackKeyTxt);

    private void StartRebind(BindTarget target, TextMeshProUGUI text)
    {
        if (waitingForBind != BindTarget.None || currentSettings == null)
            return;

        waitingForBind = target;
        previousBind = GetBind(target, currentSettings);
        activeBindText = text;

        if (activeBindText != null)
            activeBindText.text = string.Empty;

        ignoreNextInput = true;
        rebindStartTime = Time.time;

        SetUIInteractable(false);
    }

    private void FinishRebind()
    {
        waitingForBind = BindTarget.None;
        activeBindText = null;
        ignoreNextInput = false;

        SetUIInteractable(true);
        UpdateBindTexts(currentSettings);
    }

    private void SetUIInteractable(bool interactable)
    {
        if (MasterVol != null) MasterVol.interactable = interactable;
        if (MusicVol != null) MusicVol.interactable = interactable;
        if (EffectsVol != null) EffectsVol.interactable = interactable;

        if (WalkLeftButton != null) WalkLeftButton.interactable = interactable;
        if (WalkRightButton != null) WalkRightButton.interactable = interactable;
        if (JumpButton != null) JumpButton.interactable = interactable;
        if (AttackButton != null) AttackButton.interactable = interactable;
    }

    private KeyCode GetBind(BindTarget target, PlayerSettings settings)
    {
        return target switch
        {
            BindTarget.WalkLeft => settings.WalkLeft,
            BindTarget.WalkRight => settings.WalkRight,
            BindTarget.Jump => settings.Jump,
            BindTarget.Attack => settings.Attack,
            _ => KeyCode.None
        };
    }

    private void SetBind(BindTarget target, KeyCode key, PlayerSettings settings)
    {
        switch (target)
        {
            case BindTarget.WalkLeft:
                settings.WalkLeft = key;
                break;
            case BindTarget.WalkRight:
                settings.WalkRight = key;
                break;
            case BindTarget.Jump:
                settings.Jump = key;
                break;
            case BindTarget.Attack:
                settings.Attack = key;
                break;
        }
    }

    private bool IsKeyAlreadyTaken(KeyCode key, BindTarget currentTarget, PlayerSettings settings)
    {
        return currentTarget != BindTarget.WalkLeft && settings.WalkLeft == key ||
               currentTarget != BindTarget.WalkRight && settings.WalkRight == key ||
               currentTarget != BindTarget.Jump && settings.Jump == key ||
               currentTarget != BindTarget.Attack && settings.Attack == key;
    }

    private KeyCode GetPressedKey()
    {
        foreach (KeyCode keyCode in Enum.GetValues(typeof(KeyCode)))
        {
            if (Input.GetKeyDown(keyCode))
                return keyCode;
        }

        return KeyCode.None;
    }
}
