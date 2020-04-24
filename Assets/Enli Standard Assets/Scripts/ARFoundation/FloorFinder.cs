using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

namespace EnliStandardAssets.XR
{
    public class FloorFinder : MonoBehaviour
    {
        [SerializeField] MonoBehaviour ARSystemHandler;
        [SerializeField] ARPlaneManager arPlaneManager;
        public float MinFloorArea = 0.6f;

        [Header ("Debugging")]
        public bool debug = false;
        public UnityEngine.UI.Text debugText;

        /// <summary>
        /// Fires when floor searching is activated or deactivated
        /// </summary>
        public event Action<bool> FloorSearchActiveEvent;
        public delegate void FloorUpdateDelegate(ARPlane floorPlane);
        public event FloorUpdateDelegate OnFloorUpdateEvent = delegate { };
        public ARPlane FloorARPlane { get; private set; }

        public bool HasFloor { get { return FloorARPlane != null; } }
        private bool isFloorSearchActive;
        public bool IsFloorSearchActive
        {
            get { return isFloorSearchActive; }
            private set {
                bool temp = isFloorSearchActive;
                isFloorSearchActive = value;
                if (temp != value)
                    FloorSearchActiveEvent?.Invoke(isFloorSearchActive);
            }
        }

        private IARSystemHandler arSystemHandler;

        private void Awake()
        {
            FloorSearchActiveEvent += (a) => Log("Floor searching is active: " + a.ToString());

            if (ARSystemHandler != null)
                ARSystemHandler.GetInterface(out arSystemHandler);
        }

        private void OnValidate()
        {
            if (ARSystemHandler != null)
            {
                ARSystemHandler.GetInterface(out arSystemHandler);
                if (arSystemHandler == null)
                    ARSystemHandler = null;
            }
        }

        void Start()
        {
            if (debug)
            {
                OnFloorUpdateEvent += (floorPlane) =>
                {
                    if (floorPlane != null)
                        Debug.Log(string.Format("floor updated {0}, size: {1}", floorPlane.trackableId.ToString(), floorPlane.size));
                };
            }
        }

        private void OnEnable()
        {
            arPlaneManager.planesChanged += ArPlaneManager_PlanesChanged;

            if (arSystemHandler != null)
            {
                arSystemHandler.ARActivationStateChanged += ArSystemHandler_ARActivationStateChanged;
                if (arSystemHandler.IsARActive)
                {
                    IsFloorSearchActive = true;
                }
            }
            else
                IsFloorSearchActive = true;
        }

        private void OnDisable()
        {
            arPlaneManager.planesChanged -= ArPlaneManager_PlanesChanged;
            if (arSystemHandler != null)
                arSystemHandler.ARActivationStateChanged -= ArSystemHandler_ARActivationStateChanged;

            IsFloorSearchActive = false;
        }

        private void ArSystemHandler_ARActivationStateChanged(bool isActive)
        {
            IsFloorSearchActive = isActive && isActiveAndEnabled;
        }

        void Update()
        {
            if (debug)
            {
                debugText.text = "Planes count: " + arPlaneManager.trackables.count;
                if (arPlaneManager.trackables.count > 0)
                {
                    string planes = string.Empty;
                    foreach (var plane in arPlaneManager.trackables)
                    {
                        planes += "orientation: " + plane.alignment + ", position: " + plane.center + ", size: " + plane.size +
                            ",\narea: " + (plane.size.x * plane.size.y) + "\n";
                    }
                    if (FloorARPlane != null)
                        planes += "winner area: " + (FloorARPlane.size.x * FloorARPlane.size.y) + "\n";
                    debugText.text += "\n" + planes;
                }
            }
        }

        void ArPlaneManager_PlanesChanged(ARPlanesChangedEventArgs obj)
        {
            bool wasChange = false;

            foreach (var plane in obj.added)
            {
                if (OnPlaneAdded(plane))
                {
                    FloorARPlane = plane;
                    wasChange = true;
                }
            }

            foreach (var plane in obj.updated)
            {
                if (OnPlaneUpdated(plane))
                {
                    FloorARPlane = plane;
                    wasChange = true;
                }
            }

            foreach (var plane in obj.removed)
            {
                //current floor was removed
                if (FloorARPlane != null && plane.Equals(FloorARPlane.trackableId))
                {
                    ARPlane bestPlane = OnPlaneRemoved(plane);

                    FloorARPlane = bestPlane;
                    wasChange = true;
                }
            }

            if (wasChange)
                OnFloorUpdateEvent(FloorARPlane);
        }


