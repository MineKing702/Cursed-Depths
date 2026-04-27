using UnityEngine;
using CursedDepths.Core.Events;

public class HomeRunner : MonoBehaviour
{
    private void Start()
    {
        GameEvents.RequestGameStartup();
    }
}