using UnityEngine.SceneManagement;

namespace EnliStandardAssets
{
    interface ISceneLoader
    {
        void LoadScene(string sceneName, LoadSceneMode loadSceneMode);
    }
}