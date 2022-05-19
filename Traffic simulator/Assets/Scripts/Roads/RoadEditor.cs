using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RoadEditor : MonoBehaviour
{
    public Toggle AutoSetControlPointsToggle;
    public GameObject RoadSettingsPanel;

    Road currentRoad;
    RoadDisplaing currentRoadDisplaing;

    GameObject currentPoint = null;
    int currentPointIndex = -1;
    int currentSelectedSegmentIndex = -1;
    Vector3 segmentHitPoint;

    float snapRadius = 1.5f;
    SnapPoint lastSnapPoint;

    Crossroad currentCrossroad = null;

    Vector3 terrainPointOnScreenCenter
    {
        get
        {
            Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
            RaycastHit hit;
            Physics.Raycast(ray, out hit, LayerMask.GetMask("Terrain"));
            return hit.point;
        }
    }
    Vector3 positionForMove
    {
        get
        {
            Vector3 newPosition;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            bool hitTerraint = Physics.Raycast(ray, out hit, 1000, LayerMask.GetMask("Terrain"));

            //передвигать точки по земле, если нет земли, то в плоскости камеры
            if (hitTerraint)
            {
                newPosition = hit.point;
            }
            else
            {
                float mouseZ = Camera.main.WorldToScreenPoint(currentPoint.transform.position).z;
                Vector3 mousePosition = new Vector3(Input.mousePosition.x, Input.mousePosition.y, mouseZ);
                newPosition = Camera.main.ScreenToWorldPoint(mousePosition);
            }
            return newPosition;
        }
    }

    private void Update()
    {
        //проверка на нажатие на UI
        bool isOverUI = UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject();

        if (isOverUI)
            UnselectPreviousRoad();

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        #region Crossroad input
        if (Input.GetMouseButtonDown(0) && !isOverUI && !Input.GetKey(KeyCode.LeftShift))
        {
            //выделение дороги
            SelectRoad();

            bool hitCrossroadOrPoint = Physics.Raycast(ray, out hit, 1000, LayerMask.GetMask("Crossroad", "Point"));
            if (hitCrossroadOrPoint && hit.transform.GetComponentInParent<Crossroad>())
            {
                UnselectPreviousRoad();
                SelectCrossroad(hit.transform.gameObject);
            }
        }

        if (currentCrossroad)
            MoveCrossroad();

        if (Input.GetMouseButtonUp(0))
        {
            ReleaseCrossroad();
        }
        #endregion

        if (!currentRoad)
            return;

        //выделение сегмента
        bool hitRoad = Physics.Raycast(ray, out hit, 1000, LayerMask.GetMask("Road"));
        if (!isOverUI && currentRoad != null)
        {
            if (hitRoad) {
                segmentHitPoint = hit.point;
                currentSelectedSegmentIndex = currentRoadDisplaing.SelectSegment(segmentHitPoint);
            }
            else if (currentSelectedSegmentIndex >= 0)
            {
                currentSelectedSegmentIndex = -1;
                currentRoadDisplaing.UnselectSegment();
            }

        }

        if (Input.GetMouseButtonDown(0) && !isOverUI)
        {
            //выделение точки
            bool hitPoint = Physics.Raycast(ray, out hit, 1000, LayerMask.GetMask("Point"));
            if (hitPoint && currentPoint == null)
            {
                SelectPoint(hit.transform.gameObject);
            }

            //добавление точки
            if (currentSelectedSegmentIndex >= 0 && Input.GetKey(KeyCode.LeftShift))
            {
                SplitRoad(segmentHitPoint);
            }
            else
            {
                bool hitTerraint = Physics.Raycast(ray, out hit, 1000, LayerMask.GetMask("Terrain"));
                if (!currentRoad.path.IsClosed && hitTerraint && Input.GetKey(KeyCode.LeftShift))
                {
                    AddPoint(hit.point);
                }
            }
        }

        if(Input.GetMouseButtonUp(1) && !isOverUI)
        {
            //удаление точки
            bool hitPoint = Physics.Raycast(ray, out hit, 1000, LayerMask.GetMask("Point"));
            if (hitPoint)
            {
                DeletePoint(hit.transform.gameObject);
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            ReleasePoint();
        }

        if (currentPoint)
            MovePoint();
    }

    #region Crossroad
    private void MoveCrossroad()
    {
        currentCrossroad.MoveCrossroad(positionForMove);
    }

    private void ReleaseCrossroad()
    {
        currentCrossroad = null;
    }

    private void SelectCrossroad(GameObject crossroad)
    {
        currentCrossroad = crossroad.GetComponentInParent<Crossroad>();
    }

    public void CreateNewCrossroad()
    {
        UnselectPreviousRoad();
        Instantiate(Prefabs.Instance.Crossroad, terrainPointOnScreenCenter, Quaternion.identity);
    }
    #endregion

    #region Road
    private void SelectRoad()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        bool hitPoint = Physics.Raycast(ray, out hit, 1000, LayerMask.GetMask("Point"));

        //если нажал на точку, то перевыбирать дороги не надо
        if (hitPoint)
            return;

        bool hitRoad = Physics.Raycast(ray, out hit, 1000, LayerMask.GetMask("Road"));

        if (hitRoad)
        {
            //если нажали на выбраную дорогу, то перевыбирать не надо
            Road hittedRoad = hit.transform.GetComponentInParent<Road>();
            if (currentRoad == hittedRoad)
                return;

            UnselectPreviousRoad();

            currentRoad = hittedRoad;
            currentRoadDisplaing = currentRoad.GetComponentInChildren<RoadDisplaing>();
            currentRoadDisplaing.ShowPoints();

            RoadSettingsPanel.SetActive(true);
            FillRoadSettings();
        }
        else
        {
            UnselectPreviousRoad();
        }
    }

    private void FillRoadSettings()
    {
        Toggle isClosedToggle = RoadSettingsPanel.transform.GetChild(0).GetComponent<Toggle>();
        Toggle autosetControlPointsToggle = RoadSettingsPanel.transform.GetChild(1).GetComponent<Toggle>();

        isClosedToggle.isOn = currentRoad.path.IsClosed;
        autosetControlPointsToggle.isOn = currentRoad.path.AutoSetControlPoints;
    }

    private void UnselectPreviousRoad()
    {
        if (currentRoad != null)
        {
            currentRoad.UpdateLanes();

            currentRoad = null;
            currentRoadDisplaing.HidePoints();

            RoadSettingsPanel.SetActive(false);
        }
    }

    //дорога создаётся в центре экрана
    public void CreateNewRoad()
    {
        UnselectPreviousRoad();

        currentRoad = Instantiate(Prefabs.Instance.Road, terrainPointOnScreenCenter, Quaternion.identity).GetComponent<Road>();
        currentRoadDisplaing = currentRoad.GetComponentInChildren<RoadDisplaing>();

        RoadSettingsPanel.SetActive(true);
        FillRoadSettings();
    }
    #endregion

    #region Points
    private void SelectPoint(GameObject gameObject)
    {
        currentPoint = gameObject;
        currentPointIndex = currentRoadDisplaing.GetPointIndex(currentPoint);

        currentRoadDisplaing = currentRoad.GetComponentInChildren<RoadDisplaing>();
    }

    private void ReleasePoint()
    {
        currentPoint = null;
    }

    private void MovePoint()
    {
        Vector3 newPosition = positionForMove;

        //привязывать можно только крайние точки
        if (currentPointIndex == 0 || currentPointIndex == currentRoad.path.NumPoints - 1)
        {
            Collider[] snapPoints = Physics.OverlapSphere(newPosition, snapRadius, LayerMask.GetMask("Snap point"));

            //если приблизились к точке привязки, то ставим точку в ближайшую точку привязки
            if (snapPoints.Length > 0)
            {
                float minDist = float.MaxValue;
                int closestPointIndex = -1;
                for (int i = 0; i < snapPoints.Length; i++)
                {
                    float dist = Vector3.Distance(newPosition, snapPoints[i].transform.position);
                    if (dist < minDist)
                    {
                        minDist = dist;
                        closestPointIndex = i;
                    }
                }
                Vector3 snapPosition = snapPoints[closestPointIndex].transform.position;
                Vector3 controlPointDir = snapPosition - snapPoints[closestPointIndex].transform.parent.position;

                //если удалось подключить путь, то подключаем дорогу
                if (currentRoad.path.ConnectStartOrEndPoint(currentPointIndex, snapPosition, controlPointDir))
                {
                    lastSnapPoint = snapPoints[closestPointIndex].GetComponent<SnapPoint>();
                    currentRoad.ConnectToSnapPoint(lastSnapPoint, currentPointIndex == 0);
                }
                else
                {
                    currentRoad.path.MovePoint(currentPointIndex, newPosition);
                }
            }
            else
            {
                bool needDisconnectStart = currentRoad.path.StartConnected && currentPointIndex == 0;
                bool needDisconnectEnd = currentRoad.path.EndConnected && currentPointIndex == currentRoad.path.NumPoints - 1;

                if (needDisconnectStart)
                {
                    currentRoad.DisconnectSnapPoint(true);
                }else if (needDisconnectEnd)
                {
                    currentRoad.DisconnectSnapPoint(false);
                }
                
                if (needDisconnectStart || needDisconnectEnd)
                {
                    currentRoad.path.DisconnectStartOrEndPoint(currentPointIndex);
                }
                
                currentRoad.path.MovePoint(currentPointIndex, newPosition);
            }
        }
        else
        {
            currentRoad.path.MovePoint(currentPointIndex, newPosition);
        }
        currentRoadDisplaing.UpdatePoints();
    }

    private void AddPoint(Vector3 point)
    {
        currentRoad.path.AddSegment(point);
        currentRoadDisplaing.UpdatePoints();
    }

    private void DeletePoint(GameObject point)
    {
        int anchorIndex = currentRoadDisplaing.GetPointIndex(point);
        if (currentRoad.path.DeleteSegment(anchorIndex))
        {
            if (currentRoad.path.NumSegments == 1)
                AutoSetControlPointsToggle.isOn = false;
            currentRoadDisplaing.DeletePoint(anchorIndex);
        }
    }


    void SplitRoad(Vector3 point)
    {
        currentRoad.path.SplitSegment(point, currentSelectedSegmentIndex);
        currentRoadDisplaing.UpdatePoints();
    }

    #endregion

    #region Settings
    public void SetIsClosed(Toggle toggle)
    {
        if (currentRoadDisplaing.SetIsOpen(toggle.isOn))
        {
            currentRoad.path.IsClosed = toggle.isOn;
            currentRoadDisplaing.UpdatePoints();
        }
        else
        {
            toggle.isOn = !toggle.isOn;
        }
    }

    public void SetAutoSetControlPoints(Toggle toggle)
    {
        if (currentRoad.path.NumSegments == 1 && toggle.isOn)
        {
            toggle.isOn = false;
            return;
        }
        currentRoad.path.AutoSetControlPoints = toggle.isOn;
        currentRoadDisplaing.UpdatePoints();
    }

    public void SetDisplayControlPoints(Toggle toggle)
    {
        RoadEditorSettings.displayControlPoints = toggle.isOn;
        if (currentRoad != null)
            currentRoadDisplaing.SetDisplayControlPoints(toggle.isOn);
    }
    #endregion
}
