/* Todo:
 * - Conversion between world-space coordinates and integer-type region coordinates
 */

namespace RamjetAnvil.Unity.Utility {
    
    public struct IntVector2 {
        public int X;
        public int Y;

        public IntVector2(int x, int y) {
            X = x;
            Y = y;
        }

        public static IntVector2 operator +(IntVector2 a, IntVector2 b) {
            return new IntVector2(a.X + b.X, a.Y + b.Y);
        }

        public static IntVector2 operator -(IntVector2 a, IntVector2 b) {
            return new IntVector2(a.X - b.X, a.Y - b.Y);
        }

        public static IntVector2 operator *(IntVector2 a, int b) {
            return new IntVector2(a.X * b, a.Y * b);
        }

        public bool Equals(IntVector2 other) {
            return X == other.X && Y == other.Y;
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) {
                return false;
            }
            return obj is IntVector2 && Equals((IntVector2) obj);
        }

        public override int GetHashCode() {
            unchecked {
                return (X.GetHashCode() * 397) ^ Y.GetHashCode();
            }
        }

        public static bool operator ==(IntVector2 left, IntVector2 right) {
            return left.Equals(right);
        }

        public static bool operator !=(IntVector2 left, IntVector2 right) {
            return !left.Equals(right);
        }

        public override string ToString() {
            return "[" + X + ", " + Y + "]";
        }

        public static readonly IntVector2 Zero = new IntVector2(0,0);
    }

}