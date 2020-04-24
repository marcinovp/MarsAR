using System;
using UnityEngine;

namespace EnliStandardAssets.XR
{
    public interface IARSystemHandler
    {
        void ActivateARKit();
        void DeactivateARKit();
        Vector3 GetArCameraGroundPosition();
        Quaternion GetArCameraRotation();
        void SetARCameraActive(bool value);
        void SetArCameraOrientation(float heading);
        void SetArCameraPosition(Vector3 position);
        bool? IsARSupported();
        bool IsARActive { get; }

        event Action<bool?> ARSupportStatusChanged;
        event Action<bool> ARActivationStateChanged;
    }
}