using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EnliStandardAssets
{
    public class LoadingScreen : MonoBehaviour, ILoadingScreen
    {
        [SerializeField] private CanvasGroup screenParent;
        public float animationDuration = 0.5f;

        private Coroutine showCanvasGroupAnimCorout;

        private void Awake()
        {
            HideLoading(false, null);
        }

        public void ShowLoading(bool animate, Action onActionFinished)
        {
            if (showCanvasGroupAnimCorout != null)
                StopCoroutine(showCanvasGroupAnimCorout);

            if (animate)
            {
                showCanvasGroupAnimCorout = StartCoroutine(ShowCanvasGroupAnim(screenParent, true, animationDuration, onActionFinished));
            }
            else
            {
                screenParent.alpha = 1;
                screenParent.gameObject.SetActive(true);
            }
        }

        public void HideLoading(bool animate, Action onActionFinished)
        {
            if (showCanvasGroupAnimCorout != null)
                StopCoroutine(showCanvasGroupAnimCorout);

            if (animate)
            {
                showCanvasGroupAnimCorout = StartCoroutine(ShowCanvasGroupAnim(screenParent, false, animationDuration, onActionFinished));
            }
            else
            {
                screenParent.alpha = 0;
                screenParent.gameObject.SetActive(false);
            }
        }

        public void SetProgress(float progress)
        {
            //throw new NotImplementedException();
        }

        protected IEnumerator ShowCanvasGroupAnim(CanvasGroup canvasGroup, bool show, float duration, Action callback)
        {
            if (show)
            {
                canvasGroup.gameObject.SetActive(true);
                yield return new WaitForEndOfFrame();
            }

            float t = canvasGroup.alpha;
            float startTime = show ? Time.time - t * duration : Time.time - (1 - t) * duration;
            float endTime = show ? Time.time + (1 - t) * duration : Time.time + t * duration;

            do
            {
                if (show)
                    t = Mathf.InverseLerp(startTime, endTime, Time.time);
                else
                    t = Mathf.InverseLerp(endTime, startTime, Time.time);

                canvasGroup.alpha = Mathf.Lerp(0.0f, 1.0f, t);

                yield return null;
            }
            while ((show && t < 1.0f) || (!show && t > 0.0f));

            if (!show)
            {
                canvasGroup.gameObject.SetActive(false);
            }

            if (callback != null)
                callback.Invoke();
        }
    }
}