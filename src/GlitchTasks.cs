using System.Linq;
using HarmonyLib;
using UnityEngine;

namespace HackerRole;

/// <summary>
/// Glitch Task: the next normal task a hacked player opens gets a silly twist. Every effect is
/// purely visual or spatial on the local minigame and stops after <see cref="HackerSettings.GlitchMaxSeconds"/>
/// (the mercy rule), so a task can never get stuck.
/// </summary>
internal static class GlitchTasks
{
    private static ActiveGlitch _active;

    public static void TryStart(Minigame minigame, PlayerTask task)
    {
        var local = PlayerControl.LocalPlayer;
        if (_active != null || !local || task == null) return;
        if (!HackerState.PendingGlitches.TryGetValue(local.PlayerId, out var room)) return;

        // Sabotage fixes are not NormalPlayerTasks, so they are never glitched.
        if (task.TryCast<NormalPlayerTask>() == null) return;

        // Visual tasks prove innocence, so they are never touched.
        if (task.TaskType is TaskTypes.SubmitScan or TaskTypes.ClearAsteroids or TaskTypes.PrimeShields) return;

        _active = new ActiveGlitch(minigame, task.TaskType, room);
    }

    public static void Update()
    {
        if (_active == null) return;
        if (!_active.Update()) Finish(true);
    }

    /// <summary>Called when the minigame closes, or with sendTrace = false when a meeting wipes all glitches.</summary>
    public static void Finish(bool sendTrace)
    {
        if (_active == null) return;
        var glitch = _active;
        _active = null;
        glitch.Restore();

        if (!sendTrace) return;
        if (HackerState.Settings.PacketTrace)
        {
            Toast.Show($"⚠ Unauthorized access traced to: {Helpers.RoomName(glitch.Room)}", 5f, new Color(1f, 0.55f, 0.1f));
        }

        HackerNet.RpcGlitchConsumed(PlayerControl.LocalPlayer.PlayerId);
    }

    private sealed class ActiveGlitch
    {
        private static readonly string[] CardLines =
        {
            "Too fast. Are you an Impostor?",
            "Too slow. My grandma swipes faster.",
            "Card declined: insufficient vibes.",
            "...fine. Accepted.",
        };

        private static readonly string[] DialUpLines =
        {
            "Dialing... *SCREEEECH*",
            "Estimated time: 3 years, 2 months",
            "Connection lost. Retry? [YES] [ALSO YES]",
            "Reconnected at 56k. Living the dream.",
        };

        private readonly Minigame _game;
        private readonly TaskTypes _type;
        private readonly float _started;
        private readonly Vector3 _basePosition;
        private readonly Vector3 _baseScale;
        private readonly Quaternion _baseRotation;
        private readonly SpriteRenderer[] _renderers;
        private readonly Color[] _baseColors;
        private bool _restored;
        private int _line;
        private float _nextLine;

        public byte Room { get; }

        public ActiveGlitch(Minigame game, TaskTypes type, byte room)
        {
            _game = game;
            _type = type;
            Room = room;
            _started = Time.time;

            var transform = game.transform;
            _basePosition = transform.localPosition;
            _baseScale = transform.localScale;
            _baseRotation = transform.localRotation;
            _renderers = game.GetComponentsInChildren<SpriteRenderer>(true).ToArray();
            _baseColors = _renderers.Select(r => r.color).ToArray();

            switch (_type)
            {
                case TaskTypes.SwipeCard:
                    // Picky card reader: everything is mirrored, so you swipe the "wrong" way.
                    transform.localScale = new Vector3(-_baseScale.x, _baseScale.y, _baseScale.z);
                    Tint(new Color(1f, 0.85f, 0.3f)); // cheese
                    break;
                case TaskTypes.EmptyGarbage:
                case TaskTypes.EmptyChute:
                    // Boomerang trash: the lever is upside down, pull it UP.
                    transform.localScale = new Vector3(_baseScale.x, -_baseScale.y, _baseScale.z);
                    Toast.Show("Lever installed upside down. Pull UP.", 3f, Color.white);
                    break;
                case TaskTypes.FixWiring:
                    Tint(new Color(1f, 0.9f, 0.6f)); // spaghetti
                    Toast.Show("Spaghetti Protocol engaged 🍝", 3f, Color.white);
                    break;
                case TaskTypes.StartReactor:
                    Toast.Show("♫ DISCO REACTOR ♫", 3f, Color.magenta);
                    break;
            }

            // A short static shake tells the player something is off.
            _nextLine = Time.time;
        }

        /// <summary>Returns false once the minigame has closed.</summary>
        public bool Update()
        {
            if (!_game || !_game.gameObject.activeInHierarchy || Minigame.Instance != _game) return false;
            if (_restored) return true;

            var elapsed = Time.time - _started;
            if (elapsed >= HackerState.Settings.GlitchMaxSeconds)
            {
                Restore();
                return true;
            }

            var transform = _game.transform;

            // First 0.3 s: chromatic "static" jitter on every variant.
            if (elapsed < 0.3f)
            {
                transform.localPosition = _basePosition + (Vector3)(Random.insideUnitCircle * 0.08f);
                return true;
            }

            switch (_type)
            {
                case TaskTypes.SwipeCard:
                    NextLine(CardLines, 1.4f);
                    break;
                case TaskTypes.FixWiring:
                    // Wobbly noodles.
                    transform.localPosition = _basePosition + new Vector3(Mathf.Sin(elapsed * 6f) * 0.15f, Mathf.Cos(elapsed * 5f) * 0.1f, 0f);
                    break;
                case TaskTypes.StartReactor:
                    var disco = Color.HSVToRGB(elapsed * 1.5f % 1f, 0.5f, 1f);
                    Tint(disco);
                    transform.localRotation = _baseRotation * Quaternion.Euler(0f, 0f, Mathf.Sin(elapsed * 4f) * 4f);
                    break;
                case TaskTypes.UploadData:
                    NextLine(DialUpLines, 1.4f);
                    transform.localPosition = _basePosition + (Vector3)(Random.insideUnitCircle * 0.02f);
                    break;
                case TaskTypes.EmptyGarbage:
                case TaskTypes.EmptyChute:
                    break;
                default:
                    transform.localPosition = _basePosition + new Vector3(Mathf.Sin(elapsed * 4f) * 0.1f, 0f, 0f);
                    break;
            }

            return true;
        }

        public void Restore()
        {
            if (_restored || !_game) return;
            _restored = true;
            var transform = _game.transform;
            transform.localPosition = _basePosition;
            transform.localScale = _baseScale;
            transform.localRotation = _baseRotation;
            for (var i = 0; i < _renderers.Length; i++)
            {
                if (_renderers[i]) _renderers[i].color = _baseColors[i];
            }
        }

        private void Tint(Color tint)
        {
            for (var i = 0; i < _renderers.Length; i++)
            {
                if (_renderers[i]) _renderers[i].color = _baseColors[i] * tint;
            }
        }

        private void NextLine(string[] lines, float interval)
        {
            if (_line >= lines.Length || Time.time < _nextLine) return;
            Toast.Show(lines[_line++], interval, Color.white);
            _nextLine = Time.time + interval;
        }
    }
}

[HarmonyPatch(typeof(Minigame), nameof(Minigame.Begin))]
internal static class MinigameBeginPatch
{
    public static void Postfix(Minigame __instance, [HarmonyArgument(0)] PlayerTask task) => GlitchTasks.TryStart(__instance, task);
}
