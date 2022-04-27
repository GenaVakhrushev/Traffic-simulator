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

    public static GameObject GetPrefabByType(PrefabType type)
    {
        switch (type)
        {
            case PrefabType.Terrain:
                return null;
            case PrefabType.Road:
                return singleton.Road;
        }
        return null;
    }
}

public enum PrefabType {Terrain, Road }