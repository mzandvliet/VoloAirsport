using System;

namespace RamjetAnvil.Coroutine.Time {

    public static class TimeSpanExtensions {

        public static TimeSpan Days(this int days) {
            return TimeSpan.FromDays(days);
        }

        public static TimeSpan Hours(this int hours) {
            return TimeSpan.FromHours(hours);
        }

        public static TimeSpan Minutes(this int minutes) {
            return TimeSpan.FromHours(minutes);
        }

        public static TimeSpan Seconds(this int seconds) {
            return TimeSpan.FromSeconds(seconds);
        }

        public static TimeSpan MilliSeconds(this int milliseconds) {
            return TimeSpan.FromMilliseconds(milliseconds);
        }

        public static TimeSpan Ticks(this int ticks) {
            return TimeSpan.FromTicks(ticks);
        }

        public static TimeSpan Days(this long days) {
            return TimeSpan.FromDays(days);
        }

        public static TimeSpan Hours(this long hours) {
            return TimeSpan.FromHours(hours);
        }

        public static TimeSpan Minutes(this long minutes) {
            return TimeSpan.FromHours(minutes);
        }

        public static TimeSpan Seconds(this long seconds) {
            return TimeSpan.FromSeconds(seconds);
        }

        public static TimeSpan MilliSeconds(this long milliseconds) {
            return TimeSpan.FromMilliseconds(milliseconds);
        }

        public static TimeSpan Ticks(this long ticks) {
            return TimeSpan.FromTicks(ticks);
        }

        public static TimeSpan Days(this float days) {
            return TimeSpan.FromDays(days);
        }

        public static TimeSpan Hours(this float hours) {
            return TimeSpan.FromHours(hours);
        }

        public static TimeSpan Minutes(this float minutes) {
            return TimeSpan.FromHours(minutes);
        }

        public static TimeSpan Seconds(this float seconds) {
            return TimeSpan.FromSeconds(seconds);
        }

        public static TimeSpan MilliSeconds(this float milliseconds) {
            return TimeSpan.FromMilliseconds(milliseconds);
        }

        public static TimeSpan Days(this double days) {
            return TimeSpan.FromDays(days);
        }

        public static TimeSpan Hours(this double hours) {
            return TimeSpan.FromHours(hours);
        }

        public static TimeSpan Minutes(this double minutes) {
            return TimeSpan.FromHours(minutes);
        }

        public static TimeSpan Seconds(this double seconds) {
            return TimeSpan.FromSeconds(seconds);
        }

        public static TimeSpan MilliSeconds(this double milliseconds) {
            return TimeSpan.FromMilliseconds(milliseconds);
        }
    }
}
