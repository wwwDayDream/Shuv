using BepInEx;
using BepInEx.Logging;
using ContentSettings.API;
using HarmonyLib;
using JetBrains.Annotations;
using UnityEngine;
using Zorro.Settings;

namespace Shuv;

public class ShuvKeyCSetting : KeyCodeSetting, IExposedSetting {
    protected override KeyCode GetDefaultKey() => KeyCode.E;
    public SettingCategory GetSettingCategory() => SettingCategory.Controls;
    public string GetDisplayName() => "Hold to Shove";
}

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInDependency("commander__cat.contentwarning.contentsettings")]
public class Shuv : BaseUnityPlugin {
    public static Shuv Instance { get; private set; } = null!;
    [UsedImplicitly]
    internal new static ManualLogSource Logger { get; private set; } = null!;
    [UsedImplicitly]
    internal static Harmony? Harmony { get; set; }

    public static GlobalInputHandler.InputKey ShuvKey { get; private set; } = new GlobalInputHandler.InputKey();

    private void Awake()
    {
        Logger = base.Logger;
        Instance = this;

        Patch();

        var shuvKey = new ShuvKeyCSetting();
        ShuvKey.SetKeybind(shuvKey);
        
        SettingsLoader.RegisterSetting(shuvKey);

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