using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CarType { Passenger, Truck, Public }
public enum IntervalType { Fixed, Random }

public class CarSpawner : Clickable, IPauseable
{
    public CarType CarType = CarType.Passenger;
    public IntervalType IntervalType = IntervalType.Fixed;
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
    public float IntervalStart;
    public float IntervalEnd;
    public bool IsActive
    {
        get
        {
            return isActive;
        }
        set
        {
            isActive = value;
            if (isActive && GameStateManager.CurrentGameState == GameState.Play)
            {
                StartSpawn();
            }
            else
            {
                StopSpawn();
            }
        }
    }

    float spawnDeltaTime = 2f;
    bool isActive = true;
    [HideInInspector]
    public bool onStart;

    Road road;

    public override void Start()
    {
        base.Start();
        road = GetComponentInParent<Road>();

        GameStateManager.OnGameStateChanged.AddListener(OnGameStateChanged);
    }

    public void StartSpawn()
    {
        StopAllCoroutines();
        if (gameObject.activeSelf && isActive)
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
        if (IntervalType == IntervalType.Fixed)
            yield return new WaitForSeconds(SpawnDeltaTime);
        else
            yield return new WaitForSeconds(Random.Range(IntervalStart, IntervalEnd));
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
