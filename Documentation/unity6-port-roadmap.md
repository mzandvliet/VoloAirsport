# Unity 6 Port Roadmap

Status: mid-port, compiles clean, boots and runs - gamepad now reaches the
title screen's Play/Title-screen selection. Not yet fully playable.
Branch: `unity6-port` (branched from `butcher`)
Last updated: 2026-08-08 (runtime debugging session)

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

**Next session should pick up with:** getting further past the title screen into actual
gameplay (spawn, flight), then the rebinding UI / `JoystickActivator` / `InputBindings<T>`
real implementation, then flight-model retuning against Unity 6's PhysX.

**Known landmine directly in that path:** the "`Debug.LogWarning("I want to handle
input")`, real logic commented out beneath it" dead-stub pattern (same shape as the
`GlobalMenuInputEventEmitter`/`TitleScreen` bugs just fixed) still exists in **5 more
files** — `grep -rl "I want to handle" Assets/Scripts` finds `FlyWingsuit.cs`,
`ParachuteStates.cs`, `Playing.cs`, `SpawnScreen.cs`, `SpectatorMode.cs`, plus
`HeadCameraController.cs` and `OpenVrCameraRig.cs` (lower priority — VR/head-look).
`Playing.cs` and `SpawnScreen.cs` are almost certainly the very next ones to bite,
since they're on the direct path from title screen into spawn/flight. Expect the same
"nothing responds to input in this state" symptom, same fix shape (reconnect to the
already-working `MenuActionMapProvider`/`PilotActionMapProvider`/`Events.*` rather than
trying to resurrect the commented-out Impero code).
