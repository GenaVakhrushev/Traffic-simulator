using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Road : MonoBehaviour, ISaveable
{
    public Path path;

    SnapPoint startSnapPoint = null;
    SnapPoint endSnapPoint = null;

    RoadDisplaing roadDisplaing;
    
    public RoadDisplaing RoadDisplaing => roadDisplaing;
    public SnapPoint StartSnapPoint => startSnapPoint;
    public SnapPoint EndSnapPoint => endSnapPoint;

    void Awake()
    {
        path = new Path(transform.position);
        roadDisplaing = GetComponentInChildren<RoadDisplaing>();
    }

    public void ConnectToSnapPoint(SnapPoint snapPoint, bool startConnecting)
    {
        if(startConnecting)
        {
            startSnapPoint = snapPoint;
        }
        else
        {
            endSnapPoint = snapPoint;
        }
        snapPoint.ConnectRoad(this, startConnecting);
    }

    public void DisconnectSnapPoint(bool isStartSnapPoint)
    {
        if (isStartSnapPoint)
        {
            startSnapPoint.DisconnectRoad();
            startSnapPoint = null;
        }
        else
        {
            endSnapPoint.DisconnectRoad();
            endSnapPoint = null;
        }
    }

    #region For saving
    public PrefabType Prefab => PrefabType.Road;

    public void LoadInfo(byte[] info)
    {
        RoadInfo roadInfo = Helper.ByteArrayToObject(info) as RoadInfo;
        Path newPath = new Path(roadInfo.PathInfo);
        path = newPath;
        
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
        return Helper.ObjectToByteArray(new RoadInfo(path));
    }
    #endregion
}

//сохраняемая о дороге информация
[System.Serializable]
public class RoadInfo
{
    PathInfo pathInfo;

    public PathInfo PathInfo => pathInfo;

    public RoadInfo(Path path)
    {
        pathInfo = new PathInfo(path);
    }
}
