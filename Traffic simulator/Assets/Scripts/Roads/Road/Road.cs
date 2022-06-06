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

    public PathInfo PathInfo => pathInfo;
    public CarSpawnerInfo StartCarSpawnerInfo => startCarSpawnerInfo;
    public CarSpawnerInfo EndCarSpawnerInfo => endCarSpawnerInfo;

    public RoadInfo(Path path, CarSpawner startCarSpawner, CarSpawner endCarSpawner)
    {
        pathInfo = new PathInfo(path);
        startCarSpawnerInfo = new CarSpawnerInfo(startCarSpawner);
        endCarSpawnerInfo = new CarSpawnerInfo(endCarSpawner);
    }
}

public class Road : Clickable, ISaveable, ILaneable, IDeleteable
{
    public Path path;
    public List<Car> cars;
    public List<Lane> startLanes;
    public List<Lane> endLanes;

    SnapPoint startSnapPoint = null;
    SnapPoint endSnapPoint = null;

    RoadDisplaing roadDisplaing;
    
    public RoadDisplaing RoadDisplaing => roadDisplaing;
    public SnapPoint StartSnapPoint => startSnapPoint;
    public SnapPoint EndSnapPoint => endSnapPoint;

    public CarSpawner StartCarSpawner;
    public CarSpawner EndCarSpawner;

    void Awake()
    {
        path = new Path(transform.position);
        roadDisplaing = GetComponentInChildren<RoadDisplaing>();

        cars = new List<Car>();
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

            endLanes[0].SetDefaultConnectSpeed(true);
        }
        else
        {
            endSnapPoint = snapPoint;
            EndCarSpawner.gameObject.SetActive(false);

            startLanes[0].SetDefaultConnectSpeed(false);
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
            path.DisconnectStartOrEndPoint(0);
        }
        else
        {
            endSnapPoint.DisconnectRoad();
            endSnapPoint = null;
            EndCarSpawner.gameObject.SetActive(true);
            path.DisconnectStartOrEndPoint(path.NumPoints - 1);
        }
    }

    public Lane GetLane(Car car)
    {
        return car.fromStartToEnd ? startLanes[0] : endLanes[0];
    }
    public ILaneable GetNextLaneable(Car car)
    {
        cars.Remove(car);
        if (car.fromStartToEnd)
        {
            car.fromStartToEnd = true;
            if (endSnapPoint)
                return endSnapPoint.crossroadPath;
            else
                return null;
        }
        else
        {
            car.fromStartToEnd = true;
            if (startSnapPoint)
                return startSnapPoint.crossroadPath;
            else
                return null;
        }
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

        startLanes.Add(new Lane(path, true, RoadDisplaing.roadWidth * 0.2f, 60));
        endLanes.Add(new Lane(path, false, RoadDisplaing.roadWidth * 0.2f, 60));
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

        if (path.StartConnected && startSnapPoint == null)
        {
            StartCoroutine(LoadConnectStart());
        }
        if(path.EndConnected && endSnapPoint == null)
        {
            StartCoroutine(LoadConnectEnd());
        }

        UpdateLanes();
    }

    IEnumerator LoadConnectStart()
    {
        Collider[] snapPoints;
        while(true)
        {
            snapPoints = Physics.OverlapSphere(path[0], 0.5f, LayerMask.GetMask("Snap point"));

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
            snapPoints = Physics.OverlapSphere(path[path.NumPoints - 1], 0.5f, LayerMask.GetMask("Snap point"));

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
        return Helper.ObjectToByteArray(new RoadInfo(path, StartCarSpawner, EndCarSpawner));
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
