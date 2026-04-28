using UnityEngine;
using CursedDepths.Core.Events;
using CursedDepths.Core.Settings;
using UnityEngine.UI;

public class SettingsManager : MonoBehaviour
{
    public PlayerSettings playerSettings;

    public Slider MasterVolSlider;
    public Slider MusicVolSlider;
    public Slider EffectsVolSlider;

    private void OnEnable()
    {
        GameEvents.GameStartupRequested += LoadSettings;
        GameEvents.OpenSettingsMenu += LoadSettings;
        GameEvents.OpenSettingsMenu += UpdateSliders;
        GameEvents.ClosedSettingsMenu += SaveSettings;
    }

    private void OnDisable()
    {
        GameEvents.GameStartupRequested -= LoadSettings;
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
    }
    void SaveSettings(PlayerSettings settings)
    {
        PlayerPrefs.SetFloat("MasterVolume", settings.MasterVolume);
        PlayerPrefs.SetFloat("MusicVolume", settings.MusicVolume);
        PlayerPrefs.SetFloat("SoundEffectsVolume", settings.SoundEffects);

        PlayerPrefs.Save();
    }

    void UpdateSliders()
    {
        MasterVolSlider.value = playerSettings.MasterVolume / 100;
        MusicVolSlider.value = playerSettings.MusicVolume / 100;
        EffectsVolSlider.value = playerSettings.SoundEffects / 100;
    }

    public void UpdateMasterVol()
    {
        playerSettings.MasterVolume = MasterVolSlider.value;
        SaveSettings(playerSettings);
    }
    public void UpdateMusicVol()
    {
        playerSettings.MusicVolume = MusicVolSlider.value;
        SaveSettings(playerSettings);
    }
    public void UpdateEffectVol()
    {
        playerSettings.SoundEffects = EffectsVolSlider.value;
        SaveSettings(playerSettings);
    }
}