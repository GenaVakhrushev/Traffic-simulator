using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoadEditor : MonoBehaviour
{
    public GameObject RoadPrefab;

    private bool isBuilding = false;

    private GameObject currentRoad = null;

    Ray ray { get { return Camera.main.ScreenPointToRay(Input.mousePosition); } }
    RaycastHit hit;
    bool terrainHit { get { return Physics.Raycast(ray, out hit, 1000, LayerMask.GetMask("Terrain")); } }

    void Start()
    {
        
    }

    void Update()
    {
        if (Input.GetMouseButtonUp(0))
        {
            if (isBuilding)
                BuildRoad();
            else
                BuildStartOfRoad();
        }

        if (isBuilding)
            ModifyRoad();
    }

    private void ModifyRoad()
    {
        if (terrainHit)
        {
            currentRoad.transform.LookAt(hit.point);
            currentRoad.transform.localScale = new Vector3(1, 1, Vector3.Distance(hit.point, currentRoad.transform.position) / 4);
        }    
    }

    private void BuildStartOfRoad()
    {
        //создать дорогу в месте клика
        if(terrainHit)
        {
            currentRoad = Instantiate(RoadPrefab, hit.point, Quaternion.identity);

            foreach(Renderer renderer in currentRoad.GetComponentsInChildren<Renderer>())
            {
                Color newColor = renderer.material.color;
                newColor.a = 0.5f;
                renderer.material.color = newColor;
            }
            
            isBuilding = true;
        }       
    }

    private void BuildRoad()
    {
        isBuilding = false;

        foreach (Renderer renderer in currentRoad.GetComponentsInChildren<Renderer>())
        {
            Color newColor = renderer.material.color;
            newColor.a = 1f;
            renderer.material.color = newColor;
        }
    }
}
