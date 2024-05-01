using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BepInEx;
using HarmonyLib;
using Photon.Pun;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Shuv.Patches;

[HarmonyPatch(typeof(Player))]
public class PlayerUpdatePatch {
    private static float Charge { get; set; } = 0f;
    
    [HarmonyPatch(nameof(Player.Update))]
    [HarmonyPostfix]
    private static void CheckForShuvBinding(Player __instance)
    {
        if (!__instance.refs.view.IsMine) return;
        if(__instance.data.health <= 0f || __instance.data.dead) return; // Don't allow shoving when dead, we use a '.health' check because the Defibrillator doesn't change '.dead' to false (iirc)
        // If the Defibrillator does change '.dead' to false, feel free to remove the '.health' check

        if (Shuv.ShuvKey.GetKey() && __instance.refs != null && __instance.refs.items != null)
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
            var ray = new Ray(__instance.refs.cameraPos.position, __instance.refs.cameraPos.forward);
            var hits = Physics.RaycastAll(ray, 2f, HelperFunctions.GetMask(HelperFunctions.LayerType.All));
            if (hits == null)
            {
                Charge = 0f;
                return;
            }
            
            RaycastHit hit = default;
            Player? hitPlayer = null;
            foreach (var raycastHit in hits)
                if ((hitPlayer = raycastHit.collider.transform.parent.GetComponentInParent<Player>()) && !hitPlayer.IsLocal)
                    break;
            
            Shuv.Logger.LogDebug("Trying shove...");
            if (hitPlayer == null || !hitPlayer)
            {
                Charge = 0f;
                return;
            }
            if (hitPlayer != null)
            {
                Shuv.Logger.LogDebug(hitPlayer.ai ? "Shoving AI" : "Shoving Player");
                if (hitPlayer == __instance)
                {
                    Shuv.Logger.LogWarning("Just prevented self-shove!");
                    Charge = 0f;
                    return;
                }
                if (!hitPlayer.ai || ShuvConfig.ShoveEnemies)
                {
                    hitPlayer.CallTakeDamageAndAddForceAndFall(ShuvConfig.Damage, 
                        __instance.refs.cameraPos.forward * Charge * (ShuvConfig.Strength / (hitPlayer.ai ? 4f : 1f)), 
                        Charge * ShuvConfig.RagdollTime + 0.5f);
                    hitPlayer.CallMakeSound(0);
                }
            }
            Charge = 0f;
        }
        if (!Shuv.ShuvKey.GetKey() && Charge > 0) Charge = 0f;
    }
}