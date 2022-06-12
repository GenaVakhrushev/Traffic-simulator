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

    public GameObject RoadNumbers;
    public Text RoadNumberText;

    Crossroad crossroad;
    List<Text> roadNumbers;

    public override void FillSettings(Clickable clickable)
    {
        base.FillSettings(clickable);
        crossroad = (Crossroad)clickable;

        haveMainRoadToggle.isOn = crossroad.HaveMainRoad;
        mainRoadIndexes.SetActive(haveMainRoadToggle.isOn);
        mainRoadFirstIndexInpitField.SetTextWithoutNotify(crossroad.MainRoadPointIndexes[0].ToString());
        mainRoadSecondIndexInpitField.SetTextWithoutNotify(crossroad.MainRoadPointIndexes[1].ToString());

        UpdateRoadNumbers();
    }


    private void Update()
    {
        UpdateRoadTextsPositions();
    }

    void UpdateRoadTextsPositions()
    {
        for (int i = 0; i < crossroad.SnapPoints.Length; i++)
        {
            roadNumbers[i].transform.position = Camera.main.WorldToScreenPoint(crossroad.SnapPoints[i].transform.position);
        }
    }

    void UpdateRoadNumbers()
    {
        roadNumbers = new List<Text>(RoadNumbers.GetComponentsInChildren<Text>());

        for (int i = 0; i < crossroad.SnapPoints.Length; i++)
        {
            if(i >= roadNumbers.Count)
            {
                roadNumbers.Add(Instantiate(RoadNumberText, RoadNumbers.transform));
            }
            roadNumbers[i].text = i.ToString();
        }
        UpdateRoadTextsPositions();
    }

    public void SetHaveMainRoad()
    {
        crossroad.SetHaveMainRoad(haveMainRoadToggle.isOn);
        mainRoadIndexes.SetActive(haveMainRoadToggle.isOn);
        UpdateRoadNumbers();
    }

    public void SetMainRoadIndexes()
    {
        int firstIndex;
        int secondIndex;

        int.TryParse(mainRoadFirstIndexInpitField.text,out firstIndex);
        int.TryParse(mainRoadSecondIndexInpitField.text, out secondIndex);

        if (firstIndex < 0 || firstIndex >= crossroad.SnapPoints.Length || secondIndex < 0 || secondIndex >= crossroad.SnapPoints.Length || firstIndex == secondIndex)
        {
            // mainRoadFirstIndexInpitField.SetTextWithoutNotify(crossroad.MainRoadPointIndexes[0].ToString());
            // mainRoadSecondIndexInpitField.SetTextWithoutNotify(crossroad.MainRoadPointIndexes[1].ToString());
            mainRoadFirstIndexInpitField.text = crossroad.MainRoadPointIndexes[0].ToString();
            mainRoadSecondIndexInpitField.text = crossroad.MainRoadPointIndexes[1].ToString();
            return;
        }

        crossroad.SetMainRoad(firstIndex, secondIndex);
    }
}
