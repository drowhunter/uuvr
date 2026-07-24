#if CPP
using System;
#endif
using UnityEngine;

namespace Uuvr.TheatreMode;

/// <summary>
/// Attach to a flat (non-VR) camera.  
/// Intercepts <c>OnRenderImage</c> to copy the final rendered frame into the Theatre Mode
/// <see cref="RenderTexture"/> while still passing it through normally so the game display
/// is unaffected.
/// </summary>
public class TheatreCapture : MonoBehaviour
{
#if CPP
    public TheatreCapture(IntPtr pointer) : base(pointer) { }
#endif

    private RenderTexture? _theatreRt;

    // ── Public API ──────────────────────────────────────────────────────────

    /// <summary>Sets the destination RenderTexture that receives the blit.</summary>
    public void SetTheatreRenderTexture(RenderTexture rt)
    {
        _theatreRt = rt;
    }

    /// <summary>Returns the currently assigned Theatre RenderTexture, or null.</summary>
    public RenderTexture? GetTheatreRenderTexture() => _theatreRt;

    // ── Unity callbacks ─────────────────────────────────────────────────────

    // OnRenderImage is called by Unity after all effects have been applied to this camera.
    // We must always blit src → dest to preserve the camera's output, and additionally
    // blit src → _theatreRt so the Theatre Screen quad shows the same frame.
#if !CPP
    private void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        PerformBlit(src, dest);
    }
#endif
    // Note: IL2CPP builds do not support the OnRenderImage callback via the standard
    // MonoBehaviour mechanism. In IL2CPP projects a Harmony/HookGen patch must call
    // PerformBlit(src, dest) directly on the TheatreCapture component after obtaining it
    // via camera.GetComponent<TheatreCapture>().

    /// <summary>Performs the double blit (called from IL2CPP hook if needed).</summary>
    public void PerformBlit(RenderTexture src, RenderTexture dest)
    {
        Graphics.Blit(src, dest);
        if (_theatreRt != null)
            Graphics.Blit(src, _theatreRt);
    }
}
