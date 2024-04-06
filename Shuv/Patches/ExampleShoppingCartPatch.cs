using System;
using System.Reflection;
using BepInEx;
using HarmonyLib;
using UnityEngine;

namespace Shuv.Patches;

[HarmonyPatch(typeof(Player))]
public class PlayerUpdatePatch {
    private static float Charge { get; set; } = 0f;
    private const float ChargePower = 10f;
    private const float MaxFallTime = 1.5f;
    [HarmonyPatch(nameof(Player.Update))]
    [HarmonyPostfix]
    private static void CheckForShuvBinding(Player __instance)
    {
        if (!__instance.refs.view.IsMine) return;

        if (Shuv.ShuvKey.GetKey())
        {
            Charge = Mathf.MoveTowards(Charge, 1f, Time.deltaTime);
            if (Time.time > __instance.refs.items.shakeTime + 0.1f)
            {
                GamefeelHandler.instance.perlin.AddShake(Charge, 0.2f, 15f);
                __instance.refs.items.shakeTime = Time.time;
                return;
            }
        }
        if (!Shuv.ShuvKey.GetKey() && Charge > 0.25f)
        {
            
            var rayHit = HelperFunctions.LineCheck(__instance.refs.cameraPos.position, __instance.refs.cameraPos.position + __instance.refs.cameraPos.forward * 2f,
                HelperFunctions.LayerType.All, 0.5f);
                
            if (rayHit.collider != null)
            {
                var player = rayHit.collider.transform.parent.GetComponentInParent<Player>();
                if (player != null)
                {
                    player.CallTakeDamageAndAddForceAndFall(0f, __instance.refs.cameraPos.forward * Charge * 
                                                                (ChargePower / (player.ai ? 4f : 1f)), Charge * MaxFallTime + 0.5f);
                    player.CallMakeSound(0);
                }
            }
            Charge = 0f;
        }
        if (!Shuv.ShuvKey.GetKey() && Charge > 0) Charge = 0f;
    }
}