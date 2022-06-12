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

    public Dropdown StartSpeedIntervalTypeDropdown;
    public InputField FixedSpeedInputField;
    public InputField FromSpeedInputField;
    public InputField ToSpeedInputField;
    public GameObject FixedSpeed;
    public GameObject RandomSpeed;

    CarSpawner carSpawner;

    public override void FillSettings(Clickable clickable)
    {
        base.FillSettings(clickable);
        carSpawner = (CarSpawner)clickable;

        IsActiveToggle.isOn = carSpawner.IsActive;
        CarTypeDropdown.value = (int)carSpawner.CarType;
        IntervalTypeDropdown.value = (int)carSpawner.IntervalType;
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
        IntervalTimeInputField.text = carSpawner.SpawnDeltaTime.ToString();
        IntervalStartInputField.text = carSpawner.IntervalStart.ToString();
        IntervalEndInputField.text = carSpawner.IntervalEnd.ToString();

        if (carSpawner.StartSpeedIntervalType == IntervalType.Fixed)
        {
            FixedSpeed.SetActive(true);
            RandomSpeed.SetActive(false);
        }
        else
        {
            FixedSpeed.SetActive(false);
            RandomSpeed.SetActive(true);
        }
        StartSpeedIntervalTypeDropdown.value = (int)carSpawner.StartSpeedIntervalType;
        FixedSpeedInputField.text = carSpawner.FixedStartSpeed.ToString();
        FromSpeedInputField.text = carSpawner.FromStartSpeed.ToString();
        ToSpeedInputField.text = carSpawner.ToStartSpeed.ToString();
    }

    public void SetIsActive()
    {
        if (carSpawner.IsActive == IsActiveToggle.isOn)
            return;

        carSpawner.IsActive = IsActiveToggle.isOn;
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

    public void SetStartSpeedIntervalType()
    {
        if (carSpawner.StartSpeedIntervalType == (IntervalType)StartSpeedIntervalTypeDropdown.value)
            return;

        carSpawner.StartSpeedIntervalType = (IntervalType)StartSpeedIntervalTypeDropdown.value;

        if (carSpawner.StartSpeedIntervalType == IntervalType.Fixed)
        {
            FixedSpeed.SetActive(true);
            RandomSpeed.SetActive(false);
        }
        else
        {
            FixedSpeed.SetActive(false);
            RandomSpeed.SetActive(true);
        }
    }

    public void SetFixedSpeed()
    {
        carSpawner.FixedStartSpeed = float.Parse(FixedSpeedInputField.text);
    }

    public void SetRandomFromSpeed()
    {
        carSpawner.FromStartSpeed = float.Parse(FromSpeedInputField.text);
    }

    public void SetRandomToSpeed()
    {
        carSpawner.ToStartSpeed = float.Parse(ToSpeedInputField.text);
    }
}
