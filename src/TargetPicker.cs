using UnityEngine;

namespace HackerRole;

/// <summary>Simple on-screen list for choosing who gets the next glitched task.</summary>
internal static class TargetPicker
{
    private static bool _open;

    public static void Open() => _open = true;

    public static void Close() => _open = false;

    public static void OnGUI()
    {
        if (!_open) return;
        if (!HackerState.LocalHackerAlive || MeetingHud.Instance)
        {
            _open = false;
            return;
        }

        GUILayout.BeginArea(new Rect(Screen.width / 2f - 160f, Screen.height / 2f - 220f, 320f, 440f), GUI.skin.box);
        GUILayout.Label(">_ HACK: choose whose next task to glitch");

        foreach (var player in PlayerControl.AllPlayerControls)
        {
            if (!Helpers.IsAlive(player) || player.Data.Role.IsImpostor) continue;
            if (HackerState.PendingGlitches.ContainsKey(player.PlayerId)) continue;

            if (GUILayout.Button(player.Data.PlayerName))
            {
                HackerNet.RpcHackTask(player.PlayerId, CurrentRoom());
                HackerHud.HackButton.MaxTimer = HackerHud.HackButton.Timer = HackerState.Settings.HackCooldown;
                Toast.Show($"Payload queued for {player.Data.PlayerName}", 2f, HackerState.HackerColor);
                _open = false;
            }
        }

        if (GUILayout.Button("Cancel")) _open = false;
        GUILayout.EndArea();
    }

    private static byte CurrentRoom()
    {
        var room = HudManager.Instance.roomTracker ? HudManager.Instance.roomTracker.LastRoom : null;
        return room ? (byte)room.RoomId : HackerState.Hallway;
    }
}
