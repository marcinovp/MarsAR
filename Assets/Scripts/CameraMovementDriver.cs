using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class CameraMovementDriver : MonoBehaviour
{
    [SerializeField] private bool active = true;
    [SerializeField] private ARSessionOrigin arSessionOrigin;
    [SerializeField] private Camera arCamera;

    public Vector3 totalCorrected;
    public Vector3 cameraPosition;

    private Transform cameraTransform;
    private Transform originTransform;
    private Transform cameraPivotTransform;
    private Vector3 cameraArmOffset;
    private Vector3 centerPoint;

    private void Awake()
    {
        cameraTransform = arCamera.transform;
        cameraArmOffset = cameraTransform.localPosition;
        centerPoint = arSessionOrigin.transform.position;
    }

    private void Start()
    {
        originTransform = arSessionOrigin.transform;
        cameraPivotTransform = cameraTransform.parent;
        
        cameraTransform.localPosition = Vector3.zero;
    }

    private void Update()
    {
        cameraPosition = cameraTransform.position;
    }

    private void LateUpdate()
    {
        if (!active)
            return;

        //Vector3 correction = -cameraTransform.localPosition;
        //arSessionOrigin.transform.Translate(correction, Space.World);

        //totalCorrected += correction;

        //cameraTransform.localPosition = cameraTransform.localRotation * cameraArmOffset;

        //cameraTransform.localPosition = cameraTransform.localRotation * cameraArmOffset - cameraArmOffset;

        Vector3 desiredLocalPosition = cameraTransform.localRotation * cameraArmOffset;

        Vector3 desiredCameraPosition = desiredLocalPosition + centerPoint;
        arSessionOrigin.transform.position -= cameraTransform.position - desiredCameraPosition;
    }

    public void Rotate(Quaternion rotation)
    {
        arSessionOrigin.transform.rotation *= rotation;
    }

    public void Rotate(Vector3 rotation, Space relativeTo)
    {
        arSessionOrigin.transform.Rotate(rotation, relativeTo);
    }

    public void ActivateLimiter(bool value) { active = value; }
}
