# Volo Airsport

Summary of what you need to know to contribute to Volo Airsport.

## Input
The game reads input from the `Config/Input/GameInputConfig_User.xml` file. If that file cannot be found, which means that there is no user-defined input configuration, a default input configuration will be used. Currently it uses the file `Config/Input/GameInputConfig_KeyboardDefault.xml` for that.

If you want to use the Xbox 360 controller instead of the keyboard simply copy the `Config/Input/GameInputConfig_ControllerDefault.xml` to `Config/Input/GameInputConfig_User.xml`.