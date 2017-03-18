using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameStates : MonoBehaviour {
    public GameObject player;

    public void QuitGame()
    {
        Application.Quit();
    }
    public void StartGame()
    {
        SceneManager.LoadScene(1);
    }
    public void LoseGame()
    {
        DontDestroyOnLoad(player);
        SceneManager.LoadScene(2);
    }
}
