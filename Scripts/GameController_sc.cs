using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameController_sc : MonoBehaviour
{
    public TextMeshProUGUI highScoreText;
    public TextMeshProUGUI scoreText;
    public int score;
    public int highScore;
    public GameObject gamePausePanel;
    public GameObject gamePauseButton;
    public ScoreManager_sc scoreManager;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        gamePausePanel.SetActive(false);
        gamePauseButton.SetActive(true);
    }

    // Update is called once per frame
    void Update()
    {
        highScore = PlayerPrefs.GetInt("high_score");
        score = ScoreManager_sc.lastScore;

        highScoreText.text = "High Score: " + highScore.ToString();
        scoreText.text = "Your Score: " + score.ToString();
    }
    public void Restart()
    {
        SceneManager.LoadScene("RacingGame");
    }
    public void PauseGame()
    {
        Time.timeScale = 0;
        gamePausePanel.SetActive(true);
        gamePauseButton.SetActive(false);
    }
    public void ResumeGame()
    {
        Time.timeScale = 1;
        gamePausePanel.SetActive(false);
        gamePauseButton.SetActive(true);
    }
}
