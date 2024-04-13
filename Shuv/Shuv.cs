using System.Collections.Generic;
using BepInEx;
using BepInEx.Logging;
using ContentSettings.API;
using ContentSettings.API.Settings;
using HarmonyLib;
using JetBrains.Annotations;
using UnityEngine;
using Zorro.Settings;
using Unity.Mathematics;

namespace Shuv;

public class ShuvKeyCSetting : KeyCodeSetting, ICustomSetting {
    public override KeyCode GetDefaultKey() => KeyCode.E;
    public string GetDisplayName() => "Hold to Shove";
}

public class ShuvEnemiesBoolSetting : EnumSetting, ICustomSetting {
    public override void ApplyValue()
    {
        Shuv.Logger.LogInfo("Shove Enemies: " + GetChoices()[Value]);
    }
    public override int GetDefaultValue() => 1;
    public override List<string> GetChoices() => [ "No", "Yes" ];
    public string GetDisplayName() => "Shove Enemies";
}

public class ShuvRagdollFloatSetting : FloatSetting, ICustomSetting {
    public override void ApplyValue() {}
    public override float GetDefaultValue() => 1f;
    public override float2 GetMinMaxValue() => new float2(0.1f, 2.5f);
    public string GetDisplayName() => "Ragdoll Time";
}

public class ShuvPowerFloatSetting : FloatSetting, ICustomSetting {
    public override void ApplyValue() {}
    public override float GetDefaultValue() => 10f;
    public override float2 GetMinMaxValue() => new float2(1f, 100f);
    public string GetDisplayName() => "Shove Strength";
}
#if DEBUG
[ContentWarningPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_VERSION, true)]
#else
[ContentWarningPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_VERSION, false)]
#endif
[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInDependency(ContentSettings.MyPluginInfo.PLUGIN_GUID)]
public class Shuv : BaseUnityPlugin {
    public static Shuv Instance { get; private set; } = null!;
    [UsedImplicitly]
    internal new static ManualLogSource Logger { get; private set; } = null!;
    [UsedImplicitly]
    internal static Harmony? Harmony { get; set; }

    public static GlobalInputHandler.InputKey ShuvKey { get; private set; } = new GlobalInputHandler.InputKey();
    public static ShuvEnemiesBoolSetting ShuvEnemies { get; private set; } = new ShuvEnemiesBoolSetting();
    public static ShuvRagdollFloatSetting RagdollTime { get; private set; } = new ShuvRagdollFloatSetting();
    public static ShuvPowerFloatSetting ShuvStrength { get; private set; } = new ShuvPowerFloatSetting();

    private void Awake()
    {
        Logger = base.Logger;
        Instance = this;

        Patch();

        var shuvKey = new ShuvKeyCSetting();
        ShuvKey.SetKeybind(shuvKey);
        
        SettingsLoader.RegisterSetting("Modded", "Shuv", shuvKey);
        SettingsLoader.RegisterSetting("Modded", "Shuv", ShuvEnemies);
        SettingsLoader.RegisterSetting("Modded", "Shuv", RagdollTime);
        SettingsLoader.RegisterSetting("Modded", "Shuv", ShuvStrength);

        Logger.LogInfo($"{MyPluginInfo.PLUGIN_GUID} v{MyPluginInfo.PLUGIN_VERSION} has loaded!");
    }

    internal static void Patch()
    {
        Harmony ??= new Harmony(MyPluginInfo.PLUGIN_GUID);

        Logger.LogDebug("Patching...");

        Harmony.PatchAll();

        Logger.LogDebug("Finished patching!");
    }

    internal static void Unpatch()
    {
        Logger.LogDebug("Unpatching...");

        Harmony?.UnpatchSelf();

        Logger.LogDebug("Finished unpatching!");
    }
}