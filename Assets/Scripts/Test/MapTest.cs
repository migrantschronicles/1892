using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapTest : MonoBehaviour
{
    public void UnlockAllLocations()
    {
        NewGameManager.Instance.UnlockAllLocations();
    }

    public void UnlockLocation(string name)
    {
        NewGameManager.Instance.UnlockLocation(name);
    }
}
