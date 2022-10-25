using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField] private int startScene;
    [SerializeField] private Button _defaultButton;

    private void Awake()
    {
        _defaultButton.Select();
    }
    public void StartGame()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(startScene);
    }

    public void BackToMainMenu()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(0);
    }

    public void QuitGame()
    {
        Debug.Log("Quitting the Game");
        Application.Quit();
    }
}
