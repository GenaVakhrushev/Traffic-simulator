using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RoadPanel : Panel
{
    public InputField LengthInputField;
    public InputField AvgSpeedInputField;
    public InputField DensityInputField;

    Road road;
    Path path => road.path;

    float upadateInterval = 1f;
    public override void FillSettings(Clickable clickable)
    {
        base.FillSettings(clickable);
        road = (Road)clickable;
        
        StartCoroutine(UpdateSettings());
    }

    public override void HidePanel()
    {
        base.HidePanel();
        StopAllCoroutines();
    }

    IEnumerator UpdateSettings()
    {
        float length = (path.CachedEvenlySpacedPoints.Length - 1) * RoadDisplaing.spacing;

        LengthInputField.text = length.ToString();
        AvgSpeedInputField.text = AvgSpeed().ToString();
        DensityInputField.text = (road.cars.Count / length).ToString();

        yield return new WaitForSeconds(upadateInterval);
        StartCoroutine(UpdateSettings());
    }

    float AvgSpeed()
    {
        if (road.cars.Count == 0)
            return 0;

        float speedSum = 0;
        foreach(Car car in road.cars)
        {
            speedSum += car.Speed;
        }
        return speedSum / road.cars.Count;
    }
}
