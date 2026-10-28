using System.Collections.Generic;
using Hazel;
using UnityEngine;

namespace HackerRole;

internal sealed class HackerSettings
{
    public float KillCooldownPenalty = 5f;
    public float HackCooldown = 30f;
    public byte MaxActiveGlitches = 3;
    public float GlitchMaxSeconds = 6f;
    public bool PacketTrace = true;
    public float CamDuration = 10f;
    public float CamRecharge = 25f;
    public bool BlueScreenEnabled = true;
    public byte MinAliveForExecute = 6;
    public float PlantTime = 0.75f;

    public static HackerSettings FromConfig() => new()
    {
        KillCooldownPenalty = HackerConfig.KillCooldownPenalty.Value,
        HackCooldown = HackerConfig.HackCooldown.Value,
        MaxActiveGlitches = (byte)Mathf.Clamp(HackerConfig.MaxActiveGlitches.Value, 1, 5),
        GlitchMaxSeconds = HackerConfig.GlitchMaxSeconds.Value,
        PacketTrace = HackerConfig.PacketTrace.Value,
        CamDuration = HackerConfig.CamDuration.Value,
        CamRecharge = HackerConfig.CamRecharge.Value,
        BlueScreenEnabled = HackerConfig.BlueScreenEnabled.Value,
        MinAliveForExecute = (byte)Mathf.Clamp(HackerConfig.MinAliveForExecute.Value, 4, 15),
        PlantTime = HackerConfig.PlantTime.Value,
    };

    public void Write(MessageWriter writer)
    {
        writer.Write(KillCooldownPenalty);
        writer.Write(HackCooldown);
        writer.Write(MaxActiveGlitches);
        writer.Write(GlitchMaxSeconds);
        writer.Write(PacketTrace);
        writer.Write(CamDuration);
        writer.Write(CamRecharge);
        writer.Write(BlueScreenEnabled);
        writer.Write(MinAliveForExecute);
        writer.Write(PlantTime);
    }

    public static HackerSettings Read(MessageReader reader) => new()
    {
        KillCooldownPenalty = reader.ReadSingle(),
        HackCooldown = reader.ReadSingle(),
        MaxActiveGlitches = reader.ReadByte(),
        GlitchMaxSeconds = reader.ReadSingle(),
        PacketTrace = reader.ReadBoolean(),
        CamDuration = reader.ReadSingle(),
        CamRecharge = reader.ReadSingle(),
        BlueScreenEnabled = reader.ReadBoolean(),
        MinAliveForExecute = reader.ReadByte(),
        PlantTime = reader.ReadSingle(),
    };
}

/// <summary>Game-wide Hacker state. Every client keeps the same copy, updated through <see cref="HackerRpc"/>.</summary>
internal static class HackerState
{
    public const byte None = byte.MaxValue;
    public const byte Hallway = byte.MaxValue;
    public static readonly Color HackerColor = new(0.2f, 1f, 0.4f);

    public static byte HackerId = None;
    public static HackerSettings Settings = new();

    /// <summary>Glitched player id -> room the Hacker was standing in when hacking.</summary>
    public static readonly Dictionary<byte, byte> PendingGlitches = new();

    public static byte PlantedTargetId = None;
    public static bool ExecuteUsed;
    public static bool PlantLocked;
    public static bool HackerExecutedThisMeeting;
    public static float MeetingStartTime;

    public static PlayerControl Hacker => HackerId == None ? null : Helpers.PlayerById(HackerId);

    public static bool IsHacker(PlayerControl player) => player && player.PlayerId == HackerId;

    public static bool LocalIsHacker => IsHacker(PlayerControl.LocalPlayer);

    public static bool LocalHackerAlive => LocalIsHacker && !PlayerControl.LocalPlayer.Data.IsDead;

    public static void Reset()
    {
        HackerId = None;
        PendingGlitches.Clear();
        PlantedTargetId = None;
        ExecuteUsed = false;
        PlantLocked = false;
        HackerExecutedThisMeeting = false;
    }
}

internal static class Helpers
{
    public static PlayerControl PlayerById(byte id)
    {
        foreach (var player in PlayerControl.AllPlayerControls)
        {
            if (player && player.PlayerId == id) return player;
        }

        return null;
    }

    public static bool IsAlive(PlayerControl player) =>
        player && player.Data != null && !player.Data.IsDead && !player.Data.Disconnected;

    public static string RoomName(byte roomId) =>
        roomId == HackerState.Hallway ? "HALLWAY" : ((SystemTypes)roomId).ToString().ToUpperInvariant();
}
