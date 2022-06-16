using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnapPoint : MonoBehaviour
{

    [HideInInspector] public CrossroadPath crossroadPath;
    [HideInInspector] public TrafficLight trafficLight;
    [HideInInspector] public Road connectedRoad;
    [HideInInspector] public bool startOfRoadConnected;

    private void Awake()
    {
        crossroadPath = GetComponentInChildren<CrossroadPath>();
        trafficLight = GetComponentInChildren<TrafficLight>();
    }

    public void MoveConnectedRoad()
    {
        if (connectedRoad)
        {
            if (startOfRoadConnected)
            {
                connectedRoad.Path.ConnectStartOrEndPoint(0, transform.position, connectedRoad.Path.StartControlPointDir);
            }
            else
            {
                connectedRoad.Path.ConnectStartOrEndPoint(connectedRoad.Path.NumPoints - 1, transform.position, connectedRoad.Path.EndControlPointDir);
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
