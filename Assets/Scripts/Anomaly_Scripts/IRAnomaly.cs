using UnityEngine;

public class IRAnomaly : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnCollisionEnter(Collision collision)
    {
        if(collision.collider.name == "IRFlashlight")
        {
            print("IRFlashlight detected");
            gameObject.SetActive(true);
        }
        else
        {
            print("Player Bumped");
        }
    }
}
