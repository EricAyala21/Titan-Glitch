using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{

#region Initializations
    // Resubmitting the Pull Request for the WASD Player movements for reviewer approval per the syllabus
    // This includes the player gravity, vectors, position, and WASD keybinds to move the human model in the game.
    /*  ===============
        Movement Variables
        =============== */


    //Create the Player Movement Speed Variable 
    // Can adjust later based on playtest
    public float moveSpeed = 5f;

    //Creating play gravity which pulls the character down
    public float gravity = -9.81f;

        /* ===============
           Camera Variables
           =============== */

    public Camera camera;
    public float mouseSensitivity = 0.1f;
    public float defaultFOV = 60f;
    public float zoomFOV    = 30f;
    public float zoomSpeed  = 10f;
    private bool zooming    = false;

    //Stores a reference of the character controller component
    private CharacterController controller;

    //Initializing private variables of the player move and look Input
    private Vector2 moveInput;
    private Vector2 lookInput;

    // Stores player's current vertical movement
    private Vector3 velocity;

    //Tracking of the  camera up/down rotations
    private float xRotation = 0f; 

    //Player's Flashlight
    public Light flashlight;
#endregion



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Find character controller attached to the gameObject
        controller = GetComponent<CharacterController>();

        Cursor.lockState = CursorLockMode.Locked;


        //Toggling of Cursor visibility
        Cursor.visible = false;

    }

#region Onx Functions
    //Unity call this component because of the PlayerInput attached
    public void OnMove(InputValue value)
    {
        //Read Vector2 value from move action
        // W = (0,1) , S = (0,-1) , A = (-1,0) , D = (1,0)
        moveInput = value.Get<Vector2>();
    }

    public void OnLook(InputValue value)
    {
        //Read mouse movement values from Vector2
        // X = mouse movement left/right , Y = mouse movement up/down
        lookInput = value.Get<Vector2>();
    }

    public void OnFlashlight()
    {
        // Toggle flashlight on and off
        flashlight.enabled = !flashlight.enabled;
    }

    public void OnZoom(InputValue value)
    {
        // camera.fieldOfView = Mathf.Lerp(camera.fieldOfView, zoomFOV, Time.deltaTime * zoomSpeed);
        // camera.fieldOfView = zoomFOV;
        zooming = !zooming; // start zoom in/out
    }
#endregion

    // Update is called once per frame
    void Update()
    {
        // Call Handlers denoted below to control the player
        HandleMovement();
        HandleCamera();
        HandleZoom();
    }


#region Handlers
    void HandleMovement()
    {

        //Create a 3D movement direction
        Vector3 move = transform.right * moveInput.x +
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

    void HandleCamera()
    {
        //Read Mouse Horizontal Movements
        float mouseX = lookInput.x * mouseSensitivity;

        //Read Mouse Vertical Movements
        float mouseY = lookInput.y * mouseSensitivity;

        //Looking up and down , by subrtracting the Y mouse movement from the current rotation
        xRotation -= mouseY;

        //Prevents the camera from rotating too far up or down and spin

        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        //Rotates only the camera
         camera.GetComponent<Transform>().localRotation = 
         Quaternion.Euler(xRotation, 0f,0f);

         //Rotating the entire player left and right using the wasd movements
        // rotating the player also changes the direction of W movements

        transform.Rotate(Vector3.up * mouseX);
    }

    void HandleZoom()
    {
        if(zooming)
        {
            camera.fieldOfView = Mathf.Lerp(camera.fieldOfView, zoomFOV, Time.deltaTime * zoomSpeed);
        }
        else
        {
            camera.fieldOfView = Mathf.Lerp(camera.fieldOfView, defaultFOV, Time.deltaTime * zoomSpeed);
        }
    }
}


#endregion
