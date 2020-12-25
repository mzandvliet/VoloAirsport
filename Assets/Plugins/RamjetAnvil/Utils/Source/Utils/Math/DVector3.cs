using System;
using RamjetAnvil.Unity.Utility;
using UnityEngine;

public struct DVector3
{
    public static readonly DVector3 Zero = new DVector3(0d, 0d, 0d);
    public static readonly DVector3 One = new DVector3(1d, 1d, 1d);
    public static readonly DVector3 Right = new DVector3(1d, 0d, 0d);
    public static readonly DVector3 Up = new DVector3(0d, 1d, 0d);
    public static readonly DVector3 Forward = new DVector3(0d, 0d, 1d);

    public const double kEpsilon = 1E-05d;
    private double _x;
    private double _y;
    private double _z;

    public double X
    {
        get { return _x; }
        set { _x = value; }
    }

    public double Y
    {
        get { return _y; }
        set { _y = value; }
    }

    public double Z
    {
        get { return _z; }
        set { _z = value; }
    }

    public DVector3(double x, double y, double z)
    {
        _x = x;
        _y = y;
        _z = z;
    }

    public DVector3(Vector3 vector)
    {
        _x = vector.x;
        _y = vector.y;
        _z = vector.z;
    }

    public double this[int index]
    {
        get
        {
            switch (index)
            {
                case 0:
                    return _x;
                case 1:
                    return _y;
                case 2:
                    return _z;
                default:
                    throw new IndexOutOfRangeException("Invalid DoubleVector3 index!");
            }
        }
        set
        {
            switch (index)
            {
                case 0:
                    _x = value;
                    break;
                case 1:
                    _y = value;
                    break;
                case 2:
                    _z = value;
                    break;
                default:
                    throw new IndexOutOfRangeException("Invalid DoubleVector3 index!");
            }
        }
    }

