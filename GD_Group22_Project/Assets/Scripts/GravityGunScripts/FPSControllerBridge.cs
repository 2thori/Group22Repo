using System;
using System.Reflection;
using UnityEngine;

/// <summary>
/// Bridge between PlayerController and PhysicsGunInteractionBehavior.
/// Tries to call PlayerController.SetLookEnabled(bool) when rotation events come in.
/// Works even if PlayerController doesn't expose that method (logs a warning).
/// </summary>
public class FPSControllerBridge : MonoBehaviour
{
    private PlayerController _playerController;
    private PhysicsGunInteractionBehavior _gun;
    private MethodInfo _setLookMethod;
    private FieldInfo _lookField;

    private void Start()
    {
        _playerController = FindObjectOfType<PlayerController>();
        if (_playerController == null)
        {
            Debug.LogError($"{nameof(FPSControllerBridge)} is missing {nameof(PlayerController)}", this);
            return;
        }

        // try to find SetLookEnabled(bool) method
        _setLookMethod = _playerController.GetType().GetMethod("SetLookEnabled", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (_setLookMethod == null)
        {
            // fallback to a boolean field or property named lookEnabled or LookEnabled
            _lookField = _playerController.GetType().GetField("lookEnabled", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        }

        _gun = FindObjectOfType<PhysicsGunInteractionBehavior>();
        if (_gun != null)
        {
            _gun.OnRotation.AddListener(OnRotation);
        }
        else
        {
            Debug.LogWarning($"{nameof(FPSControllerBridge)}: No PhysicsGunInteractionBehavior found in scene.", this);
        }
    }

    private void OnRotation(bool rotation)
    {
        // rotation == true means the user is rotating the grabbed object.
        // We want to disable look input when rotating.
        bool enableLook = !rotation;

        if (_setLookMethod != null)
        {
            try
            {
                _setLookMethod.Invoke(_playerController, new object[] { enableLook });
                return;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Error invoking SetLookEnabled: {e.Message}");
            }
        }

        if (_lookField != null)
        {
            try
            {
                _lookField.SetValue(_playerController, enableLook);
                return;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Error setting lookEnabled field: {e.Message}");
            }
        }

        Debug.LogWarning($"{nameof(FPSControllerBridge)}: Couldn't find a SetLookEnabled method or lookEnabled field on PlayerController. Implement SetLookEnabled(bool) in PlayerController to allow the gun to lock look.");
    }

    private void OnDestroy()
    {
        if (_gun != null)
            _gun.OnRotation.RemoveListener(OnRotation);
    }
}
