using UnityEngine;
using UnityEngine.SceneManagement;

namespace EnliStandardAssets
{
    public class AdditiveSceneLoad : MonoBehaviour
    {
        [SerializeField] private MonoBehaviour SceneLoader;
        [SerializeField] private MonoBehaviour LoadingScreen;
        [SerializeField] private string[] additiveScenes;
        [SerializeField] private int[] additiveScenesIndex;

        private ISceneLoader sceneLoader;

        void Start()
        {
            SceneLoader.GetInterface(out sceneLoader);

            if (LoadingScreen != null)
            {
                (LoadingScreen as ILoadingScreen).ShowLoading(false, null);
            }

            foreach (string sceneName in additiveScenes)
            {
                sceneLoader.LoadScene(sceneName, UnityEngine.SceneManagement.LoadSceneMode.Additive);
            }

            foreach (int sceneIndex in additiveScenesIndex)
            {
                string sceneName = GetSceneNameFromBuildIndex(sceneIndex);
                sceneLoader.LoadScene(sceneName, UnityEngine.SceneManagement.LoadSceneMode.Additive);
            }
        }

        private string GetSceneNameFromBuildIndex(int index)
        {
            string scenePath = SceneUtility.GetScenePathByBuildIndex(index);
            string sceneName = System.IO.Path.GetFileNameWithoutExtension(scenePath);

            return sceneName;
        }
    }
}