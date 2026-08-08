# Unity 6 Port Roadmap

Status: planning, pre-upgrade
Branch: `unity6-port` (to be branched from `butcher`)
Last updated: 2026-08-08

## Why

Volo Airsport has been neglected since 2017 because a Unity upgrade seemed like too
large a job. People still ask for it. This document captures what a first
investigation pass found, and the plan for a first week of porting work, so that
decision isn't made from vague memory of a 7-year-old codebase.

Two possible destinations once a Unity 6 build exists and is playable:
- Update the open source release, notify potential maintainers/modders.
- Update the Steam release.

Neither is committed to yet. Week one is about finding out whether the core is as
portable as it looks, not about shipping either of those.

## Findings

### The core is small and clean

`Assets/Plugins/RamjetAnvil/Aero` (the airfoil/wind model) is ~1,900 lines across 19
files. Self-contained, standard `Rigidbody.AddForceAtPosition`-style physics code,
nothing tied to a specific engine version. `Assets/Scripts/Character` and
`Assets/Scripts/Player` are similarly modest. This is not where the porting risk is.

Aerodynamics/physics tuning will still drift and need re-tweaking regardless of code
changes — PhysX has moved multiple major versions since Unity 5.5.

Total project: ~23,000 lines of C#, no assembly definitions (monolithic compile —
simpler to reason about, if slower to iterate on). API compatibility level is
currently .NET 2.0 Subset and will need bumping.

### There is a pre-existing compile blocker, independent of engine version

The commercial `InControl` input asset was stripped from this repo (same as Time of
Day and Vectrocity, per the top-level README) but 5 files still `using InControl`:
`Assets/Scripts/Input/ParachuteInput.cs`, `Assets/Scripts/Input/PilotInput.cs`,
`Assets/Scripts/Ui/QuickControlOverview.cs`,
`Assets/Scripts/Ui/OptionsMenu/OptionsMenu.cs`,
`Assets/Scripts/Ui/InputMappingsViewModel.cs`. The project does not compile as
committed, in any Unity version, until this is addressed.

**Decision: replace with Unity's new Input System**, rather than sourcing a replacement
commercial asset.

### Rendering

- 16 custom `.shader` files, 14 `.cginc` files — Built-in Render Pipeline. Unity 6
  still ships the Built-in Render Pipeline, so week one can target it directly and
  skip a URP/HDRP migration.
- `Assets/Standard Assets/Effects/*` (Bloom, DoF, AA, SSR, tonemapping, etc.) is the
  legacy Unity "Standard Assets" image effects stack — deprecated, not shipped in
  modern Unity by default. Needs re-wiring or dropping per-effect.
- Atmospheric scattering (Time of Day) and the aero visualizer (Vectrocity) were
  already stripped as commercial assets prior to this investigation — reimplementing
  or replacing them is a pre-existing gap, not new work created by the port.

### Terrain and grass

Custom system, not Unity's terrain detail-mesh renderer: `GrassManager.cs`
(`Assets/Plugins/RamjetAnvil/Grass/Scripts/GrassManager.cs`) runs a quadtree over the
terrain, streams grass patches in/out based on predicted player position, loads
pre-baked height/normal/splat data from a custom binary `.land` file on a background
job queue, and builds instanced billboard-quad meshes on completion. This was built
specifically to avoid frame drops in Unity's stock terrain detail streaming, which was
a known problem at the time and remains imperfect today.

- Code risk: low. Nothing in it touches deprecated APIs (`Mesh`, `MeshFilter`,
  `Shader.SetGlobalFloat`, `Profiler` are all stable in Unity 6).
- Shader risk: moderate. `Assets/Shaders/Terrain/TerrainSplatmapCommonCustom.cginc`
  layers triplanar mapping and a detail-blend layer on top of Unity's own
  `TerrainSplatmapCommon.cginc`, and that stock include file's internals have shifted
  across Unity versions. This is the piece most likely to need rework rather than a
  straight port.

