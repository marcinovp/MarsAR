using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

namespace EnliStandardAssets.XR
{
    public class ARFoundationHandler : MonoBehaviour, IARSystemHandler
    {
        public ARSession arSession;
        public ARSessionOrigin arSessionOrigin;
        public ARCameraBackground arCameraBackground;
        public bool reportARAsSupportedInEditor;

        [Header ("Debugging")]
        public UnityEngine.UI.Text debugText;

        private Camera arCamera;
        public Camera ArCamera
        {
            get
            {
                if (arCamera == null)
                    arCamera = arSessionOrigin.camera;
                return arCamera;
            }
        }

        public bool IsARActive { get; private set; }
        public event Action<bool?> ARSupportStatusChanged = delegate { };
        public event Action<bool> ARActivationStateChanged;

        private ARSessionState arState = ARSessionState.None;

        void Start()
        {
            arState = ARSession.state;
            //ARSupportStatusChanged += (obj) => Debug.Log(string.Format("Status changed: {0}", arState));
        }

        void Update()
        {
            //      if (cameraParent.activeSelf)
            //	Debug.Log("parent position: " + cameraParent.transform.position + ", LocalPosition: " + arCamera.transform.localPosition + ", actual position: " + arCamera.transform.position);

            //debugText.text = "Parent rotation: " + cameraParent.transform.eulerAngles + " + camera localrotation: " + ArCamera.transform.localEulerAngles  + " = camera rotation: " + ArCamera.transform.eulerAngles
            //	+ "\nFPS rotation: " + fpsCameraDebug.eulerAngles;

            //debugText.text = "Parent: " + cameraParent.transform.position + " + camera local: " + ArCamera.transform.localPosition  + " = camera world: " + ArCamera.transform.position
            //	+ "\nFPS position: " + fpsCameraDebug.position;
        }

        private void OnEnable()
        {
            ARSession.stateChanged += ARSession_StateChanged;
        }

        private void OnDisable()
        {
            ARSession.stateChanged -= ARSession_StateChanged;
        }

        public void ActivateARKit()
        {
            arSession.enabled = true;
            //ArCamera.enabled = true;
            ArCamera.gameObject.SetActive(true);
            IsARActive = true;
            ARActivationStateChanged?.Invoke(IsARActive);
        }

        public void DeactivateARKit()
        {
            arSession.enabled = false;
            //ArCamera.enabled = false;
            ArCamera.gameObject.SetActive(false);
            IsARActive = false;
            ARActivationStateChanged?.Invoke(IsARActive);
        }

        public void SetArCameraPosition(Vector3 position)
        {
            Vector3 translation = position - ArCamera.transform.position;
            translation.y = 0;
            arSessionOrigin.transform.Translate(translation, Space.World);

            if (debugText != null)
            {
                debugText.text = "SetArCameraPosition: parent position: " + ArCamera.transform.parent.position + ", LocalPosition: " + ArCamera.transform.localPosition
                    + ", new position should be: " + position + ", and actually is: " + ArCamera.transform.position;
            }
        }

        public void SetArCameraOrientation(float heading)
        {
            //TODO nefunguje

            /*Vector3 cameraForward = ArCamera.transform.forward;
            cameraForward.y = 0;
            cameraForward.Normalize();

            Vector3 desiredForward = Quaternion.AngleAxis(heading, Vector3.up).eulerAngles;
            float orientationOffset = Vector3.SignedAngle(desiredForward, cameraForward, Vector3.up);
            arSession.transform.Rotate(0, orientationOffset, 0, Space.World);*/

            //Debug.Log("Nakoniec: " + container.eulerAngles + ", desired bolo: " + heading);
        }

        public void SetARCameraActive(bool value)
        {
            if (value)
            {
                arCameraBackground.enabled = true;
                ArCamera.clearFlags = CameraClearFlags.SolidColor;
            }
            else
            {
                arCameraBackground.enabled = false;
                ArCamera.clearFlags = CameraClearFlags.Skybox;
            }
        }

        void ARSession_StateChanged(ARSessionStateChangedEventArgs obj)
        {
            var newState = obj.state;
            if (newState != arState)
            {
                arState = newState;
                ARSupportStatusChanged(IsARSupported());
            }
        }


        public bool? IsARSupported()
        {
            //if someone calls this before my own Start
            if (arState == ARSessionState.None)
                arState = ARSession.state;

            if (Application.isEditor)
            {
                arState = reportARAsSupportedInEditor ? ARSessionState.Ready : ARSessionState.Unsupported;
                return reportARAsSupportedInEditor;
            }

            switch (arState)
            {
                case ARSessionState.None:
                    return null;
                case ARSessionState.Unsupported:
                    return false;
                case ARSessionState.CheckingAvailability:
                    return null;
                case ARSessionState.NeedsInstall:
                    return false;
                case ARSessionState.Installing:
                    return null;
                case ARSessionState.Ready:
                    return true;
                case ARSessionState.SessionInitializing:
                    return true;
                case ARSessionState.SessionTracking:
                    return true;
                default:
                    return false;
            }
        }

        public Vector3 GetArCameraGroundPosition() { return new Vector3(ArCamera.transform.position.x, 0, ArCamera.transform.position.z); }
        public Quaternion GetArCameraRotation() { return ArCamera.transform.rotation; }
    }
}