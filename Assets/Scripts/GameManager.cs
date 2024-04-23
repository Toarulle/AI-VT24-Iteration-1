using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private List<SugarCube> sugarCubes;

    [SerializeField] private string endGameScene;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip eatingSound;
    private int score, maxScore;

    private void Start()
    {
        maxScore = sugarCubes.Count;
        score = 0;
        UpdateScoreText();
    }

    public void PlaySound(AudioClip clip)
    {
        audioSource.PlayOneShot(clip);
    }
    public void OnGetSugarCube(SugarCube sugarCube)
    {
        score++;
        sugarCube.gameObject.SetActive(false);
        UpdateScoreText();
        PlaySound(eatingSound);
        if (score == maxScore)
        {
            EndGame();
        }
    }

    private void EndGame()
    {
        SceneManager.LoadScene(endGameScene, LoadSceneMode.Single);
    }
    
    private void UpdateScoreText()
    {
        scoreText.text = $"{score}/{maxScore}";
    }
    
    private void OnEnable()
    {
        foreach (var cube in sugarCubes)
        {
            cube.OnCollectSugarCube += OnGetSugarCube;
        }
    }

    private void OnDisable()
    {
        foreach (var cube in sugarCubes)
        {
            cube.OnCollectSugarCube -= OnGetSugarCube;
        }
    }
}