**Decision: keep the custom grass/terrain system as-is**, budget real time for the
terrain shader specifically.

### Dependency injection

`Assets/Plugins/RamjetAnvil/DependencyInjection` (~14 files) is a small home-grown
reflection-based container: `[Dependency]`-attributed fields get resolved by a
`MonoBehaviourInjector` at scene setup. No Unity-version-specific API, not
performance-hot.

**Decision: keep it.** Rewriting it during a port buys nothing but risk and time; it
was written because nothing else did the job at the time, and nothing about the port
changes that calculus. Revisit only if it becomes an actual pain point later, not
preemptively. The same reasoning applies to the other bespoke plugins under
`Assets/Plugins/RamjetAnvil/` (Reactive, StateMachine, CoroutineScheduler, etc.) —
default to keeping unless one is specifically blocking something.

### Networking

`Assets/Plugins/RamjetAnvil/Ramnet` (~6,000 lines, built on Lidgren, not Unity's old
UNET) plus `PadroneClient` (an opaque prebuilt DLL, backend/auth client) ended up
unused in the shipped game.

**Decision: strip it entirely.** Removes ~6,000+ lines and a black-box DLL from the
porting surface for free — no compile risk, nothing to reintegrate.

### VR

Only 5 files reference Oculus/native VR, all old pre-XR-plugin-framework integration.

**Decision: ignore for now.** Not in scope for week one; revisit with the modern
OpenXR plugin if/when VR support is wanted again.

### UI

~5,800 lines across `Assets/Scripts/Ui/`. 47 files use `UnityEngine.UI` (uGUI, still
fully supported in Unity 6) against only 12 legacy `OnGUI` call sites (minor debug
overlays, not the main menu itself).

**Decision: keep and port the existing uGUI menu system** rather than rebuilding it —
better shape than initially assumed. Revisit only if it turns out to fight the DI/event
plumbing more than expected once we're actually in it.

### Audio (FMOD)

The `butcher` branch (created for the open-source release) manually stripped FMOD:
the entire `FMODAssets/` bank tree, `Editor Default Resources/FMOD/`, and
`FMODMigrationUtil.cs` are deleted, along with `OVR/`, `OculusPlatform/`, `OvrFMOD/`,
and `SteamVR/`. 40 scripts were hand-edited rather than left broken — e.g.
`Assets/Scripts/Sound/AmbientSound.cs` had its FMOD event-instance body deleted down
to an empty class; `Assets/Scripts/Sound/EarDistortionSound.cs` has its
`FMOD_StudioEventEmitter` calls commented out line by line. Every file under
`Assets/Scripts/Sound/` still exists and still compiles — every sound hook still
fires on schedule, each one is just a no-op. This was almost certainly the correct
call under FMOD's redistribution terms as written (see below), not a mistake.

Source material still exists: a 2017 copy of the FMOD Studio project at
`E:\audio\fmod\volo_airsport\Fmod Repo`.

**Decision: bring FMOD back in.** Licensing notes from investigation:

- FMOD Studio is free for indies under $200k/yr revenue and <$500k dev budget
  (self-verify Volo still qualifies), with a non-waivable requirement to show FMOD
  attribution/logo at startup or in credits.
- FMOD's redistribution terms permit shipping the compiled runtime binaries embedded
  in a final built game, but *not* republishing the SDK/package files themselves —
  this is why the original binaries/banks couldn't just stay in the OSS repo.
- FMOD's own official Unity integration repo (`fmod/UnityIntegration` on GitHub)
  follows exactly this split: MIT-licensed C# glue code is published, native binaries
  are excluded and must be downloaded separately from fmod.com or the Asset Store.
  **Plan: mirror this pattern for Volo** — integration/game-side code goes in the
  repo, FMOD's compiled binaries do not, and a short setup doc covers fetching them.
