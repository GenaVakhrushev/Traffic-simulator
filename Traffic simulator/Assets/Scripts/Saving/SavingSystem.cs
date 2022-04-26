using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Linq;
using System;

public class SavingSystem : MonoBehaviour
{
    //����������
    public static void Save()
    {
        BinaryFormatter formatter = new BinaryFormatter();
        string path = Application.persistentDataPath + "/testlevel.save";
        FileStream stream = new FileStream(path, FileMode.Create);
        SaveData saveData = new SaveData();

        //��� ����������� �������
        var saveables = FindObjectsOfType<MonoBehaviour>().OfType<ISaveable>();

        //��������� ��� ������� � ������ � ��
        foreach (ISaveable saveable in saveables)
        {
            saveData.objects.Add(new ObjectInfo { type = saveable.GetType(), info = saveable.SaveInfo() });
        }
            
        formatter.Serialize(stream, saveData);
        
        stream.Close();
    }

    public static void Load()
    {
        string path = Application.persistentDataPath + "/testlevel.save";
        if (File.Exists(path))
        {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(path, FileMode.Open);

            //���������� ������
            SaveData saveData = formatter.Deserialize(stream) as SaveData;
            stream.Close();

            //��� ������� ������������ �������(�� �����) ������ ������(��������� ������) � ��������� ������
            foreach(ObjectInfo objectInfo in saveData.objects)
            {
                object obj = Activator.CreateInstance(objectInfo.type);
                ((ISaveable)obj).LoadInfo(objectInfo.info);
            }
        }
        else
        {
            Debug.LogError("Save file not found in " + path);
        }
    }
}

//��� ������ ��� ����������
[System.Serializable]
public class SaveData
{
    public List<ObjectInfo> objects = new List<ObjectInfo>();
}
//������ �� ����� �������
[System.Serializable]
public class ObjectInfo
{
    public Type type;
    public byte[] info;
}