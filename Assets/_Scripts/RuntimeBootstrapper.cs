using UnityEngine;

/// <summary>
/// Ensures persistent runtime services exist before the first scene loads.
/// </summary>
public static class RuntimeBootstrapper
{
    /// <summary>
    /// Creates required persistent services when missing.
    /// </summary>
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void Bootstrap()
    {
        if (SettingsManager.Instance != null)
        {
            return;
        }

        GameObject settingsManagerObject = new GameObject("SettingsManager");
        settingsManagerObject.tag = "SettingsManager";
        settingsManagerObject.AddComponent<SettingsManager>();
    }
}
