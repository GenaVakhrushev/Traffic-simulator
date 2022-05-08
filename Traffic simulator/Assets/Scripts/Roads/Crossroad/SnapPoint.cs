using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnapPoint : MonoBehaviour
{
    [HideInInspector]
    public CrossroadPath crossroadPath;
    [HideInInspector]
    public Road connectedRoad;
    [HideInInspector]
    public bool startOfRoadConnected;

    private void Start()
    {
        crossroadPath = GetComponentInChildren<CrossroadPath>();
    }

    public void MoveConnectedRoad()
    {
        if (connectedRoad)
        {
            if (startOfRoadConnected)
            {
                connectedRoad.path.ConnectStartOrEndPoint(0, transform.position, connectedRoad.path.StartControlPointDir);
            }
            else
            {
                connectedRoad.path.ConnectStartOrEndPoint(connectedRoad.path.NumPoints - 1, transform.position, connectedRoad.path.EndControlPointDir);
            }
            connectedRoad.RoadDisplaing.UpdatePoints();
        }
    }

    public void ConnectRoad(Road road, bool startConnecting)
    {
        connectedRoad = road;
        startOfRoadConnected = startConnecting;
    }

    public void DisconnectRoad()
    {
        connectedRoad = null;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(transform.position, 0.2f);
    }
}
