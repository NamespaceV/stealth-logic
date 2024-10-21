using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArtRotator : MonoBehaviour
{
    public bool Rotate;
    public bool ChangleAngle;

    private Transform cameraTransform;

    private void Start()
    {
        cameraTransform = Camera.main.transform;
    }

    private void Update()
    {
        if (Rotate)
        {
            transform.LookAt(cameraTransform);
            transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0);
        }
    }
}
