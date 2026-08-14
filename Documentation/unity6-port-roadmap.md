# Unity 6 Port Roadmap

Status: **barebones playable.** Boot -> title -> main menu -> spawn select ->
fly -> pause menu -> respawn / change spawnpoint all work end to end, repeatably.
Flight controls (pitch/roll/yaw, respawn, parachute deploy, camera switch,
spectator, pause) are wired. Three previously-precompiled RamjetAnvil
dependencies (CoroutineScheduler, StateMachine, PadroneClient) are now embedded
as in-project source. **Correction (2026-08-11, Mar, after flying both builds
side by side): the wingsuit/parachute flight model was assumed to need
retuning against Unity 6's PhysX, but actually feels intact — comparable to
5.5.** Remaining known gaps: the rebinding UI is still an inert stub (input
config/rebinding doesn't work yet), FMOD is not reintegrated (silent), and the
parachute can go unstable via impulse cascades and "explode" (flight feels
normal right up until it happens — see Known Issues).
Branch: `unity6-port` (branched from `butcher`)
Last updated: 2026-08-11 (flight model status correction)

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

**Update (2026-08-11): this was a reasonable expectation going in, but it turned out
not to hold.** Once actually flyable, the wingsuit/parachute flight model feels
intact and comparable to the original 5.5 build side by side — no retuning pass has
been needed. The remaining physics problem is narrower and different in kind: the
parachute can go unstable via impulse cascades and "explode" under conditions not
yet characterized (see Known Issues) — a stability/robustness bug, not a tuning gap.

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

**Superseded (2026-08-10): Time of Day replaced.** A custom single-scattering
Rayleigh/Mie model now drives both the skybox and distance fog from one shared
integrator — see the progress log entry below. Vectrocity (aero visualizer) is
still unreplaced; still a pre-existing gap, not blocking.

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

### Clocks and coroutine schedulers — deliberate, do not "tidy away"

There are multiple `AbstractUnityClock` instances (MenuClock, GameClock, FixedClock,
an environment clock) and a matching `UnityCoroutineScheduler` per clock — three
schedulers live in the scene at runtime. **This is deliberate architecture, not
accumulated duplication.**

The reason is the pending multiplayer build (Mar, from memory, 2026-08-10): in
singleplayer the game should freeze while the pause menu is open, but on a live
multiplayer server it obviously can't. Separating the time domains is what makes
that switchable — menu/UI work runs on a clock that never stops, simulation work
runs on clocks that singleplayer is free to pause.

Concretely: `Pause()` sets `TimeScale = 0`, which disables the clock MonoBehaviour,
which freezes its `FrameCount`; `UnityCoroutineScheduler.Update()` only advances its
routines when its clock's `FrameCount` moves. So "which scheduler you run a coroutine
on" really means "which time domain does this work belong to." `FlyWingsuit.OnSuspend()`
pausing `GameClock`/`FixedClock` is exactly the singleplayer-only semantics described
above, and it is load-bearing for the current pause menu.

Practical consequences:
- Do not consolidate the schedulers. Two attempts to "simplify" this during the port
  were wrong: one collapsed the outer (menu) and inner (Playing) state machines onto
  one scheduler and caused a silent black-screen hang, because
  `UnityCoroutineScheduler.Run()` pumps `Update()` as its first step — so a coroutine
  calling `Run()` on the same scheduler that is currently mid-`Update()` reenters that
  `Update()` while it's iterating its own routine list. Verified at runtime: the outer
  machine uses a different scheduler instance (`_CoroutineScheduler`) than the inner
  `Playing` machine (`CoroutineScheduler`).
- `VoloModule.Run()` still picks the outer machine's scheduler via
  `GameObject.FindObjectOfType<UnityCoroutineScheduler>()`, which with three instances
  in the scene is effectively picking a time domain by chance. It currently lands on a
  working one. **Latent bug worth fixing** — but the fix is an explicit serialized
  reference naming the intended menu-domain scheduler, *not* pointing it at some other
  existing scheduler reference.
- If multiplayer is definitively dropped, this separation becomes a standing complexity
  cost for a feature that will never ship, and *could* be collapsed — but that is a
  deliberate design decision, not a cleanup. Same category as the RamNet finding below.

### Networking

`Assets/Plugins/RamjetAnvil/Ramnet` (~6,000 lines, built on Lidgren, not Unity's old
UNET) plus `PadroneClient` (an opaque prebuilt DLL, backend/auth client) ended up
unused in the shipped game.

**Original decision: strip it entirely.** Removes ~6,000+ lines and a black-box DLL
from the porting surface for free — no compile risk, nothing to reintegrate.

**Superseded (2026-08-10): RamNet is NOT unused.** Singleplayer routes through the
full replication layer via `HostAsSingleplayer` — it is what spawns the player and
replicates pre-existing networked scene objects (spawn points, turbines, balloons)
via `GameObjectNetworkId` / `PreExistingObjects.FindAll()`. Stripping it would break
entering the game. `PadroneClient` (master server) genuinely is dead and is now
embedded as source with its transport stubbed out (see progress log).

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

## Known issues (not yet investigated)

- **Parachute physics can blow up via impulse cascades.** Under conditions not yet
  characterized, the parachute simulation goes unstable — an impulse cascade rather
  than a general tuning problem — and "explodes." **Not a symptom of the flight
  model needing retuning**: per Mar (2026-08-11, having flown both builds side by
  side), the flight model otherwise feels intact and comparable to 5.5, and this
  happens abruptly rather than the parachute gradually feeling wrong beforehand.
  This currently trips some failsafe that quits the game outright rather than
  recovering or visibly erroring, which also means there's no console output yet
  pointing at a cause. The quit-on-failsafe behavior is itself worth understanding —
  a debug build should probably not hard-exit on this. Needs repro steps and a look
  at whatever's calling `Application.Quit`/aborting on the failsafe path before the
  physics bug itself can be chased.

## Progress log

### 2026-08-08, session 1 — clean compile reached

After the Input System first pass (below), a full close-the-Editor-and-recompile-headless
cycle surfaced ~225 further errors that earlier partial/incremental compiles had never
reached (Unity's incremental compiler skips full re-analysis of files while errors are
present elsewhere, so this was the *first* time the true full picture was visible - not a
regression). Root causes, once traced:

- `ParachuteInput`'s real shape (`Brakes`/`FrontRisers`/`RearRisers`/`WeightShift`/
  `SelectedLine`/`SelectedLinePull`, all `Vector2`/`ParachuteLine?`) had to be reverse
  engineered from actual call sites in `ParachuteController.cs`, `PilotAnimator.cs`, and
  `GameHud.cs` - my first-pass guess at this struct's shape (written blind before those
  files were reachable) was wrong.
- A pre-existing `IParachuteActionMap` interface at
  `Assets/Scripts/Test/Parachute/ParachuteActionMap.cs` (separate from the new
  `Assets/Scripts/Input/ParachuteActionMap.cs`) had its `Input`/`ParachuteConfigToggle`
  members commented out, waiting for `ParachuteInput` to exist - uncommented now that it does.
  `ThirdPersonCameraController` needed a public `PlayerActionMap` property wrapping its
  existing private field, `ParachuteController` needed its `_actionMap`/`_input` fields
  restored (only `_input` was even present, commented out).
