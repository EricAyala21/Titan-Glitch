using UnityEngine;

// Test-only anomaly for SOC 2 runtime monitoring.
public class SOC2TestAnomaly : AnomalyBase
{
    public override void ActivateAnomaly()
    {
        anomalyActive = true;
    }

    public override void ResetAnomaly()
    {
        anomalyActive = false;
    }
}