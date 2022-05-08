using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarSpawner : MonoBehaviour, IPauseable
{
    
    [HideInInspector]
    public bool onStart;
    public float SpawnDeltaTime
    {
        get
        {
            return spawnDeltaTime;
        }
        set
        {
            spawnDeltaTime = value;
            StartSpawn();
        }
    }

    float spawnDeltaTime = 2f;

    Road road;

    void Start()
    {
        road = GetComponentInParent<Road>();

        GameStateManager.OnGameStateChanged.AddListener(OnGameStateChanged);
    }

    public void StartSpawn()
    {
        StopAllCoroutines();
        if (gameObject.activeSelf)
            StartCoroutine(SpawnCar());
    }

    public void StopSpawn()
    {
        StopAllCoroutines();
    }

    IEnumerator SpawnCar()
    {
        Car newCar = Instantiate(Prefabs.Instance.Car, transform.position, Quaternion.identity).GetComponent<Car>();
        newCar.currentPathable = road;
        newCar.fromStartToEnd = onStart;
        yield return new WaitForSeconds(SpawnDeltaTime);
        StartCoroutine(SpawnCar());
    }

    public void OnGameStateChanged(GameState gameState)
    {
        if(gameState == GameState.Pause)
        {
            StopSpawn();
        }
        else
        {
            StartSpawn();
        }
    }

    public void OnRestart()
    {
        StopSpawn();
    }
}
