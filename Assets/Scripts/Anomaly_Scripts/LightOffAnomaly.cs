using UnityEngine;

// Anomaly: a light in the scene turns off when it should be on.
public class LightOffAnomaly : MonoBehaviour
{
    [SerializeField] private Light targetLight;
    private bool isAnomalous = false;

    public bool IsAnomalous => isAnomalous;

    private void Start()
    {
        ResetAnomaly();
    }

    [ContextMenu("Activate Anomaly")]
    public void ActivateAnomaly()
    {
        if (targetLight == null)
        {
            Debug.LogWarning("LightOffAnomaly: no light assigned.");
            return;
        }
        targetLight.enabled = false;
        isAnomalous = true;
    }

    [ContextMenu("Reset Anomaly")]
    public void ResetAnomaly()
    {
        if (targetLight == null)
        {
            Debug.LogWarning("LightOffAnomaly: no light assigned.");
            return;
        }
        targetLight.enabled = true;
        isAnomalous = false;
    }

    [ContextMenu("Toggle")]
    public void Toggle()
    {
        if (isAnomalous) ResetAnomaly();
        else ActivateAnomaly();
    }
}