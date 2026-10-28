# 🖥️ Hacker Role in Among Us

> *"An Impostor who never needs to touch you. They mess with the ship, watch you on the cameras, and once per game they can wipe you out while everyone is talking."*

**The Hacker** is a new **Impostor** role for Among Us. It is designed as a community mod (BepInEx + Reactor) for **private lobbies where every player has the mod installed**.

> ⚠️ **Status: v0.1.0, first playable build, not yet playtested.** The code in [`src/`](src) implements all three abilities. CI compiles it against the game's reference assemblies, but nobody has played it in a real lobby yet. Expect bugs, and please report them.

---

## 📄 Documentation

| Document | Contents |
|---|---|
| [`HACKER_ROLE_DESIGN.md`](HACKER_ROLE_DESIGN.md) | Full design: mechanics, balance, counters, edge cases, lobby settings, visuals and sounds, technical architecture, delivery plan |
| [`src/`](src) | The mod's source code (C#, BepInEx + Reactor) |

---

## 🎭 The role at a glance

| | |
|---|---|
| **Team** | Impostor (takes one Impostor slot) |
| **Keeps** | Kill, sabotage, vents |
| **Trade-off** | Kill cooldown **+5 s** |

### Abilities

| Ability | What it does | Limits | Crew counter |
|---|---|---|---|
| 🐛 **Glitch Task** | From anywhere, makes a player's next task a silly variant (cheese card swipe, spaghetti wires, disco reactor, boomerang trash, dial-up upload) | 30 s cooldown, max 3 active, ≤ +6 s per task, never on sabotage fixes or visual tasks | **Packet Trace:** the victim sees which room the hack came from |
| 📱 **Pocket Cam** | Security cameras on a handheld tablet (Door Log on Mira, binoculars on Fungle) | 10 s use / 25 s recharge, can't move or kill while it's open | Camera red lights blink while Security is empty, and Comms sabotage blacks it out |
| 💀 **Blue Screen** | **PLANT** a bug on a player in person, then **EXECUTE** them during the next meeting | Once per game, blocked when ≤ 5 players are alive or the kill would end the game | The plant animation can be seen, and the Hacker loses their vote, which shows in the results |

---

## 🎮 How to play (everyone in the lobby must do this)

The mod is built for **Among Us v2024.10.29 (Steam)**, the same version the reference libraries target. Other game versions may fail to load it.

1. Install **BepInEx 6 (IL2CPP, build 697 or newer)** into your Among Us folder.
2. Install **[Reactor 2.3.1](https://github.com/NuclearPowered/Reactor/releases)**: put `Reactor.dll` in `Among Us/BepInEx/plugins/`.
3. Download **`HackerRole.dll`** from the latest successful [Build workflow run](../../actions/workflows/build.yml) (under *Artifacts*) and put it in `Among Us/BepInEx/plugins/`.
4. Start the game and host a **private lobby**. Reactor checks that every player has the mod and rejects players who don't.
5. Set at least 1 Impostor. When the game starts, one plain Impostor (not Shapeshifter or Phantom) becomes the Hacker.

### Controls (Hacker only)

| Key | Button | Action |
|---|---|---|
| `G` | HACK | Opens the target list. The next task that player opens gets glitched |
| `H` | CAM | Pocket Cam: the map's security view from anywhere |
| `J` | PLANT | Plants the bug on the closest crewmate in kill range |
| Click | EXECUTE (blue X on a vote card) | In the next meeting: kills the planted player |

### Host settings

After the first launch, edit `Among Us/BepInEx/config/com.favoritedevx.hackerrole.cfg`. The host's values are sent to everyone when the game starts: spawn chance, kill penalty, cooldowns, glitch cap, Pocket Cam battery, Blue Screen on/off, minimum players alive for EXECUTE, and plant time.

### Building from source

```bash
dotnet build src/HackerRole.csproj -c Release -p:AmongUs="/path/to/Among Us"
```

### Where v0.1.0 differs from the design spec

| Spec | v0.1.0 |
|---|---|
| Custom art and sound for each glitch (cheese card, noodles, disco ball, modem sound) | No custom assets yet. Glitches use transform and color effects plus text: mirrored card reader, wobbly tinted wires, rainbow reader, upside-down lever, dial-up messages |
| Target picker styled like the Shapeshifter menu | Simple on-screen list |
| BSOD art on the vote card | The vote card shows `:( SUS_EXCEPTION` in blue |
| Hacker count 0–2 | Always 1 |
| In-lobby settings menu | Config file on the host |
| Host checks every ability (anti-cheat) | Checked on the client that uses the ability, the same trust model as most role mods |
| Clear Asteroids can be glitched when Visual Tasks is OFF | Clear Asteroids, Scan and Prime Shields are never glitched |

---

## 🛠️ Technical stack

| Layer | Tool |
|---|---|
| Mod loader | BepInEx 6 (IL2CPP) |
| Networking / RPCs | Reactor |
| Role assignment | Vanilla Impostor, upgraded to Hacker by the host (no MiraAPI) |
| Patching | HarmonyX |
| Play mode | Private lobbies or a community *Impostor* server, with **all players on the same mod build** |

> Third-party mods **cannot** add roles to official public matchmaking. Only Innersloth can do that.

---

## 🗺️ Roadmap

- [x] Design document
- [x] **M1–M4 (code):** role, buttons, Pocket Cam, Glitch Task (5 variants + Packet Trace), Blue Screen
- [ ] **M1–M4 (testing):** check on all 5 maps and run the §5.4 edge cases with 4 clients
- [ ] Custom art and sounds, in-lobby settings menu
- [ ] **M5:** Balance playtests (20+ games, target Hacker win rate 45–55%)

---

## 🤝 Contributing

Ideas, balance feedback, and new glitch variants are welcome. Open an issue or a pull request. Each new glitched task should include:
1. a visual gag,
2. a mechanical twist,
3. a **mercy rule** that keeps the extra delay at **+6 s or less**.

---

## ⚖️ Disclaimer

This is an **unofficial fan project** and is not affiliated with or endorsed by Innersloth. *Among Us* is a trademark of Innersloth LLC. All art and sounds made for this project must be original. It will be distributed free and never monetized, in line with Innersloth's fan-content and modding guidelines.
