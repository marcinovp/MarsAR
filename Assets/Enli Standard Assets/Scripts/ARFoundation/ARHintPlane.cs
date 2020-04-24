using UnityEngine;

namespace EnliStandardAssets.XR
{
    public class ARHintPlane : MonoBehaviour, IARHintPlane
    {
        [SerializeField] private bool visible = true;

        private MeshRenderer mesh;

        private void Awake()
        {
            mesh = GetComponent<MeshRenderer>();

            SetPlaneVisible(visible);
        }

        public void SetPlaneVisible(bool visible)
        {
            this.visible = visible;
            mesh.enabled = visible;

            gameObject.SetActive(visible);
        }
    }
}