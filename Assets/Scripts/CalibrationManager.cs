using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class CalibrationManager : MonoBehaviour
{
    [SerializeField] private ARSessionOrigin arSessionOrigin;
    [SerializeField] private Calibrator calibrator;
    [SerializeField] private CalibrationReferenceLibrary calibrationReferenceLibrary;
    [SerializeField] private ARTrackedImageManager trackedImageManager;
    [SerializeField] private float angleErrorThreshold = 1f;

    [Header("Debug")]
    public bool debug;
    public Text debugText;
    public ARTrackedImage debugTrackedImage;
    public Transform debugCalibrationReference;

    private Transform cameraTransform;
    private Transform calibrationReference;
    private ARTrackedImage trackedImage;

    void Start()
    {
        cameraTransform = arSessionOrigin.camera.transform;
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

    private IEnumerator Control()
    {
        // To give time to Calibrator to run its Start function
        yield return new WaitForSeconds(0.2f);

        float minUpdateCycle = 1f;
        float lastUpdateTime = Time.time - minUpdateCycle;

        while (true)
        {
            trackedImage = GetBestTrackingImage();

            if (trackedImage == null)
            {
                calibrationReference = null;
                LogOnScreen(string.Format("No suitable image tracked"));
                yield return null;
            }
            else
            {
                string screenLogMessage = "";

                calibrationReference = calibrationReferenceLibrary.GetCalibrationPoint(trackedImage);

                float angle = Mathf.Abs(calibrator.GetErrorAngle(trackedImage, calibrationReference));
                if (angle > angleErrorThreshold)
                {
                    screenLogMessage += string.Format("Error angle: {0}, kalibrujem pre {1}", angle, trackedImage.referenceImage.name);
                    LogOnScreen(screenLogMessage);

                    Calibrate();

                    yield return new WaitForSeconds(2);
                }
                else
                {
                    screenLogMessage += string.Format("Error angle: {0} pri {1}", angle, trackedImage.referenceImage.name);
                    LogOnScreen(screenLogMessage);

                    yield return new WaitForSeconds(minUpdateCycle);
                }
            }
        }
    }

    private ARTrackedImage GetBestTrackingImage()
    {
        TrackableCollection<ARTrackedImage> trackables = trackedImageManager.trackables;
        ARTrackedImage winner = null;
        Vector3 winningDistance = Vector3.one * 1000;

        foreach (ARTrackedImage trackedImage in trackables)
        {
            if (trackedImage.trackingState != TrackingState.Tracking)
                continue;

            Vector3 screenPoint = arSessionOrigin.camera.WorldToViewportPoint(trackedImage.transform.position);
            bool onScreen = screenPoint.z > 0 && screenPoint.x > 0 && screenPoint.x < 1 && screenPoint.y > 0 && screenPoint.y < 1;

            screenPoint.z = 0;
            if (onScreen && screenPoint.sqrMagnitude < winningDistance.sqrMagnitude)
            {
                winner = trackedImage;
                winningDistance = screenPoint;
            }
        }

        return winner;
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
