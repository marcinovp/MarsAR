using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EnliStandardAssets
{
    public static class Helper
    {
        public static readonly float INCH_TO_CM = 2.54f;
        private static readonly float DEFUALT_DPI = 96.0f;

        public static float DeviceDiagonalSizeInInches()
        {
            float screenWidth = Screen.width / Screen.dpi;
            float screenHeight = Screen.height / Screen.dpi;
            float diagonalInches = Mathf.Sqrt(Mathf.Pow(screenWidth, 2) + Mathf.Pow(screenHeight, 2));

            //Debug.Log("Getting device inches: " + diagonalInches);

            return diagonalInches;
        }

        public static float PixelsToCentimeters(float pixels)
        {
            //Debug.Log(Screen.dpi + "-" + ((pixels / Screen.dpi) * INCH_TO_CM));
            if (Screen.dpi > 0)
                return (pixels / Screen.dpi) * INCH_TO_CM;
            else
            {
                Debug.LogWarning("Helper-PixelsToCentimeters- Screen.dpi = 0");
                return (pixels / DEFUALT_DPI) * INCH_TO_CM;
            }
        }

        public static float CentimetersToPixels(float cm)
        {
            float inch = cm / INCH_TO_CM;
            try
            {
                if (Screen.dpi > 0)
                    return inch * Screen.dpi;
                else
                    return inch * DEFUALT_DPI;
            }
            catch
            {
                Debug.LogWarning("Helper-CentimetersToPixels- Screen.dpi = 0");
                return inch * DEFUALT_DPI;
            }
        }

        /// <summary>
        /// Clamps an arbitrary angle to between the given angles. Will clamp to nearest boundary.
        /// </summary>
        public static float ClampAngle(float AngleDegrees, float MinAngleDegrees, float MaxAngleDegrees)
        {
            float MaxDelta = ClampAxis(MaxAngleDegrees - MinAngleDegrees) * 0.5f;           // 0..180
            float RangeCenter = ClampAxis(MinAngleDegrees + MaxDelta);                      // 0..360
            float DeltaFromCenter = NormalizeAxis(AngleDegrees - RangeCenter);              // -180..180

            // maybe clamp to nearest edge
            if (DeltaFromCenter > MaxDelta)
            {
                return NormalizeAxis(RangeCenter + MaxDelta);
            }
            else if (DeltaFromCenter < -MaxDelta)
            {
                return NormalizeAxis(RangeCenter - MaxDelta);
            }

            // already in range, just return it
            return NormalizeAxis(AngleDegrees);
        }

        /// <summary>
        /// Clamps an angle to the range of [0, 360]
        /// </summary>
        public static float ClampAxis(float angle)
        {
            float result = angle - Mathf.CeilToInt(angle / 360f) * 360f;
            if (result < 0)
            {
                result += 360f;
            }
            return result;
        }

        /// <summary>
        /// Clamps an angle to the range of [-180, 180]
        /// </summary>
        public static float NormalizeAxis(float angle)
        {
            if (angle >= -180f)
            {
                float result = angle - Mathf.CeilToInt((angle - 180f) / 360f) * 360f;
                return result;
            }
            else
            {
                float result = angle - Mathf.CeilToInt((angle - 180f) / 360f) * 360f;
                return result;
            }
        }
    }
}