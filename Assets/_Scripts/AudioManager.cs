using UnityEngine;
using CursedDepths.Core.Events;
using CursedDepths.Core.Settings;

public class AudioManager : MonoBehaviour
{
    public AudioSource MusicSource;
    public AudioSource SoundEffectsSource;

    private void OnEnable()
    {
        GameEvents.PlayerSettingsLoaded += ApplySettings;
    }

    private void OnDisable()
    {
        GameEvents.PlayerSettingsLoaded -= ApplySettings;
    }

    private void ApplySettings(PlayerSettings settings)
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