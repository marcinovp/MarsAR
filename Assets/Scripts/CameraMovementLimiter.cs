using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class CameraMovementLimiter : MonoBehaviour
{
    [SerializeField] private bool active = true;
    [SerializeField] private ARSessionOrigin arSessionOrigin;

    public Vector3 totalCorrected;
    public Vector3 cameraPosition;

    private Transform cameraTransform;

    private void Start()
    {
        cameraTransform = arSessionOrigin.camera.transform;
    }

    private void Update()
    {
        cameraPosition = cameraTransform.position;
    }

    private void LateUpdate()
    {
        if (!active)
            return;

        Vector3 correction = -cameraTransform.position;
        arSessionOrigin.transform.Translate(correction, Space.World);

        totalCorrected += correction;
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
