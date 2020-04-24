using UnityEngine;

namespace EnliStandardAssets
{
    public static class ExtensionsForInterfaces
    {
        public static void GetInterface<T>(this MonoBehaviour extractFrom, out T interfaceObject) where T : class
        {
            interfaceObject = extractFrom as T;
            if (interfaceObject == null)
            {
                Debug.LogError(string.Format("Object '{0}' must implement interface '{1}'", extractFrom.name, typeof(T).Name));
            }
        }

        public static void GetInterface<T>(this GameObject extractFrom, out T interfaceObject) where T : class
        {
            interfaceObject = extractFrom.GetComponent<T>();
            if (interfaceObject == null)
            {
                Debug.LogError(string.Format("Object '{0}' must have a script that implements interface '{1}'", extractFrom.name, typeof(T).Name));
            }
        }
    }
}