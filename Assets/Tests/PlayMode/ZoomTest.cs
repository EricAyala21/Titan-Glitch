
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;
using UnityEngine.TestTools;
using UnityEditor;
using System.Collections;

public class ZoomInputTests : InputTestFixture
{
    private GameObject player;
    private Camera playerCamera;
    private Mouse mouse;
    private Keyboard keyboard;
    private PlayerInput playerInput;

    public override void Setup()
    {
        base.Setup();

        // Create virtual input devices
        keyboard = InputSystem.AddDevice<Keyboard>();
        mouse = InputSystem.AddDevice<Mouse>();

        // Load player prefab
        GameObject prefab =
            AssetDatabase.LoadAssetAtPath<GameObject>(
                "Assets/Player.prefab"
            );

        Assert.IsNotNull(prefab, "Player prefab not found!");

        // Instantiate player
        player = Object.Instantiate(prefab);
        player.SetActive(true);

        // Find Main Camera
        playerCamera = player.GetComponentInChildren<Camera>(true);

        Assert.IsNotNull(
            playerCamera,
            "Player camera was not found!"
        );

        // Get PlayerInput component
        playerInput = player.GetComponent<PlayerInput>();

        Assert.IsNotNull(
            playerInput,
            "PlayerInput component not found!"
        );

        // Enable PlayerInput
        playerInput.enabled = true;

        // Pair virtual keyboard and mouse
        InputUser.PerformPairingWithDevice(
            keyboard,
            playerInput.user
        );

        InputUser.PerformPairingWithDevice(
            mouse,
            playerInput.user
        );

        // Activate input
        playerInput.ActivateInput();

        // Verify Zoom action exists and is enabled
        InputAction zoomAction =
            playerInput.actions.FindAction("Zoom");

        Assert.IsNotNull(
            zoomAction,
            "Zoom action was not found!"
        );

        Assert.IsTrue(
            zoomAction.enabled,
            "Zoom action is not enabled!"
        );
    }

    [UnityTest]
    public IEnumerator RightClick_ZoomsIn()
    {
        // Record original FOV
        float originalFOV = playerCamera.fieldOfView;

        // Simulate right mouse click
        Press(mouse.rightButton);

        // Allow zoom to update
        for (int i = 0; i < 10; i++)
        {
            yield return null;
        }

        // Verify camera zoomed in
        Assert.Less(
            playerCamera.fieldOfView,
            originalFOV,
            "Camera did not zoom in!"
        );

        // Release right mouse button
        Release(mouse.rightButton);
    }


    public override void TearDown()
    {
        if (player != null)
        {
            Object.DestroyImmediate(player);
        }

        base.TearDown();
    }
}
