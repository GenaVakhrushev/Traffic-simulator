using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Car : MonoBehaviour, IPauseable
{
    public IPathable currentPathable;
    public bool fromStartToEnd = true;

    Path currentPath => currentPathable == null ? null : currentPathable.GetPath(this);

    public float Speed = 60f;

    float roadComplitionPercent = 0;

    bool canMove = true;
    private void Start()
    {
        GameStateManager.OnGameStateChanged.AddListener(OnGameStateChanged);
    }
    
    private void FixedUpdate()
    {
        if (!canMove)
            return;

        float moveVectorLen = Speed * Time.deltaTime * 0.02f;

        int currentPathIndex = (int)(currentPath.CachedEvenlySpacedPoints.Length * roadComplitionPercent);

        if (!fromStartToEnd)
        {
            currentPathIndex = currentPath.CachedEvenlySpacedPoints.Length - currentPathIndex;
        }

        Vector3 moveVector;
        float distToNextPoint;

        int i = 1;
        do
        {
            int nextPointIndex = currentPathIndex + i * (fromStartToEnd ? 1 : -1);

            if (nextPointIndex < 0 || nextPointIndex >= currentPath.CachedEvenlySpacedPoints.Length)
            {
                currentPathable = currentPathable.GetNextPathable(this);
                
                if (currentPathable == null)
                {
                    Destroy(gameObject);
                }
                roadComplitionPercent = 0;
                return;
            }

            Vector3 nextPoint = currentPath.CachedEvenlySpacedPoints[nextPointIndex];
            distToNextPoint = Vector3.Distance(transform.position, nextPoint);
            moveVector = (nextPoint - transform.position).normalized * moveVectorLen;

            if (fromStartToEnd)
            {
                roadComplitionPercent = (float)nextPointIndex / currentPath.CachedEvenlySpacedPoints.Length;
            }
            else
            {
                roadComplitionPercent = 1 - (float)nextPointIndex / currentPath.CachedEvenlySpacedPoints.Length;
            }
            i++;
        } while (moveVectorLen > distToNextPoint);

        transform.Translate(moveVector, Space.World);
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
