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
    public Text debugText;

    [Header("Debug")]
    public bool debug;
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

        float minUpdateCycle = 1;
        float lastUpdateTime = Time.time - minUpdateCycle;
        float angleErrorThreshold = 1f;

        while (true)
        {
            if (trackedImage == null)
                yield return null;
            else
            {
                float angle = ErrorAngle(cameraTransform.position, trackedImage.transform.position, calibrationReference.position);
                debugText.text = string.Format("Error angle: {0}", angle);

                if (angle > angleErrorThreshold)
                {
                    debugText.text = string.Format("Error angle: {0}, kalibrujem", angle);
                    Calibrate();

                    yield return new WaitForSeconds(2);
                }
                else
                {
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

    private float ErrorAngle(Vector3 cameraPosition, Vector3 trackedImagePosition, Vector3 calibrationReferencePosition)
    {
        Vector3 fromVector = trackedImagePosition - cameraPosition;
        Vector3 toVector = calibrationReferencePosition - cameraPosition;
        float errorAngle = Vector3.Angle(fromVector, toVector);

        return errorAngle;
    }

    private void Log(string message)
    {
        if (debug)
            Debug.Log(message);
    }
}
