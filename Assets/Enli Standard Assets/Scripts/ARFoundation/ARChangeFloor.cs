using EnliStandardAssets.XR;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace EnliStandardAssets.XR
{
    public class ARChangeFloor : MonoBehaviour
    {
        [SerializeField] private SetARCameraElevation cameraElevationSetter;
        [SerializeField] private AnimationCurve speedCurve;
        [SerializeField] private float transitionDuration = 0.5f;
        [SerializeField] private int startingFloor = 0;
        [Tooltip("List of all floor elevations. Need to be in ascending order - Element 0 is first floor, Element 1 is second floor...")]
        [SerializeField] private float[] floorElevations;
        [Tooltip("Optional field. List of strings to name particular floors, e.g. 1NP. 2NP, Roof... If length is 0, default naming by floor number is used.")]
        public string[] floorNames;

        [Header("GUI")]
        [Tooltip("This text will show current floor")]
        [SerializeField] private Text currentFloorText;
        [SerializeField] private GameObject upArrow;
        [SerializeField] private GameObject downArrow;

        private int currentFloor = 0;
        private Coroutine transitionCoroutine;

        private void Awake()
        {
            currentFloor = startingFloor;
        }

        void Start()
        {
            ShowHideFloorGuiArrows(upArrow, downArrow, floorElevations, currentFloor);
            SetFloorNumberText(currentFloor);
        }

        public void Ascend()
        {
            if (currentFloor >= floorElevations.Length - 1)
                return;

            currentFloor++;
            ShowHideFloorGuiArrows(upArrow, downArrow, floorElevations, currentFloor);
            SetFloorNumberText(currentFloor);

            if (transitionCoroutine != null)
                StopCoroutine(transitionCoroutine);
            transitionCoroutine = StartCoroutine(Transition(floorElevations[currentFloor]));
        }

        public void Descend()
        {
            if (currentFloor <= 0)
                return;

            currentFloor--;
            ShowHideFloorGuiArrows(upArrow, downArrow, floorElevations, currentFloor);
            SetFloorNumberText(currentFloor);

            if (transitionCoroutine != null)
                StopCoroutine(transitionCoroutine);
            transitionCoroutine = StartCoroutine(Transition(floorElevations[currentFloor]));
        }

        IEnumerator Transition(float targetElevation)
        {
            float startingElevation = cameraElevationSetter.Offset;
            float timeNormalized = 0;
            float startTime = Time.time;

            if (transitionDuration > 0)
            {
                while (timeNormalized <= 0.99f)
                {
                    timeNormalized = (Time.time - startTime) / transitionDuration;
                    timeNormalized = Mathf.Clamp01(timeNormalized);
                    float value = speedCurve.Evaluate(timeNormalized);
                    cameraElevationSetter.Offset = Mathf.Lerp(startingElevation, targetElevation, value);

                    Debug.Log(string.Format("Time normalized: {0}, value: {1}, offset: {2}", timeNormalized, value, cameraElevationSetter.Offset));

                    yield return null;
                }
            }

            cameraElevationSetter.Offset = targetElevation;
            Debug.Log(string.Format("Finished transition: offset: {0}", cameraElevationSetter.Offset));
        }

        private void ShowHideFloorGuiArrows(GameObject upArrow, GameObject downArrow, System.Array floors, int currentFloorIndex)
        {
            if (upArrow != null)
                upArrow.SetActive(currentFloorIndex < floors.Length - 1);
            if (downArrow != null)
                downArrow.SetActive(currentFloorIndex > 0);
        }

        private void SetFloorNumberText(int floorIndex)
        {
            if (currentFloorText != null)
            {
                if (floorNames.Length == 0)
                    currentFloorText.text = (floorIndex + 1).ToString();
                else if (floorNames.Length > floorIndex)
                    currentFloorText.text = floorNames[floorIndex];
            }
        }
    }
}