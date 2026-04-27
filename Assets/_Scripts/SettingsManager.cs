using UnityEngine;
using CursedDepths.Core.Events;
using CursedDepths.Core.Settings;

public class SettingsManager : MonoBehaviour
{
    private PlayerSettings playerSettings;

    private void OnEnable()
    {
        GameEvents.GameStartupRequested += LoadSettings;
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

        GameEvents.LoadPlayerSettings(playerSettings);
    }

    public void SaveSettings(PlayerSettings settings)
    {
        PlayerPrefs.SetFloat("MasterVolume", settings.MasterVolume);
        PlayerPrefs.SetFloat("MusicVolume", settings.MusicVolume);
        PlayerPrefs.SetFloat("SoundEffectsVolume", settings.SoundEffects);

        PlayerPrefs.Save();

        GameEvents.LoadPlayerSettings(settings);
    }
}