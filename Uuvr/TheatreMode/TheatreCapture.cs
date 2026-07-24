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
        // Preserve normal rendering path.
        Graphics.Blit(src, dest);

        // Copy to the theatre screen if a target texture has been assigned.
        if (_theatreRt != null)
            Graphics.Blit(src, _theatreRt);
    }
#endif
    // Note: IL2CPP builds hook OnRenderImage differently.
    // When building with the CPP backend, the hook is injected via
    // TheatreCaptureIl2Cpp (see TheatreModeIntegration) and delegates to
    // PerformBlit below so the logic stays in one place.

    /// <summary>Performs the double blit (called from IL2CPP hook if needed).</summary>
    public void PerformBlit(RenderTexture src, RenderTexture dest)
    {
        Graphics.Blit(src, dest);
        if (_theatreRt != null)
            Graphics.Blit(src, _theatreRt);
    }
}
