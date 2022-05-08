using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Prefabs : MonoBehaviour
{
    static Prefabs instance;

    public static Prefabs Instance
    {
        get
        {
            if(instance == null)
            {
                instance = new Prefabs();
            }

            return instance;
        }
    }

    public GameObject Road;
    public GameObject Crossroad;
    public GameObject CarSpawner;
    public GameObject Car;

    private void Start()
    {
        instance = this;
    }

    public static GameObject GetPrefabByType(PrefabType type)
    {
        switch (type)
        {
            case PrefabType.Road:
                return Instance.Road;
            case PrefabType.Crossroad:
                return Instance.Crossroad;
        }
        return null;
    }
}

public enum PrefabType {Terrain, Road, Crossroad }