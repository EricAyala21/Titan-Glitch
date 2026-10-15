
using UnityEngine;
using TMPro;

// Player HUD for Titan Glitch
// Displays the current level and score.
public class PlayerHUD : MonoBehaviour
{
    [Header("HUD Text References")]
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private TMP_Text scoreText;

    [Header("Game References")]
    [SerializeField] private AnomalyManager anomalyManager;

    [Header("Score Settings")]
    [SerializeField] private int pointsPerLevel = 100;

    private void Start()
    {
        // Find the AnomalyManager if not assigned.
        if (anomalyManager == null)
        {
            anomalyManager =
            FindAnyObjectByType<AnomalyManager>();
        }

        UpdateHUD();
    }

    // Update the level and score displayed on screen.
    public void UpdateHUD()
    {
        if (anomalyManager == null)
        {
            Debug.LogWarning(
                "PlayerHUD: AnomalyManager not found."
            );
            return;
        }

        int completedLevels =
            anomalyManager.GetCompletedAnomalyCount();

        int currentLevel = completedLevels + 1;

        int currentScore =
            completedLevels * pointsPerLevel;

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
