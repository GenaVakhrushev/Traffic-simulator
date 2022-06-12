using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RoadPanel : Panel
{
    public Dropdown LaneSelectDropdown;
    public InputField MaxSpeedInputField;
    public InputField LengthInputField;
    public InputField AvgSpeedInputField;
    public InputField DensityInputField;

    public GameObject LaneTexts;
    public Text LaneText;

    List<Text> laneTexts;

    Road road;
    Path path => road.Path;
    Lane currentLane;

    float upadateInterval = 1f;
    public override void FillSettings(Clickable clickable)
    {
        base.FillSettings(clickable);
        road = (Road)clickable;

        UpdateLaneTexts();
        SetCurrentLane();
        StartCoroutine(UpdateStats());
    }

    private void Update()
    {
        UpdateLaneTextsPositions();
    }

    public override void HidePanel()
    {
        base.HidePanel();
        StopAllCoroutines();
    }

    void UpdateLaneTextsPositions()
    {
        int i = 0;
        int j = 0;

        for (; i < road.StartLanes.Count; i++)
        {
            Vector3 laneMiddlePoint = Camera.main.WorldToScreenPoint(road.StartLanes[i][road.StartLanes[i].NumPoints / 2].position);
            Vector3 laneNextToMiddlePoint = Camera.main.WorldToScreenPoint(road.StartLanes[i][road.StartLanes[i].NumPoints / 2 + 1].position);
            float angle = Vector2.Angle(Vector2.right, laneNextToMiddlePoint - laneMiddlePoint);

            laneTexts[i].transform.position = laneMiddlePoint;
            laneTexts[i].transform.rotation = Quaternion.Euler(0, 0, angle);
        }
        
        for (; j < road.EndLanes.Count; j++)
        {
            Vector3 laneMiddlePoint = Camera.main.WorldToScreenPoint(road.EndLanes[j][road.EndLanes[j].NumPoints / 2].position);
            Vector3 laneNextToMiddlePoint = Camera.main.WorldToScreenPoint(road.EndLanes[j][road.EndLanes[j].NumPoints / 2 - 1].position);
            float angle = Vector2.Angle(Vector2.right, laneNextToMiddlePoint - laneMiddlePoint);

            laneTexts[j + i].transform.position = laneMiddlePoint;
            laneTexts[j + i].transform.rotation = Quaternion.Euler(0, 0, angle);
        }
    }

    void UpdateLaneTexts()
    {
        LaneSelectDropdown.ClearOptions();
        laneTexts = new List<Text>(LaneTexts.GetComponentsInChildren<Text>());

        int i = 0;
        int j = 0;

        for (; i < road.StartLanes.Count; i++)
        {
            if (i >= laneTexts.Count)
            {
                laneTexts.Add(Instantiate(LaneText, LaneTexts.transform));
            }
            laneTexts[i].text = "Полоса " + i.ToString();
        }
        
        for (; j < road.EndLanes.Count; j++)
        {
            if (j + i >= laneTexts.Count )
            {
                laneTexts.Add(Instantiate(LaneText, LaneTexts.transform));
            }
            laneTexts[j + i].text = "Полоса " + i.ToString();
        }
        UpdateLaneTextsPositions();

        foreach (Text text in laneTexts)
        {
            Dropdown.OptionData optionData = new Dropdown.OptionData(text.text);
            LaneSelectDropdown.options.Add(optionData);
        }
        LaneSelectDropdown.value = 0;
        LaneSelectDropdown.RefreshShownValue();
    }

    IEnumerator UpdateStats()
    {
        float length = (path.CachedEvenlySpacedPoints.Length - 1) * RoadDisplaing.spacing;

        LengthInputField.text = length.ToString();
        AvgSpeedInputField.text = AvgSpeed().ToString();
        DensityInputField.text = (CarsCount() / length).ToString();

        yield return new WaitForSeconds(upadateInterval);
        StartCoroutine(UpdateStats());
    }

    Lane GetRoadLane(int index)
    {
        if (index < road.StartLanes.Count)
            return road.StartLanes[index];

        return road.EndLanes[index - road.StartLanes.Count];
    }

    public void SetCurrentLane()
    {
        currentLane = GetRoadLane(LaneSelectDropdown.value);
        MaxSpeedInputField.text = currentLane.MaxSpeed.ToString();
    }

    public void SetMaxSpeed()
    {
        currentLane.MaxSpeed = int.Parse(MaxSpeedInputField.text);
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
