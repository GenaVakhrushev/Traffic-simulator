using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Road : MonoBehaviour, ISaveable
{
    public Path path;

    void Awake()
    {
        path = new Path(transform.position);
    }

    #region For saving
    public PrefabType Prefab => PrefabType.Road;

    public void LoadInfo(byte[] info)
    {
        RoadInfo roadInfo = Helper.ByteArrayToObject(info) as RoadInfo;
        Path newPath = new Path(roadInfo.Points, roadInfo.IsClosed, roadInfo.AutoSetControlPoints);
        path = newPath;
        
        RoadDisplaing roadDisplaing = GetComponentInChildren<RoadDisplaing>();
        roadDisplaing.UpdatePoints();
        roadDisplaing.HidePoints();
    }

    public byte[] SaveInfo()
    {
        return Helper.ObjectToByteArray(new RoadInfo(path));
    }
    #endregion
}

//сохраняемая о дороге информация
[System.Serializable]
public class RoadInfo
{
    //path info
    float[,] points;
    bool isClosed;
    bool autoSetControlPoints;

    public Vector3[] Points
    {
        get
        {
            Vector3[] result = new Vector3[points.GetLength(0)];
            for (int i = 0; i < points.GetLength(0); i++)
            {
                result[i] = new Vector3(points[i, 0], points[i, 1], points[i, 2]);
            }
            return result;
        }
    }

    public bool IsClosed => isClosed;
    public bool AutoSetControlPoints => autoSetControlPoints;

    public RoadInfo(Path path)
    {
        points = new float[path.NumPoints, 3];
        for (int i = 0; i < path.NumPoints; i++)
        {
            points[i, 0] = path[i].x;
            points[i, 1] = path[i].y;
            points[i, 2] = path[i].z;
        }

        isClosed = path.IsClosed;
        autoSetControlPoints = path.AutoSetControlPoints;
    }
}
