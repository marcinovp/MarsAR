using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EnliStandardAssets.XR
{
    public class OcclusionPlane : MonoBehaviour, IOcclusionPlane
    {
        [SerializeField] private bool visible = true;
        [SerializeField] private Material occlusionMaterial;
        [SerializeField] private Material nonOcclusionMaterial;
        [SerializeField] private bool occlude = true;

        private MeshRenderer mesh;
        private LineRenderer lineRenderer;
        private float lineWidth;

        private void Awake()
        {
            mesh = GetComponent<MeshRenderer>();
            lineRenderer = GetComponent<LineRenderer>();
            lineWidth = lineRenderer.startWidth;

            SetOcclusionActive(occlude);
            SetPlaneVisible(visible);
        }

        public void SetPlaneVisible(bool visible)
        {
            lineRenderer.startWidth = visible ? lineWidth : 0;
            lineRenderer.endWidth = visible ? lineWidth : 0;
            //lineRenderer.enabled = active;  //toto nefunguje
            //Destroy(lineRenderer);
            this.visible = visible;
        }

        public void SetOcclusionActive(bool active)
        {
            if (active)
            {
                mesh.material = occlusionMaterial;
            }
            else
            {
                mesh.material = nonOcclusionMaterial;
            }
            occlude = active;
        }
    }
}