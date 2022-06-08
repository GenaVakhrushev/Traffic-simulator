using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class CrossroadInfo
{
    float[] position = new float[3];
    bool haveMainRoad;
    CrossroadType crossroadType;

    public Vector3 Position => new Vector3(position[0], position[1], position[2]);
    public bool HaveMainRoad => haveMainRoad;
    public CrossroadType CrossroadType => crossroadType;

    public CrossroadInfo(Crossroad crossroad)
    {
        position[0] = crossroad.transform.position.x;
        position[1] = crossroad.transform.position.y;
        position[2] = crossroad.transform.position.z;

        haveMainRoad = crossroad.HaveMainRoad;
        crossroadType = crossroad.CrossroadType;
    }
}

public enum CrossroadType { Unregulated, Regulated }

public class Crossroad : Clickable, ISaveable, IDeleteable
{
    bool haveMainRoad = false;
    int[] mainRoadPointIndexes = new int[2];
    CrossroadType crossroadType = CrossroadType.Unregulated;

    public CrossroadType CrossroadType => crossroadType;
    public bool HaveMainRoad => haveMainRoad;
    public int[] MainRoadPointIndexes => mainRoadPointIndexes;
    public SnapPoint[] SnapPoints => snapPoints;

    SnapPoint[] snapPoints;

    LineRenderer lineRenderer;

    void Awake()
    {
        snapPoints = GetComponentsInChildren<SnapPoint>();
        lineRenderer = GetComponent<LineRenderer>();
        SetMainRoad(0, 1);
        lineRenderer.enabled = false;
    }

    public void MoveCrossroad(Vector3 positionForMove)
    {
        //при передвижении перекрёстка двигать подключённые дороги
        transform.position = positionForMove;
        foreach (SnapPoint snapPoint in snapPoints)
        {
            snapPoint.MoveConnectedRoad();
        }
    }

    public void UpdateCrossroadPaths()
    {
        foreach(SnapPoint snapPoint in snapPoints)
        {
            snapPoint.crossroadPath.Start();
        }
    }

    #region For saving
    public PrefabType Prefab => PrefabType.Crossroad;
    public byte[] SaveInfo()
    {
        return Helper.ObjectToByteArray(new CrossroadInfo(this));
    }

    public void LoadInfo(byte[] info)
    {
        CrossroadInfo crossroadInfo = Helper.ByteArrayToObject(info) as CrossroadInfo;
        transform.position = crossroadInfo.Position;

        SetHaveMainRoad(crossroadInfo.HaveMainRoad);
        SetType(crossroadInfo.CrossroadType);

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
    public void Delete()
    {
        Destroy(gameObject);
        for (int i = 0; i < snapPoints.Length; i++)
        {
            snapPoints[i].connectedRoad.DisconnectSnapPoint(snapPoints[i].startOfRoadConnected);
        }
    }

    public SnapPoint GetRightSnapPoint(SnapPoint snapPoint)
    {
        for (int i = 0; i < snapPoints.Length; i++)
        {
            if (snapPoint == snapPoints[i] && i < snapPoints.Length - 1)
                return snapPoints[i + 1];
            if (snapPoint == snapPoints[i] && i == snapPoints.Length - 1)
                return snapPoints[0];
        }
        return null;
    }

    public void SetHaveMainRoad(bool value)
    {
        haveMainRoad = value;
        lineRenderer.enabled = value;
        UpdateMainRoadDisplay();
    }

    public void SetMainRoad(int firstIndex, int secondIndex)
    {
        mainRoadPointIndexes[0] = firstIndex;
        mainRoadPointIndexes[1] = secondIndex;

        UpdateMainRoadDisplay();
    }

    void UpdateMainRoadDisplay()
    {
        Vector3[] points = Bezier.EvaluateBezierPoints(snapPoints[mainRoadPointIndexes[0]].transform.position,
                                                      transform.position,
                                                      transform.position,
                                                      snapPoints[mainRoadPointIndexes[1]].transform.position,
                                                      20);

        for (int i = 0; i < points.Length; i++)
        {
            points[i].y += 0.1f;
        }

        lineRenderer.positionCount = points.Length;
        lineRenderer.SetPositions(points);
    }

    public void SetType(CrossroadType crossroadType)
    {
        this.crossroadType = crossroadType;
    }
}
