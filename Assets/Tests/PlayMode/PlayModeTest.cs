using NUnit.Framework;
using UnityEngine.TestTools;
using System.Collections;

public class PlayModeTest
{
    [UnityTest]
    public IEnumerator BasicTest()
    {
        Assert.IsTrue(true);

        yield return null;
    }
}