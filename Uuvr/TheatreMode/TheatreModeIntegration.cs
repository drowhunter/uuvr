#if CPP
using System;
#endif
using System.Collections.Generic;
using UnityEngine;
using VrCameraClass = Uuvr.VrCamera.VrCamera;

namespace Uuvr.TheatreMode;

/// <summary>
/// Central integration component.  
/// Hooks into UUVR's camera detection to route each camera to either Immersive VR mode or
/// Theatre Mode depending on <see cref="CameraModeRouter"/>.
/// </summary>
public class TheatreModeIntegration : MonoBehaviour
{
#if CPP
    public TheatreModeIntegration(IntPtr pointer) : base(pointer) { }
#endif

    public static TheatreModeIntegration? Instance { get; private set; }

    private CameraModeRouter    _router  = null!;
    private TheatreScreenManager? _screen;

    // Track which flat cameras already have a TheatreCapture component.
    private readonly HashSet<Camera> _capturedCameras = new();

    private void Awake()
    {
        Instance = this;
        _router  = new CameraModeRouter();

        LoadConfiguredPatterns();
    }

    private void Start()
    {
        _screen = gameObject.AddComponent<TheatreScreenManager>();
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    // ── Configuration ────────────────────────────────────────────────────────

    private void LoadConfiguredPatterns()
    {
        var patterns = ModConfiguration.Instance.TheatreImmersiveCameras.Value;
        if (string.IsNullOrWhiteSpace(patterns)) return;

        foreach (var p in patterns.Split('/'))
        {
            var trimmed = p.Trim();
            if (!string.IsNullOrEmpty(trimmed))
                _router.AddImmersiveCamera(trimmed);
        }
    }

    // ── Public API ───────────────────────────────────────────────────────────

    public CameraModeRouter     Router => _router;
    public TheatreScreenManager? Screen => _screen;

    // ── Per-frame camera routing ─────────────────────────────────────────────

    private void Update()
    {
        AttachToHighestDepthVrCamera();
    }

    /// <summary>
    /// Called by <see cref="Uuvr.VrCamera.VrCameraManager"/> every frame for every camera
    /// that UUVR is aware of.
    /// </summary>
    public void OnCameraRender(Camera cam)
    {
        if (_router.IsImmersive(cam))
            SwitchToImmersiveVR(cam);
        else
            SwitchToTheatreMode(cam);
    }

    // ── Mode switching ────────────────────────────────────────────────────────

    private void SwitchToImmersiveVR(Camera cam)
    {
        // Hide the theatre screen while an immersive camera is active.
        _screen?.Hide();

        // Stereo VR cameras are already enabled by UUVR's normal pipeline;
        // nothing extra is needed for immersive cameras.
    }

    private void SwitchToTheatreMode(Camera cam)
    {
        // Ensure a TheatreCapture component is attached to this flat camera.
        EnsureCapture(cam);

        // Show the theatre screen.
        _screen?.Show();

        // Point the screen capture at the shared RenderTexture.
        if (_screen != null && cam.TryGetComponent<TheatreCapture>(out var capture))
        {
            var rt = _screen.GetRenderTexture();
            if (rt != null)
            {
                capture.SetTheatreRenderTexture(rt);
                _screen.SetTexture(rt);
            }
        }
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private void EnsureCapture(Camera cam)
    {
        if (_capturedCameras.Contains(cam)) return;
        if (!cam.TryGetComponent<TheatreCapture>(out _))
            cam.gameObject.AddComponent<TheatreCapture>();
        _capturedCameras.Add(cam);
    }

    private void AttachToHighestDepthVrCamera()
    {
        if (_screen == null) return;
        if (VrCameraClass.HighestDepthVrCamera?.ParentCamera == null) return;

        var vrCamTransform = VrCameraClass.HighestDepthVrCamera.ParentCamera.transform;
        if (_screen.transform.parent != vrCamTransform)
            _screen.AttachToCamera(vrCamTransform);
    }
}
