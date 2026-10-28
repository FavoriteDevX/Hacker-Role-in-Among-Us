using System.Collections.Generic;
using HarmonyLib;
using Hazel;
using UnityEngine;

namespace HackerRole;

/// <summary>The EXECUTE button the Hacker sees on the planted player's vote card.</summary>
internal static class MeetingButtons
{
    private const string ButtonName = "HackerExecuteButton";
    private static readonly Dictionary<byte, GameObject> Buttons = new();

    public static void Add(MeetingHud meeting)
    {
        Buttons.Clear();
        if (!HackerState.LocalHackerAlive || HackerState.ExecuteUsed || HackerState.PlantedTargetId == HackerState.None) return;

        foreach (var area in meeting.playerStates)
        {
            if (area.AmDead || area.TargetPlayerId != HackerState.PlantedTargetId) continue;
            if (area.transform.Find(ButtonName)) continue;

            var template = area.Buttons.transform.Find("CancelButton").gameObject;
            var button = Object.Instantiate(template, area.transform);
            button.name = ButtonName;
            button.transform.localPosition = new Vector3(-0.95f, 0.03f, -1.3f);
            button.GetComponent<SpriteRenderer>().color = new Color(0.1f, 0.45f, 1f);

            var targetId = area.TargetPlayerId;
            var passive = button.GetComponent<PassiveButton>();
            passive.OnClick.RemoveAllListeners();
            passive.OnClick.AddListener((System.Action)(() =>
            {
                if (!CanExecute(meeting, out var reason))
                {
                    Toast.Show(reason, 2.5f, Color.yellow);
                    return;
                }

                HackerNet.RpcExecute(targetId);
            }));

            Buttons[targetId] = button;
        }
    }

    public static void Remove(PlayerVoteArea area)
    {
        if (!Buttons.TryGetValue(area.TargetPlayerId, out var button)) return;
        if (button) Object.Destroy(button);
        Buttons.Remove(area.TargetPlayerId);
    }

    public static void Update(MeetingHud meeting)
    {
        foreach (var button in Buttons.Values)
        {
            if (!button) continue;
            var visible = HackerState.LocalHackerAlive && !HackerState.ExecuteUsed
                          && meeting.state is not (MeetingHud.VoteStates.Results or MeetingHud.VoteStates.Proceeding);
            button.SetActive(visible);
            if (visible)
            {
                // Pulse blue while usable, dim while the time window is closed.
                var ready = CanExecute(meeting, out _);
                var pulse = ready ? 0.75f + Mathf.PingPong(Time.time * 2f, 0.25f) : 0.35f;
                button.GetComponent<SpriteRenderer>().color = new Color(0.1f * pulse, 0.45f * pulse, pulse);
            }
        }
    }

    public static bool CanExecute(MeetingHud meeting, out string reason)
    {
        reason = null;
        var options = GameOptionsManager.Instance.currentNormalGameOptions;
        var elapsed = Time.time - HackerState.MeetingStartTime;

        if (elapsed < 15f)
        {
            reason = "Compiling payload... (wait 15 s into the meeting)";
            return false;
        }

        if (options.VotingTime > 0 && elapsed > options.DiscussionTime + options.VotingTime - 10f)
        {
            reason = "Too late: the vote is closing.";
            return false;
        }

        int alive = 0, impostors = 0;
        foreach (var player in PlayerControl.AllPlayerControls)
        {
            if (!Helpers.IsAlive(player)) continue;
            alive++;
            if (player.Data.Role.IsImpostor) impostors++;
        }

        if (alive < HackerState.Settings.MinAliveForExecute)
        {
            reason = $"Blocked: needs at least {HackerState.Settings.MinAliveForExecute} players alive.";
            return false;
        }

        // A meeting kill must never end the game on its own.
        if (impostors >= alive - impostors - 1)
        {
            reason = "Blocked: this would end the game.";
            return false;
        }

        return meeting.state is not (MeetingHud.VoteStates.Results or MeetingHud.VoteStates.Proceeding);
    }
}

[HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Start))]
internal static class MeetingStartPatch
{
    public static void Postfix() => HackerTicker.OnMeetingStarted();
}

// The host builds vote cards in ServerStart, clients in the first Deserialize.
[HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.ServerStart))]
internal static class MeetingServerStartPatch
{
    public static void Postfix(MeetingHud __instance) => MeetingButtons.Add(__instance);
}

[HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Deserialize))]
internal static class MeetingDeserializePatch
{
    public static void Postfix(MeetingHud __instance, [HarmonyArgument(0)] MessageReader reader, [HarmonyArgument(1)] bool initialState)
    {
        if (initialState) MeetingButtons.Add(__instance);
    }
}

[HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Update))]
internal static class MeetingUpdatePatch
{
    public static void Postfix(MeetingHud __instance) => MeetingButtons.Update(__instance);
}

/// <summary>After EXECUTE the Hacker cannot vote again in that meeting (enforced by the host).</summary>
[HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.CastVote))]
internal static class CastVotePatch
{
    public static bool Prefix([HarmonyArgument(0)] byte srcPlayerId) =>
        !(HackerState.HackerExecutedThisMeeting && srcPlayerId == HackerState.HackerId);
}

[HarmonyPatch(typeof(PlayerVoteArea), nameof(PlayerVoteArea.Select))]
internal static class VoteSelectPatch
{
    public static bool Prefix() => !(HackerState.HackerExecutedThisMeeting && HackerState.LocalIsHacker);
}
