using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Linq;
using System;
using UnityEditor;

public class SavingSystem : MonoBehaviour
{
    //сохранение
    public static void Save()
    {
        //string path = Application.persistentDataPath + "/testlevel.save";
        string path = EditorUtility.SaveFilePanel("Выберите путь сохранения", "", "My save", "save");
        if (path.Length == 0)
            return;
        FileStream stream = new FileStream(path, FileMode.Create);

        BinaryFormatter formatter = new BinaryFormatter();

        SaveData saveData = new SaveData();

        //все сохраняемые объекты
        var saveables = FindObjectsOfType<MonoBehaviour>().OfType<ISaveable>();

        //сохраняем тип объекта и данные о нём
        foreach (ISaveable saveable in saveables)
        {
            saveData.objects.Add(new ObjectInfo {prefabType = saveable.Prefab, type = saveable.GetType(), info = saveable.SaveInfo() });
        }
            
        formatter.Serialize(stream, saveData);
        
        stream.Close();
    }

    public static void Load()
    {
        //string path = Application.persistentDataPath + "/testlevel.save";
        string path = EditorUtility.OpenFilePanel("Выберите файл сохранения", "", "save");
        if (File.Exists(path))
        {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(path, FileMode.Open);

            //сохранённые данные
            SaveData saveData = formatter.Deserialize(stream) as SaveData;
            stream.Close();

            //для каждого сорхранённого объекта(на сцене) создаём объект(экземпляр класса) и загружаем данные
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
        else
        {
            Debug.LogError("Save file not found in " + path);
        }
    }
}

//все данные для сохранения
[System.Serializable]
public class SaveData
{
    public List<ObjectInfo> objects = new List<ObjectInfo>();
}
//данные об одном объекте
[System.Serializable]
public class ObjectInfo
{
    public PrefabType prefabType;
    public Type type;
    public byte[] info;
}