using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class Clickable : MonoBehaviour
{
    [HideInInspector] public Panel panel;

    public virtual void Start()
    {
        panel = PanelsManager.Panels[GetType()].GetComponent<Panel>(); ;
    }

    public void OnClick()
    {
        panel.FillSettings(this);
    }
}
