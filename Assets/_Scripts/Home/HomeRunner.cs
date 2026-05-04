using UnityEngine;
using CursedDepths.Core.Events;
using CursedDepths.Core.Settings;
using UnityEngine.SceneManagement;

/// <summary>
/// Coordinates Home scene actions and scene transitions.
/// </summary>
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

    /// <summary>
    /// Requests opening the settings menu.
    /// </summary>
    public void OpenSettings()
    {
        GameEvents.OpenSettings();
    }

    /// <summary>
    /// Requests closing the settings menu using currently loaded settings.
    /// </summary>
    public void CloseSettings()
    {
        if (currentSettings == null)
            return;

        GameEvents.CloseSettingsMenu(new ClosedSettingsMenuEventArgs(currentSettings));
    }

    /// <summary>
    /// Closes the application or exits play mode in the editor.
    /// </summary>
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

    /// <summary>
    /// Loads the main gameplay scene.
    /// </summary>
    public void StartGame()
    {
        SceneManager.LoadScene("Starting Area");
    }
}
