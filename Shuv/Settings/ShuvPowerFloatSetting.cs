using ContentSettings.API.Settings;
using MyceliumNetworking;
using Unity.Mathematics;
using Zorro.Settings;

namespace Shuv.Settings;

public class ShuvPowerFloatSetting : FloatSetting, ICustomSetting {
    public override void ApplyValue()
    {
        if (!SteamManager.Initialized || !MyceliumNetwork.InLobby || MyceliumNetwork.IsHost)
            ShuvConfig.Strength = Value;
        if (SteamManager.Initialized && MyceliumNetwork.InLobby && MyceliumNetwork.IsHost)
            MyceliumNetwork.SetLobbyData("Shuv_Strength", Value);
    }
    public override float GetDefaultValue() => 10f;
    public override float2 GetMinMaxValue() => new float2(1f, 50f);
    public string GetDisplayName() => "Shove Strength";
}