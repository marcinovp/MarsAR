using System.Collections;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class Calibrator : MonoBehaviour
{
    [SerializeField] private ARSessionOrigin arSessionOrigin;
    [SerializeField] private CameraMovementDriver cameraMover;
    //[SerializeField] private Transform calibrationReference;
    [SerializeField] private int strategy;

    [Header("Debug")]
    public bool debug;

    private Transform cameraTransform;
    private Coroutine calibration;

    void Start()
    {
        cameraTransform = arSessionOrigin.camera.transform;
    }

    void Update()
    {
        if (debug)
        {
            //session origin forward
            Debug.DrawRay(arSessionOrigin.transform.position, arSessionOrigin.transform.forward, Color.yellow);
        }
    }

    public void Calibrate(ARTrackedImage trackedImage, Transform calibrationReference)
    {
        switch (strategy)
        {
            case 0:
                calibration = StartCoroutine(LerpToRotation(trackedImage, calibrationReference));
                break;
            case 1:
                calibration = StartCoroutine(LerpToRotation2(trackedImage, calibrationReference));
                break;
            case 2:
                calibration = StartCoroutine(LerpToRotation3(trackedImage, calibrationReference));
                break;
            default:
                break;
        }
    }

    // funguje na 100%, nulova uhlova chyba
    IEnumerator LerpToRotation(ARTrackedImage trackedImage, Transform calibrationReference)
    {
        float errorAngle = float.PositiveInfinity;
        float duration = .6f;

        for (int i = 0; i < 3; i++)
        {
            Vector3 eulerRotation = arSessionOrigin.transform.eulerAngles;
            eulerRotation.z = 0;
            arSessionOrigin.transform.eulerAngles = eulerRotation;

            Vector3 fromVector = trackedImage.transform.position - cameraTransform.position;
            Vector3 toVector = calibrationReference.position - cameraTransform.position;
            Quaternion fromRotation = Quaternion.LookRotation(fromVector);
            Quaternion toRotation = Quaternion.LookRotation(toVector);
            toRotation = toRotation * Quaternion.Inverse(fromRotation);
            Quaternion startingRotation = arSessionOrigin.transform.rotation;

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
    IEnumerator LerpToRotation2(ARTrackedImage trackedImage, Transform calibrationReference)
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

    // Kalibracia len pre yaw os
    IEnumerator LerpToRotation3(ARTrackedImage trackedImage, Transform calibrationReference)
    {
        float errorAngle = float.PositiveInfinity;
        float maxSpeed = 180f;
        bool exit = false;

        while (true)
        {
            Vector3 fromVector = trackedImage.transform.position - cameraTransform.position;
            Vector3 toVector = calibrationReference.position - cameraTransform.position;
            Vector3 projectedHFrom = Vector3.ProjectOnPlane(fromVector, Vector3.up);
            Vector3 projectedHTo = Vector3.ProjectOnPlane(toVector, Vector3.up);

            float yawDifference = Vector3.SignedAngle(projectedHFrom, projectedHTo, Vector3.up);
            float yawRotation = 0;

            //float pitchDifference = 0;
            //Vector3 vNormal = Vector3.Cross(toVector, Vector3.up);
            //if (Mathf.Abs(yawDifference) < 1)
            //{
            //    Vector3 projectedVFrom = Vector3.ProjectOnPlane(fromVector, vNormal);
            //    Vector3 projectedVTo = Vector3.ProjectOnPlane(toVector, vNormal);
            //    pitchDifference = Vector3.SignedAngle(projectedVFrom, projectedVTo, vNormal);
            //    //errorAngle = Vector3.Angle(projectedVFrom, projectedVTo);
            //}

            errorAngle = Vector3.Angle(projectedHFrom, projectedHTo);

            if (exit)
                break;

            if (errorAngle < .5f)
            {
                exit = true;
                yawRotation = yawDifference;
            }
            else
            {
                yawRotation = Mathf.Min(Mathf.Abs(yawDifference), maxSpeed * Time.deltaTime) * Mathf.Sign(yawDifference);
            }

            arSessionOrigin.transform.Rotate(0, yawRotation, 0, Space.World);

            //float pitchRotation = Mathf.Min(Mathf.Abs(pitchDifference), maxSpeed / 2 * Time.deltaTime) * Mathf.Sign(pitchDifference);
            //arSessionOrigin.transform.Rotate(vNormal, pitchRotation);
            //Debug.Log(string.Format("pitch rotation: {0}, normal: {1}", pitchRotation, vNormal));

            yield return null;
        }

        Log("Uhlova chyba: " + errorAngle);
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
