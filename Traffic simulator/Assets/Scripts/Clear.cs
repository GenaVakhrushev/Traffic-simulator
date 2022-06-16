using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Clear : MonoBehaviour
{
    public static void ClearScene()
    {
        var deleteables = FindObjectsOfType<MonoBehaviour>().OfType<IDeleteable>();
        foreach (IDeleteable deleteable in deleteables)
        {
            deleteable.Delete();
        }
    }
}
