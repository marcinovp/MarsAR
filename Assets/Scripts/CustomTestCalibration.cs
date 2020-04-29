using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomTestCalibration : MonoBehaviour
{
    public bool active;
    public bool makePauses;
    public float timeLimit = 2;
    public Calibrator calibrator;
    public Transform arSessionOrigin;
    public Transform arCamera;
    public Transform fakeTarget;

    private List<Quaternion> originRotations;
    private List<Quaternion> cameraRotations;
    private List<Quaternion> detectedTargetRelativeDirections;

    [Header("Output")]
    public int originIndex;
    public int cameraIndex;
    public int targetIndex;

    void Update()
    {
        if (!active)
            return;

        if (Input.GetKeyUp(KeyCode.T))
        {
            RunTest();
        }
    }

    public void RunTest()
    {
        originRotations = new List<Quaternion>
        {
            Quaternion.identity,
            Quaternion.Euler(-10, 20, 0),
            Quaternion.Euler(5, 220, 0),
            Quaternion.Euler(0, 90, 0)
        };

        cameraRotations = new List<Quaternion>
        {
            Quaternion.identity,
            Quaternion.Euler(10, 20, 0),
            Quaternion.Euler(23, 160, 26),
            Quaternion.Euler(19, 240, 5)
        };

        detectedTargetRelativeDirections = new List<Quaternion>
        {
            Quaternion.identity,
            Quaternion.Euler(20, 30, 0),
            Quaternion.Euler(-20, 30, 0),
            Quaternion.Euler(20, -30, 0),
            Quaternion.Euler(-20, -30, 0)
        };

        StartCoroutine(CalibrationTest1());
    }

    public IEnumerator CalibrationTest1()
    {
        float startTime = Time.time;
        int successCount = 0;
        int totalCount = 0;

        for (int k = originIndex; k < originRotations.Count; k++)
        {
            for (int j = 0; j < cameraRotations.Count; j++)
            {
                for (int i = 0; i < detectedTargetRelativeDirections.Count; i++)
                {
                    totalCount++;

                    originIndex = k;
                    cameraIndex = j;
                    targetIndex = i;

                    arSessionOrigin.rotation = originRotations[k];
                    arCamera.localRotation = cameraRotations[j];
                    SetDetectedTargetPosition(detectedTargetRelativeDirections[i]);

                    if (makePauses)
                        yield return new WaitForSeconds(1f);

                    calibrator.ProcessTrackedImageChange();

                    CoroutineWithData cd = new CoroutineWithData(this, WaitForCalibrationFinish());
                    yield return cd.coroutine;

                    if (!(bool)cd.result)
                    {
                        calibrator.StopCalibration();
                        Debug.LogError(string.Format("Fail kalibracie pri origin: {2}, camera: {1}, target relative direction: {0}", i, j, k));
                    }
                    else
                        successCount++;

                    if (makePauses)
                        yield return new WaitForSeconds(.5f);
                }
            }
        }

        Debug.Log(string.Format("CalibrationTest1 result: {0}\nTotal time: {1:0.0}\nSuccessful: {2}/{3}", 
            totalCount == successCount ? "Success" : "FAIL", Time.time - startTime, successCount, totalCount));
        yield return null;
    }

    private IEnumerator WaitForCalibrationFinish()
    {
        float startTime = Time.time;
        Vector3 lastAngle = arCamera.eulerAngles;

        while (Time.time < startTime + timeLimit)
        {
            yield return null;
            yield return null;
            yield return null;

            Vector3 currentAngle = arCamera.eulerAngles;
            if (lastAngle == currentAngle)
            {
                yield return true;
                yield break;
            }

            lastAngle = currentAngle;
        }
        yield return false;
    }

    private void SetDetectedTargetPosition(Quaternion rotation)
    {
        Quaternion originalRotation = arCamera.rotation;
        arCamera.rotation = Quaternion.identity;
        Vector3 directionVector = rotation * arCamera.forward * 1.1f;
        fakeTarget.position = directionVector;

        arCamera.rotation = originalRotation;
    }
}

public class CoroutineWithData
{
    public Coroutine coroutine { get; private set; }
    public object result;
    private IEnumerator target;
    public CoroutineWithData(MonoBehaviour owner, IEnumerator target)
    {
        this.target = target;
        this.coroutine = owner.StartCoroutine(Run());
    }

    private IEnumerator Run()
    {
        while (target.MoveNext())
        {
            result = target.Current;
            yield return result;
        }
    }
}
