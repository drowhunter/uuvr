using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Uuvr.TheatreMode;

/// <summary>
/// Decides whether a given camera should render in Immersive VR (IVR) or Theatre Mode.
/// Cameras whose name matches any entry in the immersive list (exact or regex) use IVR;
/// all others fall back to Theatre Mode.
/// </summary>
public class CameraModeRouter
{
    public static CameraModeRouter Instance { get; private set; } = new CameraModeRouter();

    private readonly List<string> _immersiveCameraPatterns = new List<string>();

    public CameraModeRouter()
    {
        Instance = this;
    }

    /// <summary>Returns true if the camera should render in Immersive VR mode.</summary>
    public bool IsImmersive(Camera cam)
    {
        if (cam == null) return false;
        if (_immersiveCameraPatterns.Count == 0) return false;

        var name = cam.name;
        foreach (var pattern in _immersiveCameraPatterns)
        {
            if (string.IsNullOrEmpty(pattern)) continue;

            // Try exact match first, then regex.
            if (name == pattern) return true;

            try
            {
                if (Regex.IsMatch(name, pattern)) return true;
            }
            catch (ArgumentException)
            {
                // Pattern is not valid regex — already handled by exact match above.
            }
        }

        return false;
    }

    /// <summary>Adds a camera name or regex pattern to the immersive list.</summary>
    public void AddImmersiveCamera(string pattern)
    {
        if (!string.IsNullOrEmpty(pattern) && !_immersiveCameraPatterns.Contains(pattern))
            _immersiveCameraPatterns.Add(pattern);
    }

    /// <summary>Removes a camera name or regex pattern from the immersive list.</summary>
    public void RemoveImmersiveCamera(string pattern)
    {
        _immersiveCameraPatterns.Remove(pattern);
    }

    /// <summary>Replaces the entire immersive camera list.</summary>
    public void SetImmersiveCameras(IEnumerable<string> patterns)
    {
        _immersiveCameraPatterns.Clear();
        foreach (var p in patterns)
            AddImmersiveCamera(p);
    }

    /// <summary>Returns a read-only snapshot of the current immersive patterns.</summary>
    public IReadOnlyList<string> GetImmersiveCameraPatterns() =>
        _immersiveCameraPatterns.AsReadOnly();
}
