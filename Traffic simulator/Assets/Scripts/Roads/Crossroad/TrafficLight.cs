using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TrafficLightInfo
{
    float redTime;
    float yellowTime;
    float greenTime;
    float[] startColor = new float[4];

    public float RedTime => redTime;
    public float YellowTime => yellowTime;
    public float GreenTime => greenTime;
    public Color StartColor => new Color(startColor[0], startColor[1], startColor[2], startColor[3]);

    public TrafficLightInfo(TrafficLight trafficLight)
    {
        redTime = trafficLight.RedTime;
        yellowTime = trafficLight.YellowTime;
        greenTime = trafficLight.GreenTime;
        startColor[0] = trafficLight.StartColor.r;
        startColor[1] = trafficLight.StartColor.g;
        startColor[2] = trafficLight.StartColor.b;
        startColor[3] = trafficLight.StartColor.a;
    }
}

public class TrafficLight : Clickable, IPauseable
{
    LineRenderer lineRenderer;

    public float RedTime = 1;
    public float YellowTime = 1;
    public float GreenTime = 1;
    public Color LightColor
    {
        get
        {
            return lineRenderer.startColor;
        }
        set
        {
            lineRenderer.startColor = value;
            lineRenderer.endColor = value;
        }
    }
    public Color StartColor { get; private set; }

    void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        StartColor = Color.red;
        LightColor = StartColor;
        GameStateManager.OnGameStateChanged.AddListener(OnGameStateChanged);
    }

    public void OnGameStateChanged(GameState gameState)
    {
        if (gameState == GameState.Play && gameObject.activeSelf)
        {
            StartCoroutine(ChangeColor());
        }
    }

    public void OnRestart()
    {
        LightColor = StartColor;
        StopAllCoroutines();
    }

    IEnumerator ChangeColor()
    {
        if(LightColor == Color.red)
        {
            yield return new WaitForSeconds(RedTime);
            LightColor = Color.yellow;
        }
        else if(LightColor == Color.yellow)
        {
            yield return new WaitForSeconds(YellowTime);
            LightColor = Color.green;
        }else if(LightColor == Color.green)
        {
            yield return new WaitForSeconds(GreenTime);
            LightColor = Color.red;
        }
        else
        {
            Debug.LogWarning("Not supposed color, change to red");
            LightColor = Color.red;
        }
        StartCoroutine(ChangeColor());
    }

    public void SetStartColor(Color color)
    {
        LightColor = color;
        StartColor = color;
    }

    public void LoadInfo(TrafficLightInfo trafficLightInfo)
    {
        RedTime = trafficLightInfo.RedTime;
        YellowTime = trafficLightInfo.YellowTime;
        GreenTime = trafficLightInfo.GreenTime;
        SetStartColor(trafficLightInfo.StartColor);
    }
}
