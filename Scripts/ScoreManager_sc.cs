using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ScoreManager_sc : MonoBehaviour
{
    public int score = 0;
    public int highScore = 0;
    public static int lastScore;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI highScoreText;
    public TextMeshProUGUI lastScoreText;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartCoroutine(Score());
        // StartCoroutine(Reload());
        highScore = PlayerPrefs.GetInt("high_score", 0);
        highScoreText.text = "High Score: " + highScore.ToString();
        lastScoreText.text = "Last Score: " + lastScore.ToString();
    }

    // Update is called once per frame
    void Update()
    {
        scoreText.text = score.ToString();

        if(score > highScore)
        {
            highScore = score;
            PlayerPrefs.SetInt("high_score", highScore);
            highScoreText.text = "High Score: " + highScore.ToString();
        }
    }
    IEnumerator Score()
    {
        while(true)
        {
            yield return new WaitForSeconds(0.8f);
            score = score + 1;
            lastScore = score;
        }
    }
    // IEnumerator Reload()
    // {
    //     while(true)
    //     {
    //         yield return new WaitForSeconds(Random.Range(5,10));
    //         SceneManager.LoadScene("RacingGame");
    //     }
    // }
}
