using Assets.Gameplay.Manager;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    private Vector3 _lastMousePosition;

    private GameManager _gameManager;

    private Vector3 lookTarget;

    public float cameraDistance;
    public float minZoom;
    public float maxZoom;
    public float zoomSpeed;

    public float angleDegrees;
    public float minAngle = 15;
    public float maxAngle = 85f;

    public float directionDegrees = 0f;

    public float rotationSpeed;

    private bool wasInPlayMode;

    private void Start()
    {
        _gameManager = GameManager.Instance;
    }

    void Update()
    {
        if (_gameManager.isPlaying != wasInPlayMode)
        {
            CameraTypeChanged(_gameManager.isPlaying);
        }
        if(_gameManager.isPlaying)
        {
            HandleInGameCamera();
        }
        else
        {
            HandleEditorCamera();
        }
    }

    private void CameraTypeChanged(bool isPlayMode)
    {
        wasInPlayMode= isPlayMode;
        if (isPlayMode) {
            Camera.main.orthographic = false;
            directionDegrees = 90;
            angleDegrees = 45;

        } else {
            // editor
            Camera.main.orthographic = true;
            transform.position = new Vector3(5, 4.5f, -10);
            transform.rotation = Quaternion.identity;
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
            //Debug.Log($"Move = {delta} = {currentMousePosition} - {_lastMousePosition} Mouse {Input.mousePosition}");
        }

        _lastMousePosition = currentMousePosition;
    }

    private void HandleInGameCamera()
    {

        // Rotate camera (right mouse button)
        if (Input.GetMouseButton(1))
        {
            float deltaX = Input.GetAxis("Mouse X");
            directionDegrees -= deltaX * rotationSpeed;
            float deltaY = Input.GetAxis("Mouse Y");
            angleDegrees -= deltaY * rotationSpeed;
            angleDegrees = Mathf.Clamp(angleDegrees, minAngle, maxAngle);
        }

        // Zoom  (middle)
        float scrollInput = Input.GetAxis("Mouse ScrollWheel");
        if (scrollInput != 0)
        {
            cameraDistance += scrollInput * zoomSpeed;
            cameraDistance = Mathf.Clamp(cameraDistance, minZoom, maxZoom);
        }

        var rotation = Quaternion.Euler(0, directionDegrees, angleDegrees);
        lookTarget = new Vector3(_gameManager.mapWidth / 2, 0, _gameManager.mapHeight / 2);
        transform.position = lookTarget + rotation * new Vector3(cameraDistance, 0,0);
        transform.LookAt(lookTarget);
    }

    private void OnDrawGizmos()
    {
        //Gizmos.DrawSphere(Camera.main.ScreenToWorldPoint(Input.mousePosition), 1);
    }
}
