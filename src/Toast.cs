using TMPro;
using UnityEngine;

namespace HackerRole;

/// <summary>One line of text at the top of the screen. Used for Packet Trace, glitch jokes and system messages.</summary>
internal static class Toast
{
    private static TextMeshPro _text;
    private static float _hideAt;
    private static float _showAt;
    private static string _pendingText;
    private static Color _pendingColor;
    private static float _pendingDuration;

    public static void Show(string message, float duration, Color color, float delay = 0f)
    {
        _pendingText = message;
        _pendingColor = color;
        _pendingDuration = duration;
        _showAt = Time.time + delay;
    }

    public static void Update()
    {
        if (!HudManager.Instance) return;

        if (_pendingText != null && Time.time >= _showAt)
        {
            var text = GetOrCreate();
            text.text = _pendingText;
            text.color = _pendingColor;
            text.gameObject.SetActive(true);
            _hideAt = Time.time + _pendingDuration;
            _pendingText = null;
        }

        if (_text && _text.gameObject.activeSelf && Time.time >= _hideAt) _text.gameObject.SetActive(false);
    }

    private static TextMeshPro GetOrCreate()
    {
        if (_text) return _text;

        var hud = HudManager.Instance;
        _text = Object.Instantiate(hud.KillButton.cooldownTimerText, hud.transform);
        _text.name = "HackerToast";
        _text.enableWordWrapping = false;
        _text.alignment = TextAlignmentOptions.Center;
        _text.transform.localScale = Vector3.one * 0.55f;
        _text.transform.localPosition = new Vector3(0f, 2.2f, -100f);
        return _text;
    }
}
