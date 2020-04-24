using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

namespace EnliStandardAssets.XR
{
    public class FloorVisualizationHint : MonoBehaviour
    {
        [SerializeField] FloorFinder floorFinder;
        [SerializeField] ARPlaneManager arPlaneManager;
        [SerializeField] bool disableOcclusionOnFloor;
        [Tooltip("Disable occlusion also on horizontal planes that's elevation is within this tolerance from the floor. This prevents occlusion by mistakenly detected planes couple of cm around the floor.")]
        [SerializeField] float disableOcclusionTolerance = 0.3f;
        //[SerializeField] ARPointCloudManager pointCloudManager;   //disabled - can't hide already visible points

        private bool hintsActive;
        private ARPlane currentFloorPlane;

        void Start()
        {
            SetHintsActive(true);
        }

        private void OnEnable()
        {
            arPlaneManager.planesChanged += ArPlaneManager_PlanesChanged;
            floorFinder.OnFloorUpdateEvent += FloorFinder_OnFloorUpdateEvent;
        }

        private void OnDisable()
        {
            arPlaneManager.planesChanged -= ArPlaneManager_PlanesChanged;
            floorFinder.OnFloorUpdateEvent -= FloorFinder_OnFloorUpdateEvent;
        }

        void FloorFinder_OnFloorUpdateEvent(ARPlane floorPlane)
        {
            if (currentFloorPlane == null && floorPlane != null)
            {
                SetHintsActive(false);
            }

            // Floor plane can't occlude, because objects' shadows would be occluded too (fast shadow)
            if (disableOcclusionOnFloor)
            {
                foreach (var plane in arPlaneManager.trackables)
                {
                    IOcclusionPlane occlusionPlane = plane.GetComponent<IOcclusionPlane>();
                    bool correctAlignment = plane.alignment == PlaneAlignment.HorizontalUp;
                    bool inTolerance = plane.center.y <= floorPlane.center.y + disableOcclusionTolerance && plane.center.y >= floorPlane.center.y - disableOcclusionTolerance;

                    occlusionPlane?.SetOcclusionActive(!correctAlignment || !inTolerance);
                }
            }
            currentFloorPlane = floorPlane;
        }

        //new planes found by arPlaneManager are always visible by default - if hints are disabled, we need to disable them immediately as they are added
        void ArPlaneManager_PlanesChanged(ARPlanesChangedEventArgs obj)
        {
            if (obj.added.Count > 0 && !hintsActive)
            {
                foreach (var plane in obj.added)
                {
                    SetPlaneActive(plane, false);
                }
            }
        }

        public void SetHintsActive(bool value)
        {
            //Debug.Log(string.Format("............SetHintsActive: {0}", value));
            SetPlaneDetectionActive(value);
            //SetPointsActive(value);

            hintsActive = value;
        }

        //void SetPointsActive(bool value)
        //{
        //    if (pointCloudManager != null)
        //    {
        //        pointCloudManager.enabled = value;
        //        //pointCloudManager.pointCloud?.gameObject.SetActive(value);
        //    }
        //}

        /// <summary>
        /// Toggles plane detection and the visualization of the planes.
        /// </summary>
        void SetPlaneDetectionActive(bool value)
        {
            if (arPlaneManager != null)
            {
                SetAllPlanesActive(value);
            }
        }

        /// <summary>
        /// Iterates over all the existing planes and activates
        /// or deactivates their <c>GameObject</c>s'.
        /// </summary>
        /// <param name="value">Each planes' GameObject is SetActive with this value.</param>
        void SetAllPlanesActive(bool value)
        {
            foreach (var plane in arPlaneManager.trackables)
                SetPlaneActive(plane, value);
        }

        private void SetPlaneActive(ARPlane plane, bool active)
        {
            IARHintPlane hintPlane = plane.GetComponent<IARHintPlane>();
            hintPlane.SetPlaneVisible(active);
        }
    }
}