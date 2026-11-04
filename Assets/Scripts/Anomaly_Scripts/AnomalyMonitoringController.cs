
using UnityEngine;

// Test-scene controller for SOC 2 CC7.2 monitoring.
public class AnomalyMonitoringController : MonoBehaviour
{
    [SerializeField] private SOC2TestAnomaly testAnomaly;
    [SerializeField] private AnomalyStateMonitor monitor;

    private void Start()
    {
        if (testAnomaly == null || monitor == null)
        {
            Debug.LogError(
                "[SOC2 DEMO] Missing anomaly or monitor reference."
            );
            enabled = false;
            return;
        }

        testAnomaly.ResetAnomaly();
        monitor.Initialize(testAnomaly);

        Debug.Log(
            "[SOC2 DEMO] Ready. " +
            "Press 1 = Activate, 2 = Reset, " +
            "3 = Unexpected activation."
        );
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            monitor.ExpectState(true);
            testAnomaly.ActivateAnomaly();
            monitor.CheckState();

            Debug.Log("[SOC2 DEMO] Approved activation.");
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            monitor.ExpectState(false);
            testAnomaly.ResetAnomaly();
            monitor.CheckState();

            Debug.Log("[SOC2 DEMO] Approved reset.");
        }

        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            // Deliberately change the state without
            // registering an approved operation.
            monitor.ExpectState(false);
            testAnomaly.ResetAnomaly();

            testAnomaly.ActivateAnomaly();

            // Monitor should detect the mismatch.
            monitor.CheckState();
        }
    }
}
