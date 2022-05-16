using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Car : MonoBehaviour, IPauseable
{
    public ILaneable currentLaneable;
    Lane currentLane => currentLaneable == null ? null : currentLaneable.GetLane(this);
    public bool fromStartToEnd = true;

    public float Speed = 60f;
    float moveVectorLen => Speed * Time.deltaTime * 0.02f;

    float roadComplitionPercent = 0;



    int nextPointIndex;
    Vector3 nextPoint => currentLane[nextPointIndex].position;

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
                currentLaneable = currentLaneable.GetNextLaneable(this);
                //если дальше некуда идти, то уничтожить машину, иначе перейти к следующему участку дороги
                if(currentLaneable == null)
                {
                    Destroy(gameObject);
                    return -1;
                }
                roadComplitionPercent = 0; 
                return CalculateNextPointIndex(); ;
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
    public void OnGameStateChanged(GameState gameState)
    {
        canMove = gameState == GameState.Play;
    }

    public void OnRestart()
    {
        Destroy(gameObject);
    }
}
