using Assets.Gameplay.Manager;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    private Vector3 _lastMousePosition;

    private GameManager _gameManager;

    public float rotationSpeed;
    private Vector3 lookTarget;
    public float cameraDistance;
    public float angle;
    public float minZoom;
    public float maxZoom;
    public float zoomSpeed;
    private float currentRotation = 0f;
    public float minAngle = -80f;
    public float maxAngle = 80f;

    private void Start()
    {
        _gameManager = GameManager.Instance;
    }

    void Update()
    {
        if(_gameManager.isPlaying)
        {
            HandleInGameCamera();
        }
        else
        {
            HandleEditorCamera();
        }
    }

    private void HandleEditorCamera()
    {
        var currentMousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        if (Input.GetMouseButton(2))
        {
            var delta = currentMousePosition - _lastMousePosition;
            transform.position -= delta;
            currentMousePosition -= delta;
        }

        _lastMousePosition = currentMousePosition;
    }

    private void HandleInGameCamera()
    {
        lookTarget = new Vector3(_gameManager.mapWidth / 2, 0, _gameManager.mapHeight / 2);

        // Horizontal rotation (right mouse button)
        if (Input.GetMouseButton(1))
        {
            float deltaX = Input.GetAxis("Mouse X");
            currentRotation -= deltaX * rotationSpeed;
        }

        // Vertical rotation (left mouse button)
        if (Input.GetMouseButton(0))
        {
            float deltaY = Input.GetAxis("Mouse Y");
            angle -= deltaY * rotationSpeed;
            angle = Mathf.Clamp(angle, minAngle, maxAngle);
        }

        // Calculate camera position
        float radianAngle = currentRotation * Mathf.Deg2Rad;
        float x = Mathf.Sin(radianAngle) * cameraDistance;
        float z = Mathf.Cos(radianAngle) * cameraDistance;
        float y = Mathf.Sin(angle * Mathf.Deg2Rad) * cameraDistance;

        transform.position = lookTarget + new Vector3(x, y, z);

        // Zoom functionality
        float scrollInput = Input.GetAxis("Mouse ScrollWheel");
        if (scrollInput != 0)
        {
            cameraDistance += scrollInput * zoomSpeed;
            cameraDistance = Mathf.Clamp(cameraDistance, minZoom, maxZoom);
            transform.position = lookTarget + new Vector3(x, y, z);
        }

        // Make the camera look at the target
        transform.LookAt(lookTarget);
    }

    private void OnDrawGizmos()
    {
        //Gizmos.DrawSphere(Camera.main.ScreenToWorldPoint(Input.mousePosition), 1);
    }
}
