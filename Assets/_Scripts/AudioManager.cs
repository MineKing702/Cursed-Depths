using UnityEngine;
using CursedDepths.Core.Events;
using CursedDepths.Core.Settings;

public class AudioManager : MonoBehaviour
{
    public AudioSource MusicSource;
    public AudioSource SoundEffectsSource;

    private void OnEnable()
    {
        GameEvents.SettingsLoaded += ApplySettings;
        GameEvents.SettingsSaved += ApplySettings;
    }

    private void OnDisable()
    {
        GameEvents.SettingsLoaded -= ApplySettings;
        GameEvents.SettingsSaved -= ApplySettings;
    }

    private void ApplySettings(SettingsLoadedEventArgs arg)
    {
        float master = arg.playerSettings.MasterVolume / 100f;
        float music = arg.playerSettings.MusicVolume / 100f;
        float sfx = arg.playerSettings.SoundEffects / 100f;

        if (MusicSource != null)
            MusicSource.volume = master * music;

        if (SoundEffectsSource != null)
            SoundEffectsSource.volume = master * sfx;
    }
    private void ApplySettings(SettingsSavedEventArgs arg)
    {
        float master = arg.playerSettings.MasterVolume / 100f;
        float music = arg.playerSettings.MusicVolume / 100f;
        float sfx = arg.playerSettings.SoundEffects / 100f;

        if (MusicSource != null)
            MusicSource.volume = master * music;

        if (SoundEffectsSource != null)
            SoundEffectsSource.volume = master * sfx;
    }
}