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
        if (!__instance.refs.view.IsMine || __instance.refs == null) return;
        var alive = __instance.data.health > 0f && !__instance.data.dead;
        // Don't allow shoving when dead, we use a '.health' check because the Defibrillator doesn't change '.dead' to false (iirc)
        // If the Defibrillator does change '.dead' to false, feel free to remove the '.health' check

        if (Shuv.ShuvKey.GetKey() && __instance.refs.items != null && alive)
        {
            Charge = Mathf.MoveTowards(Charge, 1f, Time.deltaTime);
            if (Time.time > __instance.refs.items.shakeTime + 0.1f)
            {
                GamefeelHandler.instance.perlin.AddShake(Charge, 0.2f, 15f);
                __instance.refs.items.shakeTime = Time.time;
            }
        }
        if (!Shuv.ShuvKey.GetKey() && Charge > 0.25f && alive)
        {
            var ray = new Ray(__instance.refs.cameraPos.position, __instance.refs.cameraPos.forward);
            // ReSharper disable once Unity.PreferNonAllocApi
            var hits = Physics.RaycastAll(ray, 2f, HelperFunctions.GetMask(HelperFunctions.LayerType.All));

            Player? hitPlayer = hits
                .Select(raycastHit => raycastHit.collider.transform.parent.GetComponentInParent<Player>())
                .TakeWhile(potentialPlayer => potentialPlayer && potentialPlayer != null)
                .FirstOrDefault(potentialPlayer => potentialPlayer && !potentialPlayer.IsLocal);

            Shuv.Logger.LogDebug("Trying shove...");
            if (hitPlayer != null && hitPlayer)
            {
                Shuv.Logger.LogDebug(hitPlayer.ai ? "Shoving AI" : "Shoving Player");
                if (!hitPlayer.ai || ShuvConfig.ShoveEnemies)
                {
                    hitPlayer.CallTakeDamageAndAddForceAndFall(ShuvConfig.Damage, 
                        __instance.refs.cameraPos.forward * Charge * (ShuvConfig.Strength / (hitPlayer.ai ? 4f : 1f)), 
                        Charge * ShuvConfig.RagdollTime + 0.5f);
                    hitPlayer.CallMakeSound(0);
                }
            }
        }
        if ((!alive || !Shuv.ShuvKey.GetKey()) && Charge > 0) Charge = 0f;
    }
}