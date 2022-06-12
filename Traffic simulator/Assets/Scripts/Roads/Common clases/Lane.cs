using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lane
{
    List<Car> cars = new List<Car>();
    List<LanePoint> lanePoints;
    float maxSpeed;

    public LanePoint this[int i]
    {
        get
        {
            return lanePoints[i];
        }
    }
    public int NumPoints => lanePoints.Count;

    public List<Car> Cars => cars;
    public int NumCars => cars.Count;
    public Car LastCar => NumCars > 0 ? cars[NumCars - 1] : null;

    public float MaxSpeed
    {
        get
        {
            return maxSpeed;
        }
        set
        {
            if(maxSpeed != value)
            {
                maxSpeed = value;
                foreach (LanePoint lanePoint in lanePoints)
                {
                    lanePoint.maxSpeed = maxSpeed;
                }
            }
        }
    }

    delegate float Speed(int[] args);

    public Lane(Path path, bool fromStartToEnd, float offset, float maxSpeed)
    {
        lanePoints = new List<LanePoint>();
        this.maxSpeed = maxSpeed;
        for (int i = 0; i < path.CachedEvenlySpacedPoints.Length; i++)
        {
            Vector3 point = path.CachedEvenlySpacedPoints[i];
            Vector3 right = Vector3.zero;
            if (offset != 0)
            {
                Vector3 forward = Vector3.zero;
                if (fromStartToEnd)
                {
                    if (i < path.CachedEvenlySpacedPoints.Length - 1)
                    {
                        forward += path.CachedEvenlySpacedPoints[i + 1] - point;
                    }
                    if (i > 0)
                    {
                        forward += point - path.CachedEvenlySpacedPoints[i - 1];
                    }
                }
                else
                {
                    if (i > 0)
                    {
                        forward += path.CachedEvenlySpacedPoints[i - 1] - point;
                    }
                    if (i < path.CachedEvenlySpacedPoints.Length - 1)
                    {
                        forward += point - path.CachedEvenlySpacedPoints[i + 1];
                    }
                }

                forward.Normalize();
                forward *= offset;

                right = new Vector3(forward.z, forward.y, -forward.x);
            }
            
            lanePoints.Add(new LanePoint { position = point + right, maxSpeed = maxSpeed });
        }
        if (!fromStartToEnd)
            lanePoints.Reverse();
    }

    void ChangeSpeed(Speed speed, int index, float distance, bool fromStart)
    {
        int distToIndexDifference = (int)(distance / RoadDisplaing.spacing);
        int startIndex = index - distToIndexDifference * (fromStart ? -1 : 1);
        
        for (int i = startIndex; i != index + (fromStart ? -1 : 1); i += fromStart ? -1 : 1)
        {
            if (i >= 0 && i < NumPoints)
            {
                lanePoints[i].maxSpeed = speed.Invoke(new int[] { i, startIndex});
            }
        }
    }

    public void SetMaxSpeed(int index, float distance, bool fromStart)
    {
        ChangeSpeed((args) => maxSpeed, index, distance, fromStart);
    }

    public void SetSpeed(float speed, int index, float distance, bool fromStart)
    {
        ChangeSpeed((args) => speed, index, distance, fromStart);
    }

    public void SetGradientSpeed(float speed, int index, float distance, bool fromStart)
    {
        float diffSpeedAndMaxSpeed = maxSpeed - speed;
        ChangeSpeed((args) => maxSpeed - diffSpeedAndMaxSpeed * (args[1] - args[0]) / (args[1] - index)
        , index, distance, fromStart);

    }

    public void ResetSpeed(int index, float distance, bool fromStart)
    {
        ChangeSpeed((args) => maxSpeed, index, distance, fromStart);
    }

    public void AddCar(Car car)
    {
        cars.Add(car);
    }

    public void RemoveCar(Car car)
    {
        cars.Remove(car);
    }
}

public class LanePoint
{
    public Vector3 position;
    public float maxSpeed;
}
