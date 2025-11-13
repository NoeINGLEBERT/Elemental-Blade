using System;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.XR;

public class InputDebugger : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("When true, prints detected button presses to the console.")]
    public bool debugLogs = true;

    [Tooltip("Seconds between polling. Lower = more responsive, higher = less spam.")]
    public float pollInterval = 0.05f;

    private float nextPoll = 0f;

    void Update()
    {
        if (!debugLogs) return;

        if (Time.unscaledTime < nextPoll) return;
        nextPoll = Time.unscaledTime + Mathf.Max(0.01f, pollInterval);

        // Iterate all devices
        foreach (var device in InputSystem.devices)
        {
            foreach (var control in device.allControls)
            {
                if (control is ButtonControl btn)
                {
                    try
                    {
                        if (btn.wasPressedThisFrame)
                        {
                            Debug.Log($"[InputDebugger] BUTTON PRESSED -> Device: '{device.displayName}' ({device.name}), Control: '{control.name}' (path: {control.path})");
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.LogWarning($"[InputDebugger] Error reading ButtonControl '{control.name}' on device '{device.name}': {e.Message}");
                    }
                }
            }
        }

        // Optional: summary of XR controllers and their button names
        var xrControllers = InputSystem.devices.OfType<XRController>();
        foreach (var xr in xrControllers)
        {
            var buttonNames = xr.allControls
                .OfType<ButtonControl>()
                .Select(c => c.name)
                .Where(n => !string.IsNullOrEmpty(n))
                .Distinct()
                .ToArray();

            if (buttonNames.Length > 0)
                Debug.Log($"[InputDebugger] XR Device: '{xr.displayName}' ({xr.name}) buttons: {string.Join(", ", buttonNames)}");
        }
    }
}
