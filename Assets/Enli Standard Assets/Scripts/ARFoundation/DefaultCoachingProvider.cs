using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace EnliStandardAssets.XR
{
    public class DefaultCoachingProvider : MonoBehaviour, ICoachingProvider
    {
        public Graphic animatedGraphics;
        public Animation hintAnimation;
        public Image deviceImage;
        public Sprite tabletHintImage;

        public bool IsSupported => true;
        public bool IsCoachingActive { get; private set; }

        void Start()
        {
            if (!IsCoachingActive)
                HideHint();

            bool isTablet = Helper.DeviceDiagonalSizeInInches() > 6.5f;
            if (isTablet)
                deviceImage.sprite = tabletHintImage;
        }

        public void ShowHint()
        {
            animatedGraphics.enabled = true;
            if (hintAnimation != null)
                hintAnimation.Play();

            IsCoachingActive = true;
        }

        public void HideHint()
        {
            animatedGraphics.enabled = false;
            if (hintAnimation != null)
                hintAnimation.Stop();

            IsCoachingActive = false;
        }
    }
}