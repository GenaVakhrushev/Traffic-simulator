using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Car : MonoBehaviour, IPauseable
{
    public ILaneable currentPathable;
    Path currentPath => currentPathable == null ? null : currentPathable.GetLane(this);
    public bool fromStartToEnd = true;

    public float Speed = 60f;
    float moveVectorLen => Speed * Time.deltaTime * 0.02f;

    float roadComplitionPercent = 0;
    int nextPointIndex;
    Vector3 nextPoint => currentPath.CachedEvenlySpacedPoints[nextPointIndex] + RightOffset(nextPointIndex);

    bool canMove = true;
    private void Start()
    {
        GameStateManager.OnGameStateChanged.AddListener(OnGameStateChanged);
        nextPointIndex = CalculateNextPointIndex();
    }

    private void FixedUpdate()
    {
        if (!canMove)
            return;

        float distToNextPoint = Vector3.Distance(transform.position, nextPoint);

        //если машина дошла до следующей точки - определить следующую
        if(distToNextPoint < moveVectorLen)
        {
            nextPointIndex = CalculateNextPointIndex();
            //если следующей точки нет, значит машина уничтожена, то есть двигать не надо
            if (nextPointIndex < 0)
            {
                return;
            }
        }

        Vector3 moveVector = (nextPoint - transform.position).normalized * moveVectorLen;
        
        transform.Translate(moveVector, Space.World);
        transform.LookAt(transform.position + moveVector);         
    }

    //получаем вектор вправо таким же способом как при создании меша
    Vector3 RightOffset(int pointIndex)
    {
        if (currentPathable.GetType() != typeof(Road))
            return Vector3.zero;

        Vector3 point = currentPath.CachedEvenlySpacedPoints[pointIndex];
        Vector3 forward = Vector3.zero;
        if (fromStartToEnd)
        {
            if (pointIndex < currentPath.CachedEvenlySpacedPoints.Length - 1)
            {
                forward += currentPath.CachedEvenlySpacedPoints[pointIndex + 1] - point;
            }
            if (pointIndex > 0)
            {
                forward += point - currentPath.CachedEvenlySpacedPoints[pointIndex - 1];
            }
        }
        else
        {
            if (pointIndex > 0)
            {
                forward += currentPath.CachedEvenlySpacedPoints[pointIndex - 1] - point;
            }
            if (pointIndex < currentPath.CachedEvenlySpacedPoints.Length - 1)
            {
                forward += point - currentPath.CachedEvenlySpacedPoints[pointIndex + 1];
            }
        }

        forward.Normalize();
        forward *= 0.2f;

        return new Vector3(forward.z, forward.y, -forward.x);
    }

    int CalculateNextPointIndex()
    {
        int currentPathIndex = (int)(currentPath.CachedEvenlySpacedPoints.Length * roadComplitionPercent);

        if (!fromStartToEnd)
        {
            currentPathIndex = currentPath.CachedEvenlySpacedPoints.Length - 1 - currentPathIndex;
        }
        //ищем ближайшую точку, расстояние до которой больше, чем длина вектора перемещения
        for (int i = currentPathIndex + (fromStartToEnd ? 1 : -1); ; i += fromStartToEnd ? 1 : -1)
        {
            if(i <= -1 || i >= currentPath.CachedEvenlySpacedPoints.Length)
            {
                currentPathable = currentPathable.GetNextLaneable(this);
                //если дальше некуда идти, то уничтожить машину, иначе перейти к следующему участку дороги
                if(currentPathable == null)
                {
                    Destroy(gameObject);
                    return -1;
                }
                roadComplitionPercent = 0; 
                return CalculateNextPointIndex(); ;
            }
            if(Vector3.Distance(currentPath.CachedEvenlySpacedPoints[i] + RightOffset(i),transform.position) > moveVectorLen)
            {
                if (fromStartToEnd)
                {
                    roadComplitionPercent = (float)i / currentPath.CachedEvenlySpacedPoints.Length;
                }
                else
                {
                    roadComplitionPercent = 1 - (float)i / currentPath.CachedEvenlySpacedPoints.Length;
                }

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
