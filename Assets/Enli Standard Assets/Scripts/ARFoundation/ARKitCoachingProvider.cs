using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
#if UNITY_IOS
using UnityEngine.XR.ARKit;
#endif

namespace EnliStandardAssets.XR
{
    /// <summary>
    /// This example shows how to activate the [ARCoachingOverlayView](https://developer.apple.com/documentation/arkit/arcoachingoverlayview)
    /// </summary>
    public class ARKitCoachingProvider : MonoBehaviour, ICoachingProvider
    {
        [SerializeField] private ARSession arSession;

        // Duplicate the ARCoachingGoal enum so that we can use it on a serialized field
        enum CoachingGoal
        {
            Tracking,
            HorizontalPlane,
            VerticalPlane,
            AnyPlane
        }

        [SerializeField]
        [Tooltip("The coaching goal associated with the coaching overlay.")]
#if !UNITY_IOS
#pragma warning disable CS0414
#endif
        CoachingGoal m_Goal = CoachingGoal.Tracking;
#if !UNITY_IOS
#pragma warning restore CS0414
#endif

#if UNITY_IOS
    /// <summary>
    /// The [ARCoachingGoal](https://developer.apple.com/documentation/arkit/arcoachinggoal) associated with the coaching overlay
    /// </summary>
    public ARCoachingGoal goal
    {
        get
        {
            if (arSession.subsystem is ARKitSessionSubsystem sessionSubsystem)
            {
                return sessionSubsystem.coachingGoal;
            }
            else
            {
                return (ARCoachingGoal)m_Goal;
            }
        }

        set
        {
            m_Goal = (CoachingGoal)value;
            if (IsSupported && arSession.subsystem is ARKitSessionSubsystem sessionSubsystem)
            {
                sessionSubsystem.coachingGoal = value;
            }
        }
    }
#endif

        [SerializeField]
        [Tooltip("Whether the coaching overlay activates automatically.")]
        bool m_ActivatesAutomatically = true;

        /// <summary>
        /// Whether the coaching overlay activates automatically
        /// </summary>
        public bool activatesAutomatically
        {
            get
            {
#if UNITY_IOS
            if (IsSupported && arSession.subsystem is ARKitSessionSubsystem sessionSubsystem)
            {
                return sessionSubsystem.coachingActivatesAutomatically;
            }
#endif
                return m_ActivatesAutomatically;
            }

            set
            {
                m_ActivatesAutomatically = value;

#if UNITY_IOS
            if (IsSupported && arSession.subsystem is ARKitSessionSubsystem sessionSubsystem)
            {
                sessionSubsystem.coachingActivatesAutomatically = value;
            }
#endif
            }
        }

        /// <summary>
        /// Whether the [ARCoachingGoal](https://developer.apple.com/documentation/arkit/arcoachinggoal) is supported.
        /// </summary>
        public bool IsSupported
        {
            get
            {
#if UNITY_IOS
                return ARKitSessionSubsystem.coachingOverlaySupported;
#else
                return false;
#endif
            }
        }

        public bool IsCoachingActive
        {
            get
            {
#if UNITY_IOS
                if (IsSupported && arSession.subsystem is ARKitSessionSubsystem sessionSubsystem)
                {
                    return sessionSubsystem.coachingActive;
                }
#endif
                return false;
            }
        }

        void OnEnable()
        {
#if UNITY_IOS
        if (IsSupported && arSession.subsystem is ARKitSessionSubsystem sessionSubsystem)
        {
            sessionSubsystem.coachingGoal = (ARCoachingGoal)m_Goal;
            sessionSubsystem.coachingActivatesAutomatically = m_ActivatesAutomatically;
        }
        else
#endif
            {
                Debug.LogError("ARCoachingOverlayView is not supported by this device.");
            }
        }

        public void ShowHint()
        {
#if UNITY_IOS
            //ActivateCoaching(true);
            if (arSession.subsystem is ARKitSessionSubsystem sessionSubsystem)
            {
                sessionSubsystem.coachingActivatesAutomatically = true;
            }
#endif
        }

        public void HideHint()
        {
#if UNITY_IOS
            if (arSession.subsystem is ARKitSessionSubsystem sessionSubsystem)
            {
                sessionSubsystem.coachingActivatesAutomatically = false;
            }
#endif
        }

        /// <summary>
        /// Activates the [ARCoachingGoal](https://developer.apple.com/documentation/arkit/arcoachinggoal)
        /// </summary>
        /// <param name="animated">If <c>true</c>, the coaching overlay is animated, e.g. fades in. If <c>false</c>, the coaching overlay appears instantly, without any transition.</param>
        public void ActivateCoaching(bool animated)
        {
#if UNITY_IOS
        if (IsSupported && arSession.subsystem is ARKitSessionSubsystem sessionSubsystem)
        {
            sessionSubsystem.SetCoachingActive(true, animated ? ARCoachingOverlayTransition.Animated : ARCoachingOverlayTransition.Instant);
        }
        else
#endif
            {
                throw new NotSupportedException("ARCoachingOverlay is not supported");
            }
        }
    }
}