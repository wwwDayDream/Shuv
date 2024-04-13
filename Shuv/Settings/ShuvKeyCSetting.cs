using ContentSettings.API.Settings;
using UnityEngine;
using Zorro.Settings;

namespace Shuv.Settings;

public class ShuvKeyCSetting : KeyCodeSetting, ICustomSetting {
    public override KeyCode GetDefaultKey() => KeyCode.E;
    public string GetDisplayName() => "Hold to Shove";
}