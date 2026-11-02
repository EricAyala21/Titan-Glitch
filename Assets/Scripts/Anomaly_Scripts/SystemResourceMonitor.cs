
using UnityEngine;

// SOC 2 CC7.2 - System Resource Utilization Monitoring
// Monitors FPS and managed memory usage during gameplay.

public class SystemResourceMonitor : MonoBehaviour
{
    [Header("Monitoring Thresholds")]
    [SerializeField] private float minimumFPS = 30f;
    [SerializeField] private float maximumMemoryMB = 1024f;
    [SerializeField] private float checkInterval = 1f;

    private float elapsedTime;
    private int frameCount;
    private float checkTimer;

    public float CurrentFPS { get; private set; }
    public float CurrentMemoryMB { get; private set; }
    public int WarningCount { get; private set; }

    private void Update()
    {
        elapsedTime += Time.unscaledDeltaTime;
        frameCount++;
        checkTimer += Time.unscaledDeltaTime;

        if (checkTimer >= checkInterval)
        {
            CurrentFPS = frameCount / elapsedTime;

            CurrentMemoryMB =
                System.GC.GetTotalMemory(false) / (1024f * 1024f);

            CheckThresholds(CurrentFPS, CurrentMemoryMB);

            elapsedTime = 0f;
            frameCount = 0;
            checkTimer = 0f;
        }
    }

    public bool CheckThresholds(float fps, float memoryMB)
    {
        bool healthy = true;

        if (fps < minimumFPS)
        {
            Debug.LogWarning(
                "[SOC2 RESOURCE] Low FPS detected: " + fps
            );

            WarningCount++;
            healthy = false;
        }

        if (memoryMB > maximumMemoryMB)
        {
            Debug.LogWarning(
                "[SOC2 RESOURCE] High memory usage detected: " +
                memoryMB + " MB"
            );

            WarningCount++;
            healthy = false;
        }

        return healthy;
    }

private void OnGUI()
{
    GUIStyle style = new GUIStyle(GUI.skin.box);

    style.fontSize = 22;
    style.normal.textColor =
        CurrentFPS < minimumFPS ? Color.red : Color.green;

    style.alignment = TextAnchor.MiddleCenter;

    float boxWidth = 150f;
    float boxHeight = 40f;
    float margin = 10f;

    GUI.Box(
        new Rect(
            Screen.width - boxWidth - margin,
            Screen.height - boxHeight - margin,
            boxWidth,
            boxHeight
        ),
        "FPS: " + CurrentFPS.ToString("F1"),
        style
    );
}

}
