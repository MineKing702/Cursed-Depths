using UnityEngine;
using CursedDepths.Core.Events;
using CursedDepths.Core.Settings;
using UnityEngine.UI;
using System;

public class SettingsManager : MonoBehaviour
{
    public PlayerSettings playerSettings;

    public Slider MasterVolSlider;
    public Slider MusicVolSlider;
    public Slider EffectsVolSlider;

    private void OnEnable()
    {
        GameEvents.GameStartupRequested += LoadSettings;
        GameEvents.GameStartupRequested += UpdateSliders;
        GameEvents.OpenSettingsMenu += LoadSettings;
        GameEvents.OpenSettingsMenu += UpdateSliders;
        GameEvents.ClosedSettingsMenu += SaveSettings;
        GameEvents.SettingsSaved += SaveSettings;
    }

    private void OnDisable()
    {
        GameEvents.GameStartupRequested -= LoadSettings;
        GameEvents.GameStartupRequested -= UpdateSliders;
        GameEvents.OpenSettingsMenu -= LoadSettings;
        GameEvents.OpenSettingsMenu -= UpdateSliders;
        GameEvents.ClosedSettingsMenu -= SaveSettings;
        GameEvents.SettingsSaved -= SaveSettings;
    }

    private void LoadSettings()
    {
        playerSettings = new PlayerSettings();

        playerSettings.MasterVolume = PlayerPrefs.GetFloat("MasterVolume", 100f);
        playerSettings.MusicVolume = PlayerPrefs.GetFloat("MusicVolume", 100f);
        playerSettings.SoundEffects = PlayerPrefs.GetFloat("SoundEffectsVolume", 100f);
    }

    private void SaveSettings(ClosedSettingsMenuEventArgs arg)
    {
        PlayerPrefs.SetFloat("MasterVolume", arg.playerSettings.MasterVolume);
        PlayerPrefs.SetFloat("MusicVolume", arg.playerSettings.MusicVolume);
        PlayerPrefs.SetFloat("SoundEffectsVolume", arg.playerSettings.SoundEffects);

        PlayerPrefs.Save();

        // Debug.Log($"Saved Settings: MasterVol: {PlayerPrefs.GetFloat("MasterVolume", 100f)}, MusicVol{PlayerPrefs.GetFloat("MusicVolume", 100f)}, EffectVol{PlayerPrefs.GetFloat("SoundEffectsVolume", 100f)}");
    }
    private void SaveSettings(SettingsSavedEventArgs arg)
    {
        PlayerPrefs.SetFloat("MasterVolume", arg.playerSettings.MasterVolume);
        PlayerPrefs.SetFloat("MusicVolume", arg.playerSettings.MusicVolume);
        PlayerPrefs.SetFloat("SoundEffectsVolume", arg.playerSettings.SoundEffects);

        PlayerPrefs.Save();
    }
    void SaveSettings(PlayerSettings settings)
    {
        PlayerPrefs.SetFloat("MasterVolume", settings.MasterVolume);
        PlayerPrefs.SetFloat("MusicVolume", settings.MusicVolume);
        PlayerPrefs.SetFloat("SoundEffectsVolume", settings.SoundEffects);

        PlayerPrefs.Save();

        // Debug.Log($"Saved Settings: MasterVol: {PlayerPrefs.GetFloat("MasterVolume", 100f)}, MusicVol{PlayerPrefs.GetFloat("MusicVolume", 100f)}, EffectVol{PlayerPrefs.GetFloat("SoundEffectsVolume", 100f)}");
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
}