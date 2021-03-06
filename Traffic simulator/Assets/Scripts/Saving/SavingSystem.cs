using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Linq;
using System;
using UnityEditor;
using AnotherFileBrowser.Windows;

public class SavingSystem : MonoBehaviour
{
    //??????????
    public static void Save()
    {
        string path = "";

        var bp = new BrowserProperties();
        bp.filter = "Save files (*.save)|*.save";

        new FileBrowser().SaveFileBrowser(bp, result => path = result);
        
        if (path.Length == 0)
            return;
        FileStream stream = new FileStream(path, FileMode.Create);

        BinaryFormatter formatter = new BinaryFormatter();

        SaveData saveData = new SaveData();

        //??? ??????????? ???????
        var saveables = FindObjectsOfType<MonoBehaviour>().OfType<ISaveable>();

        //????????? ??? ??????? ? ?????? ? ???
        foreach (ISaveable saveable in saveables)
        {
            saveData.objects.Add(new ObjectInfo {prefabType = saveable.Prefab, type = saveable.GetType(), info = saveable.SaveInfo() });
        }
            
        formatter.Serialize(stream, saveData);
        
        stream.Close();
    }

    public static void Load()
    {
        string path = "";

        var bp = new BrowserProperties();
        bp.filter = "Save files (*.save)|*.save";

        new FileBrowser().OpenFileBrowser(bp, result => path = result);

        if (File.Exists(path))
        {
            Clear.ClearScene();

            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(path, FileMode.Open);

            //??????????? ??????
            SaveData saveData = formatter.Deserialize(stream) as SaveData;
            stream.Close();

            //??? ??????? ????????????? ???????(?? ?????) ??????? ??????(????????? ??????) ? ????????? ??????
            foreach(ObjectInfo objectInfo in saveData.objects)
            {
                bool needToDestroy = false;
                GameObject savedObject;
                GameObject prefab = Prefabs.GetPrefabByType(objectInfo.prefabType);
                if(prefab != null)
                {
                    savedObject = Instantiate(prefab);
                }
                else
                {
                    savedObject = new GameObject();
                    savedObject.AddComponent(objectInfo.type);
                    needToDestroy = true;
                }
                //object obj = Activator.CreateInstance(objectInfo.type);
                savedObject.GetComponent<ISaveable>().LoadInfo(objectInfo.info);
                if (needToDestroy)
                    Destroy(savedObject);
            }
        }
    }
}

//??? ?????? ??? ??????????
[System.Serializable]
public class SaveData
{
    public List<ObjectInfo> objects = new List<ObjectInfo>();
}
//?????? ?? ????? ???????
[System.Serializable]
public class ObjectInfo
{
    public PrefabType prefabType;
    public Type type;
    public byte[] info;
}