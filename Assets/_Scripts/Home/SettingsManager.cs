using System;
using UnityEngine;
using CursedDepths.Core.Events;
using CursedDepths.Core.Settings;

public class SettingsManager : MonoBehaviour
{
    public PlayerSettings playerSettings;

    private void OnEnable()
    {
        GameEvents.GameStartupRequested += LoadAndBroadcastSettings;
        GameEvents.OpenSettingsMenu += LoadAndBroadcastSettings;
        GameEvents.ClosedSettingsMenu += SaveSettings;
        GameEvents.SettingsSaved += SaveSettings;

        DontDestroyOnLoad(gameObject);
    }

    private void OnDisable()
    {
        GameEvents.GameStartupRequested -= LoadAndBroadcastSettings;
        GameEvents.OpenSettingsMenu -= LoadAndBroadcastSettings;
        GameEvents.ClosedSettingsMenu -= SaveSettings;
        GameEvents.SettingsSaved -= SaveSettings;
    }

    private void LoadAndBroadcastSettings()
    {
        LoadSettings();
        GameEvents.LoadedSettings(new SettingsLoadedEventArgs(playerSettings));
    }

    private void LoadSettings()
    {
        playerSettings = new PlayerSettings
        {
            MasterVolume = PlayerPrefs.GetFloat("MasterVolume", 100f),
            MusicVolume = PlayerPrefs.GetFloat("MusicVolume", 100f),
            SoundEffects = PlayerPrefs.GetFloat("SoundEffectsVolume", 100f),
            WalkLeft = GetKey("WalkLeftBind", KeyCode.A),
            WalkRight = GetKey("WalkRightBind", KeyCode.D),
            Jump = GetKey("JumpBind", KeyCode.Space),
            Attack = GetKey("AttackBind", KeyCode.Mouse0)
        };
    }

    private KeyCode GetKey(string prefKey, KeyCode defaultKey)
    {
        string key = PlayerPrefs.GetString(prefKey, string.Empty);

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

    private void SaveSettings(PlayerSettings settings)
    {
        if (settings == null)
            return;

        playerSettings = settings;

        PlayerPrefs.SetFloat("MasterVolume", settings.MasterVolume);
        PlayerPrefs.SetFloat("MusicVolume", settings.MusicVolume);
        PlayerPrefs.SetFloat("SoundEffectsVolume", settings.SoundEffects);

        PlayerPrefs.SetString("WalkLeftBind", settings.WalkLeft.ToString());
        PlayerPrefs.SetString("WalkRightBind", settings.WalkRight.ToString());
        PlayerPrefs.SetString("JumpBind", settings.Jump.ToString());
        PlayerPrefs.SetString("AttackBind", settings.Attack.ToString());

        PlayerPrefs.Save();
    }
}
