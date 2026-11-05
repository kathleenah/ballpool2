using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour{
    public int totalBalls = 5;
    private int ballsCleared = 0;
    public GameObject winPanel;

    private int score;
    
    public TMP_Text scoreText;
    public TMP_Text highScoreText;

    public int highScore;

    public void AddScore(int jumlah)
    {
        score += jumlah;
        scoreText.text = "Score: "+score;
        Debug.Log("score "+ score);

    }

    void Start(){
        highScore = PlayerPrefs.GetInt("HighScore", 0);
        winPanel.SetActive(false);
    }

    void Start2(){
        Time.timeScale = 1f;
        SceneManager.LoadScene("Game2Scene");
    }

    public void BallExitedPool(){
        ballsCleared++;

        if(ballsCleared >= totalBalls){
            TriggerWin();
        }
    }

    public void TriggerWin(){
        
        if(score > highScore)
        {
            highScore = score;
            PlayerPrefs.SetInt("HighScore", highScore);
            PlayerPrefs.Save();

        }
        highScoreText.text = "High score: " + highScore;
        winPanel.SetActive(true);
        FindAnyObjectByType<AudioManager>().PlayWin();
        Time.timeScale=0f;

        

    }
    
}
