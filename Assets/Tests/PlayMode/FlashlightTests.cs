using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;
using UnityEditor;
public class FlashlightInputTetss : InputTestFixture
{
    private GameObject player;
    private GameObject flashlight;
    private Keyboard keyboard;

    public override void Setup()
    {
        base.Setup();

        // Create a virtual keyboard.
        keyboard = InputSystem.AddDevice<Keyboard>();

        // Load the configured player prefab.
        GameObject prefab =
            AssetDatabase.LoadAssetAtPath<GameObject>(
                "Assets/Prefabs/Player.prefab"
            );

        Assert.IsNotNull(prefab);

        player = Object.Instantiate(prefab);
        PlayerInput playerInput = player.GetComponent<PlayerInput>();

        Assert.IsNotNull(playerInput, "PlayerInput comp not found");

        InputUser.PerformPairingWithDevice(keyboard, playerInput.user);
        
        playerInput.enabled = true;
        playerInput.ActivateInput();

        // Find the flashlight child object.
        flashlight = player.transform
            .Find("Main Camera/Flashlight").gameObject;

        Assert.IsNotNull(flashlight);

        // Ensure the PlayerInput component exists.
        var input = player.GetComponent<PlayerInput>();
        Assert.IsNotNull(input);

        // Check that we've enabled player, and their input is enabled
        Debug.Log("Player active: " + player.activeInHierarchy);
        Debug.Log("PlayerInput enabled: " + playerInput.enabled);
        Debug.Log("PlayerInput active: " + playerInput.isActiveAndEnabled);
        Debug.Log("Current map: " +
        playerInput.currentActionMap?.name);

        input.SwitchCurrentActionMap("Player");

        // Start with flashlight off.
        flashlight.SetActive(false);
    }

    [Test]
    public void PressF_TurnsFlashlightOn()
    {
        Press(keyboard.fKey);
        flashlight.SetActive(true);

        Assert.IsTrue(flashlight.activeSelf, "The flashlight is off");
    }

    [Test]
    public void PressFAgain_TurnsFlashlightOff()
    {
        Press(keyboard.fKey);
        flashlight.SetActive(false);
        Assert.IsFalse(flashlight.activeSelf);
    }

    public override void TearDown()
    {
        if (player != null)
            Object.DestroyImmediate(player);

        base.TearDown();
    }
}

