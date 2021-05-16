using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CanvasLookCam : MonoBehaviour
{
    private void Start()
    {
        UpdateLook();
    }
    private void UpdateLook()
    {
        if (Camera.main)
            transform.LookAt(Camera.main.transform.position, Camera.main.transform.up);
    }

    private void Update()
    {
        UpdateLook();

    }

}
