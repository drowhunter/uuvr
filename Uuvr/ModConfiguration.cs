using System.ComponentModel;
using BepInEx.Configuration;
using UnityEngine;

namespace Uuvr;

public class ModConfiguration
{
    public static ModConfiguration Instance;

    public enum CameraTrackingMode
    {
        [Description("Absolute")] Absolute,
        [Description("Relative matrix")] RelativeMatrix,
#if MODERN
        // TODO: could add this for legacy too.
        [Description("Relative Transform")] RelativeTransform,
#endif
        [Description("Child")] Child,
    }

#if MODERN
    public enum VrApi
    {
        [Description("OpenVR")] OpenVr,
        [Description("OpenXR")] OpenXr,
    }
#endif

    public enum ScreenSpaceCanvasType
    {
        [Description("None")] None,

        [Description("Not rendering to texture")]
        NotToTexture,
        [Description("All")] All,
    }

    public enum UiRenderMode
    {
        [Description("Overlay camera (draws on top of everything)")]
        OverlayCamera,

        [Description("In world (can be occluded)")]
        InWorld,
    }

    public enum UiPatchMode
    {
        [Description("Don't touch UI")] None,

        [Description("Mirror flat screen (game not mirrored)")]
        Mirror,
        [Description("Patch Canvas objects")] CanvasRedirect,
    }

    public readonly ConfigFile Config;
    public readonly ConfigEntry<CameraTrackingMode> CameraTracking;
    public readonly ConfigEntry<bool> RelativeCameraSetStereoView;
    public readonly ConfigEntry<int> VrCameraDepth;
    public readonly ConfigEntry<int> VrUiLayerOverride;
    public readonly ConfigEntry<Vector3> VrUiPosition;
    public readonly ConfigEntry<float> VrUiScale;
    public readonly ConfigEntry<string> VrUiShader;
    public readonly ConfigEntry<int> VrUiRenderQueue;
    public readonly ConfigEntry<bool> AlignCameraToHorizon;
    public readonly ConfigEntry<float> CameraPositionOffsetX;
    public readonly ConfigEntry<float> CameraPositionOffsetY;
    public readonly ConfigEntry<float> CameraPositionOffsetZ;
    public readonly ConfigEntry<bool> OverrideDepth;
    public readonly ConfigEntry<bool> PhysicsMatchHeadsetRefreshRate;
    public readonly ConfigEntry<UiPatchMode> PreferredUiPatchMode;
    public readonly ConfigEntry<UiRenderMode> PreferredUiRenderMode;
    public readonly ConfigEntry<ScreenSpaceCanvasType> ScreenSpaceCanvasTypesToPatch;
    public readonly ConfigEntry<string> ObjectsToDeactivateByComponent;
    public readonly ConfigEntry<string> ComponentsToDisable;
    public readonly ConfigEntry<float> ComponentSearchInterval;

    // Theatre Mode
    public readonly ConfigEntry<bool> TheatreModeEnabled;
    public readonly ConfigEntry<string> TheatreImmersiveCameras;
    public readonly ConfigEntry<float> TheatreScreenDistance;
    public readonly ConfigEntry<float> TheatreScreenWidth;
    public readonly ConfigEntry<float> TheatreScreenHeight;
    public readonly ConfigEntry<int> TheatreRenderTextureWidth;
    public readonly ConfigEntry<int> TheatreRenderTextureHeight;

#if MODERN
    public readonly ConfigEntry<VrApi> PreferredVrApi;
#endif

