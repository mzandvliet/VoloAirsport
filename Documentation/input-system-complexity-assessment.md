# Input Routing / Configuration System — Complexity Assessment

Date: 2026-08-08 (written same session as the first Input System port pass, see
[unity6-port-roadmap.md](unity6-port-roadmap.md))

## Context

While reverse-engineering the old Impero + InControl input stack in order to stub it out
enough to compile against the new Input System (none of `RamjetAnvil.Impero.StandardInput`,
`RamjetAnvil.Impero.Unity`, `InputBinder`, `InputBindingViewModel`, `InputBindings<T>`,
`JoystickActivator`, `ControllerId`/`ControllerType` existed in the repo — see the roadmap's
progress log), the full shape of the old system became visible for the first time this port.
Mar's memory going in: "gosh, this is all getting complex and over my head, I never liked it
— but we needed *some* flexibility, and Frank used his functional programming background to
build something real." This document is the grounded version of that memory, written after
actually reading the code, plus a recommendation for what to do differently when the real
rebinding UI gets built (the current state is an inert stub — see roadmap progress log).

## What's there

Layering, innermost to outermost:
- Per-domain action enums (`WingsuitAction`, `MenuAction`, `ParachuteAction`, `SpectatorAction`).
- An action-map polling layer (originally Impero+InControl; now `ActionMap<TAction>` on the
  new Input System) exposing `PollAxis`/`PollButton`/`PollButtonEvent`/`PollMouseAxis`.
- Three `...ActionMapProvider` MonoBehaviours (Pilot/Menu/Parachute), each DI-injected.
- `IReadonlyRef<T>` / `Ref<T>` — a hand-rolled mutable-cell indirection so a live-rebound
  action map can be swapped out while existing references keep seeing the current one.
- `InputBindings<TAction>` — reactive (`IObservable<InputSettings>` in,
  `IObservable<ActionMapConfig<TAction>>` out), tracks the active controller, exposes
  `UpdateMapping`/`LoadDefaultActionMap`, serializes to disk.
- Four near-identical static classes (`PilotInput.Bindings`, `MenuInput.Bindings`,
  `SpectatorInput.Bindings`, `ParachuteControls`) providing `InitialMapping()`,
  `DefaultControllerMappings`, `CustomInputMappingFilePath`, `ToBindings(...)`.
- `JoystickActivator` — controller-connect detection as `IObservable<ConnectedController?>`.
- `InputBinder` — the interactive "press a button to bind it" capture flow.
- `InputBindingId`/`InputBindingGroup`/`InputBindingViewModel` — a flat, UI-facing list that
  merges all four action domains into one array for the rebinding screen.
- Dependency injection (`[Dependency, SerializeField]`) wiring most of the above into scene
  MonoBehaviours, on top of Unity's own serialization.

## Verdict: yes, there's real excess complexity, not just unfamiliarity

Four concrete smells, found by actually reading the code rather than impression:

1. **Three overlapping "what controller is this" enums.** `ControllerId` (XInput/DirectInput/
   Other — an API-level distinction), `ControllerType` (Xbox360/XboxOne/SteamController/
   Playstation4/Other — a hardware-family distinction), and `InputDefaults`
   (KeyboardAndMouse/KeyboardOnly/Xbox360/XboxOne/SteamController/Playstation4/XInput — a
   settings-preset concept overlapping both of the others). Converted back and forth via
   extension methods (`ToControllerType`, `ToInputDefaults`, `VerifyController`). Classic
   "three enums that should have been one" — looks like InControl's device-API concept,
   Impero's hardware-family concept, and the settings system's serialized-preset concept
   each got bolted on separately instead of unified.

2. **Four hand-copied static classes** (`PilotInput.Bindings`, `MenuInput.Bindings`,
   `SpectatorInput.Bindings`, `ParachuteControls`) that are structurally identical, varying
   only by the generic action-enum type — never generalized into one
   `InputBindingsCatalog<TAction>`-style factory.

3. **A type-erased tagged union for the rebind UI's flat list.** `InputBindingId.ActionId` is
   typed `object`, cast back to the real enum at the point of use
   (`(MenuAction) bindingId.ActionId`). Reimplements what a proper discriminated union (or
   just four separate lists) gives for free, and loses compile-time safety in exactly the
   rebinding flow, where a wrong cast fails silently rather than refusing to compile.

4. **Inconsistent access shape across three structurally-identical provider types.**
   `PilotActionMapProvider.ActionMap` returns the map directly; `MenuActionMapProvider
   .ActionMap` returns a ref-cell (`.ActionMap.V`); `ParachuteActionMapProvider` *is* the
   ref-cell itself (`.V`). Same concept, three different shapes to remember, no payoff for
   the inconsistency.

## What genuinely earned its keep

The reactive (`IObservable`/`CombineLatest`) backbone threading settings → bindings → UI is
legitimate, not gratuitous — "recompute the rebind list whenever settings change OR the
controller changes OR the language changes" is exactly what `CombineLatest` is for; hand-rolled
events would have been worse. Frank's instinct there was sound.

The DI container is a wash, not a clear win: it didn't eliminate manual wiring the way it was
meant to — `VoloModule.cs`'s composition root still does a pile of `FindObjectOfType` calls by
hand — so it added a layer of indirection without fully delivering on removing the plumbing it
was supposed to replace.

## Recommendation for the real rebinding UI pass

Don't port the old shape faithfully. The new Input System already provides built-in
replacements for most of the custom machinery above:

- `InputActionAsset` already solves "flat list of rebindable things across multiple action
  maps" natively — every binding across every map can be enumerated generically. No
  hand-rolled `InputBindingGroup`/`InputBindingId` tagged union needed.
- Bindings are first-class serializable *paths* (strings), with built-in interactive
  rebinding (`InputActionRebindingExtensions.PerformInteractiveRebinding`) and JSON save/load
  on the asset itself — this can absorb the entire `InputBindings<T>`/`InputSourceMapping<T>`
  config-and-persistence layer currently stubbed out.
- Device detection is just `InputSystem.onDeviceChange` / `Gamepad.current` — the Input
  System's own control-scheme concept can likely replace `ControllerId` + `ControllerType` +
  `InputDefaults` in one shot, rather than needing three enums cross-converted by hand.

Expected outcome: a redesigned rebinding UI built around what the Input System already gives
you should end up meaningfully *smaller* than the current stub's design surface, not just a
re-skin of the same layering. Worth treating as an explicit design goal for that session, not
an incidental cleanup.
