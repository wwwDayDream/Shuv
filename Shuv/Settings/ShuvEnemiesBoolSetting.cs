using System.Collections.Generic;
using ContentSettings.API.Settings;
using MyceliumNetworking;
using Zorro.Settings;

namespace Shuv.Settings;

public class ShuvEnemiesBoolSetting : EnumSetting, ICustomSetting {
    public override void ApplyValue()
    {
        if (!SteamManager.Initialized || !MyceliumNetwork.InLobby || MyceliumNetwork.IsHost)
            ShuvConfig.ShoveEnemies = Value == 1;
        if (SteamManager.Initialized && MyceliumNetwork.InLobby && MyceliumNetwork.IsHost)
            MyceliumNetwork.SetLobbyData("Shuv_Enemies", Value);
    }
    public override int GetDefaultValue() => 1;
    public override List<string> GetChoices() => [ "No", "Yes" ];
    public string GetDisplayName() => "Shove Enemies";
}