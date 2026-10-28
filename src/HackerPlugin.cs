using System;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using Reactor;
using Reactor.Networking;
using Reactor.Networking.Attributes;
using Reactor.Utilities.Attributes;
using UnityEngine;

namespace HackerRole;

[BepInPlugin(Id, "Hacker Role", Version)]
[BepInProcess("Among Us.exe")]
[BepInDependency(ReactorPlugin.Id)]
[ReactorModFlags(ModFlags.RequireOnAllClients)]
public class HackerPlugin : BasePlugin
{
    public const string Id = "com.favoritedevx.hackerrole";
    public const string Version = "0.1.0";

    public Harmony Harmony { get; } = new(Id);

    public override void Load()
    {
        HackerConfig.Bind(Config);
        Harmony.PatchAll();
        AddComponent<HackerBehaviour>();
        Log.LogInfo($"Hacker Role {Version} loaded");
    }
}

/// <summary>Host-side lobby settings, stored in BepInEx/config/com.favoritedevx.hackerrole.cfg.</summary>
internal static class HackerConfig
{
    public static ConfigEntry<int> SpawnChance;
    public static ConfigEntry<float> KillCooldownPenalty;
    public static ConfigEntry<float> HackCooldown;
    public static ConfigEntry<int> MaxActiveGlitches;
    public static ConfigEntry<float> GlitchMaxSeconds;
    public static ConfigEntry<bool> PacketTrace;
    public static ConfigEntry<float> CamDuration;
    public static ConfigEntry<float> CamRecharge;
    public static ConfigEntry<bool> BlueScreenEnabled;
    public static ConfigEntry<int> MinAliveForExecute;
    public static ConfigEntry<float> PlantTime;

    public static void Bind(ConfigFile config)
    {
        SpawnChance = config.Bind("Hacker", "SpawnChance", 100, "Chance (0-100) that one plain Impostor becomes the Hacker");
        KillCooldownPenalty = config.Bind("Hacker", "KillCooldownPenalty", 5f, "Seconds added to the Hacker's kill cooldown");
        HackCooldown = config.Bind("Hacker", "HackCooldown", 30f, "Glitch Task cooldown in seconds");
        MaxActiveGlitches = config.Bind("Hacker", "MaxActiveGlitches", 3, "Maximum glitches waiting at the same time");
        GlitchMaxSeconds = config.Bind("Hacker", "GlitchMaxSeconds", 6f, "Mercy rule: a glitch effect stops after this many seconds");
        PacketTrace = config.Bind("Hacker", "PacketTrace", true, "Tell the victim which room the hack came from");
        CamDuration = config.Bind("Hacker", "PocketCamDuration", 10f, "Pocket Cam battery in seconds");
        CamRecharge = config.Bind("Hacker", "PocketCamRecharge", 25f, "Pocket Cam full recharge time in seconds");
        BlueScreenEnabled = config.Bind("Hacker", "BlueScreenEnabled", true, "Enable the once-per-game meeting kill");
        MinAliveForExecute = config.Bind("Hacker", "MinAliveForExecute", 6, "EXECUTE is blocked below this many living players");
        PlantTime = config.Bind("Hacker", "PlantTime", 0.75f, "Seconds the PLANT animation freezes the Hacker");
    }
}

[RegisterInIl2Cpp]
public class HackerBehaviour : MonoBehaviour
{
    public HackerBehaviour(IntPtr ptr) : base(ptr)
    {
    }

    private void Update() => HackerTicker.Update();

    private void OnGUI() => TargetPicker.OnGUI();
}
