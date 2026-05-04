using UnityEngine;
using CursedDepths.Core.Events;
using CursedDepths.Core.Settings;
using UnityEngine.SceneManagement;

public class HomeRunner : MonoBehaviour
{
    private PlayerSettings currentSettings;

    private void OnEnable()
    {
        GameEvents.SettingsLoaded += OnSettingsLoaded;
        GameEvents.SettingsSaved += OnSettingsSaved;
    }

    private void OnDisable()
    {
        GameEvents.SettingsLoaded -= OnSettingsLoaded;
        GameEvents.SettingsSaved -= OnSettingsSaved;
    }

    private void Start()
    {
        GameEvents.RequestGameStartup();
    }

    public void OpenSettings()
    {
        GameEvents.OpenSettings();
    }

    public void CloseSettings()
    {
        if (currentSettings == null)
            return;

        GameEvents.CloseSettingsMenu(new ClosedSettingsMenuEventArgs(currentSettings));
    }

    public void CloseGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void OnSettingsLoaded(SettingsLoadedEventArgs arg)
    {
        currentSettings = arg.playerSettings;
    }

    private void OnSettingsSaved(SettingsSavedEventArgs arg)
    {
        currentSettings = arg.playerSettings;
    }

    public void StartGame()
    {
        SceneManager.LoadScene("Starting Area");
    }
}
