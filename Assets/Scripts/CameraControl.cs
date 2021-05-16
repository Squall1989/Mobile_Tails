using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControl : MonoBehaviour
{
    public static CameraControl cameraControl;
    public int cameraRotateSpeed;
    public Vector3 startPosition;
    private Coroutine rotateCorout;
    private const int stepsRotate = 20;
    private Camera thisCamera;
    // Start is called before the first frame update
    void Start()
    {
        thisCamera = GetComponent<Camera>();
        cameraControl = this;
    }

    public void CameraRotate(int clockwise)
    {
        if(rotateCorout == null)
            rotateCorout = StartCoroutine(rotateCamCorout(clockwise));
    }

    public void CullingMaskLayer(int layer_, bool turnOn)
    {
        if(turnOn)
            thisCamera.cullingMask |= (1 << layer_);
        else
            thisCamera.cullingMask = ~(1 << layer_);

    }

    private IEnumerator rotateCamCorout(int wiseInt)
    {
        Vector3 tailsCenter = TailsTable.talesTable.GetCenterOfTalesArray();
        float rotatePeriod = 90f / (stepsRotate);

        for(int i = 0; i < stepsRotate; i++)
        {

            transform.RotateAround(tailsCenter, Vector3.up, rotatePeriod * wiseInt);
            yield return null;// new WaitForSeconds(rotatePeriod);
        }
        rotateCorout = null;
        SelectCanvas.selectCanvas.transform.Rotate(0, 90f, 0);

    }
}
