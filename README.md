# Volo Airsport

A wingsuiting and parachuting simulation by Ramjet Anvil.

[![Volo Trailer](https://i.imgur.com/CI9w4XF.png)](https://www.youtube.com/watch?v=y2NQVOPU1nU)

Released in Early Access on Steam and Humble in 2014 (later on Itch.io). We halted development in 2017.

If you like this game, or this code, you can buy the last release on Itch.io and support our current and future work :)

https://ramjetanvil.itch.io/volo-airsport

**Update, 2026:** we're porting the project to Unity 6. Follow along on the [dev vlog](https://www.youtube.com/watch?v=UM4mRHCXfYM) — the game is barebones playable again (boot, fly, land, respawn, pause menu), with more work ongoing. See the roadmap and known issues below for where things stand.

## Support this work:

Buy me a [Ko-fi](https://ko-fi.com/marblackstar) 
Buy Volo Airsport on [itch.io](https://ramjetanvil.itch.io/volo-airsport)

## Instructions:

- Download **Unity 6 (6000.3.18f1 or later)** via [Unity Hub](https://unity.com/download)
- Check out the `unity6-port` branch — this is where active work is happening; `main` still reflects the original 2017 Unity 5.5 release
- Open the project folder in the Editor and let it complete its one-time upgrade

The original Unity 5.5.0f3 project is still available in git history and on the `main` branch if you want the untouched original.

## Roadmap:

Broad direction for the Unity 6 port, roughly in order:

- Rebuild the control rebinding UI on Unity's Input System (the old rebinding UI is currently a non-functional stub — see Known Issues)
- Investigate and fix the parachute physics instability (see Known Issues) — the wingsuit and parachute flight models otherwise feel intact and comparable to the original 5.5 release
- Reintegrate FMOD audio (currently silent — see Known Issues)
- Decide between prioritizing another open source release vs. a Steam re-release, once the above is in a stable enough state to make that call

## Known Issues:

- Several commercial plugins and assets were removed for this open source release, and need replacement
  - ~~Time of Day (Atmospheric Scattering, volumetric fog)~~ — replaced with a custom Rayleigh/Mie scattering shader as of the Unity 6 port; no volumetric fog or clouds yet
  - Vectrocity (rendering aerodynamics information, trajectory prediction) — not yet replaced
  - Several 3D model assets (lumberjack huts, wind turbines) — still missing
- Input rebinding UI is a non-functional stub — keyboard/mouse and gamepad both work in-game, but there is currently no way to reconfigure bindings or detect controllers from the options menu
- FMOD audio is not yet reintegrated — the game currently runs silent
- Parachute physics can occasionally go unstable via impulse cascades and "explode" — the flight model feels normal right up until this happens, we haven't yet characterized what triggers it, and it currently crashes the game rather than failing gracefully
- VR support is not currently wired up
- Multiplayer/master-server features are not exposed — the underlying networking is still present (singleplayer actually routes through it internally) but there's no way to host or join a game from the UI

## Credits:

Lead Designer, Programmer
- Martijn Zandvliet

Designer, Programmer
- Frank Versnel

Designer, Ramjet Anvil Show Host
- Xalavier Nelson

Sound Design & Music
- Michael Manning

Concept Art
- Diana van Houten
- J.J. Epping

## Parachute System

For the parachuting system, here are some starting points:

Airfoil code:

Assets\Plugins\RamjetAnvil\Aero\Scripts\Physics\Aerodynamics

Input routing from our input system to the parachute controller:

Assets\Scripts\Player\PlayerController.cs
Assets\Scripts\Input\ParachuteInput.cs

Most code for it lives here:

Assets\Scripts\Test\Parachute
Assets\Scripts\Test\Cell.cs

The parachute system creates controllable parafoils through procedural generation, and broadly functions as follows:

ParachuteConfig.cs - Parameterization data for a parachute
Parachute.cs - Top-level component for a parachute instance
ParachuteFactory.cs - System for producing an instance of a parachute from a config, including physics and visuals
ParachuteEditor.cs - In-game editor GUI for modifying ParachuteConfigs, which Factory then produces
ParachuteSpawner.cs - Uses the above to create a parachute in game

## Input

As of the Unity 6 port, input is handled by Unity's Input System package rather than the old XML-config-based system (`GameInputConfig_*.xml`, no longer present). Keyboard/mouse and any generic gamepad work out of the box; there is no in-game way to rebind controls yet (see Known Issues above).
