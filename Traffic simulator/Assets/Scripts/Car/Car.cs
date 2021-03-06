using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Direction { Back, Right, Forward, Left }

public class Car : MonoBehaviour, IPauseable
{
    public ILaneable currentLaneable;
    public Lane currentLane;

    public Direction direction;

    CrossroadPath nextCrossroadPath;
    ILaneable nextLaneable;

    public Car nextCar;
    float distanceToNextCar => nextCar ? Math.Abs(nextCar.currentPointIndex - currentPointIndex) * RoadDisplaing.spacing : float.MaxValue;
    float carsMaxDist = 3f;

    public float Speed = 60f;
    float acceleration => 20f * Time.deltaTime;
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

        Road road = (Road)currentLaneable;
        nextCrossroadPath = (CrossroadPath)currentLaneable.GetNextLaneable(this);
        nextLaneable = nextCrossroadPath;
    }

    private void FixedUpdate()
    {
        if (!canMove)
            return;
        
        Speed += acceleration;
        if (Speed > currentLane[currentPointIndex].maxSpeed)
            Speed = currentLane[currentPointIndex].maxSpeed;

        if (nextCar != null && distanceToNextCar < carsMaxDist)
        {
            Speed = nextCar.Speed;
        }

        float distToNextPoint = Vector3.Distance(transform.position, nextPoint);

        //???? ?????? ????? ?? ????????? ????? - ?????????? ?????????
        if(distToNextPoint < moveVectorLen)
        {
            currentLane.SetMaxSpeed(currentPointIndex, 3, false);

            currentPointIndex = nextPointIndex;
            nextPointIndex = CalculateNextPointIndex();

            //???? ????????? ????? ???, ?????? ?????? ??????????, ?? ???? ??????? ?? ????
            if (nextPointIndex < 0)
            {
                return;
            }
        }
        Vector3 moveVector = (nextPoint - transform.position).normalized * moveVectorLen;
        bool needToGiveWay = CheckGiveWay();
        
        if(nextLaneable != null && DistanceToEndOfLane <= 4f)
        {
            if (!nextLaneable.HaveCar(this))
            {
                nextLaneable.AddCar(this);
            }
        }

        if (!needToGiveWay)
        {
            transform.Translate(moveVector, Space.World);
            transform.LookAt(transform.position + moveVector);
        }
        else
        {
            Speed = 0;
        }                                                                         
    }

    private bool CheckGiveWay()
    {
        if (nextCrossroadPath == null)
            return false;

        if (DistanceToEndOfLane > 0.5f)
            return false;

        Crossroad crossroad = nextCrossroadPath.crossroad;
        if (crossroad.CrossroadType == CrossroadType.Regulated)
            return CheckRegulatedGiveWay(crossroad);

        if (crossroad.HaveMainRoad)
            return CheckMainRoadGiveWay(crossroad);

        return CheckNoMainRoadGiveWay(crossroad);
    }

    private bool CheckNoMainRoadGiveWay(Crossroad crossroad)
    {
        CrossroadPath rightCrossroadPath = crossroad.GetRightCrossroadPath(nextCrossroadPath);
        CrossroadPath forwardCrossroadPath = crossroad.GetForwardCrossroadPath(nextCrossroadPath);

        if (crossroad.AllLinesBlocked() && (Road)currentLaneable == crossroad.RoadWithMinID())
            return false;

        if (direction == Direction.Right && !rightCrossroadPath.HaveCarsBack())
            return false;

        bool giveWayToForwardCars = (forwardCrossroadPath.HaveCarsRight() || forwardCrossroadPath.HaveCarsForfard()) && (direction == Direction.Left || direction == Direction.Back);

        if (rightCrossroadPath.HaveCars() || giveWayToForwardCars)
            return true;

        return false;
    }

    private bool CheckMainRoadGiveWay(Crossroad crossroad)
    {
        CrossroadPath firstMainPath = crossroad.SnapPoints[crossroad.MainRoadPointIndexes[0]].crossroadPath;
        CrossroadPath secondMainPath = crossroad.SnapPoints[crossroad.MainRoadPointIndexes[1]].crossroadPath;
        CrossroadPath rightCrossroadPath = crossroad.GetRightCrossroadPath(nextCrossroadPath);


        int carRoadNum = crossroad.GetCrossroadPathNum(nextCrossroadPath);
        bool onMainRoad = carRoadNum == crossroad.MainRoadPointIndexes[0] || carRoadNum == crossroad.MainRoadPointIndexes[1];

        if (!onMainRoad && (firstMainPath.HaveCars() || secondMainPath.HaveCars() || rightCrossroadPath.HaveCars()))
            return true;

        int rightRoadNum = crossroad.GetCrossroadPathNum(rightCrossroadPath);
        bool isRightRoadMain = rightRoadNum == crossroad.MainRoadPointIndexes[0] || rightRoadNum == crossroad.MainRoadPointIndexes[1];

        if (onMainRoad && isRightRoadMain && rightCrossroadPath.HaveCars())
            return true;

        return false;
    }

    private bool CheckRegulatedGiveWay(Crossroad crossroad)
    {
        TrafficLight trafficLight = nextCrossroadPath.parentSnapPoint.trafficLight;

        if (trafficLight.LightColor != Color.green)
            return true;

        return false;
    }

    private void OnDrawGizmosSelected()
    {
        Crossroad crossroad = nextCrossroadPath.crossroad;
        CrossroadPath firstMainPath = crossroad.SnapPoints[crossroad.MainRoadPointIndexes[0]].crossroadPath;
        CrossroadPath secondMainPath = crossroad.SnapPoints[crossroad.MainRoadPointIndexes[1]].crossroadPath;
        CrossroadPath rightCrossroadPath = crossroad.GetRightCrossroadPath(nextCrossroadPath);
        int carRoadNum = crossroad.GetCrossroadPathNum(nextCrossroadPath);
        bool onMainRoad = carRoadNum == crossroad.MainRoadPointIndexes[0] || carRoadNum == crossroad.MainRoadPointIndexes[1];

        Debug.Log(Speed + " " + CheckMainRoadGiveWay(crossroad) + " |" + firstMainPath.HaveCars() + " " + secondMainPath.HaveCars() + " |" + onMainRoad);
    }

    int CalculateNextPointIndex()
    {
        int currentPathIndex = (int)(currentLane.NumPoints * roadComplitionPercent);

        //???? ????????? ?????, ?????????? ?? ??????? ??????, ??? ????? ??????? ???????????
        for (int i = currentPathIndex + 1; ; i += 1)
        {
            if(i <= -1 || i >= currentLane.NumPoints)
            {
                if (nextCrossroadPath)
                {
                    nextCrossroadPath = null;
                }

                //nextLaneable = currentLaneable.GetNextLaneable(this);
                if (nextLaneable != null)
                {
                    if (nextLaneable.GetType() == typeof(Road))
                    {
                        Road road = (Road)nextLaneable;
                        SnapPoint endSnapPoint = ((CrossroadPath)currentLaneable).GetCarEndSnapPoint(this);
                        nextCrossroadPath = endSnapPoint.startOfRoadConnected ? road.GetEndCrossroadPath() : road.GetStartCrossroadPath();
                        //nextLaneable.AddCar(this);
                    }

                   // nextLaneable.AddCar(this);
                    currentLane = nextLaneable.GetLane(this);
                }
                
                currentLaneable.RemoveCar(this);
                currentLaneable = nextLaneable;

                //???? ?????? ?????? ????, ?? ?????????? ??????, ????? ??????? ? ?????????? ??????? ??????
                if (currentLaneable == null)
                {
                    Destroy(gameObject);
                    return -1;
                }

                nextLaneable = currentLaneable.GetNextLaneable(this);

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
