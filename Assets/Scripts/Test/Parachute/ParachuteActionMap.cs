using System;
using RamjetAnvil.Impero.StandardInput;
using RamjetAnvil.Util;
using RamjetAnvil.Volo.Input;
using UnityEngine;

namespace RamjetAnvil.Volo {
    
    public interface IParachuteActionMap {
        ButtonEvent ParachuteConfigToggle { get; }
        ParachuteInput Input { get; }
    }

    public enum ParachuteLine {
        Brake, Rear, Front
    }

    public static class ParachuteLineExtensions {

        public static string Name(this ParachuteLine line) {
            switch (line) {
                case ParachuteLine.Brake:
                    return "Brake Lines";
                case ParachuteLine.Front:
                    return "Front Lines";
                case ParachuteLine.Rear:
                    return "Rear Lines";
                default:
                    return "None";
            }
        }

        public static Color Color(this ParachuteLine line) {
            switch (line) {
                case ParachuteLine.Brake:
                    return ColorUtil.FromRgb(124, 174, 255); // Blue
                case ParachuteLine.Rear:
                    return ColorUtil.FromRgb(221, 70, 86); // Red
                case ParachuteLine.Front:
                    return ColorUtil.FromRgb(67, 195, 121); // Green
                default:
                    throw new ArgumentOutOfRangeException("line", line, null);
            }
        }
    }
}