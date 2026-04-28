using UnityEngine;
using CursedDepths.Core.Events;

public class HomeRunner : MonoBehaviour
{
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
        GameEvents.CloseSettingsMenu(new ClosedSettingsMenuEventArgs(FindObjectOfType<SettingsManager>().playerSettings));
    }
}