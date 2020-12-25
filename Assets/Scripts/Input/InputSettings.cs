using System;

namespace RamjetAnvil.Volo.Input {

    public struct InputSettings : IEquatable<InputSettings> {

        public static readonly InputSettings Default = new InputSettings(inputGamma: 2.0f, wingsuitMouseSensitivity: 1f, parachuteMouseSensitivity: 0.07f, 
            inputSpeedScaling: 0.85f, joystickDeadzone: 0.2f);

        public readonly float InputGamma;
        public readonly float InputSpeedScaling;
        public readonly float WingsuitMouseSensitivity;
        public readonly float ParachuteMouseSensitivity;
        public readonly float JoystickDeadzone;

        public InputSettings(float inputGamma, float inputSpeedScaling, float wingsuitMouseSensitivity, 
            float parachuteMouseSensitivity, float joystickDeadzone) {

            InputGamma = inputGamma;
            WingsuitMouseSensitivity = wingsuitMouseSensitivity;
            ParachuteMouseSensitivity = parachuteMouseSensitivity;
            JoystickDeadzone = joystickDeadzone;
            InputSpeedScaling = inputSpeedScaling;
        }

        public static InputSettings FromGameSettings(GameSettings.InputSettings settings) {
            return new InputSettings(
                settings.InputGamma,
                settings.InputSpeedScaling,
                settings.WingsuitMouseSensitivity,
                settings.ParachuteMouseSensitivity,
                settings.JoystickDeadzone);
        }

        public bool Equals(InputSettings other) {
            return InputGamma.Equals(other.InputGamma) && InputSpeedScaling.Equals(other.InputSpeedScaling) && WingsuitMouseSensitivity.Equals(other.WingsuitMouseSensitivity) && ParachuteMouseSensitivity.Equals(other.ParachuteMouseSensitivity) && JoystickDeadzone.Equals(other.JoystickDeadzone);
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) return false;
            return obj is InputSettings && Equals((InputSettings) obj);
        }

        public override int GetHashCode() {
            unchecked {
                var hashCode = InputGamma.GetHashCode();
                hashCode = (hashCode * 397) ^ InputSpeedScaling.GetHashCode();
                hashCode = (hashCode * 397) ^ WingsuitMouseSensitivity.GetHashCode();
                hashCode = (hashCode * 397) ^ ParachuteMouseSensitivity.GetHashCode();
                hashCode = (hashCode * 397) ^ JoystickDeadzone.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(InputSettings left, InputSettings right) {
            return left.Equals(right);
        }

        public static bool operator !=(InputSettings left, InputSettings right) {
            return !left.Equals(right);
        }
    }
}