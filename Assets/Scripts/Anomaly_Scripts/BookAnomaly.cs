using UnityEngine;

public class BookAnomaly : AnomalyBase
{

//Creates a reference of the gameobject Book and a float interval for how long the visibility of the book will toggle on and off.
[SerializeField] private GameObject book;
[SerializeField] private float toggleInterval = 2f;



public override void ActivateAnomaly()
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

public override void ResetAnomaly()
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



    //Temporary Activation for testing purposes
    // Can delete the start() method when game is ready
    private void Start()
    {
        //Ensures the book is active at the start of the game.
        book.SetActive(true);
        ActivateAnomaly();
    }
}
