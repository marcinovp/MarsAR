namespace UnityEngine.UI
{
    public class CanvasScalerExtended : CanvasScaler
    {
        [SerializeField] private AnimationCurve multiplierByScreenSize;
        [SerializeField] private float monitorDPI = 95.7f;
        [SerializeField] private float multiplier = 1;

        protected override void Start()
        {
            base.Start();

            float dpi = Application.isEditor ? monitorDPI : Screen.dpi;
            float screenSize = ScreenSize(dpi);
            multiplier = multiplierByScreenSize.Evaluate(screenSize);

            //Debug.Log("Resolution: " + new Vector2(Screen.width, Screen.height) + ", DPI: " + dpi + ", Screen size: " + screenSize + " inch. Multiplier: " + multiplier);
        }

        protected override void HandleConstantPhysicalSize()
        {
            float currentDpi = Screen.dpi;
            float dpi = (currentDpi == 0 ? m_FallbackScreenDPI : currentDpi);
            float targetDPI = 1;

#if (UNITY_EDITOR)
            targetDPI = monitorDPI;
#endif

            switch (m_PhysicalUnit)
            {
                case Unit.Centimeters: targetDPI = 2.54f; break;
                case Unit.Millimeters: targetDPI = 25.4f; break;
                case Unit.Inches: targetDPI = 1; break;
                case Unit.Points: targetDPI = 72; break;
                case Unit.Picas: targetDPI = 6; break;
            }
            
            SetScaleFactor(multiplier * dpi / targetDPI);
            SetReferencePixelsPerUnit(m_ReferencePixelsPerUnit * targetDPI / m_DefaultSpriteDPI);
        }

        private float ScreenSize(float screenDPI)
        {
            Vector2 aaa = new Vector2(Screen.width, Screen.height);
            float screenDiagonal = Mathf.Sqrt(Screen.width * Screen.width + Screen.height * Screen.height);

            return screenDiagonal / screenDPI;
        }
    }
}