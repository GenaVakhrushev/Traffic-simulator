using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Path
{
    public Vector3[] CachedEvenlySpacedPoints;

    List<Vector3> points;
    bool isClosed;
    bool autoSetControlPoints;

    bool startConnected = false;
    bool endConnected = false;

    Vector3 startControlPointDir;
    Vector3 endControlPointDir;

    public int NumPoints => points.Count;
    public int NumSegments => points.Count / 3;

    public bool IsStartOrEndConnected => startConnected || endConnected;

    public bool IsClosed
    {
        get
        {
            return isClosed;
        }
        set
        {
            if (isClosed != value)
            {
                isClosed = value;

                if (isClosed)
                {
                    //добавить контрольную точку на том же расстоянии, что и контрольная точка для последней якорной точки, но с другой стороны 
                    //p3 + (p3 - p2) = 2 * p3 - p2
                    points.Add(2 * points[points.Count - 1] - points[points.Count - 2]);
                    //то же самое, но для первной точки
                    points.Add(2 * points[0] - points[1]);
                    if (autoSetControlPoints)
                    {
                        AutoSetAnchorControlPoints(0);
                        AutoSetAnchorControlPoints(points.Count - 3);
                    }
                }
                else
                {
                    points.RemoveRange(points.Count - 2, 2);
                    AutoSetStartAndEndControlls();
                }
            }
        }
    }

    public bool AutoSetControlPoints
    {
        get
        {
            return autoSetControlPoints;
        }

        set
        {
            if (autoSetControlPoints != value)
            {
                autoSetControlPoints = value;
                if (autoSetControlPoints)
                {
                    AutoSetAllControlPoints();
                }
            }
        }
    }

    public Path(Vector3 center)
    {
        float scale = 5;
        points = new List<Vector3>
        {
            center + Vector3.left * scale,
            center + (Vector3.left + Vector3.forward) *0.5f * scale,
            center + (Vector3.right + Vector3.back) * 0.5f * scale,
            center + Vector3.right * scale
        };
    }

    public Path(Vector3[] points, bool isClosed, bool autoSetControlPoints)
    {
        this.points = new List<Vector3>(points); ;
        this.isClosed = isClosed;
        this.autoSetControlPoints = autoSetControlPoints;
    }

    public Vector3 this[int i]
    {
        get
        {
            return points[i];
        }
    }

    public void AddSegment(Vector3 anchorPos)
    {
        if (endConnected)
        {
            SplitSegment(anchorPos, NumSegments - 1);
            return;
        }
        //добавить контрольную точку на том же расстоянии, что и контрольная точка для последней якорной точки, но с другой стороны 
        //p3 + (p3 - p2) = 2 * p3 - p2
        points.Add(2 * points[points.Count - 1] - points[points.Count - 2]);
        //добавить контрольную точку на полпути между новой якорной точкой и только что созданой контрольной точкой
        points.Add((points[points.Count - 1] + anchorPos) / 2);
        points.Add(anchorPos);

        if (autoSetControlPoints)
        {
            AutoSetAllAffectedControlPoint(points.Count - 1);
        }
    }

    public void SplitSegment(Vector3 anchorPos, int segmentIndex)
    {
        points.InsertRange(segmentIndex * 3 + 2, new Vector3[] { Vector3.zero, anchorPos, Vector3.zero });
        if (autoSetControlPoints)
        {
            AutoSetAllAffectedControlPoint(segmentIndex * 3 + 3);
        }
        else
        {
            AutoSetAnchorControlPoints(segmentIndex * 3 + 3);
        }
    }

    public bool DeleteSegment(int anchorIndex)
    {
        //удалять можно, если путь открытый и сегментов больше одного или сегментов больше двух
        if (NumSegments > 2 || !isClosed && NumSegments > 1)
        {
            //удаление первой якорной точки
            if (anchorIndex == 0)
            {
                if (isClosed)
                {
                    points[points.Count - 1] = points[2];
                }
                points.RemoveRange(0, 3);
            }
            //удаление последеней якорной точки
            else if (anchorIndex == points.Count - 1 && !isClosed)
            {
                points.RemoveRange(anchorIndex - 2, 3);
            }
            else
            {
                points.RemoveRange(anchorIndex - 1, 3);
            }

            if(NumSegments == 1)
            {
                autoSetControlPoints = false;
            }
            return true;
        }
        return false;
    }

    public Vector3[] GetPointsInSegment(int i)
    {
        return new Vector3[] { points[i * 3], points[i * 3 + 1], points[i * 3 + 2], points[LoopIndex(i * 3 + 3)] };
    }

    public void MovePoint(int i, Vector3 pos)
    {
        Vector3 deltaMove = pos - points[i];

        if (i % 3 == 0 || !autoSetControlPoints)
        {
            if (i == 1 && startConnected)
            {
                points[i] = points[0] + Vector3.Project(pos - points[0], startControlPointDir);
            }
            else if (i == NumPoints - 2 && endConnected)
            {
                points[i] = points[NumPoints - 1] + Vector3.Project(pos - points[NumPoints - 1], endControlPointDir);
            }
            else
            {
                points[i] = pos;

                if (autoSetControlPoints)
                {
                    AutoSetAllAffectedControlPoint(i);
                    return;
                }

                //при передвижении якорной точки соответсвенно двигать контрольные точки
                if (i % 3 == 0)
                {
                    if (i + 1 < points.Count || isClosed)
                        points[LoopIndex(i + 1)] += deltaMove;
                    if (i - 1 >= 0 || isClosed)
                        points[LoopIndex(i - 1)] += deltaMove;
                }
                else
                {
                    bool nextPointIsAnchor = (i + 1) % 3 == 0;
                    int correspondingControlIndex = nextPointIsAnchor ? i + 2 : i - 2;
                    int anchorIndex = nextPointIsAnchor ? i + 1 : i - 1;

                    //поворачивать контрольную точку напротив двигаемой, чтобы она оставаль напротив, сохраняя её расстояние до якорной точки
                    if (correspondingControlIndex >= 0 && correspondingControlIndex < points.Count || isClosed)
                    {
                        anchorIndex = LoopIndex(anchorIndex);
                        correspondingControlIndex = LoopIndex(correspondingControlIndex);
                        float dist = (points[anchorIndex] - points[correspondingControlIndex]).magnitude;
                        Vector3 dir = (points[anchorIndex] - pos).normalized;
                        points[correspondingControlIndex] = points[anchorIndex] + dir * dist;
                    }
                }
            }
        }
    }

    public bool ConnectStartOrEndPoint(int pointIndex, Vector3 pos, Vector3 controlPointDir)
    {
        //при одном сегменте  нельзя соединять оба конца дороги к одной точке
        if (NumSegments == 1 && (pointIndex == 0 && pos == points[NumPoints - 1] || pointIndex == NumPoints - 1 && pos == points[0]))
            return false;

        controlPointDir.Normalize();

        Ray ray = new Ray(pos, -controlPointDir);
        RaycastHit hit;
        float rayDist = pointIndex == 0 ? Vector3.Distance(pos, points[NumPoints - 1]) : Vector3.Distance(pos, points[0]);
        if (Physics.Raycast(ray, out hit, rayDist, LayerMask.GetMask("Snap point")))
        {
            //при одном сегменте нельзя соединять концы дороги к противополжным точкам одного перекрёстка точке
            if (NumSegments == 1 && (pointIndex == 0 && hit.transform.position == points[NumPoints - 1] || pointIndex == NumPoints - 1 && hit.transform.position == points[0]))
                return false;
        }


        MovePoint(pointIndex, pos);
        if (pointIndex == 0)
        {
            startConnected = true;
            startControlPointDir = controlPointDir;
            MovePoint(1, pos + controlPointDir.normalized * 5f);
        }
        else
        {
            endConnected = true;
            endControlPointDir = controlPointDir;
            MovePoint(pointIndex - 1, pos + controlPointDir.normalized * 5f);
        }

        return true;
    }

    public void DisconnectStartOrEndPoint(int pointIndex)
    {
        if (pointIndex == 0)
        {
            startConnected = false;
            startControlPointDir = Vector3.zero;
        }
        else
        {
            endConnected = false;
            endControlPointDir = Vector3.zero;
        }
    }

    public Vector3[] CalculateEvenlySpacedPoints(float spacing, float resolution = 1)
    {
        List<Vector3> evenlySpacedPoints = new List<Vector3>();
        evenlySpacedPoints.Add(points[0]);
        Vector3 previousPoint = points[0];
        float distSinceLastEvenPoint = 0;

        for (int segmentIndex = 0; segmentIndex < NumSegments; segmentIndex++)
        {
            Vector3[] p = GetPointsInSegment(segmentIndex);
            //длина кривой
            float controlNetLength = Vector3.Distance(p[0], p[1]) + Vector3.Distance(p[1], p[2]) + Vector3.Distance(p[2], p[3]);
            float estimatedCurveLength = Vector3.Distance(p[0], p[3]) + controlNetLength / 2f;
            //количиство разбиений
            int divisions = Mathf.CeilToInt(estimatedCurveLength * resolution * 10);
            float t = 0;
            while (t <= 1)
            {
                t += 1f / divisions;
                Vector3 pointOnCurve = Bezier.EvaluateCubic(p[0], p[1], p[2], p[3], t);
                distSinceLastEvenPoint += Vector3.Distance(previousPoint, pointOnCurve);

                //добавлем все возможные точки между предыдущей равноудалённой точкой и точкой на прямой
                //при маленьком значении spacing между точкой на прямой и прошлой равноудалённой точкой
                //может поместиться несколько равноудалённый точек
                while (distSinceLastEvenPoint >= spacing)
                {
                    //избыточное расстояние
                    float overshootDist = distSinceLastEvenPoint - spacing;
                    //новая точка будет расположена в направлении предыдущей точки на расстоянии раном избыточному
                    Vector3 newEvenlySpacedPoint = pointOnCurve + (previousPoint - pointOnCurve).normalized * overshootDist;
                    evenlySpacedPoints.Add(newEvenlySpacedPoint);
                    //теперь расстояние от предыдущей раноудалённой точки до точки на кривой равно избыточному расстоянию
                    distSinceLastEvenPoint = overshootDist;
                    //предыдущая точка теперь - новая точка
                    previousPoint = newEvenlySpacedPoint;
                }

                previousPoint = pointOnCurve;
            }
        }
        CachedEvenlySpacedPoints = evenlySpacedPoints.ToArray();
        return CachedEvenlySpacedPoints;
    }

    void AutoSetAllAffectedControlPoint(int updateAnchorIndex)
    {
        for (int i = updateAnchorIndex - 3; i <= updateAnchorIndex + 3; i += 3)
        {
            if (i >= 0 && i < points.Count || isClosed)
            {
                AutoSetAnchorControlPoints(LoopIndex(i));
            }
        }

        AutoSetStartAndEndControlls();
    }

    void AutoSetAllControlPoints()
    {
        for (int i = 0; i < points.Count; i += 3)
        {
            AutoSetAnchorControlPoints(i);
        }
        AutoSetStartAndEndControlls();
    }

    void AutoSetAnchorControlPoints(int anchorIndex)
    {
        if (anchorIndex == 0 && startConnected || anchorIndex == NumPoints - 1 && endConnected)
            return;

        Vector3 anchorPos = points[anchorIndex];
        //направление для одной контрольной точки(для другой противоположное)
        Vector3 dir = Vector3.zero;
        //расстояния до соседних якорных точек
        float[] neghbourDistances = new float[2];

        //вектор dir перпендикулярен биссектрисе угла, образованного веторами до соседних якорных точек
        if (anchorIndex - 3 >= 0 || isClosed)
        {
            Vector3 offset = points[LoopIndex(anchorIndex - 3)] - anchorPos;
            dir += offset.normalized;
            neghbourDistances[0] = offset.magnitude;
        }
        if (anchorIndex + 3 >= 0 || isClosed)
        {
            Vector3 offset = points[LoopIndex(anchorIndex + 3)] - anchorPos;
            dir -= offset.normalized;
            neghbourDistances[1] = -offset.magnitude;
        }

        dir.Normalize();

        for (int i = 0; i < 2; i++)
        {
            int controlIndex = anchorIndex + i * 2 - 1;
            if (controlIndex >= 0 && controlIndex < points.Count || isClosed)
            {
                //расстояние до котрольных точек равно половине расстояния до якорных точек
                points[LoopIndex(controlIndex)] = anchorPos + dir * neghbourDistances[i] / 2;
            }
        }
    }

    void AutoSetStartAndEndControlls()
    {
        if (!isClosed)
        {
            if (!startConnected)
                points[1] = (points[0] + points[2]) / 2;
            if (!endConnected)
                points[points.Count - 2] = (points[points.Count - 1] + points[points.Count - 3]) / 2;
        }
    }

    int LoopIndex(int i)
    {
        return (i + points.Count) % points.Count;
    }
}
