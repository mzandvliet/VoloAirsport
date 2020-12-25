﻿using System;
using Lidgren.Network;
﻿using RamjetAnvil.Unity.Utility;
﻿using UnityEngine;

namespace RamjetAnvil.Networking {

    public static class Compression {

        public struct CompressedRotation {
            public uint A;
            public uint B;
            public uint C;
            public ComponentType LargestComponentType;
            public int ComponentPrecisionInBits;

            public CompressedRotation(ComponentType largestComponentType, uint a, uint b, uint c, int componentPrecisionInBits) {
                LargestComponentType = largestComponentType;
                A = a;
                B = b;
                C = c;
                ComponentPrecisionInBits = componentPrecisionInBits;
            }
        }

        public enum ComponentType : uint {
            X = 0,
            Y = 1,
            Z = 2,
            W = 3
        }

        const float Minimum = -1.0f / 1.414214f; // note: 1.0f / sqrt(2)
        const float Maximum = +1.0f / 1.414214f;
        const int LargestComponentIndicatorBitSize = 2;

        /// <summary>
        /// See: https://gist.github.com/gafferongames/bb7e593ba1b05da35ab6
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="rotation"></param>
        /// <param name="componentPrecisionInBits">A value from 2 to 10 indicating the precision of the rotation
        /// 10 bits = 3 components * 10 bits + 2 bits (largest component indicator = 32 bits)
        /// </param>
        public static CompressedRotation CompressRotation(Quaternion rotation, int componentPrecisionInBits = 10) {
            var largestComponentType = ComponentType.X;
            {
                float absX = Math.Abs(rotation.x);
                float absY = Math.Abs(rotation.y);
                float absZ = Math.Abs(rotation.z);
                float absW = Math.Abs(rotation.w);

                float largestComponentValue = absX;
                if (absY > largestComponentValue) {
                    largestComponentValue = absY;
                    largestComponentType = ComponentType.Y;
                }
                if (absZ > largestComponentValue) {
                    largestComponentValue = absZ;
                    largestComponentType = ComponentType.Z;
                }
                if (absW > largestComponentValue) {
                    largestComponentType = ComponentType.W;
                }
            }

            float a, b, c;
            switch (largestComponentType) {
                case ComponentType.X:
                    if (rotation.x >= 0) {
                        a = rotation.y;
                        b = rotation.z;
                        c = rotation.w;
                    } else {
                        a = -rotation.y;
                        b = -rotation.z;
                        c = -rotation.w;
                    }
                    break;
                case ComponentType.Y:
                    if (rotation.y >= 0) {
                        a = rotation.x;
                        b = rotation.z;
                        c = rotation.w;
                    } else {
                        a = -rotation.x;
                        b = -rotation.z;
                        c = -rotation.w;
                    }
                    break;
                case ComponentType.Z:
                    if (rotation.z >= 0) {
                        a = rotation.x;
                        b = rotation.y;
                        c = rotation.w;
                    } else {
                        a = -rotation.x;
                        b = -rotation.y;
                        c = -rotation.w;
                    }
                    break;
                case ComponentType.W:
                    if (rotation.w >= 0) {
                        a = rotation.x;
                        b = rotation.y;
                        c = rotation.z;
                    } else {
                        a = -rotation.x;
                        b = -rotation.y;
                        c = -rotation.z;
                    }
                    break;
                default:
                    // Should never happen!
                    throw new ArgumentOutOfRangeException("Unknown rotation component type: " +
                                                            largestComponentType);
            }

            float normalizedA = (a - Minimum) / (Maximum - Minimum);
            float normalizedB = (b - Minimum) / (Maximum - Minimum);
            float normalizedC = (c - Minimum) / (Maximum - Minimum);

            float scale = (1 << componentPrecisionInBits) - 1;
            return new CompressedRotation(
                largestComponentType: largestComponentType,
                a: (uint) Math.Floor(normalizedA * scale + 0.5f),
                b: (uint) Math.Floor(normalizedB * scale + 0.5f),
                c: (uint) Math.Floor(normalizedC * scale + 0.5f),
                componentPrecisionInBits: componentPrecisionInBits);
        }