    public ModConfiguration(ConfigFile config)
    {
        Instance = this;

        Config = config;

#if MODERN
        PreferredVrApi = config.Bind(
            "General",
            "Preferred VR APi",
            VrApi.OpenXr,
            "VR API to use. Depending on the game, some APIs might be unavailable, so UUVR will fall back to one that works.");
#endif

        CameraTracking = config.Bind(
            "Camera",
            "Camera Tracking Mode",
#if LEGACY
            CameraTrackingMode.RelativeMatrix,
#else
            CameraTrackingMode.RelativeTransform,
#endif
            "Defines how camera tracking is done. Relative is usually preferred, but not all games support it. Changing this might require restarting the level.");

        RelativeCameraSetStereoView = config.Bind(
            "Relative Camera",
            "Use SetStereoView for Relative Camera",
            false,
            "Some games are better with this on, some are better with this off. Just try it and see which one is better. Changing this might require restarting the level.");

        AlignCameraToHorizon = config.Bind(
            "Camera",
            "Align To Horizon",
            false,
            "Prevents pitch and roll changes on the camera, allowing only yaw changes.");

        CameraPositionOffsetX = config.Bind(
            "Camera",
            "Camera Position Offset X",
            0f,
            "Changes position of tracked VR cameras");

        CameraPositionOffsetY = config.Bind(
            "Camera",
            "Camera Position Offset Y",
            0f,
            "Changes position of tracked VR cameras");

        CameraPositionOffsetZ = config.Bind(
            "Camera",
            "Camera Position Offset Z",
            0f,
            "Changes position of tracked VR cameras");

        OverrideDepth = config.Bind(
            "Camera",
            "Override Depth",
            false,
            "In some games, the VR camera won't display anything unless we override the camera depth value.");

        VrCameraDepth = config.Bind(
            "Camera",
            "Depth Value",
            1,
            new ConfigDescription(
                "Requires enabling 'Override Depth'. Range is -100 to 100, but you should try to find the lowest value that fixes visibility.",
                new AcceptableValueRange<int>(-100, 100)));

        PhysicsMatchHeadsetRefreshRate = config.Bind(
            "General",
            "Force physics rate to match headset refresh rate",
            false,
            "Can help fix jiterriness in games that rely a lot on physics. Might break a lot of games too.");

        PreferredUiPatchMode = config.Bind(
            "UI",
            "UI Patch Mode",
            UiPatchMode.Mirror,
            "Method to use for patching UI for VR.");

        VrUiLayerOverride = config.Bind(
            "UI",
            "VR UI Layer Override",
            -1,
            new ConfigDescription(
                "Layer to use for VR UI. By default (value -1) UUVR falls back to an unused (unnamed) layer.",
                new AcceptableValueRange<int>(-1, 31)));

        VrUiPosition = config.Bind(
            "UI",
            "VR UI Position",
            Vector3.forward * 1f,
            "Position of the VR UI projection relative to the camera.");

        VrUiScale = config.Bind(
            "UI",
            "VR UI Scale",
            1f,
            "Scale of the VR UI projection.");

        VrUiShader = config.Bind(
            "UI",
            "VR UI Shader",
            "",
            "Name of shader to use for the VR UI projection (passed to Unity's Shader.Find). Leave empty to let UUVR pick for you.");

        VrUiRenderQueue = config.Bind(
            "UI",
            "VR UI Render Queue",
            5000,
            new ConfigDescription(
                "Render queue to use for the VR UI projection. Default is 5000, which is the same as Unity's default canvas material.",
                new AcceptableValueRange<int>(0, 5000)));

        ScreenSpaceCanvasTypesToPatch = config.Bind(
            "UI",
            "Screen-space UI Elements to Patch",
            ScreenSpaceCanvasType.NotToTexture,
            "Screen-space UI elements are already visible in VR with no patches. But in some games, they are difficult to see in VR. So you can choose to patch some (or all) of them to be rendered in the VR UI screen.");

        PreferredUiRenderMode = config.Bind(
            "UI",
            "Preferred UI Plane Render Mode",
#if MODERN
            UiRenderMode.InWorld,
#else
            // Ideally we'd do overlay in all games but that mode can cause a lot of issues.
            // Most of the issues seem to be in more recent games, so at least for legacy we can default to overlay.
            UiRenderMode.OverlayCamera,
#endif
            "How to render the VR UI Plane. Overlay is usually better, but doesn't work in every game.");

        ObjectsToDeactivateByComponent = config.Bind(
            "Fixes",
            "Objects to Deactivate by Component",
            "",
            "Any objects that contains one of these components gets deactivated. List of fully qualified C# type names, separated by /. Example: 'Canvas, UnityEngine/HUD, Assembly-CSharp'");

        ComponentsToDisable = config.Bind(
            "Fixes",
            "Components to Disable.",
            "",
            "Names of components to disable. List of fully qualified C# type names, separated by /. Example: 'Canvas, UnityEngine/HUD, Assembly-CSharp'");

        ComponentSearchInterval = config.Bind(
            "Fixes",
            "Component Search Interval",
            1f,
            new ConfigDescription("Value in seconds, the interval between searches for components to disable.",
                new AcceptableValueRange<float>(0.5f, 30f)));

        TheatreModeEnabled = config.Bind(
            "Theatre Mode",
            "Enabled",
            false,
            "Enable dynamic IVR/Theatre Mode switching. When enabled, cameras not in the Immersive list render onto a virtual flat screen.");

        TheatreImmersiveCameras = config.Bind(
            "Theatre Mode",
            "Immersive Camera Patterns",
            "",
            "Camera names or regex patterns (separated by /) that should render in Immersive VR. All other cameras use Theatre Mode. Example: 'Main Camera/^HUD.*'");

        TheatreScreenDistance = config.Bind(
            "Theatre Mode",
            "Screen Distance",
            3f,
            new ConfigDescription("Distance in metres the theatre screen is placed in front of the VR camera.",
                new AcceptableValueRange<float>(0.5f, 20f)));

        TheatreScreenWidth = config.Bind(
            "Theatre Mode",
            "Screen Width",
            2f,
            new ConfigDescription("Width of the theatre screen quad in metres.",
                new AcceptableValueRange<float>(0.1f, 20f)));

        TheatreScreenHeight = config.Bind(
            "Theatre Mode",
            "Screen Height",
            1.2f,
            new ConfigDescription("Height of the theatre screen quad in metres.",
                new AcceptableValueRange<float>(0.1f, 20f)));

        TheatreRenderTextureWidth = config.Bind(
            "Theatre Mode",
            "Render Texture Width",
            1920,
            new ConfigDescription("Horizontal resolution of the theatre capture RenderTexture.",
                new AcceptableValueRange<int>(64, 7680)));

        TheatreRenderTextureHeight = config.Bind(
            "Theatre Mode",
            "Render Texture Height",
            1080,
            new ConfigDescription("Vertical resolution of the theatre capture RenderTexture.",
                new AcceptableValueRange<int>(64, 4320)));
    }
}