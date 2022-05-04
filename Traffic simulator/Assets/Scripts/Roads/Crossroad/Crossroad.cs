using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class CrossroadInfo
{
    float[] position = new float[3];

    public Vector3 Position => new Vector3(position[0], position[1], position[2]);

    public CrossroadInfo(Vector3 pos)
    {
        position[0] = pos.x;
        position[1] = pos.y;
        position[2] = pos.z;
    }
}

public class Crossroad : MonoBehaviour, ISaveable
{
    SnapPoint[] snapPoints;

    void Awake()
    {
        snapPoints = GetComponentsInChildren<SnapPoint>();
    }

    public void MoveCrossroad(Vector3 positionForMove)
    {
        transform.position = positionForMove;
        foreach (SnapPoint snapPoint in snapPoints)
        {
            snapPoint.MoveConnectedRoad();
        }
    }

    #region For saving
    public PrefabType Prefab => PrefabType.Crossroad;
    public byte[] SaveInfo()
    {
        return Helper.ObjectToByteArray(new CrossroadInfo(transform.position));
    }

    public void LoadInfo(byte[] info)
    {
        CrossroadInfo crossroadInfo = Helper.ByteArrayToObject(info) as CrossroadInfo;
        transform.position = crossroadInfo.Position;

        foreach(SnapPoint snapPoint in snapPoints)
        {
            Collider[] roadColliders = Physics.OverlapSphere(snapPoint.transform.position, 0.5f, LayerMask.GetMask("Road"));
            if (roadColliders.Length > 0)
            {
                Road road = roadColliders[0].GetComponentInParent<Road>();
                bool startConnecting;

                if(road.path.StartConnected && !road.path.EndConnected)
                {
                    startConnecting = true;
                }
                else if(!road.path.StartConnected && road.path.EndConnected)
                {
                    startConnecting = false;
                }
                else
                {
                    float distToStartPoint = Vector3.Distance(road.path[0], snapPoint.transform.position);
                    float distToEndPoint = Vector3.Distance(road.path[road.path.NumPoints - 1], snapPoint.transform.position);
                    startConnecting = (distToStartPoint < distToEndPoint) ? true : false;
                }
                
                road.ConnectToSnapPoint(snapPoint, startConnecting);
            }
        }
    }
    #endregion
}
