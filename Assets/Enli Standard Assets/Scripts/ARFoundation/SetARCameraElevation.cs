using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;

namespace EnliStandardAssets.XR
{
    public class SetARCameraElevation : MonoBehaviour
    {
        [SerializeField] private FloorFinder floorFinder;
        [SerializeField] private ARSessionOrigin arSessionOrigin;
        public float defaultARCameraHeight = 1.2f;

        [Header("Debugging")]
        public Text debugText;
        public Camera arCameraDebug;

        private float offset;
        public float Offset {
            get { return offset; }
            set
            {
                offset = value;
                if (floorPlane != null)
                    SetElevation(floorPlane);
            }
        }

        private ARPlane floorPlane;

        private void Start()
        {
            floorFinder.OnFloorUpdateEvent += FloorFinder_OnFloorUpdateEvent;
            SetElevation(new ARPlane());    //set elevation to defaultARCameraHeight
        }

        private void Update()
        {
            if (debugText != null)
            {
                float cameraElev = arCameraDebug ? arCameraDebug.transform.position.y : float.NaN;
                string text = string.Format("SessionOrigin: {1}, Camera elev: {0}", cameraElev, arSessionOrigin.transform.position.y);
                if (floorPlane != null)
                    text += string.Format(". Plane elevation: {0}, vis GO: {1}", floorPlane.centerInPlaneSpace.y, floorPlane.transform.position.y);
                debugText.text = text;
            }
        }

        void FloorFinder_OnFloorUpdateEvent(ARPlane updatedFloorPlane)
        {
            floorPlane = updatedFloorPlane;
            if (floorPlane != null)
            {
                SetElevation(floorPlane);
            }
        }

        private void SetElevation(ARPlane plane)
        {
            bool hasFloor = plane.size.sqrMagnitude > 0.01f;
            float elevation = hasFloor ? plane.transform.position.y : arSessionOrigin.transform.position.y - defaultARCameraHeight;
            float correction = Offset - elevation;
            //Debug.Log(string.Format("Camera elev: {3}. Plane elevation: {0} - sessionOrigin: {2} = correction: {1}", elevation, correction, arSessionOrigin.transform.position.y, arCameraDebug.transform.position.y));

            if (Mathf.Abs(correction) > 0.005)
            {
                //Vector3 original = arSessionOrigin.transform.position;
                arSessionOrigin.transform.Translate(0, correction, 0, Space.World);
                //Debug.Log(string.Format("Camera elev: {2}. Posunute - original: {0}, nove: {1}", original, arSessionOrigin.transform.position, arCameraDebug.transform.position.y));
            }
        }
    }
}