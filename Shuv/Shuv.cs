using System;
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

    internal static int Damage { get; set; } = 0;
}

#if DEBUG
[ContentWarningPlugin(Shuv.PLUGIN_GUID, Shuv.PLUGIN_VERSION, true)]
#else
[ContentWarningPlugin(Shuv.PLUGIN_GUID, Shuv.PLUGIN_VERSION, false)]
#endif
[BepInAutoPlugin(Shuv.PLUGIN_GUID, Shuv.PLUGIN_NAME, Shuv.PLUGIN_VERSION)]
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
    public static ShuvDamageIntSetting ShuvDamage { get; private set; } = new ShuvDamageIntSetting();
    
    // set PLUGIN_GUID
    public const string PLUGIN_GUID = "wwwDayDream.Shuv";
    public const string PLUGIN_NAME = "Shuv";
    public const string PLUGIN_VERSION = "1.1.1";

    private void Awake()
    {
        Logger = base.Logger;
        Instance = this;

        Patch();

        var shuvKey = new ShuvKeyCSetting();
        ShuvKey.SetKeybind(shuvKey);
        
        SettingsLoader.RegisterSetting("Shuv", "Keybindings", shuvKey);
        SettingsLoader.RegisterSetting("Shuv", "Options", ShuvEnemies);
        SettingsLoader.RegisterSetting("Shuv", "Options", RagdollTime);
        SettingsLoader.RegisterSetting("Shuv", "Options", ShuvStrength);
        SettingsLoader.RegisterSetting("Shuv", "Options", ShuvDamage);
        
        MyceliumNetwork.RegisterLobbyDataKey("Shuv_Enemies");
        MyceliumNetwork.RegisterLobbyDataKey("Shuv_Ragdoll");
        MyceliumNetwork.RegisterLobbyDataKey("Shuv_Strength");
        MyceliumNetwork.RegisterLobbyDataKey("Shuv_Damage");
        
        MyceliumNetwork.LobbyEntered += () =>
        {
            if (MyceliumNetwork.IsHost)
            {
                MyceliumNetwork.SetLobbyData("Shuv_Enemies", ShuvEnemies.Value);
                MyceliumNetwork.SetLobbyData("Shuv_Ragdoll", RagdollTime.Value);
                MyceliumNetwork.SetLobbyData("Shuv_Strength", ShuvStrength.Value);
                MyceliumNetwork.SetLobbyData("Shuv_Damage", ShuvDamage.Value);
            }
            else
                TakeLobbyDataToConfig();
        };
        MyceliumNetwork.LobbyDataUpdated += (_) => TakeLobbyDataToConfig();

        Logger.LogInfo($"{PLUGIN_GUID} v{PLUGIN_VERSION} has loaded!");
    }

    private void OnDestroy()
    {
        Unpatch();
        Logger.LogInfo($"{PLUGIN_GUID} v{PLUGIN_VERSION} has unloaded!");
    }

    private static void TakeLobbyDataToConfig()
    {
        if (MyceliumNetwork.IsHost) return;
        ShuvConfig.ShoveEnemies = MyceliumNetwork.GetLobbyData<int>("Shuv_Enemies") == 1;
        ShuvConfig.RagdollTime = MyceliumNetwork.GetLobbyData<float>("Shuv_Ragdoll");
        ShuvConfig.Strength = MyceliumNetwork.GetLobbyData<float>("Shuv_Strength");
        ShuvConfig.Damage = MyceliumNetwork.GetLobbyData<int>("Shuv_Damage");
        Shuv.Logger.LogWarning($"Shove Power Updated By Network: {ShuvConfig.Strength} InLobby {MyceliumNetwork.InLobby} | IsHost {MyceliumNetwork.IsHost}");
    }

    internal static void Patch()
    {
        Harmony ??= new Harmony(PLUGIN_GUID);
        
        Harmony.PatchAll();
        Logger.LogDebug($"Finished patching for {PLUGIN_GUID}!");
    }

    internal static void Unpatch()
    {
        Harmony?.UnpatchSelf();
        Logger.LogDebug($"Finished unpatching for {PLUGIN_GUID}!");
    }
}