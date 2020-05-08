using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace EnliStandardAssets
{
    public class SceneLoader : MonoBehaviour, ISceneLoader
    {
        public MonoBehaviour LoadingScreen;
        public bool useInterloader = true;
        public bool verbose = false;

        [Header("WebGL")]
        [SerializeField] private bool disableLoadingScreen = true;
        [SerializeField] private bool disableInterloader = true;

        private string currentSceneName;
        private ILoadingScreen loadingScreen;
        private AsyncOperation async = null; // When assigned, load is in progress.
        private List<AsyncOperation> loadingOperations = new List<AsyncOperation>();
        private Coroutine checkLoadingEndedCoroutine;

        public const string INTERLOADER_SCENE_NAME = "Interloader";
        private const string SCENE_TO_LOAD = "scene_to_load";

        private void Awake()
        {
            currentSceneName = SceneManager.GetActiveScene().name.ToLower();

            if (LoadingScreen != null)
                LoadingScreen.GetInterface(out loadingScreen);

#if UNITY_WEBGL
            if (disableLoadingScreen)
                loadingScreen = null;   //dont use loading screen in webgl
            if (disableInterloader)
                useInterloader = false;
#endif
        }

        void Start()
        {
            if (currentSceneName.Equals(INTERLOADER_SCENE_NAME.ToLower()))
            {
                if (loadingScreen != null)
                {
                    loadingScreen.SetProgress(0);
                }

                string sceneToLoad = RuntimeStorage.GetValueString(SCENE_TO_LOAD, string.Empty);

                if (!string.IsNullOrEmpty(sceneToLoad))
                    LoadScene(sceneToLoad, LoadSceneMode.Single);
            }
            else
            {
#if UNITY_WEBGL
            Application.ExternalCall("closeLoader");
#endif
            }
        }

        public void LoadScene(string sceneName)
        {
            LoadScene(sceneName, LoadSceneMode.Single);
        }

        public void LoadScene(string sceneName, LoadSceneMode loadSceneMode)
        {
            if (verbose)
                Debug.Log("Section loader: Loadujem " + sceneName);

            RuntimeStorage.SetValue(SCENE_TO_LOAD, sceneName);

            if (currentSceneName.Equals(INTERLOADER_SCENE_NAME.ToLower()))
            {
                //Debug.Log("Section loader: async ");
                async = LoadSceneInternal(sceneName);
                StartCoroutine(ReportProgress(async));
                //Debug.Log("Section loader: po async ");
            }
            else
            {
                if (loadSceneMode == LoadSceneMode.Additive)
                {
                    if (!IsSceneLoaded(sceneName))
                    {
                        if (loadingScreen != null)
                            loadingScreen.ShowLoading(true, null);
                        LoadSceneInternal(sceneName, loadSceneMode);
                    }
                }
                else
                {
                    if (useInterloader)
                    {
                        //Debug.Log("Section loader: load interloader ");
                        if (loadingScreen != null)
                            loadingScreen.ShowLoading(true, () => LoadSceneInternal(INTERLOADER_SCENE_NAME));
                        else
                            LoadSceneInternal(INTERLOADER_SCENE_NAME);  //loadne prazdnu scenu
                                                                                  //Debug.Log("Section loader: po load interloader ");
                    }
                    else
                    {
                        if (loadingScreen != null)
                            loadingScreen.ShowLoading(true, delegate
                            {
                                async = LoadSceneInternal(sceneName);
                                StartCoroutine(ReportProgress(async));
                            });
                        else
                            async = LoadSceneInternal(sceneName);
                    }
                }
            }
        }

        private AsyncOperation LoadSceneInternal(string sceneName, LoadSceneMode loadSceneMode = LoadSceneMode.Single)
        {
            AsyncOperation async = SceneManager.LoadSceneAsync(sceneName, loadSceneMode);
            loadingOperations.Add(async);
            async.completed += (asyncEnded) => OnLoadingEnded(asyncEnded);
            
            return async;
        }

        private IEnumerator ReportProgress(AsyncOperation asyncOperation)
        {
            float progress = 0f;
            float lastProgressState = -1f;

            do
            {
                progress = async.progress;
                if (progress - lastProgressState >= 0.01f)
                {
                    if (Application.platform != RuntimePlatform.WebGLPlayer)
                        loadingScreen.SetProgress(progress);

                    lastProgressState = progress;
                    if (verbose)
                        Debug.Log("Loading progress: " + progress);
                }

                yield return null;
            } while (progress < 1.0f);
        }

        public bool IsSceneLoaded(string sceneName)
        {
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                Scene scene = SceneManager.GetSceneAt(i);
                if (scene.name.Equals(sceneName))
                    return true;
            }
            return false;
        }

        private void OnLoadingEnded(AsyncOperation asyncEnded)
        {
            loadingOperations.Remove(asyncEnded);
            
            if (loadingOperations.Count == 0 && loadingScreen != null && !loadingScreen.Equals(null))
            {
                loadingScreen.HideLoading(true, null);
            }
        }
        
        public static string GetSceneNameFromBuildIndex(int index)
        {
            string scenePath = SceneUtility.GetScenePathByBuildIndex(index);
            string sceneName = System.IO.Path.GetFileNameWithoutExtension(scenePath);

            return sceneName;
        }
    }
}