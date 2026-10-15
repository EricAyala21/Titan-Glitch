using UnityEngine;
using TMPro;

// Displays the player's current level and score.
public class PlayerHUD : MonoBehaviour
{
    [Header("HUD Text References")]
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private TMP_Text scoreText;

    private int currentLevel = 1;
    private int currentScore = 0;

    private void Start()
    {
        UpdateHUD();
    }

    public void SetLevel(int level)
    {
        currentLevel = Mathf.Max(1, level);
        UpdateHUD();
    }

    public void AddScore(int points)
    {
        currentScore += Mathf.Max(0, points);
        UpdateHUD();
    }

    public void SetScore(int score)
    {
        currentScore = Mathf.Max(0, score);
        UpdateHUD();
    }

    public void ResetHUD()
    {
        currentLevel = 1;
        currentScore = 0;
        UpdateHUD();
    }

    private void UpdateHUD()
    {
        if (levelText != null)
        {
            levelText.text = "Level: " + currentLevel;
        }

        if (scoreText != null)
        {
            scoreText.text = "Score: " + currentScore;
        }
    }
}