        public static Quaternion ToQuaternion(this CompressedRotation compressedRotation) {
            float scale = (1 << compressedRotation.ComponentPrecisionInBits) - 1;
            float inverseScale = 1.0f / scale;

            float a = compressedRotation.A * inverseScale * (Maximum - Minimum) + Minimum;
            float b = compressedRotation.B * inverseScale * (Maximum - Minimum) + Minimum;
            float c = compressedRotation.C * inverseScale * (Maximum - Minimum) + Minimum;

            Quaternion rotation;
            switch (compressedRotation.LargestComponentType) {
                case ComponentType.X:
                    // (?) y z w
                    rotation.x = Mathf.Sqrt(1 - a * a
                                              - b * b
                                              - c * c);
                    rotation.y = a;
                    rotation.z = b;
                    rotation.w = c;
                    break;
                case ComponentType.Y:
                    // x (?) z w
                    rotation.x = a;
                    rotation.y = Mathf.Sqrt(1 - a * a
                                              - b * b
                                              - c * c);
                    rotation.z = b;
                    rotation.w = c;
                    break;
                case ComponentType.Z:
                    // x y (?) w
                    rotation.x = a;
                    rotation.y = b;
                    rotation.z = Mathf.Sqrt(1 - a * a
                                              - b * b
                                              - c * c);
                    rotation.w = c;
                    break;
                case ComponentType.W:
                    // x y z (?)
                    rotation.x = a;
                    rotation.y = b;
                    rotation.z = c;
                    rotation.w = Mathf.Sqrt(1 - a * a
                                              - b * b
                                              - c * c);
                    break;
                default:
                    // Should never happen!
                    throw new ArgumentOutOfRangeException("Unknown rotation component type: " +
                                                            compressedRotation.LargestComponentType);
            }

            return rotation;
        }

        public static void WriteRotation(NetBuffer buffer, CompressedRotation rotation) {
            buffer.Write((uint) rotation.LargestComponentType, LargestComponentIndicatorBitSize);
            buffer.Write(rotation.A, rotation.ComponentPrecisionInBits);
            buffer.Write(rotation.B, rotation.ComponentPrecisionInBits);
            buffer.Write(rotation.C, rotation.ComponentPrecisionInBits);
        }

        public static CompressedRotation ReadRotation(NetBuffer buffer, int componentPrecisionInBits = 10) {
            var largestComponentType = (ComponentType) buffer.ReadUInt32(LargestComponentIndicatorBitSize);
            uint integerA = buffer.ReadUInt32(componentPrecisionInBits);
            uint integerB = buffer.ReadUInt32(componentPrecisionInBits);
            uint integerC = buffer.ReadUInt32(componentPrecisionInBits);
            return new CompressedRotation(largestComponentType, integerA, integerB, integerC, componentPrecisionInBits);
        }

        private static readonly int[] PositionRangeBits = {5, 6, 7};
        private const int PositionSmallLimit = 15;
        private const int PositionMaxDelta = 2047;
        private static readonly int PositionLargeLimit = UnsignedRangeLimit(PositionRangeBits);
        /// <summary>
        /// Writes a compresses position with a range of -1023,1023 per component
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="position"></param>
        public static void WriteSmallPosition(this NetBuffer buffer, IntVector3 position) {
            uint deltaX = SignedToUnsigned(position.X);
            uint deltaY = SignedToUnsigned(position.Y);
            uint deltaZ = SignedToUnsigned(position.Z);

            var isAllSmall = deltaX <= PositionSmallLimit && deltaY <= PositionSmallLimit && deltaZ <= PositionSmallLimit;
            var isTooLarge = deltaX >= PositionLargeLimit || deltaY >= PositionLargeLimit || deltaZ >= PositionLargeLimit;

            buffer.Write(isAllSmall);
            if (isAllSmall) {
                buffer.WriteUInt32(0, PositionSmallLimit, deltaX);
                buffer.WriteUInt32(0, PositionSmallLimit, deltaY);
                buffer.WriteUInt32(0, PositionSmallLimit, deltaZ);
            } else {
                buffer.Write(isTooLarge);

                if (isTooLarge) {
                    buffer.WriteUInt32(0, PositionMaxDelta, deltaX);
                    buffer.WriteUInt32(0, PositionMaxDelta, deltaY);
                    buffer.WriteUInt32(0, PositionMaxDelta, deltaZ);
                } else {
                    buffer.WriteUInt32Range(PositionRangeBits, deltaX);
                    buffer.WriteUInt32Range(PositionRangeBits, deltaY);
                    buffer.WriteUInt32Range(PositionRangeBits, deltaZ);
                }
            }
        }

