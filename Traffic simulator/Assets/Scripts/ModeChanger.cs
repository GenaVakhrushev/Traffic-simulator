using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ModeChanger : MonoBehaviour
{
    public RoadEditor RoadEditor;
    public TerrainEditor TerrainEditor;

    public GameObject RoadEditorPanel;
    public GameObject TerrainModePanel;
    public GameObject PlayPausePanel;

    public InputField BrushSizeField;
    public InputField BrushSpeedField;

    private void Start()
    {
        if (BrushSizeField)
            BrushSizeField.onValueChanged.AddListener(delegate { OnBrushSizeChanged(); });
        if (BrushSpeedField)
            BrushSpeedField.onValueChanged.AddListener(delegate { OnBrushSpeedChanged(); });
    }

    private void OnBrushSpeedChanged()
    {
        TerrainEditor.ChangingSpeed = float.Parse(BrushSpeedField.text);
    }

    private void OnBrushSizeChanged()
    {
        TerrainEditor.Size = int.Parse(BrushSizeField.text);
    }

    public void SetViewMode()
    {
        RoadEditor.enabled = false;
        RoadEditorPanel.SetActive(false);

        TerrainEditor.enabled = false;
        TerrainEditor.terrainMaterial.SetFloat("_IsActive", 0);
        TerrainModePanel.SetActive(false);

        PlayPausePanel.SetActive(true);
    }

    public void SetRoadEditorMode()
    {
        RoadEditor.enabled = true;
        RoadEditorPanel.SetActive(true);

        TerrainEditor.enabled = false;
        TerrainEditor.terrainMaterial.SetFloat("_IsActive", 0);
        TerrainModePanel.SetActive(false);

        PlayPausePanel.SetActive(false);
        GameStateManager.Stop();
    }

    public void SetTerrainEditorMode()
    {
        RoadEditor.enabled = false;
        RoadEditorPanel.SetActive(false);

        TerrainEditor.enabled = true;
        TerrainEditor.terrainMaterial.SetFloat("_IsActive", 1);

        TerrainModePanel.SetActive(true);
        BrushSizeField.text = TerrainEditor.Size.ToString();
        BrushSpeedField.text = TerrainEditor.ChangingSpeed.ToString();

        PlayPausePanel.SetActive(false);
        GameStateManager.Stop();
    }

    private void OnApplicationQuit()
    {
        TerrainEditor.terrainMaterial.SetFloat("_IsActive", 0);
    }
}
