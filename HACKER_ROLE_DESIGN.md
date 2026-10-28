# The Hacker — Role Design Document

> **Status:** Design spec, v1.0. Not implemented or playtested yet.
> **Target:** A client-side mod for Among Us (BepInEx + Reactor), played in **private, fully modded lobbies**.
> Innersloth does not let third parties add roles to the official game. See [§9 Feasibility](#9-feasibility-what-executable-means-here).

---

## 0. One-line pitch

*"An Impostor who never needs to touch you. They mess with the ship, watch you on the cameras, and once per game they can wipe you out while everyone is talking."*

---

## 1. Team assignment

**Team: Impostor.** It is an Impostor role, like Shapeshifter or Phantom, and it takes one Impostor slot.

| Option | Verdict | Why |
|---|---|---|
| Crewmate | ❌ | A crewmate who kills other crewmates during meetings breaks the crew's win condition. Messing up tasks also only helps the Impostors. |
| Neutral | ⚠️ Possible later | It would need its own win condition, such as "survive until X tasks are hacked". That is a different role. The design is kept below in §8.3 for later. |
| **Impostor** | ✅ | All three abilities make the crew slower, give the Hacker information, or cause confusion. That is exactly what an Impostor needs. |

The Hacker **keeps the normal Impostor kit**: kill, sabotage, and vents. The new abilities are added to it. To keep it from getting too strong, the normal kill cooldown gets a **+5 s penalty** (see §6).

---

## 2. Ability overview

| # | Ability | Button | Cooldown | Uses | Visible to others? |
|---|---|---|---|---|---|
| A | **Glitch Task** (task manipulation) | `HACK` | 30 s | 3 hacks active at once | The victim learns the **room the hack came from** |
| B | **Pocket Cam** (cameras from anywhere) | `CAM` | Battery: 10 s use / 25 s recharge | Unlimited | Camera red lights blink, as in vanilla |
| C | **Blue Screen** (kill during a meeting) | `PLANT` in the round, then `EXECUTE` in the meeting | — | **Once per game** | A planting animation; a death everyone sees; a missing vote |

Every value is a host lobby setting (see §7).

---

## 3. Ability A: Glitch Task

### 3.1 How it works

1. The Hacker presses `HACK` from **anywhere on the map** (no range limit).
2. A target picker opens, laid out like the Shapeshifter menu. It shows living players as icons. The Hacker cannot pick themselves or another Impostor.
3. The Hacker picks a player. The **next task that player opens** gets a *glitched variant* for that one attempt.
4. When the glitched task closes, whether finished or not, the glitch is used up.

### 3.2 Limits

- **Cooldown 30 s.** It shares no timer with the kill.
- **At most 3 glitches** waiting at once, and **at most 1 per target**.
- **Time cap:** a glitched task can add **6 s at most** to its normal completion time. Each variant below has a built-in "mercy rule" that ends the extra trouble. Glitches are meant to be funny and slow players down a little. They must never turn into a soft-lock.
- **No effect on sabotage fixes** (Reactor, O2, Lights, Comms). Glitching those would make the Hacker's sabotage impossible to stop.
- **Visual tasks are immune** (Medbay scan, Clear Asteroids with visuals on, Empty Garbage when it shows the animation). Visual tasks exist to clear players, so letting the Hacker fake or ruin that proof would break deduction. *Exception:* Clear Asteroids **in the cockpit view only** can be glitched when "Visual Tasks" is OFF in the lobby.
- Glitches are **wiped when a meeting is called**, so the ship starts clean every round.

### 3.3 Counterplay: "Packet Trace"

When a glitched task closes, the victim gets a small toast message:

> `⚠ Unauthorized access traced to: ELECTRICAL`

This shows the **room the Hacker was standing in when they pressed HACK** ("Hallway" if they were between rooms). Hacking from anywhere is powerful, but it leaks the Hacker's position at an earlier moment. Players can compare that with alibis at the meeting.

### 3.4 Five glitched task variants (with mechanics)

#### 1. Swipe Card → *"The Picky Card Reader"* (Admin)
- **What you see:** the card is now a **slice of cheese**. The reader screen shows sassy, made-up messages.
- **How it plays:** the "correct speed" window **moves randomly after each failed swipe**. Each fail shows a new message, in order:
  1. `Too fast. Are you an Impostor?`
  2. `Too slow. My grandma swipes faster.`
  3. `Card declined: insufficient vibes.`
  4. `...fine. Accepted.`
- **Mercy rule:** the **4th attempt always succeeds**. The worst case is about +5 s.
- **Sound:** a cheese squish on each swipe, then a sad trombone.

#### 2. Fix Wiring → *"Spaghetti Protocol"* (Electrical, Storage, etc.)
- **What you see:** the wires become **spaghetti noodles**. All four colors become slightly different pasta shades (penne yellow, marinara red, pesto green, squid-ink blue). A tiny color tag still appears at each end so colorblind players can play it.
- **How it plays:** the **left column is mirrored top to bottom**. While dragging, the noodle **wobbles** with a sine-wave delay, so you have to aim more carefully.
- **Mercy rule:** a noodle snaps into place if you let go within **1.5×** the normal snap radius. Worst case is about +4 s.
- **Sound:** wet *slurp* on each connection. An Italian accordion sting when the task is done.

#### 3. Start Reactor (Simon Says) → *"Disco Reactor"* (Reactor)
- **What you see:** a disco ball drops down. The buttons glow in rainbow colors.
- **How it plays:** after each round of the sequence, the **3×3 button grid shuffles its positions**, with a quick animation the player can follow. The **sequence itself does not change**: the player has to follow the *buttons*, not the *spots*.
- **Mercy rule:** the shuffle only happens on rounds 1–3. Rounds 4–5 play normally. Worst case is about +5 s.
- **Sound:** a 4-bar disco loop. The record scratches if you make a mistake.

#### 4. Empty Garbage / Empty Chute → *"Boomerang Trash"* (Cafeteria, O2, Storage)
- **What you see:** the lever is **upside down**, so you have to pull it **UP**.
- **How it plays:** on the first pull, the trash goes down, then **flies back up** with a *boing*, and a fish lands on the glass. The second pull works.
- **Mercy rule:** at most one boomerang. Worst case is about +3 s.
- **Rule note:** this variant only applies to the **cockpit (minigame) view**. The outside chute animation that others can see stays exactly the same, so it does not mess with visual proof (§3.2).

#### 5. Download / Upload Data → *"Dial-Up Mode"* (any upload terminal)
- **What you see:** the progress bar becomes a **1998 dial-up window**. The modem screeches, and the time estimate is silly (`Estimated time: 3 years, 2 months`).
- **How it plays:** the bar fills normally until **70%**. Then a pop-up appears: `Connection lost. Retry? [YES] [ALSO YES]`. Clicking either button continues from **60%**.
- **Mercy rule:** it only disconnects once. Worst case is about +4 s.
- **Sound:** the full modem handshake sound, cut short by a "you've got mail" style ding (a new sound, not the copyrighted AOL one).

> **Bonus variant ideas for later:** *Fuel Engines* where the gas can is a juice box with a straw, *Align Engine Output* where the slider runs away from your cursor at 50% speed, and *Divert Power* where the switches are labeled in Comic Sans with wrong names.

---

## 4. Ability B: Pocket Cam

### 4.1 How it works

- Pressing `CAM` takes out a **cracked handheld tablet**: a sprite on the Hacker's character plus a full-screen overlay.
- The overlay shows **the same view the map's Security console shows**:

| Map | View shown |
|---|---|
| The Skeld | 4-camera grid (`SurveillanceMinigame`) |
| Polus | Paged cameras (`PlanetSurveillanceMinigame`) |
| Airship | Paged cameras (same type as Polus) |
| Mira HQ | **Door Log** (Mira has no cameras) |
| The Fungle | **Binoculars** view (Fungle's surveillance) |

### 4.2 Limits

- **Battery:** 10 s of watching, then a **25 s recharge**. You can close it early, and leftover charge is kept.
- **The Hacker cannot move** while the tablet is open. This is the same as standing at a Security console, so you can't scout and run at once.
- **Kill, vent, and sabotage are turned off** while it is open, and for **1 s after** closing it.
- **Comms sabotage blacks out the tablet**, the same as the real cameras. The Hacker's own Comms sabotage blinds them too.
- **Counterplay ("Tally light"):** while the tablet is open, the **red lights on the cameras blink**, exactly as in vanilla when someone uses Security. If a crewmate sees blinking cams while Security is empty, that is a tell. It helps the crew without saying who the Hacker is.

### 4.3 Visuals

- The tablet sprite has a cracked screen and a sticker of a little crewmate wearing sunglasses.
- When it opens, there is green scan-line noise (CRT effect), then the feed comes in. When the battery hits 0, there is a "low battery" beep and the feed shrinks to a dot, like an old TV turning off.

---

## 5. Ability C: Blue Screen (kill during a meeting)

This is the ability that most needs limits. It takes **two steps**, so the Hacker has to **take a physical risk during the round** before getting a safe moment in the meeting.

### 5.1 Step 1: PLANT (during the round)

- Range: **normal kill range**. The Hacker has to walk up to the target.
- Animation: **0.75 s**. The Hacker plugs a USB cable into the target's suit. Anyone who can see them sees it, including on cameras.
- Cost: **resets the Hacker's kill cooldown**. Planting uses the same window you would use for a kill.
- Only **1 plant can exist** at a time. It lasts until the **end of the next meeting**. If it isn't used, the plant is lost and the ability unlocks again after **1 full round**.
- The target is **not told** they were planted.

### 5.2 Step 2: EXECUTE (in the meeting)

- An `EXECUTE` icon (a small skull made of `0`s and `1`s) shows **only to the Hacker**, on the planted player's vote card.
- **Usable only** from **15 s after discussion starts** until **10 s before voting ends**. This means the first claims can't be cut off instantly, and the vote can't be wrecked at the last second.
- **Blocked** when, at the start of the meeting:
  - there are **5 or fewer** living players (this is a host setting), **or**
  - the kill would hand the Impostors an immediate win (Impostors ≥ crew).
  *Reason:* without this rule, a meeting kill decides the game, with no chance to respond.
- **Once per game** for each Hacker.

### 5.3 What happens

1. The target's vote card does a **0.5 s glitch flicker**, then turns into a full **Blue Screen of Death**:
   ```
   :(  Your crewmate ran into a problem and needs to restart.
       STOP CODE: SUS_EXCEPTION
   ```
2. The target's crewmate icon falls apart into pixels, with a short data-crunch sound.
3. The target is dead (a ghost). **No body is left** on the map.
4. **Votes are fixed:**
   - The target's own vote is **removed**.
   - Votes **for** the target are **given back**, and those players can vote again.
5. **The Hacker loses their vote for this meeting.** If they already voted, it is removed. If vote results are not anonymous, the Hacker's **missing vote icon** in the results is a clue anyone can see.
6. Chat shows a system message to everyone: `[SYSTEM] A crewmate's connection was terminated.` It never names who did it.

### 5.4 Edge cases

| Situation | Resolution |
|---|---|
| The Hacker is **ejected in the same meeting** | The kill still counts. The plant was already used. |
| The Hacker **dies before the meeting** | The plant is cancelled and EXECUTE never shows up. |
| The target is the person who called the meeting / reported a body | Still allowed. Their report has already happened. |
| **Another Impostor** is planted | Not allowed. Impostors can't be targeted. |
| Two Hackers in a lobby | Each has their own once-per-game charge. Only one EXECUTE can resolve per meeting, first come first served. |
| EXECUTE happens during the final vote count | Blocked by the 10 s cutoff in §5.2. |
| Votes are anonymous | Then the "missing vote" clue is gone. To make up for it, the **Hacker's vote card gives a 0.3 s static flicker** at the moment of EXECUTE. It is easy to miss but anyone who pays attention can see it. |

---

## 6. Balance summary

### 6.1 Power budget

| Lever | Value | Reason |
|---|---|---|
| Kill cooldown | Lobby kill cooldown **+5 s** | Pays for the extra information (cameras) and the delaying abilities |
| Glitch Task | +3–6 s per glitch, max 3 active | About **18 s of crew time** per round at most. That is noticeable but not a sabotage-level delay |
| Pocket Cam | 10/25 s battery, can't move | About **28% uptime**. Strong info, and it leaves a trace |
| Blue Screen | Once, two steps, physical risk | A strong tool that can only be used once, with three ways for the crew to catch it (plant animation, missing vote, Packet Trace history) |

### 6.2 Counters

| Hacker ability | Crew counter |
|---|---|
| Glitch Task | The Packet Trace toast gives the Hacker's earlier room |
| Pocket Cam | Camera lights blink while Security is empty, and Comms sabotage blacks it out |
| Blue Screen (plant) | The 0.75 s USB animation is visible in person and on cameras |
| Blue Screen (execute) | The Hacker's vote is gone or flickers. Players who saw that player near the target can link the two |

### 6.3 Role interactions

- **Engineer:** not affected. Venting is a separate system.
- **Scientist:** sees the Blue Screen death on Vitals right away, as a normal death.
- **Guardian Angel:** a protection **does block** the PLANT (the cable bounces off with a shield sound). It **does not** block EXECUTE, because a GA can't act during meetings.
- **Shapeshifter (teammate):** PLANT and EXECUTE belong only to the Hacker and can't be given to a teammate. A *Hacker* can't shapeshift.
- **Tracker / Noisemaker:** nothing special. A Noisemaker killed by Blue Screen fires no map alert, because no body exists and the meeting is already happening.

### 6.4 Points to watch in playtesting

- If Hacker win rate goes over **55%** in a 10-player lobby with 2 Impostors, first raise the PLANT animation to 1.0 s, then raise the lowest player count for EXECUTE to 6.
- If players say Glitch Task "isn't fun", cut the mercy rules, not the jokes. Being annoyed should be quick, and laughing should last longer.

---

## 7. Lobby settings (host)

| Setting | Default | Range |
|---|---|---|
| Hacker count | 0 | 0–2 (takes Impostor slots) |
| Hacker chance | 100% | 0–100% |
| Kill cooldown penalty | +5 s | 0–15 s |
| Glitch Task cooldown | 30 s | 10–60 s |
| Max active glitches | 3 | 1–5 |
| Packet Trace toast | ON | ON / OFF |
| Pocket Cam duration | 10 s | 5–20 s |
| Pocket Cam recharge | 25 s | 10–60 s |
| Blue Screen enabled | ON | ON / OFF |
| Min players alive for EXECUTE | 6 | 4–10 |
| PLANT animation time | 0.75 s | 0.25–1.5 s |

---

## 8. Visual and audio identity

### 8.1 HUD buttons (bottom right, above Kill)

| Button | Icon | Cooldown look |
|---|---|---|
| HACK | A green `>_` terminal prompt | The prompt "types" its way back as the cooldown runs |
| CAM | A tablet with a red recording dot | A battery bar along the edge |
| PLANT | A USB plug | Greyed out while the kill is on cooldown |
| EXECUTE | A skull made of binary digits (meeting only) | Pulses blue when usable |

### 8.2 World effects

- **When HACK is pressed:** the Hacker's visor shows green lines of code for 0.5 s. **Only the Hacker sees this.** Others see nothing, so it gives nothing away.
- **When a glitched task opens:** the minigame frame **shakes with color-split (chromatic aberration)** for 0.3 s before the variant loads.
- **Hacker ghost:** if the Hacker dies, their ghost has a floating `404` above its head. This is just for looks.

### 8.3 Optional Neutral variant ("Script Kiddie")
If a Neutral version is wanted later: no kill, keep Glitch Task and Pocket Cam, and the role **wins alone** by finishing **8 glitched-task completions** and surviving to the end of the game. This spec does not cover it.

---

## 9. Feasibility: what "executable" means here

### 9.1 What can't be done
- **Third parties can't add a role to the official game.** Only Innersloth can ship a role on official matchmaking. The game's terms and anti-cheat both block modded behavior in public lobbies.
- **Every player in the lobby has to run the same mod build.** Glitched minigames, new buttons, and meeting kills all need custom network messages (RPCs) that unmodded players can't receive.

### 9.2 What can be done (the supported path)
Community role mods already work this way, for example *The Other Roles* and *Town of Us*. Town of Us even has a role that kills during meetings (the Assassin), which shows Blue Screen is technically possible.

| Layer | Tool |
|---|---|
| Mod loader | **BepInEx 6 (IL2CPP)** |
| Modding framework / custom RPCs | **Reactor** |
| Custom role registration (optional) | **MiraAPI**, or a hand-written `RoleBehaviour` subclass |
| Runtime patching | **HarmonyX** |
| Lobby | **Private lobby**, or a community server running *Impostor* |

### 9.3 Architecture sketch

> Class and method names below are taken from the game's decompiled interop assemblies as known by the modding community. **Check them against the current game version before writing code.** Innersloth renames things between patches.

**Role**
- `HackerRole : ImpostorRole` adds three `ActionButton`s through a `HudManager.Update` patch.

**Glitch Task**
- RPC `HackTask(targetId, roomId)` is sent to everyone. Clients store `pendingGlitch[targetId] = roomId`. The host checks the cooldown and active-glitch caps.
- Harmony **postfix** on `Minigame.Begin(PlayerTask)`. If the local player has a pending glitch and the `TaskTypes` has a variant, attach a `GlitchVariant` MonoBehaviour that changes sprites, input, and pass rules. Variants map to `CardSlideGame` (Swipe Card), `WireMinigame` (Fix Wiring), `SimonSaysGame` (Start Reactor), `EmptyGarbageMinigame` (Garbage/Chute), `UploadDataGame` (Download/Upload).
- When `Minigame.Close` runs, clear the glitch and show the Packet Trace toast with `roomId`.
- Clear all glitches in a `MeetingHud.Start` patch.

**Pocket Cam**
- Create the map's surveillance minigame prefab (from `ShipStatus` / the map's Security console `SystemConsole.MinigamePrefab`) using `Minigame.Begin(null)`.
- Lock movement with `PlayerControl.moveable = false` while it is open.
- Send `RpcUpdateSystem(SystemTypes.Security, 1)` when opening and `…, 2` when closing. This is the same call a real console makes, so the **vanilla camera tally lights blink by themselves**.

**Blue Screen**
- RPC `Plant(targetId)` has the host check range, the kill cooldown, and Guardian Angel protection, then plays the 0.75 s animation on all clients.
- RPC `Execute(targetId)` is **checked by the host** against the time window, player count, and win-condition rules. Then on every client:
  1. `target.Die(DeathReason.Kill, false)`, with no body spawned
  2. find the target's `PlayerVoteArea`, call `SetDead(…)`, and play the BSOD overlay
  3. for each `PlayerVoteArea` whose `VotedFor == targetId`, run `UnsetVote()` and send its vote button back
  4. clear the target's own vote and the Hacker's vote
  5. `MeetingHud.Instance.CheckForEndVoting()` (host only)
- Win checking stays with the vanilla `GameManager` check after the meeting ends. The §5.2 rule already stops the meeting kill from ending the game on its own.

### 9.4 Delivery plan

| Milestone | Scope | Exit criteria |
|---|---|---|
| M1 | Role and buttons, Pocket Cam | Works on all 5 maps, tally lights blink, Comms blocks it |
| M2 | Glitch Task with 2 variants (Wiring, Card) | No soft-locks in 100 automated open/close cycles |
| M3 | Remaining 3 variants and Packet Trace | Each variant's worst case ≤ +6 s, measured |
| M4 | Blue Screen | All §5.4 edge cases pass in a 4-client local test |
| M5 | Playtest balance | 20+ games, Hacker win rate 45–55% |

---

## 10. Content and IP notes
- The art and sounds must be **new**. Don't reuse the Windows BSOD layout pixel for pixel or the AOL "You've got mail" sound. Parody that is clearly a new creation is fine.
- Follow Innersloth's fan-content and modding guidelines: free distribution, no monetization, and a clear "unofficial mod" label.
