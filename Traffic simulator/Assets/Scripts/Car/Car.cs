using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Car : MonoBehaviour, IPauseable
{
    public ILaneable currentLaneable;
    public Lane currentLane;

    CrossroadPath nextCrossroadPath;

    //Lane currentLane => currentLaneable == null ? null : currentLaneable.GetLane(this);
    //public bool fromStartOfRoad = true;

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

        //���� ������ ����� �� ��������� ����� - ���������� ���������
        if(distToNextPoint < moveVectorLen)
        {
            currentLane.SetMaxSpeed(currentPointIndex, 3, false);

            currentPointIndex = nextPointIndex;
            nextPointIndex = CalculateNextPointIndex();

            //���� ��������� ����� ���, ������ ������ ����������, �� ���� ������� �� ����
            if (nextPointIndex < 0)
            {
                return;
            }
        }
        Vector3 moveVector = (nextPoint - transform.position).normalized * moveVectorLen;
        bool needToGiveWay = currentLane.EndBlocked && DistanceToEndOfLane < 2f;

        if(currentLaneable.GetType() == typeof(Road) && needToGiveWay)
        {
            if (nextCrossroadPath.IsAllLanesBlocked() && ((Road)currentLaneable).GetInstanceID() == nextCrossroadPath.MinRoadId())
            {
                //currentLane.SetDefaultConnectSpeedNoBlockChange();
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
            currentLane.SetSpeed(0, currentPointIndex, 3, true);
        }

        if (DistanceToEndOfLane < 5f && nextCrossroadPath)
        {
            //SetStopToLeftLane();
        }
    }

    private void OnDrawGizmosSelected()
    {
        Debug.Log(Speed + " " + DistanceToEndOfLane + " " + nextCrossroadPath.IsAllLanesBlocked());
    }
    int CalculateNextPointIndex()
    {
        int currentPathIndex = (int)(currentLane.NumPoints * roadComplitionPercent);

        //���� ��������� �����, ���������� �� ������� ������, ��� ����� ������� �����������
        for (int i = currentPathIndex + 1; ; i += 1)
        {
            if(i <= -1 || i >= currentLane.NumPoints)
            {
                if (nextCrossroadPath)
                {
                    nextCrossroadPath = null;
                }

                currentLaneable = currentLaneable.GetNextLaneable(this);
               
                //���� ������ ������ ����, �� ���������� ������, ����� ������� � ���������� ������� ������
                if (currentLaneable == null)
                {
                    Destroy(gameObject);
                    return -1;
                }

                if (currentLaneable.GetType() == typeof(Road))
                {
                    nextCrossroadPath = ((Road)currentLaneable).GetEndCrossroadPath();
                    ((Road)currentLaneable).cars.Add(this);
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