- The `Tuple<,>` ambiguity fix from earlier in the session was wrong in both places it was
  applied: fully-qualifying as `System.Tuple` compiled, but `RamjetAnvil.Unity.Utility.Tuple`
  was actually intended (confirmed by `._1`/`._2` field access at the consuming call site,
  vs. `System.Tuple`'s `.Item1`/`.Item2`) - corrected in both `CourseEditorActions.cs` and
  `CourseEditorStore.cs`.
- `UnityInputDeviceProfile`: a real one already exists at
  `Assets/Plugins/RamjetAnvil/InControlDeviceProfiles/UnityInputDeviceProfile.cs` (namespace
  `InControl`) - removed the duplicate stub from `InputBindingStubs.cs` in favor of it.
- `RamjetAnvil.Impero.Util.DictionaryExtensions.ChangeValues` (the one surviving Impero
  source file) just needed a missing `using` in `LanguageSettings.cs`.
- A handful of small, genuinely unrelated pre-existing gaps surfaced alongside all this:
  `Texture2D`'s `mipmap` constructor parameter renamed to `mipChain`; `MutableString.Append`
  has no enum overload (need `.ToString()`); `WithLatestFrom` isn't in the Rx.NET 2.2.0.0
  Unity ships (added to mainline Rx later) - reimplemented minimally in a new
  `RxExtensions.cs`; two Editor-only obsolete-API errors (`BuildTarget.StandaloneOSXUniversal`
  → `StandaloneOSX`, removed `Analytics.SetUserId`); dead Impero `Adapters.MergeAxes` call in
  `PilotAnimator.cs` replaced with a local clamp-sum.

**Result: the project compiles clean.** Zero `error CS` anywhere, headless batch run exits
with "Exiting batchmode successfully now!" / return code 0. This covers the full engine port
plus the first Input System pass - GameLoader path, AttractScreen state machine, player/camera
control, parachute control, course editor, and the game-settings/language system all build.

**What's NOT yet verified:** whether it actually *runs*. Compiling and loading a scene are
different things - Mar opened the SwissAlps scene mid-session and found scattered "missing
script" warnings (expected: old Impero/InControl-based components no longer exist, new
`PilotActionMapProvider`/`MenuActionMapProvider`/`ParachuteActionMapProvider` components need
to be manually added to the right GameObjects) and black terrain (the custom
`Standard-FirstPass-Custom` terrain shader doesn't render correctly under Unity 6 - see the
`Volo > Terrain > Use Built-in Terrain Shader (temporary fallback)` menu item added at
`Assets/Editor/TerrainShaderFallback.cs`). Next session's actual next step is GUI work in the
Editor: wire up scene/prefab references for the new input providers, confirm the terrain
fallback works, then playtest.

See also [input-system-complexity-assessment.md](input-system-complexity-assessment.md) for a
grounded assessment of the old Impero+InControl input stack's complexity, written mid-session,
plus a recommendation for what to do differently when the real rebinding UI (still just an
inert stub - `InputBindingStubs.cs`, `JoystickActivator`, `InputBinder`, `InputBindings<T>`)
gets built for real.

### 2026-08-08, session 1 — engine/API port

Got the project compiling clean against Unity 6 for everything except input. Fixed,
in order: `com.unity.ugui` package missing from manifest (uGUI/EventSystems moved out
of core engine modules since ~2019); `ImmutableCollections.dll`/`Rx.NET35.dll` swapped
for modern equivalents (both were .NET-3.5-era backports whose bundled BCL interfaces
collided with mscorlib — see [[feedback-unity-port-workflow]] memory for the diagnostic
pattern); `Disruptor.dll` swapped for official NuGet `Disruptor-net` 6.0.1 with
`JobManager.cs`'s construction calls updated for its modern API; FMOD/SteamVR/native-VR
call sites stubbed to match the existing `butcher`-branch pattern; Ramnet's one dead
`UnityEngine.Network` call patched; two unrelated bugs fixed (`Tuple<,>` ambiguity,
obsolete `TextureImporterFormat` enum value).

### 2026-08-08, session 1 — Input System replacement, first pass

Confirmed with Mar: both InControl and Impero (Mar's own input-mapping framework, whose
`StandardInput`/`Unity` adapter layers turned out to be genuinely missing from the repo,
not just stripped) get replaced by Unity's new Input System — clean break on the old
XML config format, Impero deleted outright, gamepad support goes broad via the generic
`Gamepad` device abstraction (works across Xbox/PlayStation/Switch automatically)
rather than staying Xbox-360-only.

Added `com.unity.inputsystem` (1.11.2) and set `activeInputHandler: 2` ("Both") in
Player Settings so the ~14 files still using the legacy `UnityEngine.Input` class
don't break while this migrates incrementally.

Built the core plumbing at `Assets/Scripts/Input/`:
- `Core/ActionMap.cs` — generic `ActionMap<TAction>` wrapping the Input System, exposing
  the same `PollAxis`/`PollMouseAxis`/`PollButton`/`PollButtonEvent` surface the rest of
  the codebase already called against Impero+InControl, so most consumers needed zero
  changes beyond a `using` fix.
- `PilotActionMap.cs`, `MenuActionMap.cs`, `ParachuteActionMap.cs` (+ their
  `...Provider` MonoBehaviours, matching each type's existing `.ActionMap` /
  `.ActionMapRef` / `.V` access pattern exactly) — placeholder keyboard+gamepad
  bindings, explicitly flagged `Todo` for real tuning later.
- `ParachuteInput` struct (didn't exist anywhere in the repo despite being referenced —
  invented a plausible shape: weight-shift X/Y, brake left/right; needs revisiting once
  parachute flight is actually being tuned).

Result: 285 compile errors / 28 files → 147 errors / 13 files. Every remaining error is
now confined to the rebinding UI (`OptionsMenu`, `OptionsMenuModel`,
`OptionsMenuInitializer`, `InputMappingsViewModel`, `InputBindingView`,
`JoystickActivator`, `ActiveJoystickNotifier`, `InputBinder`, `InputBindingViewModel`,
`InputBindings<T>`, `ControllerId`/`ControllerType`) and a not-yet-built
`SpectatorActionMap` (4th action map, for spectator-mode camera) — deliberately
deferred, this is a real feature (controller-connect detection, live rebinding flow,
per-device profiles) that deserves its own design pass, not a rushed add-on. One
unrelated stray also still open: missing `ColorPicker` widget type in
`ParachuteEditor.cs` (in-editor parachute tuning tool, not core gameplay).

**(Superseded — see the "runtime debugging" entry further below.)** `SpectatorActionMap`
and a real (if minimal) `ColorPicker` got built the same session, and the project moved
past compiling into actually booting and running. The rebinding UI / `JoystickActivator`
/ `InputBindings<T>` replacement is still the next real feature-scoped piece of work,
just no longer the *very* next thing — see the bottom of this doc for current status.

### 2026-08-08, session 1 — runtime debugging: compiles clean → actually boots and runs

