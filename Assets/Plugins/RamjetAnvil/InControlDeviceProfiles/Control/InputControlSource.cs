using System;


namespace InControl
{
    public abstract class InputControlSource {
        private InputControlSource() {}

        public sealed class Button : InputControlSource, IEquatable<Button> {
            public readonly int Index;

            public Button(int index) {
                Index = index;
            }

            public bool Equals(Button other) {
                if (ReferenceEquals(null, other)) return false;
                if (ReferenceEquals(this, other)) return true;
                return Index == other.Index;
            }

            public override bool Equals(object obj) {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                return obj is Button && Equals((Button) obj);
            }

            public override int GetHashCode() {
                return Index;
            }

            public static bool operator ==(Button left, Button right) {
                return Equals(left, right);
            }

            public static bool operator !=(Button left, Button right) {
                return !Equals(left, right);
            }

            public override string ToString() {
                return string.Format("Button({0})", Index);
            }
        }

        public sealed class Axis : InputControlSource, IEquatable<Axis> {
            public readonly int Index;

            public Axis(int index) {
                Index = index;
            }

            public bool Equals(Axis other) {
                if (ReferenceEquals(null, other)) return false;
                if (ReferenceEquals(this, other)) return true;
                return Index == other.Index;
            }

            public override bool Equals(object obj) {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                return obj is Axis && Equals((Axis) obj);
            }

            public override int GetHashCode() {
                return Index;
            }

            public static bool operator ==(Axis left, Axis right) {
                return Equals(left, right);
            }

            public static bool operator !=(Axis left, Axis right) {
                return !Equals(left, right);
            }

            public override string ToString() {
                return string.Format("Axis({0})", Index);
            }
        }

    }

}

