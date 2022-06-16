using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PanelsManager : MonoBehaviour
{
    static PanelsManager instance;

    public static PanelsManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new PanelsManager();
            }

            return instance;
        }
    }

    public GameObject CarSpawnerPanel;
    public GameObject CrossroadPanel;
    public GameObject RoadPanel;
    public GameObject TrafficLightPanel;

    public static Dictionary<Type, GameObject> Panels = new Dictionary<Type, GameObject>();

    void Start()
    {
        instance = this;

        Panels.Add(typeof(CarSpawner), CarSpawnerPanel);
        Panels.Add(typeof(Crossroad), CrossroadPanel);
        Panels.Add(typeof(Road), RoadPanel);
        Panels.Add(typeof(TrafficLight), TrafficLightPanel);
    }

    public static void HidePanels()
    {
        foreach (var item in Panels)
        {
            item.Value.SetActive(false);
        }
    }
}
