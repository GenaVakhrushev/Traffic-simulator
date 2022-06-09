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
    Path path => road.Path;

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
        DensityInputField.text = (CarsCount() / length).ToString();

        yield return new WaitForSeconds(upadateInterval);
        StartCoroutine(UpdateSettings());
    }

    int CarsCount()
    {
        int count = 0;
        foreach (Lane lane in road.StartLanes)
        {
            count += lane.NumCars;
        }
        foreach (Lane lane in road.EndLanes)
        {
            count += lane.NumCars;
        }
        return count;
    }

    float AvgSpeed()
    {
        int carsCount = CarsCount();
        if (carsCount == 0)
            return 0;

        float speedSum = 0;
        foreach (Lane lane in road.StartLanes)
        {
            foreach (Car car in lane.Cars)
            {
                speedSum += car.Speed;
            }
        }
        foreach (Lane lane in road.EndLanes)
        {
            foreach (Car car in lane.Cars)
            {
                speedSum += car.Speed;
            }
        }

        return speedSum / carsCount;
    }
}
