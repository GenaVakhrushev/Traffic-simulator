using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CrossroadPanel : Panel
{
    public Toggle haveMainRoadToggle;
    public GameObject mainRoadIndexes;
    public InputField mainRoadFirstIndexInpitField;
    public InputField mainRoadSecondIndexInpitField;

    Crossroad crossroad;
    public override void FillSettings(Clickable clickable)
    {
        base.FillSettings(clickable);
        crossroad = (Crossroad)clickable;

        haveMainRoadToggle.isOn = crossroad.HaveMainRoad;
        mainRoadFirstIndexInpitField.SetTextWithoutNotify(crossroad.MainRoadPointIndexes[0].ToString());
        mainRoadSecondIndexInpitField.SetTextWithoutNotify(crossroad.MainRoadPointIndexes[1].ToString());
    }

    public void SetHaveMainRoad()
    {
        crossroad.SetHaveMainRoad(haveMainRoadToggle.isOn);
        mainRoadIndexes.SetActive(haveMainRoadToggle.isOn);
    }

    public void SetMainRoadIndexes()
    {
        int firstIndex;
        int secondIndex;

        int.TryParse(mainRoadFirstIndexInpitField.text,out firstIndex);
        int.TryParse(mainRoadSecondIndexInpitField.text, out secondIndex);

        if (firstIndex < 0 || firstIndex >= crossroad.SnapPoints.Length || secondIndex < 0 || secondIndex >= crossroad.SnapPoints.Length || firstIndex == secondIndex)
        {
            mainRoadFirstIndexInpitField.SetTextWithoutNotify(crossroad.MainRoadPointIndexes[0].ToString());
            mainRoadSecondIndexInpitField.SetTextWithoutNotify(crossroad.MainRoadPointIndexes[1].ToString());
            return;
        }

        crossroad.SetMainRoad(firstIndex, secondIndex);
    }
}
