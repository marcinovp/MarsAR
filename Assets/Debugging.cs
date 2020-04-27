using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;

public class Debugging : MonoBehaviour
{
    public ARSessionOrigin arSessionOrigin;
    public Transform targetFakePivot;
    public Transform targetFakeCenter;
    public Transform epoxidPivot;
    public Transform epoxidCenter;
    public Transform marsPlaceholder;
    //public ARTrackedImage trackedImage;
    public Calibrator calibrator;

    public Text debugText;
    public Text debugText2;

    private Transform cameraTransform;

    private void Start()
    {
        cameraTransform = arSessionOrigin.camera.transform;
    }

    private void Update()
    {
        DebugCamera(debugText);
        DebugScene(debugText2);
    }

    public void DebugCamera(Text uiText)
    {
        string text = string.Format("Camera global: position {0}, rotation: {1}\nCamera local: position {2}, rotation: {3}\nAR origin: position {4}, rotation: {5}",
            VectorToCm(cameraTransform.position), cameraTransform.eulerAngles,
            VectorToCm(cameraTransform.localPosition), cameraTransform.localEulerAngles,
            VectorToCm(arSessionOrigin.transform.position), arSessionOrigin.transform.eulerAngles);

        uiText.text = text;
    }

    public void DebugScene(Text uiText)
    {
        Vector3 relativeToFakeTarget = cameraTransform.InverseTransformPoint(targetFakePivot.position);
        Vector3 relativeToEpoxid = cameraTransform.InverseTransformPoint(epoxidPivot.position);
        Vector3 relativeToMars = cameraTransform.InverseTransformPoint(marsPlaceholder.position);

        Vector3 relativeToFakeTargetCenter = cameraTransform.InverseTransformPoint(targetFakeCenter.position);
        Vector3 relativeToEpoxidCenter = cameraTransform.InverseTransformPoint(epoxidCenter.position);

        string text = string.Format("Relative positions to camera\nepoxid: {0} | {3}\nmars: {1}\nfake target: {2} | {4}",
            VectorToCm(relativeToEpoxid), VectorToCm(relativeToMars), VectorToCm(relativeToFakeTarget), VectorToCm(relativeToEpoxidCenter), VectorToCm(relativeToFakeTargetCenter));

        if (calibrator.trackedImage != null)
        {
            Vector3 relativeToRealTarget = cameraTransform.InverseTransformPoint(calibrator.trackedImage.transform.position);
            text += string.Format("\nreal target: {0}", VectorToCm(relativeToRealTarget));
        }

        uiText.text = text;
    }

    private string VectorToCm(Vector3 vector)
    {
        string form = string.Format("{0:0}, {1:0}, {2:0}", Mathf.Round(vector.x * 100), Mathf.Round(vector.y * 100), Mathf.Round(vector.z * 100));
        return form;
    }
}
