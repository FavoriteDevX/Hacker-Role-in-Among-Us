using HarmonyLib;
using UnityEngine;

namespace HackerRole;

internal static class HackerHud
{
    public static HackerButton HackButton;
    public static HackerButton CamButton;
    public static HackerButton PlantButton;

    public static void Create(HudManager hud)
    {
        HackButton = new HackerButton(hud, "HACK", new Vector3(-2f, 1f, 0f), KeyCode.G,
            () => HackerState.LocalHackerAlive,
            () => PlayerControl.LocalPlayer.CanMove && HackerState.PendingGlitches.Count < HackerState.Settings.MaxActiveGlitches,
            TargetPicker.Open);

        CamButton = new HackerButton(hud, "CAM", new Vector3(-3f, 1f, 0f), KeyCode.H,
            () => HackerState.LocalHackerAlive,
            () => PlayerControl.LocalPlayer.CanMove,
            () =>
            {
                if (!PocketCam.Open()) return;
                CamButton.StartEffect(HackerState.Settings.CamDuration);
            });
        CamButton.OnEffectEnd = () =>
        {
            // Closing early only costs the part of the battery that was actually used.
            var used = 1f - Mathf.Clamp01(CamButton.EffectTimer / CamButton.EffectDuration);
            PocketCam.Close();
            CamButton.MaxTimer = HackerState.Settings.CamRecharge;
            CamButton.Timer = HackerState.Settings.CamRecharge * used;
        };

        PlantButton = new HackerButton(hud, "PLANT", new Vector3(-4f, 1f, 0f), KeyCode.J,
            () => HackerState.LocalHackerAlive && HackerState.Settings.BlueScreenEnabled && !HackerState.ExecuteUsed,
            () => PlayerControl.LocalPlayer.CanMove && PlayerControl.LocalPlayer.killTimer <= 0f
                  && HackerState.PlantedTargetId == HackerState.None && !HackerState.PlantLocked
                  && FindPlantTarget() != null,
            Plant);
        PlantButton.Label = () => HackerState.PlantedTargetId != HackerState.None ? "PLANTED"
            : HackerState.PlantLocked ? "REBOOTING" : "PLANT";
    }

    public static void ResetCooldowns()
    {
        if (HackButton == null) return;
        HackButton.MaxTimer = HackButton.Timer = HackerState.Settings.HackCooldown;
        CamButton.EndEffect();
        CamButton.MaxTimer = HackerState.Settings.CamRecharge;
        CamButton.Timer = 0f;
    }

    private static void Plant()
    {
        var target = FindPlantTarget();
        if (target == null) return;

        if (target.protectedByGuardianId > -1)
        {
            Toast.Show("Cable rejected: target is shielded.", 2f, Color.yellow);
            return;
        }

        HackerNet.RpcPlant(target.PlayerId);
        KillPenaltyPatch.ApplyPenalty(PlayerControl.LocalPlayer);
    }

    /// <summary>Closest living non-Impostor in kill range with a clear line of sight.</summary>
    public static PlayerControl FindPlantTarget()
    {
        var local = PlayerControl.LocalPlayer;
        if (!ShipStatus.Instance || !Helpers.IsAlive(local)) return null;

        var range = AmongUs.GameOptions.GameOptionsData.KillDistances[Mathf.Clamp(GameOptionsManager.Instance.currentNormalGameOptions.KillDistance, 0, 2)];
        var origin = local.GetTruePosition();
        PlayerControl result = null;

        foreach (var player in PlayerControl.AllPlayerControls)
        {
            if (player.PlayerId == local.PlayerId || !Helpers.IsAlive(player) || player.Data.Role.IsImpostor || player.inVent) continue;

            var delta = player.GetTruePosition() - origin;
            var distance = delta.magnitude;
            if (distance <= range && !PhysicsHelpers.AnyNonTriggersBetween(origin, delta.normalized, distance, Constants.ShipAndObjectsMask))
            {
                result = player;
                range = distance;
            }
        }

        return result;
    }
}

[HarmonyPatch(typeof(HudManager), nameof(HudManager.Start))]
internal static class HudManagerStartPatch
{
    public static void Postfix(HudManager __instance) => HackerHud.Create(__instance);
}

[HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
internal static class HudManagerUpdatePatch
{
    public static void Postfix(HudManager __instance)
    {
        if (AmongUsClient.Instance.GameState != InnerNet.InnerNetClient.GameStates.Started) return;

        HackerButton.UpdateAll(__instance);

        // A Pocket Cam closed by hand (Esc / X) ends the effect early.
        if (HackerHud.CamButton is { EffectActive: true } && !PocketCam.IsOpen) HackerHud.CamButton.EndEffect();

        if (HackerState.LocalIsHacker) PlayerControl.LocalPlayer.cosmetics.nameText.color = HackerState.HackerColor;
    }
}
