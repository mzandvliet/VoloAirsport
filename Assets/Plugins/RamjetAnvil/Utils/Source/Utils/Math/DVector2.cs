using System;
using RamjetAnvil.Unity.Utility;
using UnityEngine;

public struct DVector2
{
    public static readonly DVector2 Zero = new DVector2(0d, 0d);
    public static readonly DVector2 One = new DVector2(1d, 1d);
    public static readonly DVector2 Right = new DVector2(1d, 0d);
    public static readonly DVector2 Up = new DVector2(0d, 1d);

    public const double kEpsilon = 1E-05d;
    private double _x;
    private double _y;

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

    public DVector2(double x, double y)
    {
        _x = x;
        _y = y;
    }

    public DVector2(Vector2 vector)
    {
        _x = vector.x;
        _y = vector.y;
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
                default:
                    throw new IndexOutOfRangeException("Invalid DVector2 index!");
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
                default:
                    throw new IndexOutOfRangeException("Invalid DVector2 index!");
            }
        }
    }

    public static DVector2 operator +(DVector2 a, DVector2 b)
    {
        return new DVector2(a.X + b.X, a.Y + b.Y);
    }

    public static DVector2 operator -(DVector2 a, DVector2 b)
    {
        return new DVector2(a.X - b.X, a.Y - b.Y);
    }

    public static DVector2 operator -(DVector2 a)
    {
        return new DVector2(-a.X, -a.Y);
    }

    public static DVector2 operator *(DVector2 a, double d)
    {
        return new DVector2(a.X * d, a.Y * d);
    }

    public static DVector2 operator *(double d, DVector2 a)
    {
        return new DVector2(a.X * d, a.Y * d);
    }

    public static DVector2 operator /(DVector2 a, double d)
    {
        return new DVector2(a.X / d, a.Y / d);
    }

    public static bool operator ==(DVector2 lhs, DVector2 rhs)
    {
        return SqrMagnitude(lhs - rhs) < 0.0 / 1.0;
    }

    public static bool operator !=(DVector2 lhs, DVector2 rhs)
    {
        return SqrMagnitude(lhs - rhs) >= 0.0 / 1.0;
    }

    public static DVector2 Lerp(DVector2 from, DVector2 to, double t)
    {
        t = Mathd.Clamp01(t);
        return new DVector2(from.X + (to.X - from.X) * t, from.Y + (to.Y - from.Y) * t);
    }

    public DVector2 GetNormalized
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
            return Math.Sqrt(_x * _x + _y * _y);
        }
    }

    public double GetSqrMagnitude
    {
        get
        {
            return _x * _x + _y * _y;
        }
    }

    public static DVector2 Scale(DVector2 a, DVector2 b)
    {
        return new DVector2(a.X * b.X, a.Y * b.Y);
    }

    public void Scale(DVector2 scale)
    {
        _x *= scale.X;
        _y *= scale.Y;
    }

    public override int GetHashCode()
    {
        return _x.GetHashCode() ^ _y.GetHashCode() << 2;
    }

    public override bool Equals(object other)
    {
        if (!(other is DVector2))
            return false;
        DVector2 dVector3 = (DVector2)other;
        return (X.Equals(dVector3.X)) && Y.Equals(dVector3.Y);
    }

    public static DVector2 Normalize(DVector2 value)
    {
        double num = DVector2.Magnitude(value);
        if (num > 9.99999974737875E-06)
            return value / num;
        else
            return DVector2.Zero;
    }

    public void Normalize()
    {
        double num = DVector2.Magnitude(this);
        if (num > 9.99999974737875E-06)
            this = this / num;
        else
            this = DVector2.Zero;
    }

    public override string ToString()
    {
        return string.Format("({0:F1}, {1:F1})", X, Y);
    }

    public string ToString(string format)
    {
        return string.Format("({0}, {1})", X.ToString(format), Y.ToString(format));
    }

    public static double Dot(DVector2 lhs, DVector2 rhs)
    {
        return (lhs.X * rhs.X + lhs.Y * rhs.Y);
    }

    public static double Angle(DVector2 from, DVector2 to)
    {
        return Math.Acos(Mathd.Clamp(Dot(from.GetNormalized, to.GetNormalized), -1d, 1d)) * 57.29578d;
    }

    public static double Distance(DVector2 a, DVector2 b)
    {
        DVector2 dVector3 = new DVector2(a.X - b.X, a.Y - b.Y);
        return Math.Sqrt((dVector3.X * dVector3.X + dVector3.Y * dVector3.Y));
    }

    public static DVector2 ClampMagnitude(DVector2 vector3, double maxLength)
    {
        if (vector3.GetSqrMagnitude > maxLength * maxLength)
            return vector3.GetNormalized * maxLength;
        else
            return vector3;
    }

    public static double Magnitude(DVector2 a)
    {
        return Math.Sqrt((a.X * a.X + a.Y * a.Y));
    }

    public static double SqrMagnitude(DVector2 a)
    {
        return (a.X * a.X + a.Y * a.Y);
    }

    public static DVector2 Min(DVector2 lhs, DVector2 rhs)
    {
        return new DVector2(Mathd.Min(lhs.X, rhs.X), Mathd.Min(lhs.Y, rhs.Y));
    }

    public static DVector2 Max(DVector2 lhs, DVector2 rhs)
    {
        return new DVector2(Mathd.Max(lhs.X, rhs.X), Mathd.Max(lhs.Y, rhs.Y));
    }

    public static Vector2 ToVector2(DVector2 vector)
    {
        return ToVector2(vector.X, vector.Y);
    }

    public static Vector2 ToVector2(double x, double y)
    {
        return new Vector2((float)x, (float)y);
    }
}