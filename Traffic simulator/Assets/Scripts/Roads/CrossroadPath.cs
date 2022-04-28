using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class CrossroadPath : MonoBehaviour
{
    List<Path> possiplePaths;

    Transform rootPoint;
    Transform[] pathPoints;

   float spacing = 0.1f;

    private void Start()
    {
        possiplePaths = new List<Path>();

        AssignPoints();

        foreach (Transform pathPoint in pathPoints)
        {
            CreatePath(pathPoint);
        }
    }

    void AssignPoints()
    {
        rootPoint = transform.GetChild(0);
        List<Transform> points = new List<Transform>();
        foreach (Transform pathPoint in rootPoint)
        {
            points.Add(pathPoint);
        }
        pathPoints = points.ToArray();
    }

    void CreatePath(Transform point)
    {
        Vector3[] points = new Vector3[]
        {
            rootPoint.position,
            rootPoint.position + Vector3.Project(point.position - rootPoint.position, rootPoint.forward),
            point.position + Vector3.Project(rootPoint.position - point.position, rootPoint.right),
            point.position
        };
        Path newPath = new Path(points, false, false);
        newPath.CalculateEvenlySpacedPoints(spacing);
        possiplePaths.Add(newPath);
    }

    private void OnDrawGizmosSelected()
    {
        Start();

        Gizmos.color = Color.red;
        Gizmos.DrawSphere(rootPoint.position, 0.2f);

        Gizmos.color = Color.blue;
        foreach  (Transform pathPoint in pathPoints)
        {
            Gizmos.DrawSphere(pathPoint.position, 0.1f);
        }
        
        foreach (Path path in possiplePaths)
        {
            Handles.DrawBezier(path[0], path[3], path[1], path[2], Color.red, null, 2f);
        }
    }
}