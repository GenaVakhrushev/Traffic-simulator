using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Road : MonoBehaviour, ISaveable
{
    public Path path;

    void Awake()
    {
        path = new Path(transform.position);
    }

    #region For saving
    public void LoadInfo(byte[] info)
    {
        
    }

    public byte[] SaveInfo()
    {
        return null;
    }
    #endregion
}

//сохраняемая о дороге информация
[System.Serializable]
public class RoadInfo
{
    float[] pos = new float[3];
    float[] rot = new float[3];

    public Vector3 Position { get { return new Vector3(pos[0], pos[1], pos[2]); } }
    public Vector3 Rotation { get { return new Vector3(rot[0], rot[1], rot[2]); } }
    public float Length;

    public RoadInfo(Vector3 pos, Vector3 rot, float len)
    {
        this.pos[0] = pos.x;
        this.pos[1] = pos.y;
        this.pos[2] = pos.z;

        this.rot[0] = rot.x;
        this.rot[1] = rot.y;
        this.rot[2] = rot.z;

        Length = len;
    }
}
