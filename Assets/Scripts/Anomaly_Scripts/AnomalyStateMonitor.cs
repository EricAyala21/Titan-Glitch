
using UnityEngine;

// SOC 2 CC7.2 - Runtime Anomaly State Monitoring
// Detects unexpected anomaly state changes.

public class AnomalyStateMonitor : MonoBehaviour
{
    private AnomalyBase monitoredAnomaly;
    private bool expectedState;

    public int WarningCount { get; private set; }

    // Assigns an anomaly and records its initial state.
    public void Initialize(AnomalyBase anomaly)
    {
        monitoredAnomaly = anomaly;
        expectedState = anomaly.IsActive;
        WarningCount = 0;
    }

    // Sets the expected state for an approved operation.
    public void ExpectState(bool shouldBeActive)
    {
        expectedState = shouldBeActive;
    }

    // Checks whether the anomaly is in the expected state.
    public bool CheckState()
    {
        if (monitoredAnomaly == null)
        {
            Debug.LogWarning(
                "[SOC2 MONITOR] No anomaly assigned."
            );

            WarningCount++;
            return false;
        }

        if (monitoredAnomaly.IsActive != expectedState)
        {
            Debug.LogWarning(
                "[SOC2 MONITOR] Unexpected anomaly state detected. " +
                "Expected: " + expectedState +
                ", Actual: " + monitoredAnomaly.IsActive
            );

            WarningCount++;
            return false;
        }

        return true;
    }
}
