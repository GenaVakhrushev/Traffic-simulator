using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CarType { Passenger, Truck, Public }
public enum IntervalType { Fixed, Random }

[System.Serializable]
public class CarSpawnerInfo
{
    public CarType carType;
    public IntervalType intervalType;
    public float spawnDeltaTime;
    public float intervalStart;
    public float intervalEnd;
    public bool isActive;
    public bool onStart;
    public IntervalType startSpeedIntervalType;
    public float fixedStartSpeed;
    public float fromStartSpeed;
    public float toStartSpeed;

    public CarSpawnerInfo(CarSpawner carSpawner)
    {
        carType = carSpawner.CarType;
        intervalType = carSpawner.IntervalType;
        spawnDeltaTime = carSpawner.SpawnDeltaTime;
        intervalStart = carSpawner.IntervalStart;
        intervalEnd = carSpawner.IntervalEnd;
        isActive = carSpawner.IsActive;
        onStart = carSpawner.OnStart;
        startSpeedIntervalType = carSpawner.StartSpeedIntervalType;
        fixedStartSpeed = carSpawner.FixedStartSpeed;
        fromStartSpeed = carSpawner.FromStartSpeed;
        toStartSpeed = carSpawner.ToStartSpeed;
    }
}

public class CarSpawner : Clickable, IPauseable
{
    float spawnDeltaTime = 4f;
    bool isActive = true;

    public CarType CarType = CarType.Passenger;
    public IntervalType IntervalType = IntervalType.Fixed;
    public IntervalType StartSpeedIntervalType = IntervalType.Fixed;

    [HideInInspector] public bool OnStart;

    [HideInInspector] public float IntervalStart;
    [HideInInspector] public float IntervalEnd;
    
    [HideInInspector] public float FixedStartSpeed = 60;
    [HideInInspector] public float FromStartSpeed = 60;
    [HideInInspector] public float ToStartSpeed = 60;

    public float SpawnDeltaTime
    {
        get
        {
            return spawnDeltaTime;
        }
        set
        {
            spawnDeltaTime = value;
            if (GameStateManager.CurrentGameState == GameState.Play)
                StartSpawn();
        }
    }
    public bool IsActive
    {
        get
        {
            return isActive;
        }
        set
        {
            isActive = value;

            Color newColor = spawnerRenderer.material.color;
            if (IsActive)
                spawnerRenderer.material.color = new Color(newColor.r, newColor.g, newColor.b, 1f);
            else
                spawnerRenderer.material.color = new Color(newColor.r, newColor.g, newColor.b, 0.5f);

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

    Road road;

    Renderer spawnerRenderer;

    void Awake()
    {
        spawnerRenderer = GetComponentInChildren<Renderer>();
    }

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
        newCar.currentLaneable = road;
        newCar.currentLane = OnStart ? road.StartLanes[0] : road.EndLanes[0];
        if (StartSpeedIntervalType == IntervalType.Fixed)
            newCar.Speed = FixedStartSpeed;
        else
            newCar.Speed = Random.Range(FromStartSpeed, ToStartSpeed);
        road.AddCar(newCar);
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

    public void LoadInfo(CarSpawnerInfo carSpawnerInfo)
    {
        CarType = carSpawnerInfo.carType;
        IntervalType = carSpawnerInfo.intervalType;
        spawnDeltaTime = carSpawnerInfo.spawnDeltaTime;
        IntervalStart = carSpawnerInfo.intervalStart;
        IntervalEnd = carSpawnerInfo.intervalEnd;
        IsActive = carSpawnerInfo.isActive;
        OnStart = carSpawnerInfo.onStart;
        StartSpeedIntervalType = carSpawnerInfo.startSpeedIntervalType;
        FixedStartSpeed = carSpawnerInfo.fixedStartSpeed;
        FromStartSpeed = carSpawnerInfo.fromStartSpeed;
        ToStartSpeed = carSpawnerInfo.toStartSpeed;
    }
}
