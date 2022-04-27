using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshCollider))]
public class RoadDisplaing : MonoBehaviour
{
    public Material ZTestMaterial;

    public Road road;
    Path path => road.path;

    List<GameObject> pointsObjects;
    List<LineRenderer> linesFromPoints;

    int selectedSegmentIndex = -1;
    int dotsOnSegmentCount = 50;

    float roadWidth = 2;
    [Range(0.1f, 1.5f)]
    public float spacing = 0.1f;
    public float tiling = 7;

    MeshFilter meshFilter;
    MeshRenderer meshRenderer;
    MeshCollider meshCollider;

    private void Awake()
    {
        ZTestMaterial.color = RoadEditorSettings.segmentCol;

        pointsObjects = new List<GameObject>();
        linesFromPoints = new List<LineRenderer>();

        meshFilter = GetComponent<MeshFilter>();
        meshRenderer = GetComponent<MeshRenderer>();
        meshCollider = GetComponent<MeshCollider>();

        UpdatePoints();
    }

    public void UpdatePoints()
    {
        for (int i = 0; i < path.NumPoints; i++)
        {
            //если точка не создана, то создать её
            if (i >= pointsObjects.Count)
            {
                CreatePoint(i);
                if (i % 3 != 0 && !RoadEditorSettings.displayControlPoints)
                    pointsObjects[i].SetActive(false);
            }
            else
                pointsObjects[i].transform.position = path[i];
        }

        DrawLines();

        //создание меша
        Vector3[] points = path.CalculateEvenlySpacedPoints(spacing);
        meshFilter.mesh = CreateRoadMesh(points, path.IsClosed);

        int textureRepeat = Mathf.RoundToInt(tiling * points.Length * spacing * 0.05f);
        meshRenderer.sharedMaterial.mainTextureScale = new Vector2(1, textureRepeat);
        meshCollider.sharedMesh = meshFilter.mesh;
    }

    void DrawLines()
    {
        for (int i = 0; i < path.NumSegments; i++)
        {
            Vector3[] points = path.GetPointsInSegment(i);
            Vector3[] bezierPoints = SegmentBezierPoints(points[0], points[1], points[2], points[3]);

            linesFromPoints[3 * i].positionCount = bezierPoints.Length;
            //если путь открытый, то ис последней точки не идёт линий
            if (!path.IsClosed)
                linesFromPoints[3 * i + 3].positionCount = 0;

            //кривая Безье
            linesFromPoints[3 * i].SetPositions(bezierPoints);
            //линни от контрольных точек к якорным
            linesFromPoints[3 * i + 1].SetPositions(new Vector3[] { points[1], points[0] });
            linesFromPoints[3 * i + 2].SetPositions(new Vector3[] { points[2], points[3] });
        }
    }

    Vector3[] SegmentBezierPoints(Vector3 a, Vector3 b, Vector3 c, Vector3 d)
    {
        List<Vector3> positions = new List<Vector3>();
        for (float t = 0; t < 1; t += 1.0f / dotsOnSegmentCount)
        {
            positions.Add(Bezier.EvaluateCubic(a, b, c, d, t));
        }

        return positions.ToArray();
    }

    void CreatePoint(int pointIndex)
    {
        GameObject newPoint = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        float size = (pointIndex % 3 == 0) ? RoadEditorSettings.anchorDiameter : RoadEditorSettings.controlDiameter;

        //настройка позиции и размеров
        newPoint.transform.position = path[pointIndex];
        newPoint.transform.localScale = new Vector3(size, size, size);
        newPoint.transform.SetParent(transform);

        //настройка материала
        Material newPointMaterial = new Material(ZTestMaterial);
        newPointMaterial.renderQueue = 3001;
        newPointMaterial.color = (pointIndex % 3 == 0) ? RoadEditorSettings.anchorCol : RoadEditorSettings.controlCol;
        newPoint.GetComponent<Renderer>().material = newPointMaterial;

        //настройка линии
        LineRenderer lineRenderer = newPoint.AddComponent<LineRenderer>();
        lineRenderer.material = ZTestMaterial;
        lineRenderer.startWidth = RoadEditorSettings.lineThickness;
        lineRenderer.endWidth = RoadEditorSettings.lineThickness;
        linesFromPoints.Add(lineRenderer);

        pointsObjects.Add(newPoint);
        newPoint.layer = LayerMask.NameToLayer("Point");
    }

    //проверки аналогичны скрипту Path, удаляются объекты отвечающие за отображение
    public void DeletePoint(int pointIndex)
    {
        int i = -1;

        if (pointIndex == 0)
        {
            if (path.IsClosed)
            {
                pointsObjects[pointsObjects.Count - 1] = pointsObjects[2];
            }
            i = 0;
        }
        else if (pointIndex == pointsObjects.Count - 1 && !path.IsClosed)
        {
            i = pointIndex - 2;
        }
        else
        {
            i = pointIndex - 1;
        }

        DeleteObjects(i, 3);

        UpdatePoints();
    }

    public int GetPointIndex(GameObject point)
    {
        return pointsObjects.IndexOf(point);
    }

    void DeleteObjects(int startIndex, int count)
    {
        //уничтожить объект на сцене
        for (int i = startIndex; i < startIndex + count; i++)
        {
            Destroy(pointsObjects[i]);
        }
        //удалить и списков
        pointsObjects.RemoveRange(startIndex, count);
        linesFromPoints.RemoveRange(startIndex, count);
    }

