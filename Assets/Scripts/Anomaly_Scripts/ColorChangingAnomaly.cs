using UnityEngine;

//This anomaly changes color of the object
public class ColorChangingAnomaly : AnomalyBase
{
 [SerializeField] private Renderer targetRenderer;
 [SerializeField] private Color anomalyColor = Color.red;

 private Color originalColor;

 private void Awake()
    {
        originalColor = targetRenderer.material.color;
    }

    public override void ActivateAnomaly()
    {
        if (anomalyActive)
        {
            return;
        }
        anomalyActive = true;
        targetRenderer.material.color = anomalyColor;
    }

    public override void ResetAnomaly()
    {
        anomalyActive = false;
        targetRenderer.material.color = originalColor;
    }
}
