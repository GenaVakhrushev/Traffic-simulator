using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ILaneable
{
    public Lane GetLane(Car car);
    public ILaneable GetNextLaneable(Car car);
}
