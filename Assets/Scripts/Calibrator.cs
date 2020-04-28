using System.Collections;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class Calibrator : MonoBehaviour
{
    [SerializeField] private ARSessionOrigin arSessionOrigin;
    [SerializeField] private ARTrackedImageManager trackedImageManager;
    [SerializeField] private CameraMovementLimiter cameraMover;
    [SerializeField] private Transform calibrationReference;

    private Transform cameraTransform;
    public ARTrackedImage trackedImage;
    private Coroutine calibration;

    void Start()
    {
        cameraTransform = arSessionOrigin.camera.transform;
        trackedImageManager.trackedImagesChanged += OnTrackedImagesChanged;

    }

    void Update()
    {
        //if (Input.GetKeyUp(KeyCode.C))
        //    ProcessTrackedImageChange();

        //Debug.DrawRay(cameraTransform.position, cameraTransform.forward, Color.blue);
        Debug.DrawRay(cameraTransform.position, arSessionOrigin.transform.forward, Color.blue);
    }

    private void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs eventArgs)
    {
        bool wasChange = false;

        foreach (var plane in eventArgs.added)
        {
            trackedImage = plane;
            wasChange = true;
            Debug.Log("Added image, count: " + eventArgs.added.Count);
        }

        foreach (var plane in eventArgs.updated)
        {
            wasChange = true;
            Debug.Log("Changed image, count: " + eventArgs.updated.Count);
        }

        foreach (var plane in eventArgs.removed)
        {
            trackedImage = null;
            wasChange = true;
            Debug.Log("Removed image, count: " + eventArgs.removed.Count);
        }

        //if (wasChange)
        //{
        //    ProcessTrackedImageChange();
        //}

        //if (wasChange)
        //    OnFloorUpdateEvent(FloorARPlane);
    }

    public void ProcessTrackedImageChange()
    {
        //Vector3 fromVector = trackedImage.transform.position - cameraTransform.position;
        //Vector3 toVector = calibrationReference.position - cameraTransform.position;
        //Quaternion lookTo = Quaternion.FromToRotation(fromVector, toVector);
        //cameraMover.Rotate(lookTo);

        //float verticalAxis = Vector3.SignedAngle(fromVector, toVector, Vector3.up);
        //float horizontalAxis = Vector3.SignedAngle(fromVector, toVector, Vector3.forward);
        //cameraMover.Rotate(new Vector3(horizontalAxis, verticalAxis, 0), Space.World);

        calibration = StartCoroutine(LerpToRotation2());
    }

    IEnumerator LerpToRotation()
    {
        Vector3 fromVector = trackedImage.transform.position - cameraTransform.position;
        Vector3 toVector = calibrationReference.position - cameraTransform.position;
        Quaternion fromRotation = Quaternion.LookRotation(fromVector);
        Quaternion toRotation = Quaternion.LookRotation(toVector);

        //Quaternion.LookRotation
        float startTime = Time.time;
        float lerp = 0;
        float duration = 3;
        //cameraMover.Rotate(lookTo);
        while (lerp < 1)
        {
            Quaternion lookTo = Quaternion.Slerp(fromRotation, toRotation, lerp);
            arSessionOrigin.transform.rotation = lookTo;
            yield return null;
            lerp = (Time.time - startTime) / duration;
        }
        Quaternion lookTo2 = Quaternion.Slerp(fromRotation, toRotation, 1);
        arSessionOrigin.transform.rotation = lookTo2;
    }

    IEnumerator LerpToRotation2()
    {
        float speed = Mathf.PI * .5f;

        while (true)
        {
            Vector3 fromVector = trackedImage.transform.position - cameraTransform.position;


            Vector3 toVector = calibrationReference.position - cameraTransform.position;

            Debug.DrawRay(cameraTransform.position, toVector, Color.red);

            Quaternion offset = Quaternion.FromToRotation(fromVector, arSessionOrigin.transform.forward);
            //Debug.Log("offset: " + offset.eulerAngles);
            Vector3 shiftedToVector = offset * toVector;
            float angle = Vector3.Angle(arSessionOrigin.transform.forward, shiftedToVector);
            Debug.DrawLine(cameraTransform.position, cameraTransform.position + shiftedToVector / 2, Color.red);

            if (angle < 1)
                yield break;

            // The step size is equal to speed times frame time.
            float singleStep = speed * Time.deltaTime;

            // Rotate the forward vector towards the target direction by one step
            Vector3 newDirection = Vector3.RotateTowards(arSessionOrigin.transform.forward, shiftedToVector, singleStep, 0.0f);

            Debug.DrawRay(cameraTransform.position, shiftedToVector, Color.green);
            arSessionOrigin.transform.rotation = Quaternion.LookRotation(newDirection);
            yield return null;
        }
    }

    public void StopCalibration()
    {
        if (calibration != null)
            StopCoroutine(calibration);
    }
}
