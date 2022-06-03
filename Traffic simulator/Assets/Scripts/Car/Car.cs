using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Car : MonoBehaviour, IPauseable
{
    public ILaneable currentLaneable;

    CrossroadPath nextCrossroadPath;

    Lane currentLane => currentLaneable == null ? null : currentLaneable.GetLane(this);
    public bool fromStartToEnd = true;

    float acceleration = 2.5f;
    public float Speed = 60f;
    float moveVectorLen => Speed * Time.deltaTime * 0.05f;

    float roadComplitionPercent = 0;


    int currentPointIndex;
    int nextPointIndex;
    Vector3 nextPoint => currentLane[nextPointIndex].position;

    bool canMove = true;

    float DistanceToEndOfLane => (fromStartToEnd ? (currentLane.NumPoints - currentPointIndex - 1) : currentPointIndex) * RoadDisplaing.spacing;

    private void Start()
    {
        GameStateManager.OnGameStateChanged.AddListener(OnGameStateChanged);
        nextPointIndex = CalculateNextPointIndex();
        currentPointIndex = fromStartToEnd ? 0 : currentLane.NumPoints - 1;
        nextCrossroadPath = fromStartToEnd ? ((Road)currentLaneable).GetEndCrossroadPath() : ((Road)currentLaneable).GetStartCrossroadPath();
    }

    private void FixedUpdate()
    {
        if (!canMove)
            return;
        
        Speed += acceleration;
        if (Speed > currentLane[currentPointIndex].speed)
            Speed = currentLane[currentPointIndex].speed;
        currentLane.SetSpeed(Speed, currentPointIndex, 3, !fromStartToEnd);

        float distToNextPoint = Vector3.Distance(transform.position, nextPoint);

        //если машина дошла до следующей точки - определить следующую
        if(distToNextPoint < moveVectorLen)
        {
            currentLane.SetMaxSpeed(currentPointIndex, 3, !fromStartToEnd);

            currentPointIndex = nextPointIndex;
            nextPointIndex = CalculateNextPointIndex();

            //если следующей точки нет, значит машина уничтожена, то есть двигать не надо
            if (nextPointIndex < 0)
            {
                return;
            }
        }
        Vector3 moveVector = (nextPoint - transform.position).normalized * moveVectorLen;
        bool needToGiveWay = (fromStartToEnd && currentLane.EndBlocked || !fromStartToEnd && currentLane.StartBlocked) && DistanceToEndOfLane < 2f;

        if(currentLaneable.GetType() == typeof(Road) && needToGiveWay)
        {
            if (nextCrossroadPath.IsAllLanesBlocked() && ((Road)currentLaneable).GetInstanceID() == nextCrossroadPath.MinRoadId())
            {
                currentLane.SetDefaultConnectSpeedNoBlockChange(!fromStartToEnd);
                Speed = currentLane[currentPointIndex].speed;
                needToGiveWay = false;
            }            
        }

        if (!needToGiveWay)
        {
            transform.Translate(moveVector, Space.World);
            transform.LookAt(transform.position + moveVector);
        }
        else
        {
            currentLane.SetSpeed(0, currentPointIndex, 3, fromStartToEnd);
        }

        if (DistanceToEndOfLane < 5f && nextCrossroadPath)
        {
            SetStopToLeftLane();
        }
    }

    private void OnDrawGizmosSelected()
    {
        Debug.Log(Speed + " " + DistanceToEndOfLane + " " + fromStartToEnd + " " + nextCrossroadPath.IsAllLanesBlocked());
    }
    int CalculateNextPointIndex()
    {
        int currentPathIndex = (int)(currentLane.NumPoints * roadComplitionPercent);

        if (!fromStartToEnd)
        {
            currentPathIndex = currentLane.NumPoints - 1 - currentPathIndex;
        }
        //ищем ближайшую точку, расстояние до которой больше, чем длина вектора перемещения
        for (int i = currentPathIndex + (fromStartToEnd ? 1 : -1); ; i += fromStartToEnd ? 1 : -1)
        {
            if(i <= -1 || i >= currentLane.NumPoints)
            {
                if (nextCrossroadPath)
                {
                    ResetStopToLeftLane();
                    nextCrossroadPath = null;
                }

                currentLaneable = currentLaneable.GetNextLaneable(this);
               
                //если дальше некуда идти, то уничтожить машину, иначе перейти к следующему участку дороги
                if (currentLaneable == null)
                {
                    Destroy(gameObject);
                    return -1;
                }

                if (currentLaneable.GetType() == typeof(Road))
                    nextCrossroadPath = fromStartToEnd ? ((Road)currentLaneable).GetEndCrossroadPath() : ((Road)currentLaneable).GetStartCrossroadPath();

                roadComplitionPercent = 0;
                if (fromStartToEnd)
                    currentPointIndex = 0;
                else
                    currentPointIndex = currentLane.NumPoints - 1;
                return CalculateNextPointIndex();
            }

            if(Vector3.Distance(currentLane[i].position, transform.position) > moveVectorLen)
            {
                if (fromStartToEnd)
                {
                    roadComplitionPercent = (float)i / currentLane.NumPoints;
                }
                else
                {
                    roadComplitionPercent = 1 - (float)i / currentLane.NumPoints;
                }

                return i;
            }
        }
    }

    void SetStopToLeftLane()
    {
        SnapPoint snapPoint = nextCrossroadPath.leftSnapPoint;
        if (snapPoint.connectedRoad == null)
            return;
        
        if (snapPoint.startOfRoadConnected)
        {
            snapPoint.connectedRoad.endLanes[0].SetStop(true);
        }
        else
        {
            snapPoint.connectedRoad.startLanes[0].SetStop(false);
        }
    }

    void ResetStopToLeftLane()
    {
        SnapPoint snapPoint = nextCrossroadPath.leftSnapPoint;
        if (snapPoint.connectedRoad == null)
            return;

        if (snapPoint.startOfRoadConnected)
        {
            snapPoint.connectedRoad.endLanes[0].SetDefaultConnectSpeed(true);
        }
        else
        {
            snapPoint.connectedRoad.startLanes[0].SetDefaultConnectSpeed(false);
        }
    }

    public void OnGameStateChanged(GameState gameState)
    {
        canMove = gameState == GameState.Play;
    }

    public void OnRestart()
    {
        Destroy(gameObject);
    }
}
