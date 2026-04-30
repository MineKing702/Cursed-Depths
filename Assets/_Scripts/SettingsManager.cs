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
    public TextMeshProUGUI WalkLeftKeyTxt;
    public TextMeshProUGUI WalkRightKeyTxt;
    public TextMeshProUGUI JumpKeyTxt;
    public TextMeshProUGUI AttackKeyTxt;

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

    KeyCode GetKey(string prefKey, KeyCode DefaultKey)
    {
        string key = PlayerPrefs.GetString(prefKey, "").ToUpper();

        if (Enum.TryParse(key, out KeyCode keyCode))
        {
            return keyCode;
        }

        return DefaultKey;
    }

    private void SaveSettings(ClosedSettingsMenuEventArgs arg)
    {
        PlayerPrefs.SetFloat("MasterVolume", arg.playerSettings.MasterVolume);
        PlayerPrefs.SetFloat("MusicVolume", arg.playerSettings.MusicVolume);
        PlayerPrefs.SetFloat("SoundEffectsVolume", arg.playerSettings.SoundEffects);

        PlayerPrefs.SetString("WalkLeftBind", arg.playerSettings.WalkLeft.ToString());
        PlayerPrefs.SetString("WalkRightBind", arg.playerSettings.WalkRight.ToString());
        PlayerPrefs.SetString("JumpBind", arg.playerSettings.Jump.ToString());
        PlayerPrefs.SetString("AttackBind", arg.playerSettings.Attack.ToString());

        PlayerPrefs.Save();
    }
    private void SaveSettings(SettingsSavedEventArgs arg)
    {
        PlayerPrefs.SetFloat("MasterVolume", arg.playerSettings.MasterVolume);
        PlayerPrefs.SetFloat("MusicVolume", arg.playerSettings.MusicVolume);
        PlayerPrefs.SetFloat("SoundEffectsVolume", arg.playerSettings.SoundEffects);

        PlayerPrefs.SetString("WalkLeftBind", arg.playerSettings.WalkLeft.ToString());
        PlayerPrefs.SetString("WalkRightBind", arg.playerSettings.WalkRight.ToString());
        PlayerPrefs.SetString("JumpBind", arg.playerSettings.Jump.ToString());
        PlayerPrefs.SetString("AttackBind", arg.playerSettings.Attack.ToString());
        PlayerPrefs.Save();
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
        playerSettings.MasterVolume = MasterVolSlider.value;
        GameEvents.SaveSettings(new SettingsSavedEventArgs(playerSettings));
    }
    public void UpdateMusicVol()
    {
        playerSettings.MusicVolume = MusicVolSlider.value;
        GameEvents.SaveSettings(new SettingsSavedEventArgs(playerSettings));
    }
    public void UpdateEffectVol()
    {
        playerSettings.SoundEffects = EffectsVolSlider.value;
        GameEvents.SaveSettings(new SettingsSavedEventArgs(playerSettings));
    }

    void UpdateBindTexts()
    {
        WalkLeftKeyTxt.text = playerSettings.WalkLeft.ToString();
        WalkRightKeyTxt.text = playerSettings.WalkRight.ToString();
        JumpKeyTxt.text = playerSettings.Jump.ToString();
        AttackKeyTxt.text = playerSettings.Attack.ToString();
    }
}