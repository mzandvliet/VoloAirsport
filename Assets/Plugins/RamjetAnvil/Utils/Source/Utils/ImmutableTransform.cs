using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace RamjetAnvil.Unity.Utility
{
    public struct ImmutableTransform : IEquatable<ImmutableTransform> {
        public static ImmutableTransform Identity {
            get { return new ImmutableTransform(Vector3.zero, Quaternion.identity, Vector3.one); }
        }

        public Vector3 Position;
        public Quaternion Rotation;
        public Vector3 Scale;

        public ImmutableTransform(Vector3 position, Quaternion rotation) : this() {
            Position = position;
            Rotation = rotation;
            Scale = Vector3.one;
        }

        public ImmutableTransform(Vector3 position, Quaternion rotation, Vector3 scale) {
            Position = position;
            Rotation = rotation;
            Scale = scale;
        }

        public Vector3 Up {
            get { return Rotation * Vector3.up; }
        }

        public Vector3 Down
        {
            get { return Rotation * Vector3.down; }
        }

        public Vector3 Forward
        {
            get { return Rotation * Vector3.forward; }
        }

        public Vector3 Back
        {
            get { return Rotation * Vector3.back; }
        }

        public Vector3 Left
        {
            get { return Rotation * Vector3.left; }
        }

        public Vector3 Right
        {
            get { return Rotation * Vector3.right; }
        }

        public override string ToString() {
            return string.Format("Transform(position: {0}, rotation: {1}, scale: {2})", Position, Rotation.eulerAngles, Scale);
        }

        public bool Equals(ImmutableTransform other) {
            return Position.Equals(other.Position) && Rotation.Equals(other.Rotation) && Scale.Equals(other.Scale);
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) return false;
            return obj is ImmutableTransform && Equals((ImmutableTransform) obj);
        }

        public override int GetHashCode() {
            unchecked {
                int hashCode = Position.GetHashCode();
                hashCode = (hashCode * 397) ^ Rotation.GetHashCode();
                hashCode = (hashCode * 397) ^ Scale.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(ImmutableTransform left, ImmutableTransform right) {
            return left.Equals(right);
        }

        public static bool operator !=(ImmutableTransform left, ImmutableTransform right) {
            return !left.Equals(right);
        }
    }

    public static class ImmutableTransformExtensions {

        public static ImmutableTransform UpdatePosition(this ImmutableTransform t, float? x = null, float? y = null, float? z = null) {
            var position = t.Position;
            position.x = x ?? position.x;
            position.y = y ?? position.y;
            position.z = z ?? position.z;
            t.Position = position;
            return t;
        }

        public static ImmutableTransform UpdatePosition(this ImmutableTransform t, Vector3 position) {
            t.Position = position;
            return t;
        }

        public static ImmutableTransform UpdateRotation(this ImmutableTransform t, float? x = null, float? y = null, float? z = null) {
            var angles = t.Rotation.eulerAngles;
            angles.x = x ?? angles.x;
            angles.y = y ?? angles.y;
            angles.z = z ?? angles.z;
            t.Rotation = Quaternion.Euler(angles);
            return t;
        }

        public static ImmutableTransform UpdateRotation(this ImmutableTransform t, Vector3 eulerAngles) {
            t.Rotation = Quaternion.Euler(eulerAngles);
            return t;
        }

        public static ImmutableTransform UpdateRotation(this ImmutableTransform t, Quaternion rotation) {
            t.Rotation = rotation;
            return t;
        }

        public static ImmutableTransform UpdateScale(this ImmutableTransform t, Vector3 scale) {
            t.Scale = scale;
            return t;
        }

        public static ImmutableTransform Translate(this ImmutableTransform t, float x = 0f, float y = 0f, float z = 0f) {
            return t.Translate(new Vector3(x, y, z), Quaternion.identity);
        }

        public static ImmutableTransform Translate(this ImmutableTransform t, Vector3 translation) {
            return t.Translate(translation, Quaternion.identity);
        }

        public static ImmutableTransform TranslateLocally(this ImmutableTransform t, float x = 0f, float y = 0f, float z = 0f) {
            return t.Translate(new Vector3(x, y, z), t.Rotation);
        }

        public static ImmutableTransform TranslateLocally(this ImmutableTransform t, Vector3 translation) {
            return t.Translate(translation, t.Rotation);
        }

        public static ImmutableTransform Translate(this ImmutableTransform t, Vector3 translation,
            Quaternion rotation) {
            t.Position = t.Position + (rotation * translation);
            return t;
        }

        public static ImmutableTransform Rotate(this ImmutableTransform t, float x = 0f, float y = 0f, float z = 0f) {
            return t.Rotate(Quaternion.Euler(x, y, z));
        }

        public static ImmutableTransform Rotate(this ImmutableTransform t, Vector3 rotation) {
            return t.Rotate(Quaternion.Euler(rotation));
        }

        public static ImmutableTransform Rotate(this ImmutableTransform t, Quaternion rotation) {
            t.Rotation = t.Rotation * rotation;
            return t;
        }

        public static ImmutableTransform RotateWorld(this ImmutableTransform t, Quaternion rotation) {
            t.Rotation = rotation * t.Rotation;
            return t;
        }

        public static ImmutableTransform RotateWorld(this ImmutableTransform t, float x = 0f, float y = 0f, float z = 0f) {
            return t.RotateWorld(Quaternion.Euler(x, y, z));
        }

        public static ImmutableTransform RotateWorld(this ImmutableTransform t, Vector3 rotation) {
            return t.RotateWorld(Quaternion.Euler(rotation));
        }

        public static ImmutableTransform RotateAround(this ImmutableTransform t, Vector3 center, Vector3 axis, float angle) {
            var desiredRotation = Quaternion.AngleAxis(angle, axis); // get the desired rotation

            var directionToCenter = t.Position - center; // find current direction relative to center
            directionToCenter = desiredRotation * directionToCenter; // rotate the direction
            t.Position = center + directionToCenter; // define new position

            // rotate object to keep looking at the center:
            t.Rotation *= Quaternion.Inverse(t.Rotation) * desiredRotation * t.Rotation;

            return t;
        }

        public static ImmutableTransform Scale(this ImmutableTransform t, Vector3 scaleFactor) {
            t.Scale = Vector3.Scale(t.Scale, scaleFactor);
            return t;
        }

        public static ImmutableTransform LookAt(this ImmutableTransform t, Vector3 lookTarget) {
            return t.LookAt(lookTarget, up: Vector3.up);
        }

        public static ImmutableTransform LookAt(this ImmutableTransform t, Vector3 lookTarget, Vector3 up) {
            var relativePosition = lookTarget - t.Position;
            t.Rotation = Quaternion.LookRotation(relativePosition, up);
            return t;
        }

        public static Vector3 InverseTransformDirection(this ImmutableTransform t, Vector3 direction) {
            //            var matrix = Matrix4x4.TRS(t.Position, t.Rotation, t.Scale).inverse;
            //            return matrix.MultiplyVector(direction);

            return Quaternion.Inverse(t.Rotation) * direction;
        }

        public static Vector3 TransformDirection(this ImmutableTransform t, Vector3 direction) {
//            var matrix = Matrix4x4.TRS(t.Position, t.Rotation, t.Scale);
//            return matrix.MultiplyVector(direction);

            return t.Rotation * direction;
        }

        public static Vector3 TransformPoint(this ImmutableTransform t, Vector3 point) {
            var matrix = Matrix4x4.TRS(t.Position, t.Rotation, t.Scale);
            return matrix.MultiplyPoint(point);
        }

        public static Vector3 InverseTransformPoint(this ImmutableTransform t, Vector3 point) {
            var matrix = Matrix4x4.TRS(t.Position, t.Rotation, t.Scale).inverse;
            return matrix.MultiplyPoint(point);
        }

        public static ImmutableTransform MakeImmutable(this Transform t) {
            return new ImmutableTransform(t.position, t.rotation, t.localScale);
        }

        public static ImmutableTransform ImmutableTransform(this Rigidbody r) {
            return new ImmutableTransform(r.position, r.rotation);
        }

        public static ImmutableTransform ToLocalSpace(this ImmutableTransform t, ImmutableTransform local) {
            return t
                .Rotate(local.Rotation)
                .TranslateLocally(local.Position);
        }

        public static ImmutableTransform ToWorldSpace(this ImmutableTransform t, ImmutableTransform local) {
            return t
                .TranslateLocally(-local.Position)
                .Rotate(Quaternion.Inverse(local.Rotation));
        }

        public static GameObject SetTransform(this GameObject go, ImmutableTransform t) {
            if (go != null) {
                go.transform.Set(t);    
            }
            return go;
        }

        public static void Set(this Transform target, ImmutableTransform t) {
            target.position = t.Position;
            target.rotation = t.Rotation;
            target.localScale = t.Scale;
        }

        public static ImmutableTransform MakeLocalImmutable(this Transform t) {
            return new ImmutableTransform(t.localPosition, t.localRotation, t.localScale);
        }

        public static void SetLocal(this Transform target, ImmutableTransform t) {
            target.localPosition = t.Position;
            target.localRotation = t.Rotation;
            target.localScale = t.Scale;
        }
    }
}
