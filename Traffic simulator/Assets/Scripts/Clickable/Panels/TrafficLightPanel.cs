using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TrafficLightPanel : Panel
{
    public InputField RedTimeInputField;
    public InputField YellowTimeInputField;
    public InputField GreenTimeInputField;

    TrafficLight trafficLight;

    public override void FillSettings(Clickable clickable)
    {
        base.FillSettings(clickable);
        trafficLight = (TrafficLight)clickable;
        
        RedTimeInputField.text = trafficLight.RedTime.ToString();
        YellowTimeInputField.text = trafficLight.YellowTime.ToString();
        GreenTimeInputField.text = trafficLight.GreenTime.ToString();
    }

    public void SetRedTime()
    {
        trafficLight.RedTime = int.Parse(RedTimeInputField.text);
    }

    public void SetYellowTime()
    {
        trafficLight.YellowTime = int.Parse(YellowTimeInputField.text);
    }

    public void SetGreenTime()
    {
        trafficLight.GreenTime = int.Parse(GreenTimeInputField.text);
    }

    public void SetRedStartColor()
    {
        trafficLight.SetStartColor(Color.red);
    }
    public void SetYellowStartColor()
    {
        trafficLight.SetStartColor(Color.yellow);
    }
    public void SetGreenStartColor()
    {
        trafficLight.SetStartColor(Color.green);
    }
}
