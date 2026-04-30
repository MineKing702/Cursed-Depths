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
        ApplyVolumes(arg.playerSettings);
    }

    private void ApplySettings(SettingsSavedEventArgs arg)
    {
        ApplyVolumes(arg.playerSettings);
    }

    private void ApplyVolumes(PlayerSettings settings)
    {
        float master = settings.MasterVolume / 100f;
        float music = settings.MusicVolume / 100f;
        float sfx = settings.SoundEffects / 100f;

        if (MusicSource != null)
            MusicSource.volume = master * music;

        if (SoundEffectsSource != null)
            SoundEffectsSource.volume = master * sfx;
    }
}
