using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace RamjetAnvil.Unity.Utility
{
    public class EqualityOperatorComparer<T> : IEqualityComparer<T> where T:class
    {
        private static readonly Lazy<IEqualityComparer<T>> instance = new Lazy<IEqualityComparer<T>>(() => new EqualityOperatorComparer<T>());

        public static IEqualityComparer<T> Instance { get { return instance.Value; } } 

        public bool Equals(T x, T y)
        {
            return x == y;
        }

        public int GetHashCode(T obj)
        {
            return obj.GetHashCode();
        }
    }

    public class IntComparer : IEqualityComparer<int>
    {
        public static readonly IEqualityComparer<int> Instance = new IntComparer();

        private IntComparer()
        {
        }

        public bool Equals(int x, int y)
        {
            return x == y;
        }

        public int GetHashCode(int obj)
        {
            return obj;
        }
    }

    public class Vector3Comparer : IEqualityComparer<Vector3>
    {
        public static readonly IEqualityComparer<Vector3> Instance = new Vector3Comparer();

        private Vector3Comparer() { }

        public bool Equals(Vector3 subject, Vector3 other)
        {
            return subject.x.Equals(other.x) &&
                subject.y.Equals(other.y) &&
                subject.z.Equals(other.z);
        }

        public int GetHashCode(Vector3 obj)
        {
            return obj.GetHashCode();
        }
    }
}
