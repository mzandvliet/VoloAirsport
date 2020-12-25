using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RamjetAnvil.Unity.Utility
{
    public struct Tuple<T1, T2>
    {
        private readonly T1 _first;
        private readonly T2 _second;

        public Tuple(T1 first, T2 second)
        {
            _first = first;
            _second = second;
        }

        public T1 _1 {
            get { return _first; }
        }

        public T2 _2 {
            get { return _second; }
        }

        public bool Equals(Tuple<T1, T2> other) {
            return EqualityComparer<T1>.Default.Equals(_first, other._first) && EqualityComparer<T2>.Default.Equals(_second, other._second);
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) return false;
            return obj is Tuple<T1, T2> && Equals((Tuple<T1, T2>) obj);
        }

        public override int GetHashCode() {
            unchecked {
                return (EqualityComparer<T1>.Default.GetHashCode(_first)*397) ^ EqualityComparer<T2>.Default.GetHashCode(_second);
            }
        }
    }

    public struct Tuple<T1, T2, T3>
    {
        private readonly T1 _first;
        private readonly T2 _second;
        private readonly T3 _third;

        public Tuple(T1 first, T2 second, T3 third)
        {
            _first = first;
            _second = second;
            _third = third;
        }

        public T1 _1
        {
            get { return _first; }
        }

        public T2 _2
        {
            get { return _second; }
        }

        public T3 _3 {
            get { return _third; }
        }

        public bool Equals(Tuple<T1, T2, T3> other) {
            return EqualityComparer<T1>.Default.Equals(_first, other._first) && EqualityComparer<T2>.Default.Equals(_second, other._second) && EqualityComparer<T3>.Default.Equals(_third, other._third);
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) return false;
            return obj is Tuple<T1, T2, T3> && Equals((Tuple<T1, T2, T3>) obj);
        }

        public override int GetHashCode() {
            unchecked {
                int hashCode = EqualityComparer<T1>.Default.GetHashCode(_first);
                hashCode = (hashCode*397) ^ EqualityComparer<T2>.Default.GetHashCode(_second);
                hashCode = (hashCode*397) ^ EqualityComparer<T3>.Default.GetHashCode(_third);
                return hashCode;
            }
        }
    }

    public struct Tuple<T1, T2, T3, T4> {
        private readonly T1 _first;
        private readonly T2 _second;
        private readonly T3 _third;
        private readonly T4 _fourth;

        public Tuple(T1 first, T2 second, T3 third, T4 fourth) {
            _first = first;
            _second = second;
            _third = third;
            _fourth = fourth;
        }

        public T1 _1 {
            get { return _first; }
        }

        public T2 _2 {
            get { return _second; }
        }

        public T3 _3 {
            get { return _third; }
        }

        public T4 _4 {
            get { return _fourth; }
        }

        public bool Equals(Tuple<T1, T2, T3, T4> other) {
            return EqualityComparer<T1>.Default.Equals(_first, other._first) && EqualityComparer<T2>.Default.Equals(_second, other._second) && EqualityComparer<T3>.Default.Equals(_third, other._third) && EqualityComparer<T4>.Default.Equals(_fourth, other._fourth);
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) return false;
            return obj is Tuple<T1, T2, T3, T4> && Equals((Tuple<T1, T2, T3, T4>) obj);
        }

        public override int GetHashCode() {
            unchecked {
                int hashCode = EqualityComparer<T1>.Default.GetHashCode(_first);
                hashCode = (hashCode*397) ^ EqualityComparer<T2>.Default.GetHashCode(_second);
                hashCode = (hashCode*397) ^ EqualityComparer<T3>.Default.GetHashCode(_third);
                hashCode = (hashCode*397) ^ EqualityComparer<T4>.Default.GetHashCode(_fourth);
                return hashCode;
            }
        }
    }

    public struct Tuple<T1, T2, T3, T4, T5> {
        private readonly T1 _first;
        private readonly T2 _second;
        private readonly T3 _third;
        private readonly T4 _fourth;
        private readonly T5 _fifth;

        public Tuple(T1 first, T2 second, T3 third, T4 fourth, T5 fifth) {
            _first = first;
            _second = second;
            _third = third;
            _fourth = fourth;
            _fifth = fifth;
        }

        public T1 _1 {
            get { return _first; }
        }

        public T2 _2 {
            get { return _second; }
        }

        public T3 _3 {
            get { return _third; }
        }

        public T4 _4 {
            get { return _fourth; }
        }

        public T5 _5 {
            get { return _fifth; }
        }

        public bool Equals(Tuple<T1, T2, T3, T4, T5> other) {
            return EqualityComparer<T1>.Default.Equals(_first, other._first) && EqualityComparer<T2>.Default.Equals(_second, other._second) && EqualityComparer<T4>.Default.Equals(_fourth, other._fourth) && EqualityComparer<T5>.Default.Equals(_fifth, other._fifth) && EqualityComparer<T3>.Default.Equals(_third, other._third);
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) return false;
            return obj is Tuple<T1, T2, T3, T4, T5> && Equals((Tuple<T1, T2, T3, T4, T5>) obj);
        }

        public override int GetHashCode() {
            unchecked {
                int hashCode = EqualityComparer<T1>.Default.GetHashCode(_first);
                hashCode = (hashCode*397) ^ EqualityComparer<T2>.Default.GetHashCode(_second);
                hashCode = (hashCode*397) ^ EqualityComparer<T4>.Default.GetHashCode(_fourth);
                hashCode = (hashCode*397) ^ EqualityComparer<T5>.Default.GetHashCode(_fifth);
                hashCode = (hashCode*397) ^ EqualityComparer<T3>.Default.GetHashCode(_third);
                return hashCode;
            }
        }
    }

}
