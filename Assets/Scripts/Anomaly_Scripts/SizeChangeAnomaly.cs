
using UnityEngine;

public class SizeChangeAnomaly : AnomalyBase
{
    [SerializeField] private Transform targetObject;
    [SerializeField] private Vector3 anomalyScale = new Vector3(2f, 2f, 2f);

    private Vector3 originalScale;

    private void Awake()
    {
        originalScale = targetObject.localScale;

    }

    public override void ActivateAnomaly()
    {
      if (anomalyActive)
        {
            return;
        }

        anomalyActive = true;
        targetObject.localScale = anomalyScale;
        Debug.Log("SizeChangeAnomaly activated: Object scale changed to " + anomalyScale);
    }

    public override void ResetAnomaly()
    {
        anomalyActive = false;
        targetObject.localScale = originalScale;
        Debug.Log("SizeChangeAnomaly reset: Object scale returned to original");    
    }
}

