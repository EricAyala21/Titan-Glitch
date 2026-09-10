using UnityEngine;

/// <summary>
/// Anomaly type: an environmental object that vanishes when the anomaly
/// is active, and reappears when reset. Serves as the first proof-of-concept
/// anomaly for Titan Glitch.
/// </summary>
public class MissingObjectAnomaly : MonoBehaviour
{
    [Header("Target")]
    [Tooltip("The object that should disappear when this anomaly is active.")]
    [SerializeField] private GameObject targetObject;

    [Header("State")]
    [SerializeField] private bool isAnomalous = false;

    public bool IsAnomalous => isAnomalous;

    private void Start()
    {
        // Always begin a level in the normal state.
        ResetAnomaly();
    }

    /// <summary>
    /// Puts the environment into its anomalous state (object hidden).
    /// </summary>
    public void ActivateAnomaly()
    {
        if (targetObject == null)
        {
            Debug.LogWarning($"{name}: targetObject not assigned.");
            return;
        }

        isAnomalous = true;
        targetObject.SetActive(false);
    }

    /// <summary>
    /// Restores the environment to its normal (non-anomalous) state.
    /// </summary>
    public void ResetAnomaly()
    {
        if (targetObject == null)
        {
            Debug.LogWarning($"{name}: targetObject not assigned.");
            return;
        }

        isAnomalous = false;
        targetObject.SetActive(true);
    }

    /// <summary>
    /// Convenience toggle, useful for manual testing before a Game Manager exists.
    /// </summary>
    public void Toggle()
    {
        if (isAnomalous) ResetAnomaly();
        else ActivateAnomaly();
    }
}