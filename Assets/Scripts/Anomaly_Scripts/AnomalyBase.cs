using UnityEngine;

//Reusable Anomaly template that can be accessed and easily copied and modified

//Create an inheritable class called Anomaly Base
public abstract class AnomalyBase : MonoBehaviour
{
    //Name used to identify the anomaly in scope
    //Can be changed through the Unity Inspector without code editing
[SerializeField] protected string anomalyName;

//Tracking of the activation of the anomaly
protected bool anomalyActive = false;

//Allows future scripts to read the anomalyName without changing it's code
public string AnomalyName => anomalyName;

public abstract void ActivateAnomaly();

//Allows for reset back to original state
public abstract void ResetAnomaly();
}
