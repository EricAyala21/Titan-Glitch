using UnityEngine;

public class PositionAnomaly : AnomalyBase
{
    [SerializeField] private Transform targetObject;
    [SerializeField] private Vector3 anomalyPosition = new Vector3(0, 1, 0);

    private Vector3 originalPosition;

    private void Awake()
    {
        originalPosition = targetObject.position;
    }

    public override void ActivateAnomaly()
    {
        if (anomalyActive)
        {
            return;
        }
        anomalyActive = true;
        targetObject.position = originalPosition + anomalyPosition;
    }
    public override void ResetAnomaly()
    {
        anomalyActive = false;
        targetObject.position = originalPosition;
    }
}