    public void SetIsOpen(bool value)
    {
        if(!value)
        {
            DeleteObjects(pointsObjects.Count - 2, 2);
        }

        UpdatePoints();
    }

    public void SetDisplayControlPoints(bool value)
    {
        if (value)
        {
            ShowPoints();
        }
        else
        {
            HidePoints();
        }
    }

    Mesh CreateRoadMesh(Vector3[] points, bool isClosed)
    {
        //перевести координаты точек в локальные
        for (int i = 0; i < points.Length; i++)
        {
            points[i] = transform.InverseTransformPoint(points[i]);
        }

        Vector3[] verts = new Vector3[points.Length * 2];
        Vector2[] uvs = new Vector2[verts.Length];
        int numTris = 2 * (points.Length - 1) + (isClosed ? 2 : 0);
        int[] tris = new int[numTris * 3];
        int vertIndex = 0;
        int triIndex = 0;

        for (int i = 0; i < points.Length; i++)
        {
            Vector3 forward = Vector3.zero;
            //для всех точек, кроме последеней или для всех, если путь закрытый - вперёд - это направление к следующей точке
            if(i < points.Length - 1 || isClosed)
            {
                forward += points[(i + 1) % points.Length] - points[i];
            }
            //для всех точек, кроме первой или для всех, если путь закрытый - вперёд - это направление от предыдущей точке
            if (i > 0 || isClosed)
            {
                forward += points[i] - points[(i - 1 + points.Length) % points.Length];
            }
            //вперёд - это среднее значе двух полученых значений
            forward.Normalize();

            Vector3 left = new Vector3(-forward.z, forward.y, forward.x);

            //одна вершина слева от точки, другая справа
            verts[vertIndex] = points[i] + left * roadWidth / 2f;
            verts[vertIndex + 1] = points[i] - left * roadWidth / 2f;

            //приподнять, чтобы плоскости не сливались
            verts[vertIndex] += new Vector3(0, 0.001f, 0);
            verts[vertIndex + 1] += new Vector3(0, 0.001f, 0);

            //uv координаты в начале и конце 0, в середине 1(для ровного перехода от конца к началу в закрытом пути)
            float complitionPercent = i / (float)(points.Length - 1);
            float v = 1 - Mathf.Abs(2 * complitionPercent - 1);
            uvs[vertIndex] = new Vector2(0, v);
            uvs[vertIndex + 1] = new Vector2(1, v);

            if(i < points.Length - 1 || isClosed)
            {
                tris[triIndex] = vertIndex;
                tris[triIndex + 1] = (vertIndex + 2) % verts.Length;
                tris[triIndex + 2] = vertIndex + 1;

                tris[triIndex + 3] = vertIndex + 1;
                tris[triIndex + 4] = (vertIndex + 2) % verts.Length;
                tris[triIndex + 5] = (vertIndex + 3) % verts.Length;
            }

            vertIndex += 2;
            triIndex += 6;
        }

        Mesh mesh = new Mesh();
        mesh.vertices = verts;
        mesh.triangles = tris;
        mesh.uv = uvs;

        return mesh;
    }

    public int SelectSegment(Vector3 pos)
    {
        //сбрасываем прошлый выделеный сегмент
        if(selectedSegmentIndex >= 0)
            linesFromPoints[selectedSegmentIndex * 3].material.color = RoadEditorSettings.segmentCol;

        //ищем две ближайшие якорные точки
        float minDist1 = float.MaxValue;
        float minDist2 = float.MaxValue;

        int minIndex1 = -1;
        int minIndex2 = -1;

        for (int i = 0; i < path.NumPoints; i+=3)
        {
            float dist = Vector3.Distance(path[i], pos);
            if (dist < minDist1)
            {
                minDist2 = minDist1;
                minDist1 = dist;

                minIndex2 = minIndex1;
                minIndex1 = i;
            }
            else if(dist < minDist2)
            {
                minDist2 = dist;

                minIndex2 = i;
            }
        }

        //если ближе точка с меньшим номером, то началом сегмента будет эта точка, иначе другая 
        selectedSegmentIndex = (minIndex1 < minIndex2) ? minIndex1 / 3 : minIndex2 / 3;
        linesFromPoints[selectedSegmentIndex * 3].material.color = RoadEditorSettings.selectedSegmentCol;
        return selectedSegmentIndex;
    }

    public void UnselectSegment()
    {
        linesFromPoints[selectedSegmentIndex * 3].material.color = RoadEditorSettings.segmentCol;
        selectedSegmentIndex = -1;
    }

    public void HidePoint(int pointIndex)
    {
        pointsObjects[pointIndex].SetActive(false);
    }

    public void ShowPoint(int pointIndex)
    {
        if (pointIndex % 3 == 0 || RoadEditorSettings.displayControlPoints)
            pointsObjects[pointIndex].SetActive(true);
    }


    public void HidePoints()
    {
        for (int i = 0; i < pointsObjects.Count; i++)
        {
            HidePoint(i);
        }
    }

    public void ShowPoints()
    {
        for (int i = 0; i < pointsObjects.Count; i++)
        {
            ShowPoint(i);
        }
    }
}
