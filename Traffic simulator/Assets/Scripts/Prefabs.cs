using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Prefabs : MonoBehaviour
{
    public static Prefabs singleton;

    public GameObject Road;
    public GameObject Crossroad;

    private void Start()
    {
        singleton = this;
    }

    public static GameObject GetPrefabByType(PrefabType type)
    {
        switch (type)
        {
            case PrefabType.Road:
                return singleton.Road;
            case PrefabType.Crossroad:
                return singleton.Crossroad;
        }
        return null;
    }
}

public enum PrefabType {Terrain, Road, Crossroad }