        /// <summary>
        /// Reads a small compressed position with a range of -1023,1023 per component.
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public static IntVector3 ReadSmallPosition(this NetBuffer buffer) {
            var isAllSmall = buffer.ReadBoolean();
            uint positionX;
            uint positionY;
            uint positionZ;
            if (isAllSmall) {
                positionX = buffer.ReadUInt32(0, PositionSmallLimit);
                positionY = buffer.ReadUInt32(0, PositionSmallLimit);
                positionZ = buffer.ReadUInt32(0, PositionSmallLimit);
            } else {
                var isTooLarge = buffer.ReadBoolean();
                if (isTooLarge) {
                    positionX = buffer.ReadUInt32(0, PositionMaxDelta);
                    positionY = buffer.ReadUInt32(0, PositionMaxDelta);
                    positionZ = buffer.ReadUInt32(0, PositionMaxDelta);
                } else {
                    positionX = buffer.ReadUInt32Range(PositionRangeBits);
                    positionY = buffer.ReadUInt32Range(PositionRangeBits);
                    positionZ = buffer.ReadUInt32Range(PositionRangeBits);
                }
            }

            return new IntVector3(
                x: UnsignedToSigned(positionX),
                y: UnsignedToSigned(positionY),
                z: UnsignedToSigned(positionZ));
        }

        // TODO Add support for serializing small rotations

        public static void WriteInt32(this NetBuffer buffer, int min, int max, int value) {
            var bits = BitsRequired(min, max);
            var unsignedValue = (uint)(value - min);
            buffer.Write(unsignedValue, bits);
        }

        public static int ReadInt32(this NetBuffer buffer, int min, int max) {
            var bits = BitsRequired(min, max);
            var unsignedValue = buffer.ReadUInt32(bits);
            return (int)unsignedValue + min;
        }

        public static void WriteUInt32(this NetBuffer buffer, int min, int max, uint value) {
            var bits = BitsRequired(min, max);
            var unsignedValue = (uint)(value - min);
            buffer.Write(unsignedValue, bits);
        }

        public static uint ReadUInt32(this NetBuffer buffer, int min, int max) {
            var bits = BitsRequired(min, max);
            var unsignedValue = buffer.ReadUInt32(bits);
            return (uint) (unsignedValue + min);
        }

        private static void WriteUInt32Range(this NetBuffer buffer, int[] rangeBits, uint value) {
            int rangeMin = 0;
            for (int i = 0; i < rangeBits.Length; i++) {
                var rangeBit = rangeBits[i];
                int rangeMax = rangeMin + ((1 << rangeBit) - 1);
                var isInRange = value <= rangeMax;
                buffer.Write(isInRange);
                if (isInRange) {
                    buffer.WriteUInt32(rangeMin, rangeMax, value);
                    return;
                }
                rangeMin += (1 << rangeBit);
            }
            buffer.WriteUInt32(
                min: rangeMin,
                max: rangeMin + ((1 << rangeBits[rangeBits.Length - 1]) - 1),
                value: value);
        }

        private static uint ReadUInt32Range(this NetBuffer buffer, int[] rangeBits) {
            int rangeMin = 0;
            for (int i = 0; i < rangeBits.Length; i++) {
                var rangeBit = rangeBits[i];
                int rangeMax = rangeMin + ((1 << rangeBit) - 1);
                var isInRange = buffer.ReadBoolean();
                if (isInRange) {
                    return buffer.ReadUInt32(rangeMin, rangeMax);
                }
                rangeMin += (1 << rangeBit);
            }
            return buffer.ReadUInt32(
                min: rangeMin,
                max: rangeMin + ((1 << rangeBits[rangeBits.Length - 1]) - 1));
        }

        private static int BitsRequired(long min, long max) {
            return (min == max) ? 0 : (int)(Log2((uint) (max - min)) + 1);
        }

        private static int UnsignedRangeLimit(int[] rangeBits) {
            int rangeLimit = 0;
            for (int i = 0; i < rangeBits.Length; i++) {
                rangeLimit += (1 << rangeBits[i]);
            }
            return rangeLimit;
        }

        private static uint Log2(uint x) {
            uint a = x | ( x >> 1 );
            uint b = a | ( a >> 2 );
            uint c = b | ( b >> 4 );
            uint d = c | ( c >> 8 );
            uint e = d | ( d >> 16 );
            uint f = e >> 1;
            return PopCount(f);
        }

        private static uint Clamp(uint min, uint max, uint value) {
            if (value < min) {
                return min;
            }
            if (value > max) {
                return max;
            }
            return value;
        }

        private static uint PopCount(uint x) {
            uint a = x - ( ( x >> 1 )       & 0x55555555 );
            uint b =   ( ( ( a >> 2 )       & 0x33333333 ) + ( a & 0x33333333 ) );
            uint c =   ( ( ( b >> 4 ) + b ) & 0x0f0f0f0f );
            uint d =   c + ( c >> 8 );
            uint e =   d + ( d >> 16 );
            uint result = e & 0x0000003f;
            return result;
        }

        private static uint SignedToUnsigned(int n) {
            return (uint) ((n << 1) ^ (n >> 31));
        }

        private static int UnsignedToSigned(uint n) {
            return (int) ((n >> 1) ^ (-( n & 1 )));
        }
    }
}
