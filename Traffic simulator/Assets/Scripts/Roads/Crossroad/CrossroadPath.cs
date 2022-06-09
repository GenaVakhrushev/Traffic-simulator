using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class CrossroadPath : MonoBehaviour, ILaneable
{
    List<Car>[] carsByLanes;
    List<Lane> lanes;

    List<Path> possiplePaths;

    public Crossroad crossroad;
    public SnapPoint[] snapPoints;

    public SnapPoint parentSpanPoint;

   float spacing = 0.1f;

    public void Start()
    {
        lanes = new List<Lane>();
        possiplePaths = new List<Path>();

        crossroad = GetComponentInParent<Crossroad>();
        snapPoints = crossroad.GetComponentsInChildren<SnapPoint>();
        parentSpanPoint = GetComponentInParent<SnapPoint>();

        for (int i = 0; i < snapPoints.Length; i++)
        {
            CreatePath(snapPoints[i]);
        }

        carsByLanes = new List<Car>[lanes.Count];
        for (int i = 0; i < carsByLanes.Length; i++)
        {
            carsByLanes[i] = new List<Car>();
        }
    }

    public SnapPoint GetCarEndSnapPoint(Car car)
    {
        return snapPoints[GetCarLaneIndex(car)];
    }

    void CreatePath(SnapPoint point)
    {
        Vector3 snapPointBezierPoint = point.transform.position + (point.transform.position - point.transform.GetChild(0).transform.position);
        Vector3[] points;
        if (point != parentSpanPoint)
        {
            points = new Vector3[]
            {
                transform.position,
                transform.position + Vector3.Project(snapPointBezierPoint - transform.position, transform.forward),
                snapPointBezierPoint + Vector3.Project(transform.position - snapPointBezierPoint, transform.right),
                snapPointBezierPoint
            };
        }
        else
        {
            points = new Vector3[]
            {
                transform.position,
                transform.position + transform.forward * RoadDisplaing.roadWidth * 0.75f,
                snapPointBezierPoint + transform.forward * RoadDisplaing.roadWidth * 0.75f,
                snapPointBezierPoint
            };
        }
        Path newPath = new Path(points);
        newPath.CalculateEvenlySpacedPoints(spacing);
        possiplePaths.Add(newPath);
        lanes.Add(new Lane(newPath, true, 0, 60));
    }

    public Lane GetRandomLane()
    {
        return lanes[Random.Range(0, lanes.Count)];
    }

    int GetCarLaneIndex(Car car)
    {
        for (int i = 0; i < carsByLanes.Length; i++)
        {
            if(carsByLanes[i].Contains(car))
            {
                return i;
            }
        }
        return -1;
    }

    public Lane GetLane(Car car)
    {
        int carLaneIndex = GetCarLaneIndex(car);
        return lanes[carLaneIndex];
    }
    public ILaneable GetNextLaneable(Car car)
    {
        int carLaneIndex = GetCarLaneIndex(car);
        SnapPoint snapPoint = snapPoints[carLaneIndex];
        
        return snapPoint.connectedRoad;
    }

    public void AddCar(Car car)
    {
        Lane newLane = GetRandomLane();
        int newLaneIndex = lanes.IndexOf(newLane);
        carsByLanes[newLaneIndex].Add(car);
    }

    public void RemoveCar(Car car)
    {
        int carLaneIndex = GetCarLaneIndex(car);
        carsByLanes[carLaneIndex].Remove(car);
    }

    public bool HaveCars(Car car)
    {
        return GetCarLaneIndex(car) > -1;
    }
    
    public int MinRoadId()
    {
        int minId = int.MaxValue;
        for (int i = 0; i < snapPoints.Length; i++)
        {
            int roadId = snapPoints[i].connectedRoad.GetInstanceID();
            if (roadId < minId)
                minId = roadId;
        }
        return minId;
    }

    public bool HaveCars()
    {
        for (int i = 0; i < carsByLanes.Length; i++)
        {
            if (carsByLanes[i].Count != 0)
                return true;
        }
        return false;
    }

    public int CarsCount()
    {
        int count = 0;
        for (int i = 0; i < carsByLanes.Length; i++)
        {
            count += carsByLanes[i].Count;
        }
        return count;
    }

    private void OnDrawGizmosSelected()
    {
        Start();

        Gizmos.color = Color.red;
        Gizmos.DrawSphere(transform.position, 0.15f);

        Gizmos.color = Color.blue;
        foreach (SnapPoint snapPoint in snapPoints)
        {
            Gizmos.DrawSphere(snapPoint.transform.position + (snapPoint.transform.position - snapPoint.transform.GetChild(0).transform.position), 0.1f);
        }

#if UNITY_EDITOR
        foreach (Path path in possiplePaths)
        {
            Handles.DrawBezier(path[0], path[3], path[1], path[2], Color.red, null, 2f);
        }
#endif
    }
}