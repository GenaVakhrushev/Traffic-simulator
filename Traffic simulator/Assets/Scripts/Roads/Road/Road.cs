using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//сохраняемая о дороге информация
[System.Serializable]
public class RoadInfo
{
    PathInfo pathInfo;
    CarSpawnerInfo startCarSpawnerInfo;
    CarSpawnerInfo endCarSpawnerInfo;
    float[] startMaxSpeeds;
    float[] endMaxSpeeds;

    public PathInfo PathInfo => pathInfo;
    public CarSpawnerInfo StartCarSpawnerInfo => startCarSpawnerInfo;
    public CarSpawnerInfo EndCarSpawnerInfo => endCarSpawnerInfo;
    public float[] StartMaxSpeeds => startMaxSpeeds;
    public float[] EndMaxSpeeds => endMaxSpeeds;

    public RoadInfo(Road road)
    {
        pathInfo = new PathInfo(road.Path);
        startCarSpawnerInfo = new CarSpawnerInfo(road.StartCarSpawner);
        endCarSpawnerInfo = new CarSpawnerInfo(road.EndCarSpawner);
        List<float> speeds = new List<float>();
        foreach (Lane lane in road.StartLanes)
        {
            speeds.Add(lane.MaxSpeed);
        }
        startMaxSpeeds = speeds.ToArray();
        speeds.Clear();
        foreach (Lane lane in road.EndLanes)
        {
            speeds.Add(lane.MaxSpeed);
        }
        endMaxSpeeds = speeds.ToArray();
    }
}

public class Road : Clickable, ISaveable, ILaneable, IDeleteable
{
    Path path;
    List<Lane> startLanes;
    List<Lane> endLanes;

    SnapPoint startSnapPoint = null;
    SnapPoint endSnapPoint = null;

    RoadDisplaing roadDisplaing;
    
    public RoadDisplaing RoadDisplaing => roadDisplaing;
    public SnapPoint StartSnapPoint => startSnapPoint;
    public SnapPoint EndSnapPoint => endSnapPoint;

    public CarSpawner StartCarSpawner;
    public CarSpawner EndCarSpawner;

    public Path Path => path;
    public List<Lane> StartLanes => startLanes;
    public List<Lane> EndLanes => endLanes;

    void Awake()
    {
        path = new Path(transform.position);
        roadDisplaing = GetComponentInChildren<RoadDisplaing>();

        startLanes = new List<Lane>();
        endLanes = new List<Lane>();

        UpdateLanes();
    }

    public void ConnectToSnapPoint(SnapPoint snapPoint, bool startConnecting)
    {
        if(startConnecting)
        {
            startSnapPoint = snapPoint;
            StartCarSpawner.gameObject.SetActive(false);
        }
        else
        {
            endSnapPoint = snapPoint;
            EndCarSpawner.gameObject.SetActive(false);
        }
        snapPoint.ConnectRoad(this, startConnecting);
    }

    public void DisconnectSnapPoint(bool isStartSnapPoint)
    {
        if (isStartSnapPoint)
        {
            startSnapPoint.DisconnectRoad();
            startSnapPoint = null;
            StartCarSpawner.gameObject.SetActive(true);
            Path.DisconnectStartOrEndPoint(0);
        }
        else
        {
            endSnapPoint.DisconnectRoad();
            endSnapPoint = null;
            EndCarSpawner.gameObject.SetActive(true);
            Path.DisconnectStartOrEndPoint(Path.NumPoints - 1);
        }
    }

    public Lane GetLane(Car car)
    {
        if (car.currentLane == startLanes[0])
            return startLanes[0];
        if (car.currentLane == endLanes[0])
            return endLanes[0];

        if(car.currentLaneable.GetType() == typeof(CrossroadPath))
        {
            CrossroadPath crossroadPath = (CrossroadPath)car.currentLaneable;
            SnapPoint snapPoint = crossroadPath.GetCarEndSnapPoint(car);
            if (snapPoint.connectedRoad)
                return snapPoint.startOfRoadConnected ? snapPoint.connectedRoad.startLanes[0] : snapPoint.connectedRoad.endLanes[0];
        }

        return startLanes[0];
    }
    public ILaneable GetNextLaneable(Car car)
    {        
        if (car.currentLane == startLanes[0])
        {
            if (endSnapPoint)
            {
                return endSnapPoint.crossroadPath;
            }
            else
                return null;
        }
        else
        {
            if (startSnapPoint)
            {
                return startSnapPoint.crossroadPath;
            }
            else
                return null;
        }
    }

