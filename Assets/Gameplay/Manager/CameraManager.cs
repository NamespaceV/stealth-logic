using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    private Vector3 _lastMousePosition;
    
    void Update()
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

    private void OnDrawGizmos()
    {
        //Gizmos.DrawSphere(Camera.main.ScreenToWorldPoint(Input.mousePosition), 1);
    }
}
