using ContentSettings.API.Settings;
using MyceliumNetworking;
using Unity.Mathematics;
using IntSetting = ContentSettings.API.Settings.IntSetting;

namespace Shuv.Settings;

public class ShuvDamageIntSetting : IntSetting, ICustomSetting {
    public override void ApplyValue()
    {
        if (!SteamManager.Initialized || !MyceliumNetwork.InLobby || MyceliumNetwork.IsHost)
            ShuvConfig.Damage = Value;
        if (SteamManager.Initialized && MyceliumNetwork.InLobby && MyceliumNetwork.IsHost)
            MyceliumNetwork.SetLobbyData("Shuv_Damage", Value);
    }
    
    protected override int GetDefaultValue() => 0;
    protected override (int, int) GetMinMaxValue() => (0, 10);
    public string GetDisplayName() => "Shove Damage";
}