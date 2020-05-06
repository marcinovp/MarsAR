using UnityEditor;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class CalibrationReferenceLibrary : MonoBehaviour
{
    [SerializeField] private XRReferenceImageLibrary imageLibrary;
    [SerializeField] private Transform[] virtualCalibrationPoints;

    public Transform GetCalibrationPoint(ARTrackedImage trackedImage)
    {
        if (trackedImage == null)
            return null;

        int refImageIndex = imageLibrary.indexOf(trackedImage.referenceImage);

        if (refImageIndex >= 0)
            return virtualCalibrationPoints[refImageIndex];
        else
            return null;
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(CalibrationReferenceLibrary))]
public class CalibrationReferenceLibraryInspector : Editor
{
    SerializedProperty imagesLibraryProperty;
    private XRReferenceImageLibrary imageLibrary;
    private CalibrationReferenceLibrary owner;

    void OnEnable()
    {
        imagesLibraryProperty = serializedObject.FindProperty("imageLibrary");
        imageLibrary = (XRReferenceImageLibrary)imagesLibraryProperty?.objectReferenceValue;

        owner = (CalibrationReferenceLibrary)target;
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        DrawDefaultInspector();

        EditorGUILayout.Space();
        EditorGUILayout.Space();

        if (imageLibrary != null)
        {
            GUI.enabled = false;

            EditorGUILayout.LabelField("Image library items");

            int index = 0;
            foreach (var item in imageLibrary)
            {
                EditorGUILayout.LabelField(string.Format("  {0}", index), string.Format("{0}", item.name));
                index++;
            }

            GUI.enabled = true;
        }
    }
}
#endif