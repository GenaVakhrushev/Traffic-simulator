using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lane : MonoBehaviour
{
    List<LanePoint> lanePoints;
 
    void Start()
    {
        lanePoints = new List<LanePoint>();
    }

    public void SetLanePoints(Path path, Vector3 offset, float maxSpeed)
    {
        foreach (Vector3 point in path.CachedEvenlySpacedPoints)
        {
            lanePoints.Add(new LanePoint { position = point + offset, maxSpeed = maxSpeed });
        }
    }
}

public class LanePoint
{
    public Vector3 position;
    public float maxSpeed;
}
