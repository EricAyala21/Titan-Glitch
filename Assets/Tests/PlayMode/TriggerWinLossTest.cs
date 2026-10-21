using System;
using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class DoorWinLossTests
{
    private GameObject doorway;
    private GameObject player;
    private GameObject managerObject;

    private Component doorScript;
    private Component gameManager;
    private Rigidbody playerBody;

    // Find existing game scripts
    private Type FindType(string name)
    {
        foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            Type type = assembly.GetType(name);

            if (type != null)
                return type;
        }

        Assert.Fail("Could not find script: " + name);
        return null;
    }

    // Assign a private serialized field for testing.
    private void SetField(Component target, string name, object value)
    {
        FieldInfo field = target.GetType().GetField(
            name,
            BindingFlags.Instance | BindingFlags.NonPublic
        );

        Assert.IsNotNull(field);
        field.SetValue(target, value);
    }

    private string GetGameState()
    {
        PropertyInfo property =
            gameManager.GetType().GetProperty("CurrentState");

        Assert.IsNotNull(property);
        return property.GetValue(gameManager).ToString();
    }

    [SetUp]
    public void Setup()
    {
        // Create GameManager
        managerObject = new GameObject("TestManager");
        gameManager = managerObject.AddComponent(
            FindType("GameManager")
        );

        // Create doorway trigger
        doorway = new GameObject("TestDoorway");

        BoxCollider trigger = doorway.AddComponent<BoxCollider>();
        trigger.isTrigger = true;
        trigger.size = Vector3.one * 2f;

        doorScript = doorway.AddComponent(
            FindType("Door_win_loss")
        );

        SetField(doorScript, "gameManager", gameManager);

        // Configure left doorway
        object anomaly = Enum.Parse(FindType("DoorType"), "Anomaly");
        SetField(doorScript, "doorType", anomaly);

        // Create player outside doorway
        player = new GameObject("TestPlayer");
        player.tag = "Player";
        player.transform.position = new Vector3(0, 0, -5);

        player.AddComponent<BoxCollider>();

        playerBody = player.AddComponent<Rigidbody>();
        playerBody.useGravity = false;
        playerBody.isKinematic = true;
    }

    private IEnumerator EnterDoorway()
    {
        yield return new WaitForFixedUpdate();

        playerBody.MovePosition(Vector3.zero);

        yield return new WaitForFixedUpdate();
        yield return new WaitForFixedUpdate();
    }

    [UnityTest]
    public IEnumerator LeftDoor_TriggersProgression()
    {
        // Expect logs from the real GameManager.
        LogAssert.Expect(LogType.Log, "Left Door Selected");

        // No AnomalyManager is assigned in this test.
        LogAssert.Expect(
            LogType.Error,
            "AnomalyManager reference is missing!"
        );

        // Simulate player entering doorway.
        yield return EnterDoorway();

        // Verify GameManager entered Progression.
        Assert.AreEqual("Progression", GetGameState());
    }

    [UnityTest]
    public IEnumerator NonPlayer_DoesNotTriggerDoor()
    {
        // Change object tag so it is not the player.
        player.tag = "Untagged";

        // Move object into doorway.
        yield return EnterDoorway();

        // GameManager should stay in initial state.
        Assert.AreEqual("MainMenu", GetGameState());
    }

    [UnityTearDown]
    public IEnumerator Cleanup()
    {
        UnityEngine.Object.Destroy(player);
        UnityEngine.Object.Destroy(doorway);
        UnityEngine.Object.Destroy(managerObject);

        yield return null;
    }
}
