using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class InGameUIManager : MonoBehaviour
{
    [SerializeField] private GameObject _controlsPanel;
    [SerializeField] private GameObject _helpReminderPanel;
    [SerializeField] private GameObject _backMenuPanel;
    [SerializeField] private Slider _progressBarBacktoMenu;
    [SerializeField] private float _durationToGoBackToMenu;


    private bool _backMenuPanelVisible;
    private bool _controlsPanelVisible;
    private bool _quitButtonPressed;
    private float _backToMenuTimer;

    private void Awake()
    {
        _backToMenuTimer = 0f;
        _progressBarBacktoMenu.value = 0;

        _quitButtonPressed = false;
        _controlsPanelVisible = false;
        _backMenuPanelVisible = false;

        _controlsPanel.SetActive(false);
        _helpReminderPanel.SetActive(true);
        _backMenuPanel.SetActive(false);

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

    public void BackToMenuAction(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            _quitButtonPressed = true;
            StartCoroutine(BackToMenuHolding());
        }
        if (context.canceled)
        {
            _quitButtonPressed = false;
            _backToMenuTimer = 0f;
        }
    }

    private IEnumerator BackToMenuHolding()
    {
        _backMenuPanel.SetActive(true);
        while (_quitButtonPressed)
        {
            if(_backToMenuTimer > _durationToGoBackToMenu)
            {
                _backToMenuTimer = _durationToGoBackToMenu;
                _progressBarBacktoMenu.value = 1f;
               
                UnityEngine.SceneManagement.SceneManager.LoadScene(0);
            }
            _progressBarBacktoMenu.value = _backToMenuTimer / _durationToGoBackToMenu;
            _backToMenuTimer += Time.deltaTime;
            yield return null;
        }
        _backMenuPanel.SetActive(false);
    }

}
