using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class CarSpawnerPanel : Panel
{
    public Toggle IsActiveToggle;
    public Dropdown CarTypeDropdown;
    public Dropdown IntervalTypeDropdown;
    public InputField IntervalTimeInputField;
    public InputField IntervalStartInputField;
    public InputField IntervalEndInputField;

    public GameObject FixedInterval;
    public GameObject RandomInterval;

    CarSpawner carSpawner;
    Renderer spawnerRenderer;

    public override void FillSettings(Clickable clickable)
    {
        base.FillSettings(clickable);
        carSpawner = (CarSpawner)clickable;
        spawnerRenderer = carSpawner.GetComponentInChildren<Renderer>();

        IsActiveToggle.isOn = carSpawner.IsActive;
        CarTypeDropdown.value = (int)carSpawner.CarType;
        IntervalTypeDropdown.value = (int)carSpawner.IntervalType;
        IntervalTimeInputField.text = carSpawner.SpawnDeltaTime.ToString();
        IntervalStartInputField.text = carSpawner.IntervalStart.ToString();
        IntervalEndInputField.text = carSpawner.IntervalEnd.ToString();
    }

    public void SetIsActive()
    {
        if (carSpawner.IsActive == IsActiveToggle.isOn)
            return;

        carSpawner.IsActive = IsActiveToggle.isOn;
        Color newColor = spawnerRenderer.material.color;
        if (carSpawner.IsActive)
            spawnerRenderer.material.color = new Color(newColor.r, newColor.g, newColor.b, 1f);
        else
            spawnerRenderer.material.color = new Color(newColor.r, newColor.g, newColor.b, 0.5f);
    }

    public void SetCarType()
    {
        if (carSpawner.CarType == (CarType)CarTypeDropdown.value)
            return;

        carSpawner.CarType = (CarType)CarTypeDropdown.value;
    }

    public void SetIntervalType()
    {
        if (carSpawner.IntervalType == (IntervalType)IntervalTypeDropdown.value)
            return;

        carSpawner.IntervalType = (IntervalType)IntervalTypeDropdown.value;

        if(carSpawner.IntervalType == IntervalType.Fixed)
        {
            FixedInterval.SetActive(true);
            RandomInterval.SetActive(false);
        }
        else
        {
            FixedInterval.SetActive(false);
            RandomInterval.SetActive(true);
        }
    }

    public void SetFixedIntervalTime()
    {
        carSpawner.SpawnDeltaTime = float.Parse(IntervalTimeInputField.text);
    }

    public void SetRandomIntervalStart()
    {
        carSpawner.IntervalStart = float.Parse(IntervalStartInputField.text);
    }

    public void SetRandomIntervalEnd()
    {
        carSpawner.IntervalEnd = float.Parse(IntervalEndInputField.text);
    } 
}
