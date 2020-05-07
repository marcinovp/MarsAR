using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class NastavovacKameryWindow : EditorWindow
{
    private CalibrationManager calibrationManager;
    private Calibrator calibrator;
    private Debugging debugging;
    private CustomTestCalibration tests;

    private List<ARSessionOrigin> arOrigins;
    private List<EditorBuildSettingsScene> videoScenes;

    [MenuItem("Window/Mars/Nastavovac kamery")]
    public static void ShowWindow()
    {
        NastavovacKameryWindow wnd = GetWindow<NastavovacKameryWindow>();
        wnd.titleContent = new GUIContent("Nastavovac kamery");
    }

    public void OnEnable()
    {
        FindScripts();
        Debug.Log("On Enable");
    }

    private void OnGUI()
    {
        float objectFieldLabelMaxWidth = 150;
        float objectFieldMaxWidth = 350;
        float sceneLabelMaxWidth = 100;

        GUIStyle styleLabelInactive = new GUIStyle(EditorStyles.label);
        GUIStyle styleLabelActive = new GUIStyle(EditorStyles.label);
        styleLabelActive.normal.textColor = Color.red;

        EditorGUILayout.Space(20);

        if (GUILayout.Button("Refresh"))
        {
            FindScripts();
        }

        EditorGUILayout.Space(20);

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.Space(5);   // padding

        EditorGUILayout.BeginVertical();

        for (int i = 0; i < arOrigins.Count; i++)
        {
            var arOrigin = arOrigins[i];
            var videoScene = videoScenes[i];

            if (arOrigin != null)
            {
                EditorGUILayout.BeginHorizontal();

                EditorGUILayout.LabelField(arOrigin.name, arOrigin.gameObject.activeSelf ? styleLabelActive : styleLabelInactive, GUILayout.MaxWidth(objectFieldLabelMaxWidth));
                EditorGUILayout.ObjectField(arOrigin, typeof(ARSessionOrigin), true, GUILayout.MaxWidth(objectFieldMaxWidth));
                EditorGUILayout.LabelField(GetSceneName(videoScene.path), videoScene.enabled ? styleLabelActive : styleLabelInactive, GUILayout.MaxWidth(sceneLabelMaxWidth));

                if (GUILayout.Button(string.Format("Set this camera")))
                {
                    SetActiveCamera(i);
                }
                EditorGUILayout.EndHorizontal();
            }
        }
        EditorGUILayout.EndVertical();
        EditorGUILayout.Space(30);  // column separator

        EditorGUILayout.BeginVertical();
        EditorGUILayout.ObjectField("Calibration manager", calibrationManager, typeof(CalibrationManager), true, GUILayout.MaxWidth(objectFieldMaxWidth));
        EditorGUILayout.ObjectField("Calibrator", calibrator, typeof(Calibrator), true, GUILayout.MaxWidth(objectFieldMaxWidth));
        EditorGUILayout.ObjectField("Debugging", debugging, typeof(Debugging), true, GUILayout.MaxWidth(objectFieldMaxWidth));
        EditorGUILayout.ObjectField("Tests", tests, typeof(CustomTestCalibration), true, GUILayout.MaxWidth(objectFieldMaxWidth));



        EditorGUILayout.EndVertical();
        EditorGUILayout.Space(5);   // padding

        EditorGUILayout.EndHorizontal();
    }

    private void SetActiveCamera(int index)
    {
        Undo.IncrementCurrentGroup();
        Undo.SetCurrentGroupName(string.Format("Set active camera: {0}", index));
        var undoGroupIndex = Undo.GetCurrentGroup();

        for (int i = 0; i < arOrigins.Count; i++)
        {
            var scene = videoScenes[i];
            scene.enabled = i == index;

            var arOrigin = arOrigins[i];
            Undo.RecordObject(arOrigin.gameObject, "");
            arOrigin.gameObject.SetActive(i == index);
        }
        ApplySceneSettings();

        var activeAROrigin = arOrigins[index];
        var cameraMovementDriver = activeAROrigin.GetComponent<CameraMovementDriver>();

        Undo.RecordObject(calibrationManager, "");
        var calibManagerSerializedObj = new SerializedObject(calibrationManager);
        calibManagerSerializedObj.FindProperty("arSessionOrigin").objectReferenceValue = activeAROrigin;
        calibManagerSerializedObj.FindProperty("trackedImageManager").objectReferenceValue = activeAROrigin.GetComponent<ARTrackedImageManager>();
        calibManagerSerializedObj.ApplyModifiedProperties();

        Undo.RecordObject(calibrator, "");
        var calibratorSerializedObj = new SerializedObject(calibrator);
        calibratorSerializedObj.FindProperty("arSessionOrigin").objectReferenceValue = activeAROrigin;
        calibratorSerializedObj.FindProperty("cameraMover").objectReferenceValue = cameraMovementDriver;
        calibratorSerializedObj.ApplyModifiedProperties();

        Undo.RecordObject(debugging, "");
        var debuggingSerializedObj = new SerializedObject(debugging);
        debuggingSerializedObj.FindProperty("arSessionOrigin").objectReferenceValue = activeAROrigin;
        debuggingSerializedObj.FindProperty("poseDriver").objectReferenceValue = activeAROrigin.camera.GetComponent<ARPoseDriverExtended>();
        debuggingSerializedObj.FindProperty("cameraMovementDriver").objectReferenceValue = cameraMovementDriver;
        debuggingSerializedObj.ApplyModifiedProperties();

        Undo.RecordObject(tests, "");
        var testsSerializedObj = new SerializedObject(tests);
        testsSerializedObj.FindProperty("arSessionOrigin").objectReferenceValue = activeAROrigin.transform;
        testsSerializedObj.FindProperty("arCamera").objectReferenceValue = activeAROrigin.camera.transform;
        testsSerializedObj.FindProperty("fakeTarget").objectReferenceValue = activeAROrigin.camera.GetComponentInChildren<ARTrackedImage>();
        testsSerializedObj.ApplyModifiedProperties();

        Undo.CollapseUndoOperations(undoGroupIndex);
    }

    private void FindScripts()
    {
        arOrigins = null;

        calibrationManager = FindObjectOfType<CalibrationManager>();
        calibrator = FindObjectOfType<Calibrator>();
        debugging = FindObjectOfType<Debugging>();
        tests = FindObjectOfType<CustomTestCalibration>();

        arOrigins = GetAllObjectsOnlyInScene<ARSessionOrigin>();
        arOrigins.Sort((a, b) => a.name.CompareTo(b.name));

        videoScenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        videoScenes.RemoveAll((sceneObject) => !GetSceneName(sceneObject.path).Contains("Video"));
    }

    private string GetSceneName(string path)
    {
        return Path.GetFileNameWithoutExtension(path);
    }

    private List<T> GetAllObjectsOnlyInScene<T>() where T : Component
    {
        List<T> objectsInScene = new List<T>();

        foreach (T go in Resources.FindObjectsOfTypeAll<T>())
        {
            if (!EditorUtility.IsPersistent(go.transform.root.gameObject) && !(go.hideFlags == HideFlags.NotEditable || go.hideFlags == HideFlags.HideAndDontSave))
                objectsInScene.Add(go);
        }

        return objectsInScene;
    }

    private void ApplySceneSettings()
    {
        List<EditorBuildSettingsScene> editorBuildSettingsScenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        foreach (var sceneAsset in editorBuildSettingsScenes)
        {
            var updatedAsset = videoScenes.Find((x) => x.guid == sceneAsset.guid);

            if (updatedAsset != null)
                sceneAsset.enabled = updatedAsset.enabled;
        }

        EditorBuildSettings.scenes = editorBuildSettingsScenes.ToArray();
    }
}