using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class CrossroadInfo
{
    float[] position = new float[3];
    bool haveMainRoad;
    int[] mainRoadPointIndexes;
    CrossroadType crossroadType;
    TrafficLightInfo[] trafficLightInfos;

    public Vector3 Position => new Vector3(position[0], position[1], position[2]);
    public bool HaveMainRoad => haveMainRoad;
    public int[] MainRoadPointIndexes => mainRoadPointIndexes;
    public CrossroadType CrossroadType => crossroadType;
    public TrafficLightInfo[] TrafficLightInfos => trafficLightInfos;

    public CrossroadInfo(Crossroad crossroad)
    {
        position[0] = crossroad.transform.position.x;
        position[1] = crossroad.transform.position.y;
        position[2] = crossroad.transform.position.z;

        haveMainRoad = crossroad.HaveMainRoad;
        crossroadType = crossroad.CrossroadType;
        mainRoadPointIndexes = crossroad.MainRoadPointIndexes;

        trafficLightInfos = new TrafficLightInfo[crossroad.SnapPoints.Length];
        for (int i = 0; i < crossroad.SnapPoints.Length; i++)
        {
            trafficLightInfos[i] = new TrafficLightInfo(crossroad.SnapPoints[i].trafficLight);
        }
    }
}

public enum CrossroadType { Unregulated, Regulated }

public class Crossroad : Clickable, ISaveable, IDeleteable
{
    bool haveMainRoad = false;
    int[] mainRoadPointIndexes = new int[2];
    CrossroadType crossroadType = CrossroadType.Unregulated;

    SnapPoint[] snapPoints;

    LineRenderer lineRenderer;

    public bool HaveMainRoad => haveMainRoad;
    public int[] MainRoadPointIndexes => mainRoadPointIndexes;
    public CrossroadType CrossroadType => crossroadType;
    public SnapPoint[] SnapPoints => snapPoints;

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
        UpdateMainRoadDisplay();
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
        SetMainRoad(crossroadInfo.MainRoadPointIndexes[0], crossroadInfo.MainRoadPointIndexes[1]);
        SetCrossroadType(crossroadInfo.CrossroadType);

        for (int i = 0; i < snapPoints.Length; i++)
        {
            SnapPoint snapPoint = snapPoints[i];
            Collider[] roadColliders = Physics.OverlapSphere(snapPoint.transform.position, 0.5f, LayerMask.GetMask("Road"));
            if (roadColliders.Length > 0)
            {
                Road road = roadColliders[0].GetComponentInParent<Road>();
                bool startConnecting;

                if(road.Path.StartConnected && !road.Path.EndConnected)
                {
                    startConnecting = true;
                }
                else if(!road.Path.StartConnected && road.Path.EndConnected)
                {
                    startConnecting = false;
                }
                else
                {
                    float distToStartPoint = Vector3.Distance(road.Path[0], snapPoint.transform.position);
                    float distToEndPoint = Vector3.Distance(road.Path[road.Path.NumPoints - 1], snapPoint.transform.position);
                    startConnecting = (distToStartPoint < distToEndPoint) ? true : false;
                }
                
                road.ConnectToSnapPoint(snapPoint, startConnecting);
            }
            snapPoint.trafficLight.LoadInfo(crossroadInfo.TrafficLightInfos[i]);
        }
    }
    #endregion
    public void Delete()
    {
        Destroy(gameObject);
        for (int i = 0; i < snapPoints.Length; i++)
        {
            if (snapPoints[i].connectedRoad)
                snapPoints[i].connectedRoad.DisconnectSnapPoint(snapPoints[i].startOfRoadConnected);
        }
    }

    public CrossroadPath GetRightCrossroadPath(CrossroadPath crossroadPath)
    {
        SnapPoint snapPoint = crossroadPath.parentSnapPoint;
        for (int i = 0; i < snapPoints.Length; i++)
        {
            if (snapPoint == snapPoints[i])
                return snapPoints[(i + 1) % snapPoints.Length].crossroadPath;
        }
        return null;
    }

    public CrossroadPath GetForwardCrossroadPath(CrossroadPath crossroadPath)
    {
        SnapPoint snapPoint = crossroadPath.parentSnapPoint;
        for (int i = 0; i < snapPoints.Length; i++)
        {
            if (snapPoint == snapPoints[i])
                return snapPoints[(i + 2) % snapPoints.Length].crossroadPath;
        }
        return null;
    }

    public bool AllLinesBlocked()
    {
        for (int i = 0; i < snapPoints.Length; i++)
        {
            if (!snapPoints[i].crossroadPath.HaveCars())
                return false;
        }
        return true;
    }

    public Road RoadWithMinID()
    {
        int minId = int.MaxValue;
        Road resultRoad = null;

        for (int i = 0; i < snapPoints.Length; i++)
        {
            Road road = snapPoints[i].connectedRoad;
            if (road)
            {
                int roadID = road.GetInstanceID();
                if (roadID < minId)
                {
                    minId = roadID;
                    resultRoad = road;
                }
            }
        }

        return resultRoad;
    }

    public int GetCrossroadPathNum(CrossroadPath crossroadPath)
    {
        for (int i = 0; i < snapPoints.Length; i++)
        {
            if (snapPoints[i].crossroadPath == crossroadPath)
                return i;
        }
        return -1;
    }

    public void SetCrossroadType(CrossroadType crossroadType)
    {
        SetCrossroadType(crossroadType == CrossroadType.Regulated);
    }

    public void SetCrossroadType(bool regulated)
    {
        crossroadType = regulated ? CrossroadType.Regulated : CrossroadType.Unregulated;
        foreach (SnapPoint snapPoint in snapPoints)
        {
            snapPoint.trafficLight.gameObject.SetActive(regulated);
        }
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
}
