// Resubmission note: This code was originally merged in PR #2 without a
// reviewer approval due to a workflow oversight. Opening this follow-up PR
// so a teammate can formally review, per our Definition of Done.
using UnityEngine;

/// <summary>
/// Temporary manual test harness for anomaly scripts during development.
/// Press T to toggle the assigned anomaly's state. Not intended for final builds.
/// </summary>
public class AnomalyTestInput : MonoBehaviour
{
    [SerializeField] private MissingObjectAnomaly anomaly;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            if (anomaly != null)
            {
                anomaly.Toggle();
            }
            else
            {
                Debug.LogWarning("AnomalyTestInput: no anomaly assigned.");
            }
        }
    }
}
// End of resubmission note.