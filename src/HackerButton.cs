using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace HackerRole;

/// <summary>An extra HUD button, cloned from the vanilla kill button.</summary>
internal sealed class HackerButton
{
    private static readonly List<HackerButton> All = new();
    private static readonly int Desat = Shader.PropertyToID("_Desat");

    private readonly ActionButton _button;
    private readonly Func<bool> _visible;
    private readonly Func<bool> _usable;
    private readonly Action _onClick;
    private readonly Vector3 _offset;
    private readonly KeyCode _hotkey;

    public Func<string> Label;
    public float Timer;
    public float MaxTimer;
    public bool EffectActive;
    public float EffectTimer;
    public float EffectDuration;
    public Action OnEffectEnd;

    public HackerButton(HudManager hud, string label, Vector3 offset, KeyCode hotkey, Func<bool> visible, Func<bool> usable, Action onClick)
    {
        Label = () => label;
        _offset = offset;
        _hotkey = hotkey;
        _visible = visible;
        _usable = usable;
        _onClick = onClick;

        _button = UnityEngine.Object.Instantiate(hud.KillButton, hud.KillButton.transform.parent);
        _button.name = "Hacker" + label + "Button";
        var passive = _button.GetComponent<PassiveButton>();
        passive.OnClick = new Button.ButtonClickedEvent();
        passive.OnClick.AddListener((UnityEngine.Events.UnityAction)Click);
        _button.gameObject.SetActive(false);
        All.Add(this);
    }

    public void StartEffect(float duration)
    {
        EffectActive = true;
        EffectDuration = duration;
        EffectTimer = duration;
    }

    public void EndEffect()
    {
        if (!EffectActive) return;
        EffectActive = false;
        OnEffectEnd?.Invoke();
    }

    private void Click()
    {
        if (Timer > 0f || EffectActive || !_visible() || !_usable()) return;
        _onClick();
    }

    private void Update(HudManager hud)
    {
        var local = PlayerControl.LocalPlayer;
        var show = local && local.Data != null && _visible() && !MeetingHud.Instance && !ExileController.Instance
                   && (hud.UseButton.isActiveAndEnabled || hud.PetButton.isActiveAndEnabled);
        _button.gameObject.SetActive(show);
        if (!show) return;

        _button.transform.localPosition = hud.UseButton.transform.localPosition + _offset;
        _button.OverrideText(Label());

        if (EffectActive)
        {
            EffectTimer -= Time.deltaTime;
            _button.SetCoolDown(EffectTimer, EffectDuration);
            if (EffectTimer <= 0f) EndEffect();
        }
        else
        {
            if (Timer > 0f && local.moveable && !local.inVent) Timer -= Time.deltaTime;
            _button.SetCoolDown(Timer, MaxTimer);
        }

        var ready = !EffectActive && Timer <= 0f && _usable();
        _button.graphic.color = _button.buttonLabelText.color = ready ? Palette.EnabledColor : Palette.DisabledClear;
        _button.graphic.material.SetFloat(Desat, ready ? 0f : 1f);

        if (Input.GetKeyDown(_hotkey)) Click();
    }

    public static void UpdateAll(HudManager hud)
    {
        All.RemoveAll(b => !b._button);
        foreach (var button in All) button.Update(hud);
    }
}
