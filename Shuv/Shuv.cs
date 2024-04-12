using System.Collections.Generic;
using BepInEx;
using BepInEx.Logging;
using ContentSettings.API;
using HarmonyLib;
using JetBrains.Annotations;
using UnityEngine;
using Zorro.Settings;
using Unity.Mathematics;

namespace Shuv;

public class ShuvKeyCSetting : KeyCodeSetting, IExposedSetting {
    protected override KeyCode GetDefaultKey() => KeyCode.E;
    public SettingCategory GetSettingCategory() => SettingCategory.Controls;
    public string GetDisplayName() => "Hold to Shove";
}

public class ShuvEnemiesBoolSetting : EnumSetting, IExposedSetting {
    public override void ApplyValue()
    {
        Shuv.Logger.LogInfo("Shove Enemies: " + GetChoices()[Value]);
    }
    protected override int GetDefaultValue() => 1;
    public override List<string> GetChoices() => [ "No", "Yes" ];
    public SettingCategory GetSettingCategory() => SettingCategory.Controls;
    public string GetDisplayName() => "Shove Enemies";
}

public class ShuvRagdollFloatSetting : FloatSetting, IExposedSetting {
    public override void ApplyValue() {}
    protected override float GetDefaultValue() => 1f;
    protected override float2 GetMinMaxValue() => new float2(0.1f, 2.5f);
    public SettingCategory GetSettingCategory() => SettingCategory.Controls;
    public string GetDisplayName() => "Shove Strength";
}

[ContentWarningPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_VERSION, false)]
[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInDependency("commander__cat.contentwarning.contentsettings")]
public class Shuv : BaseUnityPlugin {
    public static Shuv Instance { get; private set; } = null!;
    [UsedImplicitly]
    internal new static ManualLogSource Logger { get; private set; } = null!;
    [UsedImplicitly]
    internal static Harmony? Harmony { get; set; }

    public static GlobalInputHandler.InputKey ShuvKey { get; private set; } = new GlobalInputHandler.InputKey();
    public static ShuvEnemiesBoolSetting ShuvEnemies { get; private set; } = new ShuvEnemiesBoolSetting();
    public static ShuvRagdollFloatSetting RagdollTime { get; private set; } = new ShuvRagdollFloatSetting();

    private void Awake()
    {
        Logger = base.Logger;
        Instance = this;

        Patch();

        var shuvKey = new ShuvKeyCSetting();
        ShuvKey.SetKeybind(shuvKey);
        
        SettingsLoader.RegisterSetting(shuvKey);
        SettingsLoader.RegisterSetting(ShuvEnemies);
        SettingsLoader.RegisterSetting(RagdollTime);

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