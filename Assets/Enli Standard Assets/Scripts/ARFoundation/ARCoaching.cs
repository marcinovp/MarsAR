using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

namespace EnliStandardAssets.XR
{
    public class ARCoaching : MonoBehaviour
    {
        [SerializeField] private FloorFinder floorFinder;
        [SerializeField] private bool activatesAutomatically;

        [Header("Coaching providers")]
        [SerializeField] private MonoBehaviour defaultCoachingProvider;
        [SerializeField] private MonoBehaviour arkitCoachingProvider;

        [Header("Debugging")]
        public bool debug;

        private ICoachingProvider defaultCoaching;
        private ICoachingProvider arkitCoaching;

        private ICoachingProvider activeCoaching;

        public bool IsCoachingActive { get { return activeCoaching != null && activeCoaching.IsCoachingActive; } }

        private void Awake()
        {
            defaultCoachingProvider.GetInterface(out defaultCoaching);
            arkitCoachingProvider?.GetInterface(out arkitCoaching);
        }

        void Start()
        {
            if (arkitCoaching != null && arkitCoaching.IsSupported)
            {
                Log("Coaching - set to ARKit");
                activeCoaching = arkitCoaching;
                //floorFinder.MinFloorArea = 0.1f
                //Debug.Log("ARCoaching: floorFinder.MinFloorArea must be set to low value, because arkitCoaching ends itself when any plane is found - without it, the coaching would disappear while a floor had not been found");
            }
            else
            {
                Log("Coaching - set to Default");
                activeCoaching = defaultCoaching;
            }

            if (floorFinder.IsFloorSearchActive && activatesAutomatically)
                ActivateCoaching();
            floorFinder.FloorSearchActiveEvent += FloorFinder_FloorSearchActiveEvent;
        }

        private void OnValidate()
        {
            if (defaultCoachingProvider != null)
            {
                defaultCoachingProvider.GetInterface(out defaultCoaching);
                if (defaultCoaching == null)
                    defaultCoachingProvider = null;
            }
            if (arkitCoachingProvider != null)
            {
                arkitCoachingProvider.GetInterface(out arkitCoaching);
                if (arkitCoaching == null)
                    arkitCoachingProvider = null;
            }
        }

        private void FloorFinder_FloorSearchActiveEvent(bool floorSearchingActive)
        {
            Log(string.Format("AR coaching - FloorSearchActiveEvent. Active: {0}, activates automatically: {1}", floorSearchingActive, activatesAutomatically));
            if (floorSearchingActive)
            {
                if (activatesAutomatically)
                    ActivateCoaching();
            }
            else
                DeactivateCoaching();
        }

        public void ActivateCoaching()
        {
            Log(string.Format("AR coaching - ActivateCoaching. Is coaching NOT active: {0}", !activeCoaching.IsCoachingActive));
            if (!activeCoaching.IsCoachingActive)
            {
                floorFinder.OnFloorUpdateEvent += OnFloorUpdateEvent;
                activeCoaching.ShowHint();
            }
        }

        public void DeactivateCoaching()
        {
            Log(string.Format("AR coaching - DeactivateCoaching. Is coaching active: {0}", activeCoaching.IsCoachingActive));
            if (activeCoaching.IsCoachingActive)
            {
                activeCoaching.HideHint();
            }
        }

        private void OnFloorUpdateEvent(ARPlane floorPlane)
        {
            Log("AR coaching - OnFloorUpdatedEvent - deactivating coaching");
            floorFinder.OnFloorUpdateEvent -= OnFloorUpdateEvent;
            floorFinder.FloorSearchActiveEvent -= FloorFinder_FloorSearchActiveEvent;
            DeactivateCoaching();
        }

        private void Log(string message)
        {
            if (debug)
                Debug.Log(message);
        }
    }
}