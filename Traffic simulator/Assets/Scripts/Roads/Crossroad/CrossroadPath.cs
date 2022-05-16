using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class CrossroadPath : MonoBehaviour, ILaneable
{
    List<Path> possiplePaths;
    List<Car>[] carsByPaths;

    List<Lane> lanes;

    Crossroad crossroad;
    SnapPoint[] snapPoints;

    SnapPoint parentSpanPoint;

   float spacing = 0.1f;

    private void Start()
    {
        possiplePaths = new List<Path>();
        lanes = new List<Lane>();

        crossroad = GetComponentInParent<Crossroad>();
        snapPoints = crossroad.GetComponentsInChildren<SnapPoint>();
        parentSpanPoint = GetComponentInParent<SnapPoint>();

        for (int i = 0; i < snapPoints.Length; i++)
        {
            CreatePath(snapPoints[i]);
        }

        carsByPaths = new List<Car>[possiplePaths.Count];
        for (int i = 0; i < carsByPaths.Length; i++)
        {
            carsByPaths[i] = new List<Car>();
        }
    }

    void CreatePath(SnapPoint point)
    {
        Vector3 snapPointBezierPoint = point.transform.position - point.transform.right * 0.4f;
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
                transform.position + 1.5f * transform.forward,
                snapPointBezierPoint + 1.5f * transform.forward,
                snapPointBezierPoint
            };
        }
        Path newPath = new Path(points);
        newPath.CalculateEvenlySpacedPoints(spacing);
        possiplePaths.Add(newPath);
        lanes.Add(new Lane(newPath, true, 0, 60));
    }

    public Path GetRandomPath()
    {
        return possiplePaths[Random.Range(0, possiplePaths.Count)];
    }

    int GetCarPathIndex(Car car)
    {
        for (int i = 0; i < carsByPaths.Length; i++)
        {
            if(carsByPaths[i].Contains(car))
            {
                return i;
            }
        }
        return -1;
    }

    public Lane GetLane(Car car)
    {
        int carPathIndex = GetCarPathIndex(car);
        if(carPathIndex != -1)
        {
            return lanes[carPathIndex];
        }
        else
        {
            Path newPath = GetRandomPath();
            int newPathIndex = possiplePaths.IndexOf(newPath);
            carsByPaths[newPathIndex].Add(car);
            return lanes[newPathIndex];
        }
    }
    public ILaneable GetNextLaneable(Car car)
    {
        int carPathIndex = GetCarPathIndex(car);
        SnapPoint snapPoint = snapPoints[carPathIndex];
        car.fromStartToEnd = snapPoint.startOfRoadConnected;
        carsByPaths[carPathIndex].Remove(car);

        return snapPoint.connectedRoad;
    }

    private void OnDrawGizmosSelected()
    {
        Start();

        Gizmos.color = Color.red;
        Gizmos.DrawSphere(transform.position, 0.15f);

        Gizmos.color = Color.blue;
        foreach (SnapPoint snapPoint in snapPoints)
        {
            Gizmos.DrawSphere(snapPoint.transform.position - snapPoint.transform.right * 0.4f, 0.1f);
        }

        foreach (Path path in possiplePaths)
        {
            Handles.DrawBezier(path[0], path[3], path[1], path[2], Color.red, null, 2f);
        }
    }
}