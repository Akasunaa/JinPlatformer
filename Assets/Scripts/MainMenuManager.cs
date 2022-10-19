using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField] private int startScene;

    public void StartGame()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(startScene);
    }

    public void QuitGame()
    {
        Debug.Log("Quitting the Game");
        Application.Quit();
    }
}
