using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RamjetAnvil.Util {

    public static class DateTimeExtensions {

        public static DateTime SetDayOfYear(this DateTime dateTime, int dayOfYear) {
            return dateTime.AddDays(dayOfYear - dateTime.DayOfYear);
        }
    }
}
