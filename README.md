# Volo Airsport

A wingsuiting and parachuting simulation by Ramjet Anvil.

[![Volo Trailer](https://i.imgur.com/CI9w4XF.png)](https://www.youtube.com/watch?v=y2NQVOPU1nU)

Released in Early Access programon Steam, Humble in 2014 (later on Itch.io). We halted development in 2017.

If you like this game, or this code, you can buy the last release on Itch.io and support our current and future work :)

https://ramjetanvil.itch.io/volo-airsport

## Instructions:

- Download Unity Editor 5.5.0f3 from the [Unity Download Archives](https://unity3d.com/get-unity/download/archive)
- Open the project folder

## Known Issues:

- Several commercial plugins and assets were removed for this open source release, and need replacement
  - Time of Day (Atmospheric Scattering, volumetric fog)
  - Vectrocity (Rendering aerodynamics information, trajectory prediction)
  - Several 3d model assets (lumberjack huts, wind turbines)
  
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
E:\code\unity\volo_airsport_open\Assets\Scripts\Input\ParachuteInput.cs

Most code for it lives here:

E:\code\unity\volo_airsport_open\Assets\Scripts\Test\Parachute
E:\code\unity\volo_airsport_open\Assets\Scripts\Test\Cell.cs

The parachute system creates controllable parafoils through procedural generation, and broadly functions as follows:

ParachuteConfig.cs - Parameterization data for a parachute
Parachute.cs - Top-level component for a parachute instance
ParachuteFactory.cs - System for producing an instance of a parachute from a config, including physics and visuals
ParachuteEditor.cs - In-game editor GUI for modifying ParachuteConfigs, which Factory then produces
ParachuteSpawner.cs - Uses the above to create a parachute in game

## Input
The game reads input from the `Config/Input/GameInputConfig_User.xml` file. If that file cannot be found, which means that there is no user-defined input configuration, a default input configuration will be used. Currently it uses the file `Config/Input/GameInputConfig_KeyboardDefault.xml` for that.

If you want to use the Xbox 360 controller instead of the keyboard simply copy the `Config/Input/GameInputConfig_ControllerDefault.xml` to `Config/Input/GameInputConfig_User.xml`.
