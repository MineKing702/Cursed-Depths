using System;
using UnityEngine;
using CursedDepths.Core.Events;
using CursedDepths.Core.Settings;

/// <summary>
/// Owns player settings lifecycle, persistence, and event wiring for the game.
/// </summary>
public sealed class SettingsManager : MonoBehaviour
{
    private const string SettingsObjectName = "SettingsManager";

    /// <summary>
    /// Gets the global settings manager instance.
    /// </summary>
    public static SettingsManager Instance { get; private set; }

    /// <summary>
    /// Gets the current in-memory player settings.
    /// </summary>
    public PlayerSettings CurrentSettings { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        gameObject.name = SettingsObjectName;
        DontDestroyOnLoad(gameObject);

        CurrentSettings ??= CreateDefaultSettings();
    }

    private void OnEnable()
    {
        GameEvents.GameStartupRequested += LoadAndBroadcastSettings;
        GameEvents.OpenSettingsMenu += LoadAndBroadcastSettings;
        GameEvents.ClosedSettingsMenu += SaveSettings;
        GameEvents.SettingsSaved += SaveSettings;
    }

    private void OnDisable()
    {
        GameEvents.GameStartupRequested -= LoadAndBroadcastSettings;
        GameEvents.OpenSettingsMenu -= LoadAndBroadcastSettings;
        GameEvents.ClosedSettingsMenu -= SaveSettings;
        GameEvents.SettingsSaved -= SaveSettings;
    }

    /// <summary>
    /// Gets current settings, loading persisted values if needed.
    /// </summary>
    /// <returns>The active player settings instance.</returns>
    public PlayerSettings GetOrLoadSettings()
    {
        if (CurrentSettings == null)
        {
            LoadSettings();
        }

        return CurrentSettings;
    }

    private void LoadAndBroadcastSettings()
    {
        LoadSettings();
        GameEvents.LoadedSettings(new SettingsLoadedEventArgs(CurrentSettings));
    }

    private void LoadSettings()
    {
        CurrentSettings = new PlayerSettings
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

    private static KeyCode GetKey(string prefKey, KeyCode defaultKey)
    {
        string key = PlayerPrefs.GetString(prefKey, string.Empty);
        if (Enum.TryParse(key, out KeyCode keyCode))
        {
            return keyCode;
        }

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

    /// <summary>
    /// Saves the supplied settings and applies them as current runtime settings.
    /// </summary>
    /// <param name="settings">The settings to persist.</param>
    public void SaveSettings(PlayerSettings settings)
    {
        if (settings == null)
        {
            return;
        }

        CurrentSettings = settings;

        PlayerPrefs.SetFloat("MasterVolume", settings.MasterVolume);
        PlayerPrefs.SetFloat("MusicVolume", settings.MusicVolume);
        PlayerPrefs.SetFloat("SoundEffectsVolume", settings.SoundEffects);

        PlayerPrefs.SetString("WalkLeftBind", settings.WalkLeft.ToString());
        PlayerPrefs.SetString("WalkRightBind", settings.WalkRight.ToString());
        PlayerPrefs.SetString("JumpBind", settings.Jump.ToString());
        PlayerPrefs.SetString("AttackBind", settings.Attack.ToString());

        PlayerPrefs.Save();
    }

    private static PlayerSettings CreateDefaultSettings()
    {
        return new PlayerSettings
        {
            MasterVolume = 100f,
            MusicVolume = 100f,
            SoundEffects = 100f,
            WalkLeft = KeyCode.A,
            WalkRight = KeyCode.D,
            Jump = KeyCode.Space,
            Attack = KeyCode.Mouse0
        };
    }
}
