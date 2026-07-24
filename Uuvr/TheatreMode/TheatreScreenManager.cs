#if CPP
using System;
#endif
using UnityEngine;

namespace Uuvr.TheatreMode;

/// <summary>
/// Creates and manages the flat virtual screen quad used in Theatre Mode.
/// The quad is parented to the highest-depth VR camera so it stays in front of the player.
/// </summary>
public class TheatreScreenManager : MonoBehaviour
{
#if CPP
    public TheatreScreenManager(IntPtr pointer) : base(pointer) { }
#endif

    private GameObject? _screenQuad;
    private Renderer?   _screenRenderer;
    private RenderTexture? _renderTexture;

    private void Awake()
    {
        CreateScreen();
    }

    private void OnDestroy()
    {
        if (_renderTexture != null)
        {
            _renderTexture.Release();
            Object.Destroy(_renderTexture);
        }

        if (_screenQuad != null)
            Object.Destroy(_screenQuad);
    }

    private void CreateScreen()
    {
        var rtWidth  = ModConfiguration.Instance?.TheatreRenderTextureWidth.Value  ?? 1920;
        var rtHeight = ModConfiguration.Instance?.TheatreRenderTextureHeight.Value ?? 1080;

        _renderTexture = new RenderTexture(rtWidth, rtHeight, 0, RenderTextureFormat.ARGB32)
        {
            name = "TheatreModeRT"
        };
        _renderTexture.Create();

        _screenQuad = GameObject.CreatePrimitive(PrimitiveType.Quad);
        _screenQuad.name = "TheatreScreen";

        // Remove physics collider — it is not needed.
        var collider = _screenQuad.GetComponent("Collider");
        if (collider != null) Object.Destroy(collider);

        _screenQuad.transform.SetParent(transform, false);
        ApplyScreenTransform();

        _screenRenderer = _screenQuad.GetComponent<Renderer>();
        var material = new Material(Shader.Find("Unlit/Texture") ?? Shader.Find("Sprites/Default"))
        {
            mainTexture = _renderTexture
        };
        _screenRenderer.material = material;

        _screenQuad.SetActive(false);
    }

    private void ApplyScreenTransform()
    {
        if (_screenQuad == null) return;

        var distance = ModConfiguration.Instance?.TheatreScreenDistance.Value ?? 3f;
        var width    = ModConfiguration.Instance?.TheatreScreenWidth.Value    ?? 2f;
        var height   = ModConfiguration.Instance?.TheatreScreenHeight.Value   ?? 1.2f;

        _screenQuad.transform.localPosition = new Vector3(0f, 0f, distance);
        _screenQuad.transform.localRotation = Quaternion.identity;
        _screenQuad.transform.localScale    = new Vector3(width, height, 1f);
    }

    // ── Public API ──────────────────────────────────────────────────────────

    public void Show()
    {
        if (_screenQuad != null) _screenQuad.SetActive(true);
    }

    public void Hide()
    {
        if (_screenQuad != null) _screenQuad.SetActive(false);
    }

    public void SetTexture(RenderTexture rt)
    {
        if (_screenRenderer != null && rt != null)
            _screenRenderer.material.mainTexture = rt;
    }

    /// <summary>The RenderTexture the flat camera should blit into.</summary>
    public RenderTexture? GetRenderTexture() => _renderTexture;

    /// <summary>
    /// Reparents the screen quad to <paramref name="cameraTransform"/> so it stays in the
    /// player's field of view.
    /// </summary>
    public void AttachToCamera(Transform cameraTransform)
    {
        if (_screenQuad == null) return;
        _screenQuad.transform.SetParent(cameraTransform, false);
        ApplyScreenTransform();
    }
}
