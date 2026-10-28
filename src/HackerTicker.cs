using UnityEngine;

namespace HackerRole;

/// <summary>Per-frame timers that don't belong to a single button: animations, toasts, meeting transitions.</summary>
internal static class HackerTicker
{
    private static readonly int Outline = Shader.PropertyToID("_Outline");
    private static readonly int OutlineColor = Shader.PropertyToID("_OutlineColor");

    private static PlayerControl _plantPlayer;
    private static float _plantEndsAt;
    private static Transform _twitch;
    private static Vector3 _twitchBaseScale;
    private static float _twitchEndsAt;
    private static bool _wasInMeeting;

    public static void ResetAll()
    {
        GlitchTasks.Finish(false);
        TargetPicker.Close();
        PocketCam.Close();
        HackerHud.ResetCooldowns();
        EndPlantAnimation();
        _twitch = null;
        _wasInMeeting = false;
    }

    /// <summary>The USB cable moment: the Hacker glows green and freezes. Visible to everyone, including on cameras.</summary>
    public static void StartPlantAnimation(PlayerControl hacker, float duration)
    {
        if (!hacker) return;
        EndPlantAnimation();
        _plantPlayer = hacker;
        _plantEndsAt = Time.time + duration;
        SetOutline(hacker, true);
        if (hacker.AmOwner)
        {
            hacker.moveable = false;
            hacker.NetTransform.Halt();
        }
    }

    /// <summary>The Hacker's vote card glitches for a moment when they execute.</summary>
    public static void StartTwitch(Transform voteArea)
    {
        _twitch = voteArea;
        _twitchBaseScale = voteArea.localScale;
        _twitchEndsAt = Time.time + 0.3f;
    }

    public static void Update()
    {
        Toast.Update();
        if (AmongUsClient.Instance == null || AmongUsClient.Instance.GameState != InnerNet.InnerNetClient.GameStates.Started) return;

        GlitchTasks.Update();

        if (_plantPlayer && Time.time >= _plantEndsAt) EndPlantAnimation();

        if (_twitch)
        {
            if (Time.time < _twitchEndsAt)
            {
                _twitch.localScale = _twitchBaseScale + (Vector3)(Random.insideUnitCircle * 0.05f);
            }
            else
            {
                _twitch.localScale = _twitchBaseScale;
                _twitch = null;
            }
        }

        bool inMeeting = MeetingHud.Instance;
        if (_wasInMeeting && !inMeeting) OnMeetingEnded();
        _wasInMeeting = inMeeting;
    }

    public static void OnMeetingStarted()
    {
        HackerState.MeetingStartTime = Time.time;
        HackerState.HackerExecutedThisMeeting = false;
        HackerState.PendingGlitches.Clear();
        GlitchTasks.Finish(false);
        TargetPicker.Close();
        if (HackerHud.CamButton is { EffectActive: true }) HackerHud.CamButton.EndEffect();
        EndPlantAnimation();
    }

    private static void OnMeetingEnded()
    {
        HackerState.HackerExecutedThisMeeting = false;

        // An unused plant expires after one meeting, and PLANT stays locked for one more round.
        HackerState.PlantLocked = false;
        if (HackerState.PlantedTargetId != HackerState.None)
        {
            HackerState.PlantedTargetId = HackerState.None;
            HackerState.PlantLocked = !HackerState.ExecuteUsed;
        }

        if (HackerHud.HackButton != null) HackerHud.HackButton.Timer = HackerState.Settings.HackCooldown;
    }

    private static void EndPlantAnimation()
    {
        if (!_plantPlayer) return;
        SetOutline(_plantPlayer, false);
        if (_plantPlayer.AmOwner) _plantPlayer.moveable = true;
        _plantPlayer = null;
    }

    private static void SetOutline(PlayerControl player, bool on)
    {
        var body = player.cosmetics?.currentBodySprite?.BodySprite;
        if (body == null) return;
        body.material.SetFloat(Outline, on ? 1f : 0f);
        body.material.SetColor(OutlineColor, HackerState.HackerColor);
    }
}
