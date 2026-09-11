using UnityEngine;
using UnityEngine.InputSystem;

public class WASDMovement : MonoBehaviour
{
    //Create the Player Movement Speed Variable 
    // Can adjust later based on playtest
    public float moveSpeed = 5f;

    //Creating play gravity which pulls the character down
    public float gravity = -9.81f;


    //Stores a reference of the character controller component
    private CharacterController controller;


    private Vector2 moveInput;

    // Stores player's current vertical movement
    private Vector3 velocity;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Find character controller attached to the gameObject
        controller = GetComponent<CharacterController>();

    }

    //Unity call this component because of the PlayerInput attached
    public void OnMove(InputValue value)
    {
        //Read vector 2 value from move action
        // W = (0,1) , S = (0,-1) , A = (-1,0) , D = (1,0)
        moveInput = value.get<Vector2>();
    }

    // Update is called once per frame
    void Update()
    {
   
   //Create a 3D movement direction

   Vector3 move = transform.right * movevInput.x +
                  transform.forward * moveInput.y;

    // Prevent diagonal movement from being faster than just straight movements
    move = Vector3.ClampMagnitude(move, 1f);

    // Move the player horizontally
    controller.Move (move * moveSpeed * Time.deltaTime);

        // Applies gravity to the player to keep them grounded
        if (controller.isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }
        //Add gravity over time
        velocity.y += gravity * Time.deltaTime;

        //Apply vertical movement
        controller.Move(velocity * Time.deltaTime);
    }
}
