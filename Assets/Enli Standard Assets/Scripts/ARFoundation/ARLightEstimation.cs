using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;

namespace EnliStandardAssets.XR
{
	public class ARLightEstimation : MonoBehaviour
    {
        [SerializeField] ARCameraManager arCameraManager;
        [SerializeField] private float ambientLightBaseIntensity = 1.0f;
        [Tooltip("How much intensity will be added to base intensity when max brightness value is detected")]
        [SerializeField] private float lightEstimationWeight = 0.6f;
        public bool UpdateAmbientLight = true;
        public bool UpdateLightComponent = false;
        [SerializeField] private Light m_Light;

        [Header("Debug")]
        public Text lightIntensityDebug;
		public Text estimationSliderText;
		public Text ambientSliderText;

        /// <summary>
        /// The estimated brightness of the physical environment, if available. Value interval on iOS is 0-1
        /// </summary>
        public float? Brightness { get; private set; }

        /// <summary>
        /// The estimated color temperature of the physical environment, if available.
        /// </summary>
        public float? ColorTemperature { get; private set; }

        /// <summary>
        /// The estimated color correction value of the physical environment, if available.
        /// </summary>
        public Color? ColorCorrection { get; private set; }


        public void Awake()
        {
            if (m_Light == null)
                m_Light = GetComponent<Light>();
        }

        void OnEnable()
        {
            arCameraManager.frameReceived += FrameChanged;
        }

        void OnDisable()
        {
            arCameraManager.frameReceived -= FrameChanged;
        }

        public void Update()
		{
            if (lightIntensityDebug != null)
			    lightIntensityDebug.text = RenderSettings.ambientLight.r.ToString();
		}

        void FrameChanged(ARCameraFrameEventArgs args)
        {
            if (args.lightEstimation.averageBrightness.HasValue)
            {
                Brightness = args.lightEstimation.averageBrightness.Value;
            }

            if (args.lightEstimation.averageColorTemperature.HasValue)
            {
                ColorTemperature = args.lightEstimation.averageColorTemperature.Value;
            }

            if (args.lightEstimation.colorCorrection.HasValue)
            {
                ColorCorrection = args.lightEstimation.colorCorrection.Value;
            }

            if (UpdateAmbientLight)
                SetAmbientLight();
            if (UpdateLightComponent)
                SetLightComponent();
        }

        private void SetLightComponent()
        {
            if (m_Light == null)
                return;

            if (Brightness.HasValue)
            {
                m_Light.intensity = Brightness.Value;
            }

            if (ColorTemperature.HasValue)
            {
                m_Light.colorTemperature = ColorTemperature.Value;
            }

            if (ColorCorrection.HasValue)
            {
                m_Light.color = ColorCorrection.Value;
            }
        }

        private void SetAmbientLight()
        {
            if (Brightness.HasValue)
            {
                float ambientIntensity = ambientLightBaseIntensity + ((Brightness.Value - 0.5f) * lightEstimationWeight);
                RenderSettings.ambientLight = new Color(ambientIntensity, ambientIntensity, ambientIntensity);
            }
        }

		public void SetEstimationWeight(float value)
		{
			lightEstimationWeight = value;
            if (estimationSliderText != null)
                estimationSliderText.text = value.ToString();
		}

		public void SetAmbientLightBaseIntensity(float value)
		{
			ambientLightBaseIntensity = value;
            if (ambientSliderText != null)
			    ambientSliderText.text = value.ToString();
		}
    }
}