        bool OnPlaneAdded(ARPlane plane)
        {
            Log(string.Format("Plane added {0}, alignment: {1}", plane.trackableId.ToString(), plane.alignment));

            return IsARPlaneBetter(plane);
        }

        bool OnPlaneUpdated(ARPlane plane)
        {
            //if (debug)
            //{
                //if (FloorARPlane != null)
                //{
                //    Debug.Log(string.Format("Plane updated. ID 1.1: {0}, ID 1.2: {1}, ID 2.1: {2}, ID 2.2: {3}, Equals: {4}",
                //        FloorARPlane.trackableId.subId1, FloorARPlane.trackableId.subId2,
                //        plane.trackableId.subId1, plane.trackableId.subId2, FloorARPlane.trackableId.Equals(plane.trackableId)));
                //}
                //else
                //{
                //    Debug.Log(string.Format("Plane updated. ID 1.1: null, ID 1.2: null, ID 2.1: {0}, ID 2.2: {1}, Equals: false",
                //        plane.trackableId.subId1, plane.trackableId.subId2));
                //}
            //}

            //Current floor got updated
            if (FloorARPlane != null && FloorARPlane.trackableId.Equals(plane.trackableId))
            {
                return true;
            }

            //Another plane got updated and now is maybe better than current floor
            return IsARPlaneBetter(plane);
        }

        //TODO method not tested. But it seems that merging 2 planes updates larger plane and removes smaller which is never a floor
        /// <summary>
        /// Returns the best plane after removing <c>planeRemoved</c>.
        /// </summary>
        ARPlane OnPlaneRemoved(ARPlane planeRemoved)
        {
            Log(string.Format("Plane removed {0}", planeRemoved.trackableId.ToString()));

            //if we don't have FloorARPlane found, there is no point in searching for a new one on REMOVE event
            if (FloorARPlane == null)
                return null;

            //if the removed plane is not our FloorARPlane, we don't have to search for a new one
            if (!planeRemoved.trackableId.Equals(FloorARPlane.trackableId))
                return FloorARPlane;

            ARPlane bestPlane = null;

            foreach (var plane in arPlaneManager.trackables)
            {
                if (IsARPlaneBetter(plane))
                {
                    bestPlane = plane;
                }
            }

            return bestPlane;
        }


        private bool IsARPlaneBetter(ARPlane planeToCheck)
        {
            if (planeToCheck.alignment != PlaneAlignment.HorizontalUp)
                return false;

            float newPlaneArea = planeToCheck.size.x * planeToCheck.size.y;

            if (newPlaneArea < MinFloorArea)
                return false;

            if (FloorARPlane == null)
                return true;

            float oldPlaneArea = FloorARPlane.size.x * FloorARPlane.size.y;
            return newPlaneArea > oldPlaneArea;
        }

        /// <summary>
        /// Compares position and size of planes if they match - ID can be different and is not checked
        /// </summary>
        private bool PlanesMatch(ARPlane plane1, ARPlane plane2)
        {
            bool orientationMatch = plane1.alignment == plane2.alignment;
            bool positionsMatch = plane1.center == plane2.center;
            bool sizeMatch = plane1.size == plane2.size;
            bool normalMatch = plane1.normal == plane2.normal;

            return orientationMatch && positionsMatch && sizeMatch && normalMatch;
        }

        public void Debug_FindFakePlane()
        {
            GameObject planeGO = Instantiate(arPlaneManager.planePrefab);
            FloorARPlane = planeGO.GetComponent<ARPlane>();
            OnFloorUpdateEvent(FloorARPlane);
        }


        private void Log(string message)
        {
            if (debug)
                Debug.Log(message);
        }
    }
}