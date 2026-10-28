using HarmonyLib;
using Hazel;
using UnityEngine;

namespace HackerRole;

internal enum HackerRpc : byte
{
    SetHacker = 220,
    HackTask = 221,
    GlitchConsumed = 222,
    Plant = 223,
    Execute = 224,
}

/// <summary>
/// Each Rpc* method sends the call to everyone else and then runs it locally, so every client
/// goes through the same code path.
/// </summary>
internal static class HackerNet
{
    private static MessageWriter Start(HackerRpc call) =>
        AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)call, SendOption.Reliable, -1);

    private static void Finish(MessageWriter writer) => AmongUsClient.Instance.FinishRpcImmediately(writer);

    public static void RpcSetHacker(byte hackerId, HackerSettings settings)
    {
        var writer = Start(HackerRpc.SetHacker);
        writer.Write(hackerId);
        settings.Write(writer);
        Finish(writer);
        HackerActions.SetHacker(hackerId, settings);
    }

    public static void RpcHackTask(byte targetId, byte roomId)
    {
        var writer = Start(HackerRpc.HackTask);
        writer.Write(targetId);
        writer.Write(roomId);
        Finish(writer);
        HackerActions.HackTask(targetId, roomId);
    }

    public static void RpcGlitchConsumed(byte targetId)
    {
        var writer = Start(HackerRpc.GlitchConsumed);
        writer.Write(targetId);
        Finish(writer);
        HackerActions.GlitchConsumed(targetId);
    }

    public static void RpcPlant(byte targetId)
    {
        var writer = Start(HackerRpc.Plant);
        writer.Write(targetId);
        Finish(writer);
        HackerActions.Plant(targetId);
    }

    public static void RpcExecute(byte targetId)
    {
        var writer = Start(HackerRpc.Execute);
        writer.Write(targetId);
        Finish(writer);
        HackerActions.Execute(targetId);
    }
}

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.HandleRpc))]
internal static class HandleRpcPatch
{
    public static void Postfix([HarmonyArgument(0)] byte callId, [HarmonyArgument(1)] MessageReader reader)
    {
        switch ((HackerRpc)callId)
        {
            case HackerRpc.SetHacker:
                var hackerId = reader.ReadByte();
                HackerActions.SetHacker(hackerId, HackerSettings.Read(reader));
                break;
            case HackerRpc.HackTask:
                var targetId = reader.ReadByte();
                HackerActions.HackTask(targetId, reader.ReadByte());
                break;
            case HackerRpc.GlitchConsumed:
                HackerActions.GlitchConsumed(reader.ReadByte());
                break;
            case HackerRpc.Plant:
                HackerActions.Plant(reader.ReadByte());
                break;
            case HackerRpc.Execute:
                HackerActions.Execute(reader.ReadByte());
                break;
        }
    }
}

internal static class HackerActions
{
    public static void SetHacker(byte hackerId, HackerSettings settings)
    {
        HackerState.Reset();
        HackerTicker.ResetAll();
        HackerState.HackerId = hackerId;
        HackerState.Settings = settings;

        if (HackerState.LocalIsHacker)
        {
            Toast.Show("You are the HACKER\n<size=60%>G: Glitch Task   H: Pocket Cam   J: Plant</size>", 6f, HackerState.HackerColor, delay: 7f);
        }
    }

    public static void HackTask(byte targetId, byte roomId)
    {
        HackerState.PendingGlitches[targetId] = roomId;
    }

    public static void GlitchConsumed(byte targetId)
    {
        HackerState.PendingGlitches.Remove(targetId);
    }

    public static void Plant(byte targetId)
    {
        HackerState.PlantedTargetId = targetId;
        HackerTicker.StartPlantAnimation(HackerState.Hacker, HackerState.Settings.PlantTime);
    }

    public static void Execute(byte targetId)
    {
        var target = Helpers.PlayerById(targetId);
        var hacker = HackerState.Hacker;
        if (!target || target.Data.IsDead) return;

        HackerState.ExecuteUsed = true;
        HackerState.HackerExecutedThisMeeting = true;
        HackerState.PlantedTargetId = HackerState.None;

        // Dies without a body, same as an ejection.
        target.Exiled();
        if (Constants.ShouldPlaySfx()) SoundManager.Instance.PlaySound(target.KillSfx, false, 0.8f);

        var meeting = MeetingHud.Instance;
        if (meeting)
        {
            foreach (var area in meeting.playerStates)
            {
                if (area.TargetPlayerId == targetId)
                {
                    area.SetDead(area.DidReport, true);
                    area.Overlay.gameObject.SetActive(true);
                    area.NameText.text = ":( SUS_EXCEPTION";
                    area.NameText.color = new Color(0.1f, 0.45f, 1f);
                    area.UnsetVote();
                    MeetingButtons.Remove(area);
                }

                // Votes for the victim go back to their owners, and the Hacker loses theirs.
                var hackerArea = hacker && area.TargetPlayerId == hacker.PlayerId;
                if (area.VotedFor == targetId || hackerArea)
                {
                    area.UnsetVote();
                    var voter = Helpers.PlayerById(area.TargetPlayerId);
                    if (voter && voter.AmOwner) meeting.ClearVote();
                }

                if (hackerArea) HackerTicker.StartTwitch(area.transform);
            }

            if (AmongUsClient.Instance.AmHost) meeting.CheckForEndVoting();
        }

        if (target.AmOwner)
        {
            HudManager.Instance.KillOverlay.ShowKillAnimation(target.Data, target.Data);
        }

        Toast.Show("[SYSTEM] A crewmate's connection was terminated.", 4f, new Color(0.3f, 0.6f, 1f));
    }
}