Mar did the Editor upgrade click-through and scene wiring (re-attaching the new
`PilotActionMapProvider`/`MenuActionMapProvider`/`ParachuteActionMapProvider`/
`JoystickActivator`/`InputBinder` components where scenes showed "Missing Script" —
Unity requires a MonoBehaviour's filename to match its class name to be addable via the
Editor, so `PilotActionMap.cs` etc got renamed to `PilotActionMapProvider.cs` etc,
keeping the non-component helper class in the same file). From there, booting through
the loading screen surfaced a long chain of runtime-only bugs — things that compiled
fine but only broke once actually executed. In rough chronological order:

- **`.execution_order_cache` corruption** — a stale, partially-overwritten cache file at
  the project root (`UnityExecutionOrder` plugin's `[Run.Before]`/`[Run.After]`
  execution-order cache) had a stray `</ArrayOfString>` mid-file, crashing an
  `[InitializeOnLoad]` static constructor and silently skipping all execution-order
  application for the session. Deleted; it's fully regenerable.
- **`MutableString` reflection crash** — `GarbageFreeString()` used reflection to reach
  into `StringBuilder`'s *private* `"_str"` field as a no-allocation `ToString()` trick.
  That field only existed in the old Mono `StringBuilder` layout; modern .NET doesn't
  have it, so `GetField` returned `null` and the next call threw, breaking most of the
  UI (this class is used pervasively). Removed the reflection entirely; `ToString()` now
  just calls the builder's own `ToString()` — loses a minor GC micro-optimization, gains
  correctness.
- **`Newtonsoft.Json.dll` (v6.0.0.0, ~2014) had a hard-coded runtime dependency** on an
  assembly literally named `ImmutableCollections, Version=999.999.999.0` — Newtonsoft's
  own old immutable-collections add-on, tied to that specific old package name. Our
  earlier compile-time swap of `ImmutableCollections.dll`'s content didn't help here
  since Newtonsoft's own dependency table is frozen at whenever *they* built it, and
  couldn't be patched (precompiled, no source). Replaced with Unity's official
  `com.unity.nuget.newtonsoft-json` (3.2.1) package instead of a raw NuGet fetch, since
  it's built for Unity/IL2CPP; disabled the old plugin dll.
- **`MainMenu`'s constructor called `PadroneClient.Me(...)` unconditionally** with
  nothing catching failures — `PadroneClient` (precompiled, no source) uses the legacy
  `UnityEngine.WWW` class internally, whose *native* binding (`WWW.InitWWW`) was removed
  in modern Unity even though the managed wrapper still compiles, so it threw
  `MissingMethodException` at runtime and aborted the whole state-machine boot (the
  "camera rig frozen at 0,0,-6" symptom). Wrapped in try/catch — can't fix
  `PadroneClient` itself, and the master server is confirmed unreachable/unused anyway.
