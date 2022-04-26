using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ISaveable
{
    public byte[] SaveInfo();
    public void LoadInfo(byte[] info);
}
