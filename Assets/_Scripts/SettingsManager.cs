using UnityEngine;
using CursedDepths.Core.Events;
using CursedDepths.Core.Settings;
using UnityEngine.UI;
using System;
using TMPro;

public class SettingsManager : MonoBehaviour
{
    public PlayerSettings playerSettings;

    public Slider MasterVolSlider;
    public Slider MusicVolSlider;
    public Slider EffectsVolSlider;

    public Button WalkLeftButton;
    public Button WalkRightButton;
    public Button JumpButton;
    public Button AttackButton;

    public TextMeshProUGUI WalkLeftKeyTxt;
    public TextMeshProUGUI WalkRightKeyTxt;
    public TextMeshProUGUI JumpKeyTxt;
    public TextMeshProUGUI AttackKeyTxt;

    private enum BindTarget { None, WalkLeft, WalkRight, Jump, Attack }

    private BindTarget waitingForBind = BindTarget.None;
    private KeyCode previousBind;
    private TextMeshProUGUI activeBindText;

    private bool ignoreNextInput;
    private float rebindStartTime;
    private const float inputIgnoreTime = 0.1f;

    private void OnEnable()
    {
        GameEvents.GameStartupRequested += LoadSettings;
        GameEvents.GameStartupRequested += UpdateSliders;
        GameEvents.GameStartupRequested += UpdateBindTexts;
        GameEvents.OpenSettingsMenu += LoadSettings;
        GameEvents.OpenSettingsMenu += UpdateSliders;
        GameEvents.OpenSettingsMenu += UpdateBindTexts;
        GameEvents.ClosedSettingsMenu += SaveSettings;
        GameEvents.SettingsSaved += SaveSettings;
    }

    private void OnDisable()
    {
        GameEvents.GameStartupRequested -= LoadSettings;
        GameEvents.GameStartupRequested -= UpdateSliders;
        GameEvents.GameStartupRequested -= UpdateBindTexts;
        GameEvents.OpenSettingsMenu -= LoadSettings;
        GameEvents.OpenSettingsMenu -= UpdateSliders;
        GameEvents.OpenSettingsMenu -= UpdateBindTexts;
        GameEvents.ClosedSettingsMenu -= SaveSettings;
        GameEvents.SettingsSaved -= SaveSettings;
    }

    private void Update()
    {
        if (waitingForBind == BindTarget.None)
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

        if (IsKeyAlreadyTaken(pressedKey, waitingForBind))
        {
            SetBind(waitingForBind, previousBind);
            activeBindText.text = previousBind.ToString();
        }
        else
        {
            SetBind(waitingForBind, pressedKey);
            activeBindText.text = pressedKey.ToString();
            GameEvents.SaveSettings(new SettingsSavedEventArgs(playerSettings));
        }

        FinishRebind();
    }

    private void LoadSettings()
    {
        playerSettings = new PlayerSettings();

        playerSettings.MasterVolume = PlayerPrefs.GetFloat("MasterVolume", 100f);
        playerSettings.MusicVolume = PlayerPrefs.GetFloat("MusicVolume", 100f);
        playerSettings.SoundEffects = PlayerPrefs.GetFloat("SoundEffectsVolume", 100f);

        playerSettings.WalkLeft = GetKey("WalkLeftBind", KeyCode.A);
        playerSettings.WalkRight = GetKey("WalkRightBind", KeyCode.D);
        playerSettings.Jump = GetKey("JumpBind", KeyCode.Space);
        playerSettings.Attack = GetKey("AttackBind", KeyCode.Mouse0);
    }

    KeyCode GetKey(string prefKey, KeyCode defaultKey)
    {
        string key = PlayerPrefs.GetString(prefKey, "");

        if (Enum.TryParse(key, out KeyCode keyCode))
            return keyCode;

        return defaultKey;
    }

    private void SaveSettings(ClosedSettingsMenuEventArgs arg)
    {
        SaveSettings(arg.playerSettings);
    }

    private void SaveSettings(SettingsSavedEventArgs arg)
    {
        SaveSettings(arg.playerSettings);
    }