- **`InputMappingsViewModel`'s `[Dependency]`-only fields never resolved** (nothing
  wires them, unlike `OptionsMenu`'s equivalent fields which `OptionsMenuInitializer`
  sets explicitly) — unresolved `[Dependency]` fields make the DI system disable the
  component outright, so `OnEnable()` never ran and `InputMappings` stayed permanently
  null, crashing `OptionsMenu.Initialize()`'s `.Subscribe()` call. Made it
  self-initializing with a graceful empty-list fallback, called from both `OnEnable()`
  and the getter itself (so it can't be beaten by DI-disable or early-access timing).
  Same null-guard pattern applied to `OptionsMenu._joystickActivator` and
  `GlobalMenuInputEventEmitter`'s two dependency fields, and `UnityMasterServerClient`
  got an empty-URL guard so it stops attempting a malformed request every boot.
- **The News/Continue button took mouse clicks but not keyboard/gamepad** —
  `CursorInputModule.Process()` (the custom `RamjetAnvil.InputModule` pipeline, predates
  this port) gates its *entire* pipeline — mouse and keyboard/gamepad navigation both —
  behind `Cursor != null && NavigationDevice != null`. `VoloModule.cs` set `Cursor` (a
  real `MouseCursor`) but had `NavigationDevice = new MenuActionMapCursor(...)`
  commented out from the original stub pass, so *nothing* processed, not even mouse —
  except mouse clicks on `Button` components work via a separate raycast+click path that
  doesn't need `NavigationDevice` at all, which is why mouse alone appeared to work.
  Restored the missing `MenuActionMapProvider` lookup and the `NavigationDevice`
  assignment.
- **`TitleScreen`'s "press any button to continue" logic was fully dead-commented**,
  same "I want to handle input" pattern as `GlobalMenuInputEventEmitter` and
  `OpenVrCameraRig` from earlier, using the fully-removed Impero `Peripherals`/
  `ImperoCore` machinery. Rather than reimplement raw device polling, hooked it into the
  already-restored `Events.OnConfirmPressed` global event instead (`_data.EventSystem
  .Listen<Events.OnConfirmPressed>(...)`), replicating the original two-stage
  show-logo-then-transition behaviour.
- **A systemic DI-registration-timing bug**, found while chasing why the TitleScreen fix
  still didn't fire: `UnityDependencyResolver.Resolve()` injects `[Dependency]` fields
  across every scene object in one pass, using whatever's in `NonSerializableRefs` *at
  that exact moment*. In `VoloModule.cs`, `Resolve()` ran before the action-map providers
  and `cursor` were ever looked up, so every bare `[Dependency]` field of those types
  anywhere in the game was silently unresolvable from the start — not a one-off, the same
  bug surfacing repeatedly across different components. First attempt: manually register
  the four providers into `NonSerializableRefs` before `Resolve()`. That caused a
  `ArgumentException: An item with the same key has already been added` — turned out
  `Core.unity` already has ~22 `IsDependency` marker components (the DI system's actual
  intended scene-registration mechanism, found by grepping the scene YAML for the
  component's GUID, not its class name string — Unity serializes component types by
  GUID) already registering these singletons; once Mar re-attached the real components
  those markers' stale references healed and they started resolving correctly on their
  own, making the manual registration redundant rather than additive. Reverted to a
  plain `FindObjectOfType` lookup (still needed for direct, non-DI use constructing
  `MenuActionMapCursor`) and left DI registration to the scene's own mechanism. `cursor`
  still needed its own explicit registration + a second `Resolve()` call, since it's
  created fresh in code after the camera rig exists and has no scene-placed equivalent.
- **The actual root cause of "nothing responds to any input device"**: `ActionMap<T>`'s
  base constructor called `_inputActionMap.Enable()` immediately — before any derived
  class's `SetupBindings()` (called from the derived constructor body, which always runs
  *after* the base constructor completes in C#) had added a single binding. Every one of
  the four action maps (`Pilot`/`Menu`/`Parachute`/`Spectator`) was being enabled while
  still empty. Restructured: the base class now exposes a `protected void
  EnableActions()` instead of auto-enabling, and each derived map calls it as the last
  line of its own constructor, after `SetupBindings()`. This fixed both the earlier
  "Continue button ignores keyboard/gamepad" symptom and the TitleScreen block in one
  shot, since both ultimately depend on `MenuActionMap`'s bindings actually being live.
- **`ColorPicker`** (the `ParachuteEditor.cs` stray from the previous entry) got a real,
  minimal implementation — a single `Image`-backed swatch component with
  `onValueChanged`/`CurrentColor` — rather than a full HSV picker, since the original
  (a stripped commercial/custom asset) is unrecoverable and this is a level-editor
  convenience tool, not core gameplay. `_cellColorPicker`'s child hierarchy
  (`BoxSlider`, `Hue`, `Hue/Background`) confirms the original was a real interactive
  HSV picker — flagged as a known downgrade, not silently glossed over.

**Result:** boots past the loading screen, through the news panel, into the title
screen, and gamepad input now reaches the Play/Title-screen selection. Not yet fully
playable — that's the next milestone to chase.

**Debugging technique worth reusing:** when a `[SerializeField]` reference *looks*
assigned in the Inspector but the component behaves as if it's null, don't guess —
grep the `.unity`/`.prefab` file directly. Unity serializes component *types* by GUID
(from the `.meta` file), not by class name string, so search by GUID
(`grep -n "^guid:" Foo.cs.meta`) to find every scene instance of a component type, and
follow a specific field's `{fileID: N}` to `--- !u!114 &N` in the same file to check
whether that fileID resolves to a real, currently-existing object of the expected type,
or a dangling reference to something that was deleted and replaced. This resolved two
separate bugs this session (the DI duplicate-key crash, and ruling out a stale/mismatched
`EventSystem` reference) faster and more reliably than reasoning about it blind.

**(Superseded — see the two "runtime debugging, part 2" entries below.)** The project
got well past the title screen this session: spawn, flight, parachute, spectator, and
the pause menu are all now reachable and responsive. The `HeadCameraController.cs`/
`OpenVrCameraRig.cs` dead-stubs are the only ones from the original list of 7 still
unfixed — both VR-only, still low priority. See the bottom of this doc for current
status and what's next.

### 2026-08-09/10, runtime debugging, part 2 — spawn, flight, and the remaining dead stubs

Picking up from "reaches the Play/Title-screen selection": Mar drove the Editor through
spawn point selection and into actual flight, reporting console output for each new
state reached, same loop as the previous session.

- **RamNet networking turned out not to be fully unused** — singleplayer routes through
  it via `HostAsSingleplayer`, which is how spawn points and pre-existing networked
  objects (turbines, balloons) get replicated in via `GameObjectNetworkId`/
  `PreExistingObjects.FindAll()`. `ArgumentException: One or more components found in
  passed GameObject are null` on Start Game traced (via `GetComponentsInChildren`
  silently returning `null` entries for missing-script slots) to several missing-script
  GameObjects across `LoadingScreen`/`Core`/`SwissAlps` — wrote
  `Assets/Editor/MissingScriptFinder.cs` (`Volo > Diagnostics > Find Missing Scripts In
  Loaded Scenes`) since static grep couldn't find instance-level breaks the way it could
  for typed reference bugs. Most of the ~40 hits traced via `git log --all -p -S<guid>
  -- '*.meta'` to a single common cause: the deleted `Assets/Plugins/FMOD/
  StudioEventEmitter.cs` (stripped on `butcher`), safe to remove — no lost gameplay
  logic, same category as the already-stubbed FMOD calls elsewhere.
- **`SpawnScreen.cs`** — another "I want to handle input" dead stub, fixed the same way
  as `TitleScreen`/`GlobalMenuInputEventEmitter`: reconnected to
  `MenuActionMapProvider`. Needed a new `PollDiscreteCursor()` method on
  `MenuActionMap` (button-based Left/Right/Up/Down composed into a `Vector2`, for
  `NavigableUIList`'s discrete cursor input shape — the existing analog `PollAxis`
  wasn't the right fit for a list/grid UI).
- **`FlyWingsuit.cs`, `ParachuteStates.cs` (`Flying.Update()`), `SpectatorMode.cs`,
  `Playing.cs`** — the remaining four "I want to handle input" dead stubs from the
  known-landmine list, all fixed the same way (reconnect to the already-working action
  map providers/event system rather than resurrecting commented Impero code). This
  surfaced a real gap: `PilotActionMap` had bindings for flight-surface controls
  (pitch/roll/yaw/arms) but **no bindings at all** for `Respawn`/`UnfoldParachute`/
  `ChangeCamera`/`ToggleSpectatorView` — those actions existed in the enum and were
  polled by the restored code, but silently never fired since nothing was bound to
  them. Added bindings: keyboard keys match the original Impero defaults (traced via
  `git show` on the pre-strip commit — `R`=Respawn, `T`=UnfoldParachute, middle-mouse=
  ChangeCamera, `F3`=ToggleSpectatorView); gamepad bindings are new picks against the
  generic `<Gamepad>` template (Select/North/West buttons) rather than reviving the old
  Xbox-360-specific raw button indices, consistent with the "go broad on gamepad
  support" decision from the input-system pass.
- **`JoystickActivator` NRE on spawn** (`Playing.cs:83`) — `Playing.Data.JoystickActivator`
  is a plain `[SerializeField]` (no `[Dependency]`), and `VoloStateMachine`'s
  `_playingData.JoystickActivator` slot in `Core.unity` was genuinely empty
  (`{fileID: 0}`), unlike sibling fields in the same block. Started building a general
  `SerializedObject`-based Editor auto-wiring tool for this whole class of bug
  (`Assets/Editor/NullReferenceAutoWirer.cs` — walks `VoloStateMachine`'s state-data
  blocks, auto-assigns any null object-reference field when exactly one candidate of
  the matching type exists in-scene, skips/logs ambiguous cases) before Mar found and
  fixed the actual cause directly: he'd swapped the `JoystickActivator` *component* in
  the scene earlier but `VoloStateMachine`'s direct reference to it hadn't been
  updated — a stale reference, not a truly-never-assigned one. The auto-wiring tool is
  still in the project (harmless, diagnostic-only) in case a genuinely-empty case like
  this shows up again on `_flyParachuteData`/`_spectatorModeData` etc.

**Result:** player spawns and flies the wingsuit; pitch/roll/yaw, respawn,
unfold-parachute, camera-switch, spectator-toggle, and the pause menu all respond.

### 2026-08-10, runtime debugging, part 2 continued — three precompiled dependencies become source

While chasing the parachute-deploy path (`T` key), hit a `NullReferenceException` deep
inside the closed-source `RamjetAnvil.Coroutine`/`RamjetAnvil.StateMachine` DLLs, with
no application-code frames in the stack trace — nothing to grep, nowhere to add a
breakpoint. **Mar had the actual source for both on GitHub and dropped copies into
`Dependencies/CoroutineScheduler` and `Dependencies/StateMachine`** (outside the Unity
project, not previously referenced from it). This turned an unfalsifiable guessing
exercise into a normal debugging session, and became the session's main event:

- **Embedded both as in-project source** at `Assets/Plugins/RamjetAnvil/
  CoroutineScheduler/Source/` and `.../StateMachine/Source/`, disabled the old
  `CoroutineScheduler.dll`/`StateMachine.dll` (+`.pdb`/`.mdb`) by renaming to `.bak` —
  same reversible-disable pattern used earlier for `Newtonsoft.Json.dll`. Being plain
  project source now, Unity recompiles them on its own; no external `.csproj`/`msbuild`
  step needed (the standalone `.csproj`s reference a Unity 5.3.5 install path that no
  longer exists on this machine anyway).
- **Added targeted diagnostics while embedding**: `Routine.FetchNextInstruction()` now
  logs the fibre's type name before rethrowing on exception, and `StateMachine<T>
  .InvokeStateLifeCycleMethod()` catches `TargetInvocationException` specifically
  (previously only `TargetParameterCountException` was caught) and logs which
  state/method threw. This is *how* the actual bug (see below) got found instead of
  staying a mystery.
- **Fallout, round 1 — compile errors**: `RamjetAnvil.Coroutine.ObjectPool<T>` (internal,
  previously invisible outside its own assembly) collided with
  `RamjetAnvil.Unity.Utility.ObjectPool<T>` once both compiled into the same
  Assembly-CSharp — renamed to `RoutinePool<T>` (used only internally by
  `CoroutineScheduler`, safe rename). `IStateMachine.Transition`/`TransitionToParent`
  were `void` in the checked-out source but `Playing.cs` called `.WaitUntilDone()` on
  their return value and read an `IsTransitioning` property neither existed on — the
  Dependencies/ checkout was evidently a slightly different snapshot than what was
  actually compiled into the shipped DLL (a second, independent instance of this same
  drift showed up in `Routines.cs`, see below). Widened both methods to return
  `IAwaitable` (the scheduler already had the value, just wasn't returning it) and
  added `IsTransitioning`.
- **Fallout, round 2 — `Assembly.GetTypes()` poisoning**: with the scheduler embedded,
  `RamjetAnvil.RamNet.MessageTypes`'s static constructor (which reflects over every
  loaded assembly to find networked message types) started throwing
  `ReflectionTypeLoadException` the instant it touched `Padrone.dll` — that DLL's own
  `PadroneClient._coroutineScheduler` field was still typed against the now-missing
  external `CoroutineScheduler` assembly identity. Unity's plugin importer additionally
  refused to load `Padrone.dll` *at all* once its declared reference target was gone
  (cascading into `Assembly-CSharp`/`Assembly-CSharp-Editor` failing to load), worked
  around temporarily via `validateReferences: 0` in `Padrone.dll.meta`, then properly
  fixed by asking Mar for Padrone's source too (also on GitHub, dropped into
  `Dependencies/padrone-main`) and embedding it the same way at
  `Assets/Plugins/RamjetAnvil/PadroneClient/Source/`, disabling the old `Padrone.dll`.
  Confirms the master-server client can be fully removed from the closed-source
  porting surface, not just individual call sites.
- **`WWW.InitWWW` doesn't just have its native binding removed (as found earlier this
  session with the same class) — the method itself no longer exists on modern Unity's
  `WWW` type at all**, a genuine `CS1061` compile error, not a runtime
  `MissingMethodException`. `PadroneClient`'s pooled-WWW-object transport used it to
  reinitialize reused requests; since the master server is already confirmed
  unreachable, replaced `ExecuteWebRequest` with a stub that reports
  `HttpStatusCode.ServiceUnavailable` without ever constructing a `WWW`, dropped the
  now-unused `Util.WWWPool`.
- **Screen fader intermittently going black — a real, pre-existing bug, not something
  the embedding introduced.** `Routines.Animate`'s loop correctly calls
  `animator(animation(lerp))` every frame, but its *terminal* call after the loop was
  unconditionally `animator(1f)` — bypassing the animation curve entirely. Invisible for
  `FadeOut` (`animation(1f) == 1f` for a normal curve anyway) but `CameraTransitions
  .FadeIn` uses `EaseInOutAnimation.Reverse()`, whose `animation(1f) == 0` — so every
  fade-in ended by snapping straight back to opaque/black regardless of the curve.
  Fixed to `animator(animation(1f))`. (The 4-arg `Animate` overload's parameter order
  also didn't match any real call site in `CameraAnimator.cs`/`CameraTransitions.cs`,
  and `Animation.Reverse()` didn't exist at all in the Dependencies/ checkout — both
  fixed by matching the actual call sites, the same "checked-out source is a slightly
  different snapshot than the shipped DLL" pattern as the `IStateMachine` fallout above.)
- **Coroutine scheduler resilience**: the diagnostic rethrow added above turned out to
  cause its own problem — an exception mid-`CoroutineScheduler.Update()` unwinds the
  *entire* per-frame update pass, which can leave a routine partially torn down (e.g. a
  subroutine disposed/pooled but never removed from its parent's active-subroutine
  list), causing the exact same `<null fibre>` NRE to repeat every frame forever.
  Changed the catch in `FetchNextInstruction()` to mark the routine `_isDone = true`
  and return instead of rethrowing — self-heals (routine gets recycled normally next
  pass) instead of looping. The underlying one-time trigger (something returning a
  pooled `Routine` object while another part of the code still held a reference to it)
  is still unconfirmed — noted as a known minor issue, not currently blocking.

**Two more scene/component bugs found via the same debugging pass:**
- **`ActiveJoystickNotifier` NRE at `OnEnable()`** — not a wiring bug (the
  `[Dependency]` fields were correctly resolvable once the `JoystickActivator`
  `IsDependency` marker's `_reference` was fixed — it had been pointing at the sibling
  `ActiveJoystickNotifier` component instead of the actual `JoystickActivator`
  component on the same GameObject, presumably a mis-drag during an earlier component
  swap). The remaining crash was a **startup-order** bug:
  `[Dependency]`-consuming components in this codebase are authored **disabled**
  (`m_Enabled: 0`) in the Inspector and get enabled by `MonoBehaviourInjector` only
  *after* successful injection (confirmed against `ScreenshotMaker`'s scene data,
  which does start disabled) — both `ActiveJoystickNotifier` instances in `Core.unity`
  had `m_Enabled: 1`, so `OnEnable()` fired at scene load, before the dependency
  resolver ever ran. Fixed by disabling both in the Inspector.
- **`TitleScreen`'s Confirm-button listener leaked past state exit** — a bug introduced
  when this file's dead stub was fixed in the previous session: the subscription lived
  in the constructor (runs once, ever) with no matching `Dispose()`, so it kept firing
  in later states, eventually throwing `Cannot transition while another transition is
  already active` when a later Confirm press tried to re-trigger the same
  `Machine.Transition(...MainMenu)` call mid-transition. Fixed by moving the subscribe
  into `OnEnter()` and the dispose into `OnExit()`, so re-entering the title screen
  (e.g. backing out from the main menu) still works.

**Also fixed, both pre-existing and unrelated to the above:**
- `ScreenshotMaker`'s `Texture2D` construction crashed with "Texture must have width
  greater than 0" — `ScreenResolutionNotifier` seeds its resolution stream from
  `Screen.width/height` at `Awake()`, which can legitimately be `0` before the window
  is realized. Added a `.Where(width > 0 && height > 0)` filter upstream of the
  `Texture2D` construction.
- `VersionChecker`'s `JsonReaderException` (flagged as pending in the previous session)
  — the version-check endpoint returns a 200 OK with parked-domain HTML instead of
  JSON, same category as the already-dead master server and news feed. The existing
  code only guarded against network-level failures (`versionRequest.error`), not a
  successful-but-unparseable response. Wrapped the deserialize in try/catch, logs a
  warning and skips the update notification instead of crashing the coroutine.

**Result:** player spawns, flies, can respawn/deploy parachute/switch camera/spectate/
pause, and the title screen ↔ main menu ↔ spawn screen ↔ playing flow no longer throws.
Three previously-opaque precompiled dependencies (CoroutineScheduler, StateMachine,
Padrone) are now fully in-project source, closing off a whole class of "can't fix,
no source" bugs for good.

**(Superseded — see part 3 below.)**

**Debugging technique worth reusing:** when a bug traces into a precompiled RamjetAnvil
plugin with no source in the repo, ask whether Mar has it on GitHub/backup before
guessing at binary internals — worked for CoroutineScheduler, StateMachine, and Padrone
all in the same session. Once embedded as source, treat the checked-out version as
*probably* but not *definitely* identical to what was actually compiled into the
shipped DLL — this session hit two independent cases (`IStateMachine`'s
`Transition`/`TransitionToParent` return type + `IsTransitioning`, and `Routines
.Animate`'s parameter order + missing `Reverse()`) where the real call sites in
`Assets/Scripts/` expected a slightly different API than the Dependencies/ checkout
had. When that happens, trust the call sites (real, exercised game code) over the
checkout, and grep for every usage project-wide before changing a shared method's
signature.

### 2026-08-10, runtime debugging part 3 — state machine correctness, and the playable milestone

Three genuine state-machine/scheduler bugs, each surfacing as "works the first time,
breaks the second." All three were in the newly-embedded dependency source rather than
game code, and all three are the kind that only appear once you can actually play far
enough to repeat a cycle.

- **`StateMachine.TransitionToParent` double-popped the stack.** The public entry method
  popped, *and* the private coroutine it delegated to popped again. Every parent
  transition therefore removed two entries. Symptom chain: pause -> resume left the stack
  empty instead of holding `Playing`; the *second* pause then took `Transition()`'s
  "stack is empty, enter as fresh top-level state" branch instead of the child-transition
  branch, so `Playing.OnSuspend()` never ran (sim didn't freeze) and `Playing` stayed
  subscribed alongside `OptionsMenu` (both responded to input at once); closing the menu
  then hit the `_stack.Count <= 1` guard and threw. Fixed so the public method only
  peeks and the coroutine owns the single `Pop()`. Affects every child/parent pair —
  pause menu, spectator, parachute editor.
- **"Start Selection" from the in-game pause menu skipped `Playing`'s exit path.** It
  called `Machine.Transition(SpawnScreen)` directly, which from `OptionsMenu`'s
  perspective is an ordinary sibling transition and only pops `OptionsMenu` — leaving
  `Playing` buried in the stack with its inner `_playingStateMachine` never reset to
  `Initial`. Next spawn, `Playing.OnEnter` tried `Suspended -> FlyingWingsuit`, which
  isn't a permitted transition, and threw. Fixed by unwinding through `Playing` first
  (`TransitionToParent`, running its `OnExit`/inner-machine reset) and only then
  transitioning to `SpawnScreen`.
- **`IAwaitable` handles could silently never report completion.** `CoroutineScheduler.Run()`
  returned the pooled `Routine` object itself. When that coroutine finished, the scheduler
  recycled the object one `Update()` pass later — and `Reset()` sets `_isDone = false`,
  *erasing* the completion signal. Whether a waiter ever observed the brief `_isDone == true`
  window depended purely on the relative script-execution order of the two schedulers
  involved, so it failed deterministically here: the work completed, nothing noticed, and
  `WaitUntilDone()` waited forever. Fixed by handing out a generation-stamped
  `RoutineHandle` from `Run()` instead of the raw pooled object, and bumping the generation
  in **both** `Initialize()` and `Reset()` — so "already recycled" is permanently
  indistinguishable from "done" to any older handle. (Bumping only in `Initialize()`, the
  first attempt, left exactly the gap described above and did not fix the hang.)

Also, from the previous entry's open thread: the `<null fibre>` error's actual trigger was
found. `FlyWingsuit.NotifyAboutParachute()` only cleared its scheduler handle
(`_openParachuteNotification`) if `_unfoldParachuteMappingStr != null` — but that string
comes from `InputMappingsViewModel`, which always returns empty while the rebinding UI is
stubbed, so the handle was never cleared after the coroutine finished. A later `OnSuspend()`
then disposed a handle to an already-recycled `Routine`. Fixed at the call site (always
clear) and hardened in the scheduler (`Dispose()` now only marks a routine finished; the
owning container does the actual pool return on its next pass, since only it can know
nothing else still references the object).

**Two wrong turns worth recording**, both from reasoning about the code rather than
measuring it — see the clocks/schedulers section above for the corrected understanding:
1. Theorised the outer state machine was frozen because its scheduler was tied to a paused
   clock. Plausible, but contradicted by evidence already in hand (the pause menu keeps
   responding across frames while paused). Acted on it anyway; pointing the outer machine
   at `Playing`'s scheduler caused a silent black-screen hang on spawn, via the
   `Run()`-pumps-`Update()` reentrancy described above. Reverted.
2. Theorised the recycled-handle problem correctly but implemented the generation bump in
   only one of the two places it was needed, so the symptom persisted unchanged.

What finally resolved it was instrumenting the actual transition path (four log points in
`StateMachine.TransitionToParent`, plus logging every scheduler instance and which machine
got which). That immediately showed the transition running to completion while the waiter
never noticed — which falsified both theories and pointed straight at the handle. **Lesson:
once two successive theories have failed, stop reasoning and instrument.** The temporary
logs have since been removed; the permanent `[CoroutineScheduler] Exception while advancing
routine '<name>'` instrumentation (added earlier this session) stays, and it earned its
keep here by naming `Playing+<OnEnter>d__9` in a stack trace that would otherwise have been
pure scheduler frames.

**Result: barebones playable.** Boot -> title -> main menu -> spawn select -> fly ->
pause menu -> respawn / change spawnpoint, repeatably, with no errors in the normal flow.
Input *configuration* remains the known-broken area (the rebinding UI is still an inert
stub), which is expected.

**(Superseded — see the atmospheric scattering entry below for what came next.)**
Original next-steps pointer, still mostly accurate for what's left after atmosphere work:
the rebinding UI / `JoystickActivator` / `InputBindings<T>` real implementation (largest
remaining functional gap — see
[input-system-complexity-assessment.md](input-system-complexity-assessment.md)), then
flight-model retuning against Unity 6's PhysX, then FMOD reintegration.
`HeadCameraController.cs`/`OpenVrCameraRig.cs` dead-stubs remain, VR-only, still low
priority. The `VoloModule.Run()` `FindObjectOfType<UnityCoroutineScheduler>()` ambiguity
(see clocks section) is a known latent bug worth closing off with an explicit serialized
reference.

### 2026-08-10/11 — atmospheric scattering (Time of Day replacement)

First functionality work since reaching barebones-playable, rather than bug fixing.
Mar asked for the pre-port distance fog / sky look back: smooth fog-to-sky blending,
sun-angle response, no clouds needed, staying on the Built-in Render Pipeline. Read
the actual TOD docs (andererandre.github.io/TOD) for the real feature list rather than
working from memory of what it "probably" did.

**Design decision, and a correction to it mid-session:** first pass baked the real
skybox to a small cubemap and sampled it for fog colour, to guarantee the horizon
seam-matched whatever was drawing the sky. Mar pointed out the better approach: since
we're writing our own scattering model anyway, there's no separate "sky" to match —
the sky *is* the same integral as the fog, just carried to infinity instead of stopped
at geometry. Rewrote around one shared integrator
(`Assets/Shaders/Atmosphere/AtmosphericScattering.cginc`) used by both
`Skybox/AtmosphericScattering` (`AtmosphericSky.shader`) and the fullscreen fog pass
(`AtmosphericFog.shader`/`.cs`, rewritten off the legacy `PostEffectsBase` chain onto a
plain `Graphics.Blit`). This is strictly better than the cubemap approach: seamless at
full per-pixel resolution rather than a blurred mip, no bake/staleness cost (relevant
given the default day length is 2 minutes — the sun moves ~3°/second), and aerial
perspective + sky colour are physically the same computation rather than two models
that happen to agree.

Model: single-scattering Rayleigh + Mie, spherical planet, short raymarch (8 view
steps × 3 sun steps, both tunable via `#define` before including the `.cginc`).
Sunlight is attenuated along its own path to each sample point before scattering
toward the camera — this alone produces sunset reddening (low sun = long path =
blue stripped out first) without any authored gradient. `AtmosphereController.cs`
pushes sun direction/color and all tuning parameters as global shader properties, and
drives haze density from `Ecology.Weather.FogIntensity` (squared, matching the
shaping `CameraEcologyEffects` already applied for the fog particle systems).

**Below-horizon void, found by Mar after first playtest:** view rays that passed
under the terrain horizon (no geometry to stop them) integrated almost no scattering
and read as black void — the atmosphere alone doesn't cast much light back without
something to reflect it. Fixed by intersecting a stand-in planet sphere in
`AtmosSkyColor`: rays that would go below the horizon terminate on the sphere instead
of running to infinity, with a simple Lambertian ground shaded by sunlight attenuated
along *its own* path down through the atmosphere (so the far synthetic ground warms
at sunset too, consistent with everything else). Chose a sphere over a disc
deliberately — a disc never curves away, so the horizon would never close, and would
be visibly wrong at exactly the distances where it's most noticeable. Since real
terrain covers everything nearby, the synthetic ground is only ever seen where aerial
perspective has already saturated, so its exact shading barely matters — what matters
is that the integral stops.

**Found and fixed the same category of numerical bug twice in one file:** the
ray-sphere intersection's quadratic constant term was written as `dot(o,o) - r²`,
which at planet scale (~4×10¹³) subtracts two huge floats to get a ~10¹⁰ difference —
float32 can't represent that; it catastrophically cancels. Harmless for the outer
atmosphere-shell intersection (thick, forgiving), but the *new* ground intersection
sits right at the horizon where a few meters of error is a visibly jittering horizon
line. Rewrote as `(|o|-r)(|o|+r)`, which keeps the small quantity (altitude) small
throughout, and reused it for both intersection functions.

**Mar's own fix, not mine — flagged as a real bug found via a real Editor workflow
problem:** the `EcologyEffects` prefab (camera rig, formerly full of TOD components)
had missing-script references blocking Editor saves. Before deleting them, Mar asked
whether the `sky`-named `IsDependency` on it could be load-bearing for boot. Checked
concretely rather than guessing: nothing consumes it (`Dependency("sky")` matches
zero files, both real consumers are commented out) and `UnityDependencyResolver`
already drops null-referenced entries before this, so removing it changes nothing —
confirmed safe. But the same prefab's `Light` GameObject (three components: Transform,
the game's *only* directional light, and an `IsDependency` named `sunLight`) is
genuinely load-bearing and sits in the same list of TOD-looking scenery objects
(`Sky Dome`, `Stars`, `Moon`, `Sun`, `Space`, `Clouds`) — easy to delete by mistake.
Both scene files have zero `Light` components and `RenderSettings.sun` is unset in
both, so this is the *only* source of sun direction in the whole game; losing it
would disable `StaticTiledTerrain` outright via the injector (unresolved
`[Dependency]`), silently killing `_SunDir`/`_SunIntensity`/`_Fogginess` and reading
as "broken terrain shading," not "broken light." Fixed `AtmosphereController` to
take the sun via the same `[Dependency("sunLight")]` StaticTiledTerrain uses instead
of its previous fallback-only resolution (RenderSettings.sun → brightest directional
light found by scene scan) — guarantees the sky and the terrain can't disagree about
where the sun is, and removes a scan that was fragile/instantiation-order-dependent
since it was the *only* thing that could ever fire (no scene lights, no
RenderSettings.sun).

**Result:** distance fog is back, blends into the sky by construction (same
integrator), responds to sun angle without any authored day/night gradient, and no
more black void below the horizon. Mar: "this can ship." Not yet retuned to match the
original TOD look exactly — `_exposure`/`_sunIntensityScale`/`_mieCoefficient` on
`AtmosphereController` are the first knobs to reach for if it reads too dark/bright/
hazy. Performance not yet profiled (24 samples/pixel on the fullscreen fog pass;
`ATMOS_VIEW_STEPS`/`ATMOS_SUN_STEPS` are the lever if it's too expensive).

**New known issue, unrelated, reported in passing:** parachute physics can go
unstable and "explode," currently hard-quitting the game via some failsafe rather
than erroring visibly — see Known Issues section above. Not yet investigated; no
repro steps yet.

**Correction, 2026-08-11 (Mar):** the "flight model has not been retuned against
Unity 6's PhysX" gap called out at the top of this doc and repeated in several
earlier entries turned out not to hold — Mar flew both the 5.5 and Unity 6 builds
and the wingsuit/parachute flight model feels intact, comparable to the original.
The parachute-explosion bug above is narrower than that: an impulse-cascade
instability, not a general tuning problem, and flight feels normal right up until
it happens. Struck the retuning task from the roadmap accordingly (see top of doc
and Findings). The dev vlog covering the port so far is up:
https://www.youtube.com/watch?v=UM4mRHCXfYM — old fans found it within two hours.

**Next session should pick up with:** either the parachute-physics-explosion repro,
or continue toward the rebinding UI (still the largest functional gap) — Mar's call.
Vectrocity (aero visualizer) remains unreplaced, low priority, same as before.

### 2026-08-11 — public milestone checkpoint

No code changes this entry — a reflection point, marked explicitly because this is
a natural place to lose thread across a context compaction.

Mar published a dev vlog covering the port
(https://www.youtube.com/watch?v=UM4mRHCXfYM); old fans found it within two hours,
reception positive. `README.md` was rewritten to match current reality: Unity 6 /
`unity6-port` branch install instructions, a Roadmap section, and a Known Issues
list corrected for everything above (TOD replaced, flight model NOT in need of
retuning, parachute impulse-cascade explosion, rebinding stub, silent FMOD, no
VR/multiplayer UI). Pushed to `origin` (`github.com/mzandvliet/VoloAirsport`).
Discussed but not yet acted on: the repo has 57 stars / 10 forks but only 8
watchers and no Discussions/Releases enabled — starring doesn't subscribe anyone
to notifications, only Watch does, so reaching the wider star/fork audience (vs.
just the 8 watchers) needs either enabling Discussions (owner-only setting) or
cutting a GitHub Release. Left as an open thread for Mar, not blocking.

**State to carry forward, for anyone (human or AI) picking this up cold:**
- The game is genuinely playable end to end (boot → fly → land → respawn → pause →
  spawn-select), repeatably, no known crashes in that loop.
- The flight model is NOT a known problem. Do not re-open "retune against PhysX" as
  a task without new evidence — it was checked directly (5.5 vs. Unity 6, flown
  side by side) and closed.
- The one open physics bug is narrow and specific: parachute impulse-cascade
  explosion, hard-quits rather than erroring, no repro steps yet. Don't conflate it
  with general flight feel.
- Three RamjetAnvil dependencies that used to be closed-source DLLs
  (CoroutineScheduler, StateMachine, PadroneClient) are now plain project source
  under `Assets/Plugins/RamjetAnvil/*/Source/`. If a future bug traces into one of
  these, read the source directly — do not assume it's still an opaque DLL.
- See `Documentation/dont-forget-me.md` for workflow/collaboration constraints that
  matter but don't fit a technical roadmap.

### 2026-08-14 — first real test coverage: CoroutineScheduler and StateMachine

A lot of the hardest bugs this port has hit have lived in these two plugins (double-pop
stack, the recycled-handle hang, the reentrant-`Update()` black screen — see the
2026-08-10 progress entries above). Built a real EditMode test suite against both,
grounded in three sources: CoroutineScheduler's own README/public API, how the game
actually uses `StateMachine<T>` (`VoloStateMachine`/`Playing.cs`), and the specific bugs
already found and fixed. 19 tests, all passing on first real run.

**Structural change required first:** the project has no `.asmdef` files anywhere
(deliberate monolithic compile — see Findings above). Unity's Test Framework assembly
can't reference the implicit `Assembly-CSharp`, so testing types that live there needs
an explicit compile boundary around them. Gave just these two plugins their own asmdefs,
scoped tightly to their `Source/` subfolders only:
- `Assets/Plugins/RamjetAnvil/CoroutineScheduler/Source/RamjetAnvil.Coroutine.asmdef`
- `Assets/Plugins/RamjetAnvil/StateMachine/Source/RamjetAnvil.StateMachine.asmdef`
  (references the one above)

The Unity-facing wrapper MonoBehaviours (`UnityCoroutineScheduler.cs`,
`FixedUnityCoroutineScheduler.cs`, both one level up from `Source/`) were deliberately
left **outside** these asmdefs and still compile straight into `Assembly-CSharp` as
before — they depend on the DI system and `AbstractUnityClock`, neither of which has an
asmdef, so pulling them in too would have meant either giving DI its own asmdef as well
(bigger, unrequested change) or leaving them broken. This means the rest of the
project's monolithic-compile decision is untouched; only these two plugins now have a
formal boundary. Also added `com.unity.test-framework` to `Packages/manifest.json`
(pulled in `com.unity.ext.nunit` automatically) and a new
`Assets/Tests/EditMode/RamjetAnvil.Plugins.EditMode.Tests.asmdef` referencing both
plugin asmdefs plus `UnityEngine.TestRunner`/`UnityEditor.TestRunner`.

**Test files:**
- `Assets/Tests/EditMode/CoroutineSchedulerTests.cs` (11 tests) — documented-behaviour
  coverage (`WaitFrames`/`WaitSeconds` exact-tick completion, `WaitRoutine` subroutine
  composition, `Interleave` concurrency, `Dispose` cancellation, `WaitUntilDone`,
  `AndThen`) plus three regression tests: the `RoutineHandle` generation-stamp fix (an
  old handle must keep reporting done even after its pooled slot is handed to an
  unrelated new routine — forced deterministic via `growthStep: 1`, so there's only ever
  one pooled `Routine` instance to reuse), a scheduler-reentrancy safety check, and "a
  faulting routine gets marked done instead of retried forever" (using
  `LogAssert.Expect` to catch the expected `[CoroutineScheduler]` error log).
- `Assets/Tests/EditMode/StateMachineTests.cs` (8 tests) — root entry, sibling
  transitions, child-transition suspend/enter via `PermitChild`, the unpermitted-
  transition guard, the top-level `TransitionToParent` guard, the `IsTransitioning`
  reentrancy guard (using a state whose `OnEnter` actually yields, so the transition
  spans real scheduler ticks and the guard window is externally observable), and
  `[StateEvent]` routing an owner event to the active state's private method (mirrors
  `VoloStateMachine`'s `[StateEvent("Update")]` pattern exactly). Plus the one
  regression test that matters most here: `TransitionToParent_DoesNotDoublePopStack_
  AcrossRepeatedSuspendResumeCycles` runs a suspend/resume cycle twice and asserts the
  *second* cycle still suspends/resumes correctly — this only passes if the stack depth
  was left correct after the first `TransitionToParent()` call, which is exactly what
  the historical double-pop bug got wrong (pause → resume → pause used to throw).

**One honest scope gap, noted in a comment rather than glossed over:** the actual
historical reentrancy crash lived in `UnityCoroutineScheduler.Run()`'s eager
self-`Update()` call (see the 2026-08-10 entry), which needs a live Update loop and DI
to exercise — outside what a pure-C# EditMode test against the isolated core can reach.
`SchedulerRemainsConsistent_WhenARoutineSchedulesAnotherRoutineDuringUpdate` tests the
same hazard shape (scheduling new work while mid-`Update()`) at the core
`CoroutineScheduler` level instead, which is a real and useful safety property but not
a byte-for-byte repro of the original incident.

**How to run:** headless, same pattern as the compile-error loop but with `-runTests`
instead of `-quit` (combining the two races Unity's own shutdown against the test
runner and silently skips the tests — hit this on the first attempt, the log showed a
clean compile but no test results at all):
```
Unity.exe -batchmode -nographics -projectPath <path> -runTests -testPlatform EditMode \
  -testResults <path>\results.xml -logFile <path>\run.log
```
Results land in NUnit XML; `<test-run ... result="Passed" total="19" passed="19"
failed="0">` at the top, one `<test-case ... result="Passed">` per test below that.

**Result:** 19/19 passing. This is the first automated regression coverage anything in
this codebase has had — previously, "did the fix work" meant Mar reproducing the bug by
hand in the Editor. Doesn't replace that for anything touching the Unity-side wrappers,
but the pure-C# core of both plugins now has a real safety net.
