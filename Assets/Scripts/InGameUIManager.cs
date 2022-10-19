using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;


public class InGameUIManager : MonoBehaviour
{
    [SerializeField] private GameObject _controlsPanel;
    [SerializeField] private GameObject _helpReminderPanel;
    private bool _controlsPanelVisible;

    private void Awake()
    {
        _controlsPanelVisible = false;
        _controlsPanel.SetActive(false);
        _helpReminderPanel.SetActive(true);
    }

    public void ToggleHelp(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            _controlsPanelVisible = !_controlsPanelVisible;
            _controlsPanel.SetActive(_controlsPanelVisible);
            _helpReminderPanel.SetActive(!_controlsPanelVisible);
        }
    }

}
