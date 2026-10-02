using UnityEngine;

public class AnomalyTrigger : MonoBehaviour
{
    private bool playerInTrigger = false;

    void OnTriggerEnter(Collider collider)
    {
        if(collider.gameObject.CompareTag("Player"))
        {
            Debug.Log("BOO!");
            playerInTrigger = true;
        }
    }

    public void checkPlayerInTrigger()
    {
        if(playerInTrigger)
        {
            Debug.Log("Player is in the anomaly trigger.");
        }
        else
        {
            Debug.Log("Player is not in the anomaly trigger.");
        }
    }
}