    public void AddCar(Car car)
    {
        Lane lane = GetLane(car);
        if (lane.NumCars > 0)
            car.nextCar = lane.LastCar;

        lane.AddCar(car);
    }

    public void RemoveCar(Car car)
    {
        Lane lane = GetLane(car);
        lane.RemoveCar(car);
    }

    public bool HaveCars(Car car)
    {
        for (int i = 0; i < startLanes.Count; i++)
        {
            if (startLanes[i].Cars.Contains(car))
                return true;
        }

        for (int i = 0; i < endLanes.Count; i++)
        {
            if (endLanes[i].Cars.Contains(car))
                return true;
        }

        return false;
    }

    public CrossroadPath GetStartCrossroadPath()
    {
        if (startSnapPoint == null)
            return null;

        return startSnapPoint.crossroadPath;
    }

    public CrossroadPath GetEndCrossroadPath()
    {
        if (endSnapPoint == null)
            return null;

        return endSnapPoint.crossroadPath;
    }

    public void UpdateLanes()
    {
        startLanes.Clear();
        endLanes.Clear();

        startLanes.Add(new Lane(Path, true, RoadDisplaing.roadWidth * 0.2f, 60));
        endLanes.Add(new Lane(Path, false, RoadDisplaing.roadWidth * 0.2f, 60));
    }

    #region For saving
    public PrefabType Prefab => PrefabType.Road;

    public void LoadInfo(byte[] info)
    {
        RoadInfo roadInfo = Helper.ByteArrayToObject(info) as RoadInfo;
        Path newPath = new Path(roadInfo.PathInfo);
        path = newPath;

        StartCarSpawner.LoadInfo(roadInfo.StartCarSpawnerInfo);
        EndCarSpawner.LoadInfo(roadInfo.EndCarSpawnerInfo);

        roadDisplaing.UpdatePoints();
        roadDisplaing.HidePoints();

        if (Path.StartConnected && startSnapPoint == null)
        {
            StartCoroutine(LoadConnectStart());
        }
        if(Path.EndConnected && endSnapPoint == null)
        {
            StartCoroutine(LoadConnectEnd());
        }

        UpdateLanes();

        for (int i = 0; i < startLanes.Count; i++)
        {
            startLanes[i].MaxSpeed = roadInfo.StartMaxSpeeds[i];
        }
        for (int i = 0; i < endLanes.Count; i++)
        {
            endLanes[i].MaxSpeed = roadInfo.EndMaxSpeeds[i];
        }
    }

    IEnumerator LoadConnectStart()
    {
        Collider[] snapPoints;
        while(true)
        {
            snapPoints = Physics.OverlapSphere(Path[0], 0.5f, LayerMask.GetMask("Snap point"));

            if (snapPoints.Length > 0)
            {
                ConnectToSnapPoint(snapPoints[0].GetComponent<SnapPoint>(), true);
            }
            if (snapPoints.Length > 0)
                yield break;
            else
                yield return null;
        } 
    }

    IEnumerator LoadConnectEnd()
    {
        Collider[] snapPoints;
        while (true)
        {
            snapPoints = Physics.OverlapSphere(Path[Path.NumPoints - 1], 0.5f, LayerMask.GetMask("Snap point"));

            if (snapPoints.Length > 0)
            {
                ConnectToSnapPoint(snapPoints[0].GetComponent<SnapPoint>(), false);
            }
            if (snapPoints.Length > 0)
                yield break;
            else
                yield return null;
        } 
    }

    public byte[] SaveInfo()
    {
        return Helper.ObjectToByteArray(new RoadInfo(this));
    }
    #endregion

    public void Delete()
    {
        if(startSnapPoint != null)
        {
            startSnapPoint.DisconnectRoad();
        }

        if (endSnapPoint != null)
        {
            endSnapPoint.DisconnectRoad();
        }

        Destroy(gameObject);
    }
}
