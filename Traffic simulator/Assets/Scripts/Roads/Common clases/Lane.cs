using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lane
{
    List<LanePoint> lanePoints;

    public LanePoint this[int i]
    {
        get
        {
            return lanePoints[i];
        }
    }
    public int NumPoints => lanePoints.Count;

    public Lane(Path path, bool fromStartToEnd, float offset, float maxSpeed)
    {
        lanePoints = new List<LanePoint>();
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
        
    }
}

public class LanePoint
{
    public Vector3 position;
    public float maxSpeed;
}
