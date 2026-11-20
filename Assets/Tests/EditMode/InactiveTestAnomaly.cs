// Test-only anomaly that does NOT activate itself. Used by SCRUM-87 to check whether
// AnomalyManager activates the anomalies it spawns.
public class InactiveTestAnomaly : AnomalyBase
{
    public override void ActivateAnomaly() { anomalyActive = true; }
    public override void ResetAnomaly() { anomalyActive = false; }
}