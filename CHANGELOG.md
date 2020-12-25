#v3.5 (16 July 2015)
##Features

- Engine upgrade to Unity 5 (new shading solution, graphics optimizations, etc.)
- Completely new terrain graphics
- Season, weather, and day-night cycles that are completely tweak-able to your own liking
- All kinds of new sound effects by Micheal Manning
- A new logo designed by Moldybyrd Studio
- An introduction screen
- Linux 64-bit version added

##Fixes

- Missing a ring during a course does no longer stop you from finishing it

#v3.4 (28 January 2015)

##Features

- Added a Course Editor tool that allows players to create and share their own time trial courses.

##Changes

- Added Credits screen to the options menu.
- Game will now always check for a joystick even if the notification is already gone

##Fixes

- Possibly fixed an issue where the game would crash due to a bug in the sound mixer.

#v3.3 (5 December 2014)

##Features

- Introducing the new flight model: 'Wingman Mk. II'.
- Added a trajectory visualizer helping you to learn how to fly. A line visualizes
what will happen in the next five seconds if you wouldn't do anything, giving you time
to change course. For those who already have expert flight skills this feature
can be turned off in the options menu.
- Your speed, altitude and glide ratio are now displayed while playing. This can be turned off in the options menu. Both metric (km/h) and imperial (mph) unit systems are supported.
- You can now turn on the aerodynamics visualizer which will give you a glimpse of what's going on under the hood of Volo's flight model.
- Time trials can now be switched off completely for those who just want to enjoy a distractionless flight.
- Time trial rings can now be set as ghost rings disabling the collision on them making it easier to do the trails.

##Changes

- Third-person camera sways a bit more.
- Flying with mouse and keyboard is now a bit easier. A controller is still recommended though.
- Lowered sensitivity of keyboard flight controls. Flying with the keyboard
is now much easier than it was in v3.2.
- Added a 'keyboard only' input mapping to the Input menu for those who like to
fly without using the mouse.
- Accuracy of start and finish timers on the time trail gates is now much higher. You can now accurately compare your times with other players.

##Fixes
- When running in fullscreen mode the game now runs in true fullscreen mode instead
of running in a fullscreen window. This should improve performance on some systems.
- Small lag spike that occurred when starting a course is now gone.
- Notification list aspect ratio is no longer off.

#v3.2 (15 October 2014)

##Features

- Time Trials
  - Look for rings dotted throughout the landscape, there are many to find!
  - Fly through a ring to start a trial. Fly through all its rings, and when you’re finished you’ll see your completion time.
- Added a Motion Blur effect
- Added new version notifications. The game will tell you about newly released versions when it starts.
- You can add spin to your starts by holding down pitch or roll when you press respawn (a bit cumbersome, but it works)

##Changes

- Improved mouse controls. Large inputs are now buffered until you steer the other way, making it easier to do large turns.
- Screenshake effect intensity is now configurable
- 3rd Person Camera is now much less prone to glitching out
- 3rd person Look Behind feature now works properly
- Stronger friction between player skin and ground
- Camera transition in Oculus Mode is now a comfortable fade-to-black, not a sickening warp
- Default speed scaling effect is now better balanced, should make the character respond more evenly across different speeds.
- Manual is now available on the forums
- Updated documentation on using PS3 and PS4 controllers
- “Set Controller Defaults” is now more accurately named “Set Xbox 360 Defaults”

##Fixes

- Fixed Atmospheric Scattering effect not adapting to Field of View setting
- Fixed cannonball move damaging your legs
- Fixed game not working properly with multiple controllers attached (and some other controller detection issues)
  - You now need to press a button on the controller you want to use each time you play. The game tells you this.
- You can no longer crash into invisible trees. Tree visibility now has a minimum of 100 meters.
- Fixed most jarring instances of spikes/teeth in terrain geometry
- Fixed lots of performance issues (especially in menus, and with large amounts of trees visible)

#v3.1 (16 September 2014)

##Features

- Front and back flips (use the ‘cannonball’ action, replaced closing of leg wing)
- Added extra spawn points at new base exit points
- Added two new camera perspectives
- Screenshot mode (unfinished, gamepad only)

##Changes

- Added input sensitivity options
- Default sensitivity values are a lot lower, for smoother gameplay
- Pitch heading is trimmed slightly more down, causing a steeper default glide

##Fixes

- Updated to Oculus SDK 0.4.2 Beta, which fixed some judder issues
- Fixed black screen issue on AMD cards in Oculus Rift Mode
- Black terrain issue on Macs with Intel Integrated Graphics chips is fixed
  - Comes at a slight performance cost (HD4000 is the absolute lowest supported)
  - A better fix is possible, but will take some more time
  - Oculus Mode will not work on these chips at this time
- Partial fix for some controllers not being registered by the game

#v3.0 (3 September 2014)

- Revamped flight model
- A huge landscape inspired by the Swiss and French Alps, with lots of lines to try
- Multiple camera perspectives (including first-person view)
- Highly configurable graphics and input settings
- Oculus Rift support (currently experimental)
