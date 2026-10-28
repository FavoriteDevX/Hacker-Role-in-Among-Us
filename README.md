# 🖥️ Hacker Role in Among Us

> *"An Impostor who never needs to touch you. They mess with the ship, watch you on the cameras, and once per game they can wipe you out while everyone is talking."*

**The Hacker** is a new **Impostor** role for Among Us. It is designed as a community mod (BepInEx + Reactor) for **private lobbies where every player has the mod installed**.

> ⚠️ **Status: design phase.** This repository has the full design spec. The mod has not been built or playtested yet. See the [Roadmap](#-roadmap).

---

## 📄 Documentation

| Document | Contents |
|---|---|
| [`HACKER_ROLE_DESIGN.md`](HACKER_ROLE_DESIGN.md) | Full design: mechanics, balance, counters, edge cases, lobby settings, visuals and sounds, technical architecture, delivery plan |

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

## 🛠️ Planned technical stack

| Layer | Tool |
|---|---|
| Mod loader | BepInEx 6 (IL2CPP) |
| Networking / RPCs | Reactor |
| Role registration | MiraAPI (or a custom `RoleBehaviour`) |
| Patching | HarmonyX |
| Play mode | Private lobbies or a community *Impostor* server, with **all players on the same mod build** |

> Third-party mods **cannot** add roles to official public matchmaking. Only Innersloth can do that.

---

## 🗺️ Roadmap

- [x] Design document
- [ ] **M1:** Role and buttons, plus Pocket Cam on all 5 maps
- [ ] **M2:** Glitch Task with Wiring and Card Swipe variants
- [ ] **M3:** Remaining 3 variants and Packet Trace
- [ ] **M4:** Blue Screen, with every edge case tested on 4 clients
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