    void SaveSettings(PlayerSettings settings)
    {
        PlayerPrefs.SetFloat("MasterVolume", settings.MasterVolume);
        PlayerPrefs.SetFloat("MusicVolume", settings.MusicVolume);
        PlayerPrefs.SetFloat("SoundEffectsVolume", settings.SoundEffects);

        PlayerPrefs.SetString("WalkLeftBind", settings.WalkLeft.ToString());
        PlayerPrefs.SetString("WalkRightBind", settings.WalkRight.ToString());
        PlayerPrefs.SetString("JumpBind", settings.Jump.ToString());
        PlayerPrefs.SetString("AttackBind", settings.Attack.ToString());

        PlayerPrefs.Save();
    }

    private void UpdateSliders()
    {
        MasterVolSlider.SetValueWithoutNotify(playerSettings.MasterVolume);
        MusicVolSlider.SetValueWithoutNotify(playerSettings.MusicVolume);
        EffectsVolSlider.SetValueWithoutNotify(playerSettings.SoundEffects);
    }

    public void UpdateMasterVol()
    {
        if (waitingForBind != BindTarget.None) return;

        playerSettings.MasterVolume = MasterVolSlider.value;
        GameEvents.SaveSettings(new SettingsSavedEventArgs(playerSettings));
    }

    public void UpdateMusicVol()
    {
        if (waitingForBind != BindTarget.None) return;

        playerSettings.MusicVolume = MusicVolSlider.value;
        GameEvents.SaveSettings(new SettingsSavedEventArgs(playerSettings));
    }

    public void UpdateEffectVol()
    {
        if (waitingForBind != BindTarget.None) return;

        playerSettings.SoundEffects = EffectsVolSlider.value;
        GameEvents.SaveSettings(new SettingsSavedEventArgs(playerSettings));
    }

    public void RebindWalkLeft()
    {
        StartRebind(BindTarget.WalkLeft, WalkLeftKeyTxt);
    }

    public void RebindWalkRight()
    {
        StartRebind(BindTarget.WalkRight, WalkRightKeyTxt);
    }

    public void RebindJump()
    {
        StartRebind(BindTarget.Jump, JumpKeyTxt);
    }

    public void RebindAttack()
    {
        StartRebind(BindTarget.Attack, AttackKeyTxt);
    }

    private void StartRebind(BindTarget target, TextMeshProUGUI text)
    {
        if (waitingForBind != BindTarget.None)
            return;

        waitingForBind = target;
        previousBind = GetBind(target);
        activeBindText = text;

        activeBindText.text = "";

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
        UpdateBindTexts();
    }

    private void SetUIInteractable(bool interactable)
    {
        MasterVolSlider.interactable = interactable;
        MusicVolSlider.interactable = interactable;
        EffectsVolSlider.interactable = interactable;

        WalkLeftButton.interactable = interactable;
        WalkRightButton.interactable = interactable;
        JumpButton.interactable = interactable;
        AttackButton.interactable = interactable;
    }

    private KeyCode GetBind(BindTarget target)
    {
        return target switch
        {
            BindTarget.WalkLeft => playerSettings.WalkLeft,
            BindTarget.WalkRight => playerSettings.WalkRight,
            BindTarget.Jump => playerSettings.Jump,
            BindTarget.Attack => playerSettings.Attack,
            _ => KeyCode.None
        };
    }

    private void SetBind(BindTarget target, KeyCode key)
    {
        switch (target)
        {
            case BindTarget.WalkLeft:
                playerSettings.WalkLeft = key;
                break;

            case BindTarget.WalkRight:
                playerSettings.WalkRight = key;
                break;

            case BindTarget.Jump:
                playerSettings.Jump = key;
                break;

            case BindTarget.Attack:
                playerSettings.Attack = key;
                break;
        }
    }

    private bool IsKeyAlreadyTaken(KeyCode key, BindTarget currentTarget)
    {
        return currentTarget != BindTarget.WalkLeft && playerSettings.WalkLeft == key ||
               currentTarget != BindTarget.WalkRight && playerSettings.WalkRight == key ||
               currentTarget != BindTarget.Jump && playerSettings.Jump == key ||
               currentTarget != BindTarget.Attack && playerSettings.Attack == key;
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

    private void UpdateBindTexts()
    {
        WalkLeftKeyTxt.text = playerSettings.WalkLeft.ToString();
        WalkRightKeyTxt.text = playerSettings.WalkRight.ToString();
        JumpKeyTxt.text = playerSettings.Jump.ToString();
        AttackKeyTxt.text = playerSettings.Attack.ToString();
    }
}