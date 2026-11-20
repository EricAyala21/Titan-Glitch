using UnityEngine;

public class WrongObjectOrder : MonoBehaviour
{

    // Drag 2 game objects in inspector to be swapped
    public GameObject gameObject1;
    public GameObject gameObject2;

    GameObject temp = null;


    void Start()
    {
        temp = gameObject1;
        gameObject1.transform.position = gameObject2.transform.position; 
        gameObject2.transform.position = temp.transform.position;
    }


}
