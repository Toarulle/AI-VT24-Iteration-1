using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

public class EndGameManager : MonoBehaviour
{ 
    [SerializeField] private string mainMenuScene;
    public void LoadMainMenu()
    {
        SceneManager.LoadScene(mainMenuScene);
    }
}
