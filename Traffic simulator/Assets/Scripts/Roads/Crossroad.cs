using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Crossroad : MonoBehaviour
{
    CrossroadPath[] crossroadPaths;

    void Start()
    {
        crossroadPaths = GetComponentsInChildren<CrossroadPath>();
    }
}
