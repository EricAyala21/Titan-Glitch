using UnityEngine;

public class BookAnomaly : MonoBehaviour
{

//Creates a reference of the gameobject Book and a float interval for how long the visibility of the book will toggle on and off.
[SerializeField] private GameObject book;
[SerializeField] private float toggleInterval = 2f;

//Starts off the anomaly being turned off until the player activates it or a condition occurs.
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

public void ResetAnomaly()
    {
        anomalyActive = false;
        CancelInvoke(nameof(ToggleBookVisibility));

        //This restores the book to its original state, which is active.
        book.SetActive(true);
        Debug.Log("Book Anomaly reset.");

    }

    private void ToggleBookVisibility()
    {
        book.SetActive(!book.activeSelf);
    }

    public bool isAnomalyActive()
    {
        return anomalyActive;
    }

    private void Start()
    {
        //Ensures the book is active at the start of the game.
        book.SetActive(true);
        ActivateAnomaly();
    }
}
