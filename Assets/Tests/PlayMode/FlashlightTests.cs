
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;

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
        var prefab = Resources.Load<GameObject>(
            "Player"
        );

        Assert.IsNotNull(prefab);

        player = Object.Instantiate(prefab);
        PlayerInput playerInput = player.GetComponent<PlayerInput>();

        Assert.IsNotNull(playerInput, "PlayerInput comp not found");

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

        Assert.IsTrue(flashlight.activeSelf);
    }

    [Test]
    public void PressFAgain_TurnsFlashlightOff()
    {
        Press(keyboard.fKey);
        Assert.IsTrue(flashlight.activeSelf);

        Release(keyboard.fKey);
        Press(keyboard.fKey);

        Assert.IsFalse(flashlight.activeSelf);
    }

    public override void TearDown()
    {
        if (player != null)
            Object.DestroyImmediate(player);

        base.TearDown();
    }
}

