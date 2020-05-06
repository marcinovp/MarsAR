using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class CalibrationManager : MonoBehaviour
{
    [SerializeField] private ARSessionOrigin arSessionOrigin;
    [SerializeField] private Calibrator calibrator;
    [SerializeField] private XRReferenceImageLibrary imageLibrary;
    [SerializeField] private ARTrackedImageManager trackedImageManager;
    [SerializeField] private Transform calibrationReference;
    [SerializeField] private float angleErrorThreshold = 1f;

    [Header("Debug")]
    public bool debug;
    public Text debugText;
    public ARTrackedImage debugTrackedImage;
    public Transform debugCalibrationReference;

    private Transform cameraTransform;
    public ARTrackedImage trackedImage;

    void Start()
    {
        cameraTransform = arSessionOrigin.camera.transform;
        trackedImageManager.trackedImagesChanged += OnTrackedImagesChanged;
        StartCoroutine(Control());
    }

    void Update()
    {
        if (debug)
        {
            //ku detekovanemu targetu
            if (trackedImage != null)
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
    }

    private IEnumerator Control()
    {
        // To give time to Calibrator to run its Start function
        yield return new WaitForSeconds(0.2f);

        float minUpdateCycle = 1f;
        float lastUpdateTime = Time.time - minUpdateCycle;

        while (true)
        {
            if (trackedImage == null)
                yield return null;
            else
            {
                string screenLogMessage = "";

                Vector3 screenPoint = arSessionOrigin.camera.WorldToViewportPoint(trackedImage.transform.position);
                bool onScreen = screenPoint.z > 0 && screenPoint.x > 0 && screenPoint.x < 1 && screenPoint.y > 0 && screenPoint.y < 1;

                if (!onScreen || trackedImage.trackingState != TrackingState.Tracking)
                {
                    screenLogMessage += string.Format("No suitable image tracked");
                    LogOnScreen(screenLogMessage);

                    yield return null;
                    continue;
                }

                float angle = Mathf.Abs(calibrator.GetErrorAngle(trackedImage, calibrationReference));
                if (angle > angleErrorThreshold)
                {
                    screenLogMessage += string.Format("Error angle: {0}, kalibrujem", angle);
                    LogOnScreen(screenLogMessage);

                    Calibrate();

                    yield return new WaitForSeconds(2);
                }
                else
                {
                    screenLogMessage += string.Format("Error angle: {0}", angle);
                    LogOnScreen(screenLogMessage);

                    yield return new WaitForSeconds(minUpdateCycle);
                }
            }
        }
    }

    public void Calibrate()
    {
        calibrator.Calibrate(trackedImage, calibrationReference);
    }

    public void DebugCalibrate()
    {
        calibrator.Calibrate(trackedImage != null ? trackedImage : debugTrackedImage, debugCalibrationReference);
    }

    public void StopCalibration()
    {
        calibrator.StopCalibration();
    }


    private void Log(string message)
    {
        if (debug)
            Debug.Log(message);
    }

    private void LogOnScreen(string message)
    {
        if (debug && debugText != null)
            debugText.text = message;
    }
}
