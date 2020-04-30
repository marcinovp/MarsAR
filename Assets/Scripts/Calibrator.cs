using System.Collections;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class Calibrator : MonoBehaviour
{
    [SerializeField] private ARSessionOrigin arSessionOrigin;
    [SerializeField] private ARTrackedImageManager trackedImageManager;
    [SerializeField] private CameraMovementDriver cameraMover;
    [SerializeField] private Transform calibrationReference;
    public int strategy;

    [Header("Debug")]
    public bool debug;

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
        if (debug)
        {
            //session origin forward
            Debug.DrawRay(arSessionOrigin.transform.position, arSessionOrigin.transform.forward, Color.yellow);
            //ku detekovanemu targetu
            Debug.DrawLine(cameraTransform.position, trackedImage.transform.position, Color.red);
        }
    }

    private void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs eventArgs)
    {
        bool wasChange = false;

        foreach (var plane in eventArgs.added)
        {
            trackedImage = plane;
            wasChange = true;
            Log("Added image, count: " + eventArgs.added.Count);
        }

        foreach (var plane in eventArgs.updated)
        {
            wasChange = true;
            Log("Changed image, count: " + eventArgs.updated.Count);
        }

        foreach (var plane in eventArgs.removed)
        {
            trackedImage = null;
            wasChange = true;
            Log("Removed image, count: " + eventArgs.removed.Count);
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
        switch (strategy)
        {
            case 0:
                calibration = StartCoroutine(LerpToRotation());
                break;
            case 1:
                calibration = StartCoroutine(LerpToRotation2());
                break;
            default:
                break;
        }
    }

    // funguje na 100%, nulova uhlova chyba
    IEnumerator LerpToRotation()
    {
        float errorAngle = float.PositiveInfinity;
        float duration = .6f;

        for (int i = 0; i < 3; i++)
        {
            Vector3 fromVector = trackedImage.transform.position - cameraTransform.position;
            Vector3 toVector = calibrationReference.position - cameraTransform.position;
            Quaternion fromRotation = Quaternion.LookRotation(fromVector);
            Quaternion toRotation = Quaternion.LookRotation(toVector);
            toRotation = toRotation * Quaternion.Inverse(fromRotation);
            Quaternion startingRotation = arSessionOrigin.trackablesParent.rotation;

            float startTime = Time.time;
            float lerp = 0;

            while (lerp < 1)
            {
                Quaternion lookTo = Quaternion.Slerp(Quaternion.identity, toRotation, lerp);
                arSessionOrigin.transform.rotation = lookTo * startingRotation;
                yield return null;
                lerp = (Time.time - startTime) / duration;
            }

            arSessionOrigin.transform.rotation = toRotation * startingRotation;

            Vector3 fromVector2 = trackedImage.transform.position - cameraTransform.position;
            Vector3 toVector2 = calibrationReference.position - cameraTransform.position;
            errorAngle = Vector3.Angle(fromVector2, toVector2);
            duration = duration / 2;

            if (errorAngle < 0.5f)
                break;
        }

        Log("Uhlova chyba: " + errorAngle);
    }

    // funguje na 100% ale dlho to trva, niekedy chodi dookola
    IEnumerator LerpToRotation2()
    {
        float speed = Mathf.PI * .5f;

        while (true)
        {
            //od kamere ku targetu
            Vector3 fromVector = trackedImage.transform.position - cameraTransform.position;

            //od kamere ku kalibracnemu cielu
            Vector3 toVector = calibrationReference.position - cameraTransform.position;

            //Debug.DrawRay(cameraTransform.position, toVector, Color.red);

            Quaternion offset = Quaternion.FromToRotation(fromVector, toVector);

            //Log(string.Format("fromVector: {1}, offset: {0}, offset size: {2}", offset.eulerAngles, fromVector.ToString("G3"), Vector3.Angle(fromVector, toVector)));
            Vector3 shiftedToVector = offset * arSessionOrigin.transform.forward;
            float angle = Vector3.Angle(arSessionOrigin.transform.forward, shiftedToVector);

            // uplne cielovy vektor - offsetnuty
            Debug.DrawLine(cameraTransform.position, cameraTransform.position + shiftedToVector / 3, Color.green);

            if (angle < .5f)
            {
                Log("Uhlova chyba: " + angle);
                yield break;
            }

            // The step size is equal to speed times frame time.
            float singleStep = speed * Time.deltaTime;

            // Rotate the forward vector towards the target direction by one step
            Vector3 newDirection = Vector3.RotateTowards(arSessionOrigin.transform.forward, shiftedToVector, singleStep, 0.0f);

            arSessionOrigin.transform.rotation = Quaternion.LookRotation(newDirection);
            yield return null;
        }
    }

    public void StopCalibration()
    {
        if (calibration != null)
            StopCoroutine(calibration);
    }

    private void Log(string message)
    {
        if (debug)
            Debug.Log(message);
    }
}
