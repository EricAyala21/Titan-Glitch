using UnityEngine;

public class BookAnomaly : MonoBehaviour
{

[SerializedField] private GameObject book;
[SerializedField] private float toggleInterval = 2f;

private bool anomalyActive = false;

public void ActivateAnomaly()
    {
        if (anomalyActive)
        {
            return;
        }

        anomalyActive = true;

        InvokeRepeating
        (
            nameof(ToggleBookVisibility),
            toggleInterval,
            toggleInterval
        );

        Debug.Log("Book Anomaly activated.");
    }

}
