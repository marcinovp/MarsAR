using UnityEngine;
using UnityEngine.UI;

namespace UnityEditor.UI
{
    [CustomEditor(typeof(CanvasScalerExtended))]
    [CanEditMultipleObjects]
    public class CanvasScalerExtendedEditor : CanvasScalerEditor
    {
        SerializedProperty m_MultiplierByScreenSize;
        SerializedProperty m_DpiMonitor;
        SerializedProperty m_Multiplier;

        protected override void OnEnable()
        {
            base.OnEnable();

            m_MultiplierByScreenSize = serializedObject.FindProperty("multiplierByScreenSize");
            m_DpiMonitor = serializedObject.FindProperty("monitorDPI");
            m_Multiplier = serializedObject.FindProperty("multiplier");
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            EditorGUILayout.Space();
            serializedObject.Update();

            EditorGUILayout.PropertyField(m_MultiplierByScreenSize);
            EditorGUILayout.PropertyField(m_DpiMonitor);
            EditorGUILayout.PropertyField(m_Multiplier);

            serializedObject.ApplyModifiedProperties();
        }
    }
}