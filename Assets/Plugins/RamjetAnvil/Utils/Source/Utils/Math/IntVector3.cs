/* Todo:
 * - Conversion between world-space coordinates and integer-type region coordinates
 */

namespace RamjetAnvil.Unity.Utility {
    
    public struct IntVector3 {
        public int X;
        public int Y;
        public int Z;

        public IntVector3(int x, int y, int z) {
            X = x;
            Y = y;
            Z = z;
        }

        public static IntVector3 operator +(IntVector3 a, IntVector3 b) {
            return new IntVector3(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
        }

        public static IntVector3 operator -(IntVector3 a, IntVector3 b) {
            return new IntVector3(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
        }

        public bool Equals(IntVector3 other) {
            return X == other.X && Y == other.Y && Z == other.Z;
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) return false;
            return obj is IntVector3 && Equals((IntVector3) obj);
        }

        public override int GetHashCode() {
            unchecked {
                int hashCode = X.GetHashCode();
                hashCode = (hashCode*397) ^ Y.GetHashCode();
                hashCode = (hashCode*397) ^ Z.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(IntVector3 left, IntVector3 right) {
            return left.Equals(right);
        }

        public static bool operator !=(IntVector3 left, IntVector3 right) {
            return !left.Equals(right);
        }

        public override string ToString() {
            return "[" + X + ", " + Y + ", " + Z + "]";
        }

        public static readonly IntVector3 Zero = new IntVector3(0,0,0);
    }

}