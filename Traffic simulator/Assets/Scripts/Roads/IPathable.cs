using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPathable
{
    public Path GetPath(Car car);
    public IPathable GetNextPathable(Car car);
}