    public static DVector3 operator +(DVector3 a, DVector3 b)
    {
        return new DVector3(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
    }

    public static DVector3 operator -(DVector3 a, DVector3 b)
    {
        return new DVector3(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
    }

    public static DVector3 operator -(DVector3 a)
    {
        return new DVector3(-a.X, -a.Y, -a.Z);
    }

    public static DVector3 operator *(DVector3 a, double d)
    {
        return new DVector3(a.X * d, a.Y * d, a.Z * d);
    }

    public static DVector3 operator *(double d, DVector3 a)
    {
        return new DVector3(a.X * d, a.Y * d, a.Z * d);
    }

    public static DVector3 operator /(DVector3 a, double d)
    {
        return new DVector3(a.X / d, a.Y / d, a.Z / d);
    }

    public static bool operator ==(DVector3 lhs, DVector3 rhs)
    {
        return SqrMagnitude(lhs - rhs) < 0.0 / 1.0;
    }

    public static bool operator !=(DVector3 lhs, DVector3 rhs)
    {
        return SqrMagnitude(lhs - rhs) >= 0.0 / 1.0;
    }

    public static DVector3 Lerp(DVector3 from, DVector3 to, double t)
    {
        t = Mathd.Clamp01(t);
        return new DVector3(from.X + (to.X - from.X) * t, from.Y + (to.Y - from.Y) * t, from.Z + (to.Z - from.Z) * t);
    }

    public DVector3 GetNormalized
    {
        get
        {
            return Normalize(this);
        }
    }

    public double GetMagnitude
    {
        get
        {
            return Math.Sqrt(_x * _x + _y * _y + _z * _z);
        }
    }

    public double GetSqrMagnitude
    {
        get
        {
            return _x * _x + _y * _y + _z * _z;
        }
    }

    public static DVector3 Scale(DVector3 a, DVector3 b)
    {
        return new DVector3(a.X * b.X, a.Y * b.Y, a.Z * b.Z);
    }

    public void Scale(DVector3 scale)
    {
        _x *= scale.X;
        _y *= scale.Y;
        _z *= scale.Z;
    }

    public static DVector3 Cross(DVector3 lhs, DVector3 rhs)
    {
        return new DVector3((lhs.Y * rhs.Z - lhs.Z * rhs.Y), (lhs.Z * rhs.X - lhs.X * rhs.Z), (lhs.X * rhs.Y - lhs.Y * rhs.X));
    }

    public override int GetHashCode()
    {
        return X.GetHashCode() ^ Y.GetHashCode() << 2 ^ Z.GetHashCode() >> 2;
    }

    public override bool Equals(object other)
    {
        if (!(other is DVector3))
            return false;
        DVector3 dVector3 = (DVector3)other;
        if (X.Equals(dVector3.X) && Y.Equals(dVector3.Y))
            return Z.Equals(dVector3.Z);
        else
            return false;
    }

    public static DVector3 Reflect(DVector3 inDirection, DVector3 inNormal)
    {
        return -2d * DVector3.Dot(inNormal, inDirection) * inNormal + inDirection;
    }

    public static DVector3 Normalize(DVector3 value)
    {
        double num = DVector3.Magnitude(value);
        if (num > 9.99999974737875E-06)
            return value / num;
        else
            return DVector3.Zero;
    }

    public void Normalize()
    {
        double num = DVector3.Magnitude(this);
        if (num > 9.99999974737875E-06)
            this = this / num;
        else
            this = DVector3.Zero;
    }

    public override string ToString()
    {
        return string.Format("({0:F1}, {1:F1}, {2:F1})", X, Y, Z);
    }

    public string ToString(string format)
    {
        return string.Format("({0}, {1}, {2})", X.ToString(format), Y.ToString(format), Z.ToString(format));
    }

    public static double Dot(DVector3 lhs, DVector3 rhs)
    {
        return (lhs.X * rhs.X + lhs.Y * rhs.Y + lhs.Z * rhs.Z);
    }

    public static DVector3 Project(DVector3 vector3, DVector3 onNormal)
    {
        double num = Dot(onNormal, onNormal);
        if (num < 1.40129846432482E-45)
            return Zero;
        else
            return onNormal * DVector3.Dot(vector3, onNormal) / num;
    }

    public static DVector3 Exclude(DVector3 excludeThis, DVector3 fromThat)
    {
        return fromThat - DVector3.Project(fromThat, excludeThis);
    }

    public static double Angle(DVector3 from, DVector3 to)
    {
        return Math.Acos(Mathd.Clamp(Dot(from.GetNormalized, to.GetNormalized), -1d, 1d)) * 57.29578d;
    }

    public static double Distance(DVector3 a, DVector3 b)
    {
        DVector3 dVector3 = new DVector3(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
        return Math.Sqrt((dVector3.X * dVector3.X + dVector3.Y * dVector3.Y + dVector3.Z * dVector3.Z));
    }

    public static DVector3 ClampMagnitude(DVector3 vector3, double maxLength)
    {
        if (vector3.GetSqrMagnitude > maxLength * maxLength)
            return vector3.GetNormalized * maxLength;
        else
            return vector3;
    }

    public static double Magnitude(DVector3 a)
    {
        return Math.Sqrt((a.X * a.X + a.Y * a.Y + a.Z * a.Z));
    }

    public static double SqrMagnitude(DVector3 a)
    {
        return (a.X * a.X + a.Y * a.Y + a.Z * a.Z);
    }

    public static DVector3 Min(DVector3 lhs, DVector3 rhs)
    {
        return new DVector3(Mathd.Min(lhs.X, rhs.X), Mathd.Min(lhs.Y, rhs.Y), Mathd.Min(lhs.Z, rhs.Z));
    }

    public static DVector3 Max(DVector3 lhs, DVector3 rhs)
    {
        return new DVector3(Mathd.Max(lhs.X, rhs.X), Mathd.Max(lhs.Y, rhs.Y), Mathd.Max(lhs.Z, rhs.Z));
    }

    public static Vector3 ToVector3(DVector3 vector)
    {
        return ToVector3(vector.X, vector.Y, vector.Z);
    }

    public static Vector3 ToVector3(double x, double y, double z)
    {
        return new Vector3((float)x, (float)y, (float)z);
    }
}