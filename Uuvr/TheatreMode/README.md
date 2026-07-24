# UUVR — Theatre Mode Extension

Dynamic switching between **Immersive VR (IVR)** and **Theatre Mode** for UUVR.

---

## Overview

When Theatre Mode is enabled, UUVR inspects each Unity camera every frame and routes it to one of two render pipelines:

| Mode | Behaviour |
|------|-----------|
| **Immersive VR** | Normal UUVR stereo rendering (camera added to `VrCamera`). |
| **Theatre Mode** | Camera output is blitted to a RenderTexture displayed on a floating virtual screen quad parented to the VR headset camera. |

---

## Components

### `CameraModeRouter`
Maintains a list of camera name patterns (exact strings or .NET regular expressions).  
A camera whose name matches any pattern is considered *immersive*; all other cameras use Theatre Mode.

```csharp
// Check at runtime
bool immersive = TheatreModeIntegration.Instance.Router.IsImmersive(cam);

// Add / remove patterns at runtime
TheatreModeIntegration.Instance.Router.AddImmersiveCamera("Main Camera");
TheatreModeIntegration.Instance.Router.AddImmersiveCamera(@"^Player.*Camera$");
TheatreModeIntegration.Instance.Router.RemoveImmersiveCamera("Main Camera");
```

### `TheatreScreenManager`
Creates and manages the virtual flat screen:

- Quad primitive parented to the highest-depth VR camera.
- Default: 3 m forward, 2 m wide × 1.2 m tall.
- Lit with `Unlit/Texture` (falls back to `Sprites/Default`).
- Exposes `Show()`, `Hide()`, `SetTexture(RenderTexture)`, `AttachToCamera(Transform)`.

### `TheatreCapture`
`MonoBehaviour` attached to flat cameras.  
Implements `OnRenderImage` to blit the camera's final frame into the shared RenderTexture **without** disrupting the normal display.

### `TheatreModeIntegration`
Top-level coordinator added to the UUVR root `GameObject`.  
Calls `OnCameraRender(Camera)` each frame for every UUVR-tracked camera and delegates to `SwitchToImmersiveVR` / `SwitchToTheatreMode`.

---

## Configuration (BepInEx `cfg`)

All settings are in the **Theatre Mode** section of `UUVR.cfg`:

| Key | Default | Description |
|-----|---------|-------------|
| `Enabled` | `false` | Master switch for the feature. |
| `Immersive Camera Patterns` | _(empty)_ | `/`-separated camera names or regex patterns that should render in IVR. |
| `Screen Distance` | `3` | Distance (m) of the virtual screen from the VR camera. |
| `Screen Width` | `2` | Width (m) of the virtual screen quad. |
| `Screen Height` | `1.2` | Height (m) of the virtual screen quad. |

### Example patterns

```
Main Camera/^GameplayCamera.*$/UICamera
```

This routes `Main Camera`, any camera whose name starts with `GameplayCamera`, and `UICamera` to IVR — everything else gets Theatre Mode.

---

## Runtime behaviour

```
Game frame begins
 └─ VrCameraManager.Update() iterates all cameras
     └─ [Theatre Mode enabled?]
         ├─ TheatreModeIntegration.OnCameraRender(cam)
         │   ├─ Router.IsImmersive(cam) → true  → SwitchToImmersiveVR  → hide quad, VrCamera added
         │   └─ Router.IsImmersive(cam) → false → SwitchToTheatreMode  → ensure TheatreCapture attached,
         │                                                                 show quad, RT set on material
         └─ [Theatre Mode disabled] → original UUVR pipeline (all cameras → VrCamera)
```

---

## Integration points

The feature is activated by setting `Theatre Mode → Enabled = true` in `UUVR.cfg`.  
No code changes are required in the game or other UUVR components.

`TheatreModeIntegration` is added to the same `GameObject` as `UuvrCore` in `UuvrCore.Awake()`.  
`VrCameraManager.Update()` calls `TheatreModeIntegration.OnCameraRender` before deciding whether to add a `VrCamera` component.

---

## Notes

- `OnRenderImage` is **not** available in IL2CPP builds via the normal Unity callback mechanism. In those builds the blit is performed through the `TheatreCapture.PerformBlit` public method, which can be called from a Harmony/HookGen patch if required.
- The Theatre screen is destroyed and recreated if the UUVR root GameObject is destroyed (rare, but handled by `UuvrCore.OnDestroy`).
