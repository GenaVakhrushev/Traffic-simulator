using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Car : MonoBehaviour, IPauseable
{
    public ILaneable currentLaneable;
    public Lane currentLane;

    CrossroadPath nextCrossroadPath;

    float acceleration = 2.5f;
    public float Speed = 60f;
    float moveVectorLen => Speed / 3.6f * Time.deltaTime;

    float roadComplitionPercent = 0;


    int currentPointIndex;
    int nextPointIndex;
    Vector3 nextPoint => currentLane[nextPointIndex].position;

    bool canMove = true;

    float DistanceToEndOfLane => (currentLane.NumPoints - currentPointIndex - 1) * RoadDisplaing.spacing;

    private void Start()
    {
        GameStateManager.OnGameStateChanged.AddListener(OnGameStateChanged);
        nextPointIndex = CalculateNextPointIndex();
        currentPointIndex =  0;
        nextCrossroadPath = ((Road)currentLaneable).GetEndCrossroadPath();
    }

    private void FixedUpdate()
    {
        if (!canMove)
            return;
        
        Speed += acceleration;
        if (Speed > currentLane[currentPointIndex].speed)
            Speed = currentLane[currentPointIndex].speed;
        currentLane.SetSpeed(Speed, currentPointIndex, 3, false);

        float distToNextPoint = Vector3.Distance(transform.position, nextPoint);

        //если машина дошла до следующей точки - определить следующую
        if(distToNextPoint < moveVectorLen)
        {
            currentLane.SetMaxSpeed(currentPointIndex, 3, false);

            currentPointIndex = nextPointIndex;
            nextPointIndex = CalculateNextPointIndex();

            //если следующей точки нет, значит машина уничтожена, то есть двигать не надо
            if (nextPointIndex < 0)
            {
                return;
            }
        }
        Vector3 moveVector = (nextPoint - transform.position).normalized * moveVectorLen;
        bool needToGiveWay = CheckGiveWay();

        if (!needToGiveWay)
        {
            transform.Translate(moveVector, Space.World);
            transform.LookAt(transform.position + moveVector);
            currentLane.ResetSpeed(currentPointIndex, 3, true);
        }
        else
        {
            currentLane.SetSpeed(0, currentPointIndex, 3, true);
        }                                                                         
    }

    private bool CheckGiveWay()
    {
        if (nextCrossroadPath == null)
            return false;

        if (DistanceToEndOfLane > 1.5f)
            return false;

        Crossroad crossroad = nextCrossroadPath.crossroad;
        if (crossroad.crossroadType == CrossroadType.Regulated)
            return CheckRegulatedGiveWay();

        if (crossroad.HaveMainRoad)
            return CheckMainRoadGiveWay();

        CrossroadPath rightCrossroadPath = nextCrossroadPath.crossroad.GetRightSnapPoint(nextCrossroadPath.parentSpanPoint).crossroadPath;

        if (rightCrossroadPath.HaveCars())
            return true;

        return false;
    }

    private bool CheckMainRoadGiveWay()
    {
        throw new NotImplementedException();
    }
           
    private bool CheckRegulatedGiveWay()
    {
        throw new NotImplementedException();
    }

    private void OnDrawGizmosSelected()
    {
        Debug.Log(Speed + " " + DistanceToEndOfLane + " " + nextCrossroadPath);
        Helper.CreateCube(nextCrossroadPath.transform.position);
    }

    int CalculateNextPointIndex()
    {
        int currentPathIndex = (int)(currentLane.NumPoints * roadComplitionPercent);

        //ищем ближайшую точку, расстояние до которой больше, чем длина вектора перемещения
        for (int i = currentPathIndex + 1; ; i += 1)
        {
            if(i <= -1 || i >= currentLane.NumPoints)
            {
                if (nextCrossroadPath)
                {
                    nextCrossroadPath = null;
                }

                ILaneable nextLaneable = currentLaneable.GetNextLaneable(this);
                if (nextLaneable != null)
                {
                    if (nextLaneable.GetType() == typeof(Road))
                    {
                        Road road = (Road)nextLaneable;
                        SnapPoint endSnapPoint = ((CrossroadPath)currentLaneable).GetEndSnapPoint(this);
                        nextCrossroadPath = endSnapPoint.startOfRoadConnected ? road.GetEndCrossroadPath() : road.GetStartCrossroadPath();
                    }

                    nextLaneable.AddCar(this);
                    currentLane = nextLaneable.GetLane(this);
                }

                currentLaneable.RemoveCar(this);
                currentLaneable = nextLaneable;        
               
                //если дальше некуда идти, то уничтожить машину, иначе перейти к следующему участку дороги
                if (currentLaneable == null)
                {
                    Destroy(gameObject);
                    return -1;
                }

                roadComplitionPercent = 0;
                currentPointIndex = 0;
                return CalculateNextPointIndex();
            }

            if (Vector3.Distance(currentLane[i].position, transform.position) > moveVectorLen)
            {
                roadComplitionPercent = (float)i / currentLane.NumPoints;

                return i;
            }
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
