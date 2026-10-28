using System.Linq;
using UnityEngine;

namespace HackerRole;

/// <summary>
/// Opens the map's real surveillance minigame from anywhere. Because it is the vanilla minigame,
/// the camera tally lights and the Comms blackout behave exactly as at the Security console.
/// </summary>
internal static class PocketCam
{
    private static Minigame _minigame;

    public static bool IsOpen => _minigame && Minigame.Instance && Minigame.Instance == _minigame;

    public static bool Open()
    {
        if (Minigame.Instance || !Camera.main) return false;

        var console = FindSecurityConsole();
        if (console == null) return false;

        _minigame = Object.Instantiate(console.MinigamePrefab, Camera.main.transform, false);
        _minigame.transform.localPosition = new Vector3(0f, 0f, -50f);
        _minigame.Begin(null);

        PlayerControl.LocalPlayer.moveable = false;
        PlayerControl.LocalPlayer.NetTransform.Halt();
        return true;
    }

    public static void Close()
    {
        if (IsOpen) _minigame.ForceClose();
        _minigame = null;
        if (PlayerControl.LocalPlayer) PlayerControl.LocalPlayer.moveable = true;
    }

    private static SystemConsole FindSecurityConsole()
    {
        var consoles = Object.FindObjectsOfType<SystemConsole>();
        var name = GameOptionsManager.Instance.currentNormalGameOptions.MapId switch
        {
            0 or 3 => "SurvConsole", // The Skeld / dleks
            1 => "SurvLogConsole", // Mira HQ: door log
            2 => "Surv_Panel", // Polus
            4 => "task_cams", // Airship
            5 => "BinocularsSecurityConsole", // The Fungle
            _ => "Surv",
        };

        return consoles.FirstOrDefault(c => c.gameObject.name.Contains(name))
               ?? consoles.FirstOrDefault(c => c.gameObject.name.Contains("Surv") || c.gameObject.name.Contains("Cam"));
    }
}
