using System.Collections.Generic;
using AmongUs.GameOptions;
using HarmonyLib;
using UnityEngine;

namespace HackerRole;

/// <summary>The host turns one plain Impostor into the Hacker right after vanilla roles are assigned.</summary>
[HarmonyPatch(typeof(RoleManager), nameof(RoleManager.SelectRoles))]
internal static class SelectRolesPatch
{
    public static void Postfix()
    {
        if (!AmongUsClient.Instance.AmHost) return;
        if (GameOptionsManager.Instance.currentGameOptions.GameMode == GameModes.HideNSeek) return;

        var hackerId = HackerState.None;
        if (Random.Range(0, 100) < HackerConfig.SpawnChance.Value)
        {
            // Shapeshifter / Phantom keep their own ability button, so only plain Impostors qualify.
            var candidates = new List<PlayerControl>();
            foreach (var player in PlayerControl.AllPlayerControls)
            {
                if (player.Data?.Role != null && player.Data.Role.Role == RoleTypes.Impostor) candidates.Add(player);
            }

            if (candidates.Count > 0) hackerId = candidates[Random.Range(0, candidates.Count)].PlayerId;
        }

        HackerNet.RpcSetHacker(hackerId, HackerSettings.FromConfig());
    }
}

/// <summary>The Hacker pays for their extra tools with a longer kill cooldown.</summary>
[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.MurderPlayer))]
internal static class KillPenaltyPatch
{
    public static void Postfix(PlayerControl __instance)
    {
        if (__instance.AmOwner && HackerState.IsHacker(__instance)) ApplyPenalty(__instance);
    }

    public static void ApplyPenalty(PlayerControl hacker)
    {
        var cooldown = GameOptionsManager.Instance.currentNormalGameOptions.KillCooldown + HackerState.Settings.KillCooldownPenalty;
        hacker.killTimer = cooldown;
        HudManager.Instance.KillButton.SetCoolDown(cooldown, cooldown);
    }
}
