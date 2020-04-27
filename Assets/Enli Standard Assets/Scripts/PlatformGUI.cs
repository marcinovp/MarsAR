using UnityEngine;
using System.Collections;

public class PlatformGUI : MonoBehaviour
{

    public bool takeEffectInEditor = true;
    [Tooltip("If turning off should be enforced even if the gameobject is later activated")]
    public bool forceSettings = true;

    [Header("Platforms")]
    public bool turnOffInAndroid = false;
    public bool turnOffInIOS = false;
    public bool turnOffInWebgl = false;

    private bool editorAndroid = false;
    private bool editorIos = false;
    private bool editorWebgl = false;

    void Start()
    {
        if (Application.isEditor)
        {
#if UNITY_ANDROID
            editorAndroid = true;
#elif UNITY_IOS
            editorIos = true;
#elif UNITY_WEBGL
            editorWebgl = true;
#endif
        }

        //Debug.Log("Platform: "+Application.platform);
        if (SatisfyConditions())
        {
            gameObject.SetActive(false);
        }
    }

    void OnEnable()
    {
        if (!forceSettings)
            return;

        if (SatisfyConditions())
        {
            gameObject.SetActive(false);
        }
    }

    private bool SatisfyConditions()
    {
        bool satisfyForAndroid = turnOffInAndroid && (Application.platform == RuntimePlatform.Android || (editorAndroid && takeEffectInEditor));
        bool satisfyForIOS = turnOffInIOS && (Application.platform == RuntimePlatform.IPhonePlayer || (editorIos && takeEffectInEditor));
        bool satisfyForWebgl = turnOffInWebgl && (Application.platform == RuntimePlatform.WebGLPlayer || (editorWebgl && takeEffectInEditor));

        return satisfyForAndroid || satisfyForIOS || satisfyForWebgl;
    }
}
