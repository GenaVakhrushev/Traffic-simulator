using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ILaneable
{
    public Lane GetLane(Car car);
    public ILaneable GetNextLaneable(Car car);
    public void AddCar(Car car);
    public void RemoveCar(Car car);
}
