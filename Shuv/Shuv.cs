using BepInEx;
using BepInEx.Logging;
using ContentSettings.API;
using HarmonyLib;
using JetBrains.Annotations;
using MyceliumNetworking;
using Shuv.Settings;
using UnityEngine;

namespace Shuv;

internal static class ShuvConfig {
    internal static bool ShoveEnemies { get; set; } = false;
    internal static float RagdollTime { get; set; } = 1.5f;
    internal static float Strength { get; set; } = 1f;
}

#if DEBUG
[ContentWarningPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_VERSION, true)]
#else
[ContentWarningPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_VERSION, false)]
#endif
[BepInAutoPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInDependency(ContentSettings.MyPluginInfo.PLUGIN_GUID)]
public partial class Shuv : BaseUnityPlugin {
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
        
        MyceliumNetwork.RegisterLobbyDataKey("Shuv_Enemies");
        MyceliumNetwork.RegisterLobbyDataKey("Shuv_Ragdoll");
        MyceliumNetwork.RegisterLobbyDataKey("Shuv_Strength");
        
        MyceliumNetwork.LobbyEntered += () =>
        {
            if (MyceliumNetwork.IsHost)
            {
                MyceliumNetwork.SetLobbyData("Shuv_Enemies", ShuvEnemies.Value);
                MyceliumNetwork.SetLobbyData("Shuv_Ragdoll", RagdollTime.Value);
                MyceliumNetwork.SetLobbyData("Shuv_Strength", ShuvStrength.Value);
            }
            else
                TakeLobbyDataToConfig();
        };
        MyceliumNetwork.LobbyDataUpdated += (_) => TakeLobbyDataToConfig();

        Logger.LogInfo($"{MyPluginInfo.PLUGIN_GUID} v{MyPluginInfo.PLUGIN_VERSION} has loaded!");
    }

    private static void TakeLobbyDataToConfig()
    {
        if (MyceliumNetwork.IsHost) return;
        ShuvConfig.ShoveEnemies = MyceliumNetwork.GetLobbyData<int>("Shuv_Enemies") == 1;
        ShuvConfig.RagdollTime = MyceliumNetwork.GetLobbyData<float>("Shuv_Ragdoll");
        ShuvConfig.Strength = MyceliumNetwork.GetLobbyData<float>("Shuv_Strength");
        Shuv.Logger.LogWarning($"Shove Power Updated By Network: {ShuvConfig.Strength} InLobby {MyceliumNetwork.InLobby} | IsHost {MyceliumNetwork.IsHost}");
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