using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class CameraMovementDriver : MonoBehaviour
{
    [SerializeField] private bool active = true;
    [SerializeField] private ARSessionOrigin arSessionOrigin;
    [SerializeField] private Camera arCamera;
    [SerializeField] private Transform centerpointVisualisation;

    private Transform cameraTransform;
    private Transform originTransform;
    private Vector3 cameraArm;
    private Vector3 centerPoint;

    public Vector3 CenterPoint { get => centerPoint; }

    private void Awake()
    {
        cameraTransform = arCamera.transform;
        cameraArm = cameraTransform.localPosition;
        originTransform = arSessionOrigin.transform;
        centerPoint = originTransform.position;
    }

    private void Start()
    {
        cameraTransform.localPosition = Vector3.zero;
    }

#if UNITY_EDITOR
    int dir = 0;
    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.J))
            dir = -1;
        if (Input.GetKeyUp(KeyCode.K))
            dir = 0;
        if (Input.GetKeyUp(KeyCode.L))
            dir = 1;

        Rotate(Quaternion.Euler(0, dir * Time.deltaTime * 100f, 0));
    }
#endif

    private void LateUpdate()
    {
        if (!active)
            return;

        Vector3 desiredLocalPosition = cameraTransform.localRotation * cameraArm;

        Vector3 desiredCameraPosition = desiredLocalPosition + centerPoint;
        originTransform.position -= cameraTransform.position - desiredCameraPosition;

        if (centerpointVisualisation != null)
            centerpointVisualisation.position = centerPoint;
    }

    public void Rotate(Quaternion rotation)
    {
        //centerPoint = (rotation * (centerPoint - cameraTransform.position)) + cameraTransform.position;
        cameraArm = rotation * cameraArm;
        originTransform.rotation = rotation * originTransform.rotation;
    }

    public void Rotate(Vector3 rotation)
    {
        Quaternion rotQuat = Quaternion.Euler(rotation);
        //centerPoint = (rotQuat * (centerPoint - cameraTransform.position)) + cameraTransform.position;
        cameraArm = rotQuat * cameraArm;
        originTransform.Rotate(rotation, Space.World);
    }

    public void ActivateLimiter(bool value) { active = value; }
}
