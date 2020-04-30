using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class CameraMovementDriver : MonoBehaviour
{
    [SerializeField] private bool active = true;
    [SerializeField] private ARSessionOrigin arSessionOrigin;

    public Vector3 totalCorrected;
    public Vector3 cameraPosition;

    private Transform cameraTransform;
    private Transform originTransform;
    private Transform cameraPivotTransform;
    private Vector3 cameraArmOffset;

    private void Start()
    {
        cameraTransform = arSessionOrigin.camera.transform;
        originTransform = arSessionOrigin.transform;
        cameraPivotTransform = cameraTransform.parent;

        cameraArmOffset = cameraTransform.localPosition;
        //cameraArmOffset = Quaternion.Inverse(cameraTransform.localRotation) * cameraArmOffset;

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

        cameraTransform.localPosition = cameraTransform.localRotation * cameraArmOffset - cameraArmOffset;
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
