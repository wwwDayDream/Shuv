using System;
using System.Collections.Generic;
using System.Reflection;
using BepInEx;
using HarmonyLib;
using Photon.Pun;
using Steamworks;
using UnityEngine;

namespace Shuv.Patches;

[HarmonyPatch(typeof(Player))]
public class PlayerUpdatePatch {
    private static float Charge { get; set; } = 0f;
    private const float ChargePower = 10f;
    private const float MaxFallTime = 1.5f;

    private static List<CSteamID> Friends = new List<CSteamID>();
    private static List<CSteamID> AlreadyChecked = new List<CSteamID>();

    public static bool IsFriend(CSteamID steamID)
    {
        if (!AlreadyChecked.Contains(steamID))
        {
            if (SteamFriends.HasFriend(steamID, EFriendFlags.k_EFriendFlagImmediate))
            {
                Shuv.Logger.LogInfo("Is friends, can shove!");
                Friends.Add(steamID);                
            }
            AlreadyChecked.Add(steamID);
        }
        return Friends.Contains(steamID);
    }
    
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
                    if (player.ai || (SteamAvatarHandler.TryGetSteamIDForPlayer(player.refs.view.Controller, out var steamID) && IsFriend(steamID)))
                    {
                        player.CallTakeDamageAndAddForceAndFall(0f, __instance.refs.cameraPos.forward * Charge * 
                                                                    (ChargePower / (player.ai ? 4f : 1f)), Charge * MaxFallTime + 0.5f);
                        player.CallMakeSound(0);
                    } else
                    {
                        Shuv.Logger.LogInfo("Blocking Non-AI & Non-Friend Shuv!");
                    }
                }
            }
            Charge = 0f;
        }
        if (!Shuv.ShuvKey.GetKey() && Charge > 0) Charge = 0f;
    }
}