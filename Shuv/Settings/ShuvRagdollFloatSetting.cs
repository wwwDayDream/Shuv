using ContentSettings.API.Settings;
using MyceliumNetworking;
using Unity.Mathematics;
using Zorro.Settings;

namespace Shuv.Settings;

public class ShuvRagdollFloatSetting : FloatSetting, ICustomSetting {
    public override void ApplyValue() {
        if (!SteamManager.Initialized || !MyceliumNetwork.InLobby || MyceliumNetwork.IsHost)
            ShuvConfig.RagdollTime = Value;
        if (SteamManager.Initialized && MyceliumNetwork.InLobby && MyceliumNetwork.IsHost)
            MyceliumNetwork.SetLobbyData("Shuv_Ragdoll", Value);
    }
    public override float GetDefaultValue() => 1f;
    public override float2 GetMinMaxValue() => new float2(0.1f, 2.5f);
    public string GetDisplayName() => "Ragdoll Time";
}