using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CameraController : MonoBehaviour
{
    private bool isFree = false;

    [SerializeField]
    private float speed = 15f;
    [SerializeField]
    private float verticalSpeed = 150f;
    [SerializeField]
    private float mouseSensitivity = 150f;
    [SerializeField]
    private float defaultHeight = 20f;
    [SerializeField]
    private Vector3 defaultAngles = new Vector3(60, 0, 0);

    private float xRotation = 60f;
    private float yRotation = 0f;

    Clickable currentClickable;

    Ray ray => Camera.main.ScreenPointToRay(Input.mousePosition);
    RaycastHit hit;

    void Update()
    {
        if (ModeChanger.CurrentMode != Mode.View)
            return;

        bool isOverUI = UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject();

        if (isOverUI)
            return;

        if (Input.GetMouseButtonUp(0))
        {
            Physics.Raycast(ray, out hit);
            Clickable clickable = hit.transform.GetComponentInParent<Clickable>();
            
            if (clickable)
            {
                clickable.OnClick();
                currentClickable = clickable;
            }
            else if(currentClickable)
            {
                currentClickable.panel.HidePanel();
            }
        }

        if(Input.GetMouseButtonUp(1) && Input.GetKey(KeyCode.LeftAlt))
        {
            Physics.Raycast(ray, out hit);
            IDeleteable deleteable = hit.transform.GetComponentInParent<IDeleteable>();

            if (deleteable != null)
                deleteable.Delete();
        }
    }

    private void FixedUpdate()
    {
        Move();
    }

    private void Move()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            SetIsFree(false);

        //передвижение горизонтально
        Vector3 moveDirection = Vector3.zero;

        if (Input.GetKey(KeyCode.W))
            moveDirection += Vector3.forward;
        if (Input.GetKey(KeyCode.A))
            moveDirection += Vector3.left;
        if (Input.GetKey(KeyCode.S))
            moveDirection += Vector3.back;
        if (Input.GetKey(KeyCode.D))
            moveDirection += Vector3.right;

        //передвижение вертикально
        float verticalInput = -Input.mouseScrollDelta.y / 10;

        if (Input.GetKey(KeyCode.LeftControl))
            verticalInput -= 0.1f;
        if (Input.GetKey(KeyCode.Space))
            verticalInput += 0.1f;

        Vector3 move = moveDirection * speed * Time.deltaTime;

        if (isFree)
        {
            transform.Translate(move, Space.Self);
            Rotate();
        }
        else
            transform.Translate(move, Space.World);

        Vector3 verticalMove = Vector3.up * verticalInput * Time.deltaTime * verticalSpeed * transform.position.y / 10;
        transform.Translate(verticalMove, Space.World);
    }

    //поворот камеры в свободном режиме
    private void Rotate()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation ,-90f, 90f);

        yRotation += mouseX;

        transform.localRotation = Quaternion.Euler(xRotation, yRotation, 0f);
    }

    //смена режима
    public void SetIsFree(bool val)
    {
        isFree = val;

        if (isFree)
        {
            xRotation = transform.eulerAngles.x;
            yRotation = transform.eulerAngles.y;

            StopAllCoroutines();

            Cursor.lockState = CursorLockMode.Locked;
        }
        else
        {
            StartCoroutine(ToTopView());

            Cursor.lockState = CursorLockMode.None;
        }
    }

    //перемещение камеры к стандартной позиции
    public IEnumerator ToTopView()
    {
        bool correctTransform = false;
        bool correctRotation = false;

        while(!correctTransform || !correctRotation)
        {
            if (!correctTransform)
            {
                if (transform.position.y > defaultHeight)
                    transform.Translate(Vector3.down * Time.deltaTime * speed, Space.World);
                else
                    transform.Translate(Vector3.up * Time.deltaTime * speed, Space.World);

                //достижение нужной высоты
                if (transform.position.y < defaultHeight + 0.01f && transform.position.y > defaultHeight - 0.01f)
                {
                    correctTransform = true;
                }
            }

            if (!correctRotation)
            {
                transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(defaultAngles), Time.deltaTime * speed / 10);

                //достижение нужного поворота
                if (Vector3.Angle(transform.rotation.eulerAngles, defaultAngles) < 1f)
                {
                    correctRotation = true;
                    transform.rotation = Quaternion.Euler(defaultAngles);
                }
            }
            
            yield return null;
        }
    }
}
