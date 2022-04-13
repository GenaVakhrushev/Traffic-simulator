using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Prefabs : MonoBehaviour
{
    public static Prefabs singleton;

    public GameObject Road;

    private void Start()
    {
        singleton = this;
    }
}