- Separate from FMOD's own license: the actual composed audio content (Michael
  Manning's music and sound design) is a copyright question independent of FMOD's
  terms. Confirming redistribution rights for that content as open source is in
  progress (checking with Michael directly) — assumption going in is that it'll be
  fine, but not yet confirmed.

Reattaching FMOD to the working project is otherwise mechanical: point the current
FMOD Unity integration at the existing `.fspro`, do a fresh bank export, and
reconnect the ~8 files with stubbed-out calls (`AmbientSound.cs`,
`EarDistortionSound.cs`, `HudAudioSource.cs`, `RingChallenge.cs`, `CourseManager.cs`,
`VoloModule.cs`, `Turret.cs`/`Bullet.cs`, `OptionsMenu.cs`/`OptionsMenuInitializer.cs`).

## Guiding principle for what goes in the public repo

Anyone should be able to clone the repo, follow a short setup doc for the handful of
pieces that can't legally be committed (FMOD binaries, previously InControl), and get
a working, fully-functional build — same bar the project already held for Time of Day
and Vectrocity in the existing README.

## Week one scope

Goal: a **compiling, flyable, ugly build** in Unity 6 — not a shippable one.

In scope:
- New Unity 6 project via in-place upgrade of the `unity6-port` branch.
- Core layers (Utils, Reactive, DI, Aero, Character/Player) compiling clean.
- InControl replaced with the new Input System; keyboard/mouse input flowing to
  `PlayerController`.
- Ramnet/PadroneClient/VR references removed as part of getting a clean compile.
- Physics/Rigidbody API breaks in Aero and Character fixed; pilot flies on a test
  scene.
- Terrain (Landmass/SwissAlps) loading; terrain shader brought up to at least a
  non-broken state.
- Shader compile errors across the 16 custom shaders resolved.
- A bare-bones menu: Main Menu → Play, nothing more.
- A playtesting pass and a build.

Explicitly out of scope for week one: full options menu, server browser, VR, FMOD
reintegration (audio stays silent-but-functional this week, per the existing
`butcher` state), atmospheric scattering / Vectrocity reimplementation, asset
replacements for stripped 3D models, Steam packaging.

## Process

Unity 6 (`6000.3.18f1`) is already installed via Unity Hub on this machine, alongside
2022.3.

1. **Branch**: done — `unity6-port`, created off `butcher`.
2. **One manual step**: open the project in the Unity 6 Editor and let it run its
   one-time upgrade (re-serializing scenes/prefabs, API updater). This needs the GUI
   and a human confirming dialogs — not something driven from the terminal.
3. **From there, most iteration runs headlessly**: `Unity.exe -batchmode -nographics
   -quit -logFile <path>` forces a script recompile and writes every compiler error
   to a log file. Read the log, fix the break, re-run — the same loop as any other
   compile-error-fixing task, just against the Editor instead of a normal build.
4. **Git checkpoints** at each milestone (compiles clean, terrain loads, character
   flies, menu boots) so we can diff or roll back. Expect large, noisy diffs on
   `.unity`/`.prefab`/`.mat`/`.asset` files purely from Unity's format migration —
   normal, not worth fighting.
5. **Division of labour**: Editor upgrade click-through, playtesting/feel checks
   (glide ratio, control response, visual glitches), and any GUI-only Editor work
   stay with Mar. Batch-mode compile fixes, C# API breaks, shader fixes, git hygiene,
   and stripping the FMOD/InControl/Ramnet/VR surface are driven from the terminal.

## Open questions

- Confirm with Michael Manning whether the FMOD audio content (music + sound design)
  can be released as part of the open source tree, or should stay build-local only.
- Self-verify Volo Airsport still qualifies for FMOD's indie license tier (<$200k/yr
  revenue, <$500k dev budget) before relying on the free tier for a Steam re-release.
- Decide open-source-release vs. Steam-release-first once a playable Unity 6 build
  exists — not a week-one decision.
