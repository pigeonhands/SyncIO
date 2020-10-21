namespace FileManagerServerPlugin.Extensions
{
    using System;

    static class Format
    {
        enum Unit { Bytes, KB, MB, GB, TB, PB, EB, ZB, YB, ER }
        public static string ToBytes(this long bytes)
        {
            return ToBytes((double)bytes);
        }
        public static string ToBytes(this double bytes)
        {
            int unit = 0;
            while (bytes > 1024)
            {
                bytes /= 1024;
                ++unit;
            }

            return string.Format("{0} {1}", bytes.ToString("F"), ((Unit)unit).ToString());
        }
        public static string ToKB(this double bytes)
        {
            return string.Format("{0:#,##0} KB", bytes / 1024);
        }
        public static string ToTime(this TimeSpan ts)
        {
            string date = null;
            if (ts.Hours > 0)
                date += ts.Hours + " hours, ";
            if (ts.Minutes > 0)
                date += ts.Minutes + " minutes, ";
            return date += " and " + ts.Seconds + " seconds";
        }
        public static string ToSystemTime(this long milliseconds)
        {
            TimeSpan ts = TimeSpan.FromMilliseconds(milliseconds);
            return (ts.Days > 0 ? ts.Days + "d:" : "") +
                (ts.Hours > 0 ? ts.Seconds == 0 ? ts.Minutes == 0 ? ts.Hours + "h" : ts.Hours + "h:" : ts.Hours + "h:" : "") +
                (ts.Minutes > 0 ? ts.Seconds == 0 ? ts.Minutes + "m" : ts.Minutes + "m:" : "") +
                (ts.Seconds > 0 ? ts.Seconds + "s" : "");
        }
        public static string ToNumeric(this object value)
        {
            return string.Format("{0:N0}", value);
        }
        public static string ToMegaHertz(this object value)
        {
            return value + " MHz";
        }
        public static string ToHex(this object value)
        {
            return string.Format("{0:X2}", value);
        }
        public static string ToPercentage(this object value)
        {
            return value + "%";
        }
        public static string ToBoolean(this object value)
        {
            return Convert.ToBoolean(value) ? "Yes" : "No";
        }
        public static string ToSeconds(this double secs)
        {
            TimeSpan ts = TimeSpan.FromSeconds(secs);
            return string.Format("{0:00}:{1:00}:{2:00}", Math.Floor(ts.TotalHours), ts.Minutes, ts.Seconds);
        }
        public static string ToDateTime(this DateTime dateTime)
        {
            return dateTime.ToString("MM/dd/yyyy hh:mm:ss tt");
        }
        public static string ToUnscore(this object value)
        {
            return value.ToString().Replace("_", " ");
        }
        public static string ToProcessorManufacturer(string value)
        {
            switch (value)
            {
                case "GenuineIntel":
                    return "Intel Corporation";
                case "AuthenticAMD":
                    return "Advanced Micro Devices";
                default:
                    return value;
            }
        }
        public static string ToVoltageCaps(uint value)
        {
            switch (value)
            {
                case 0x1:
                    return "5 Volts";
                case 0x2:
                    return "3.3 Volts";
                case 0x4:
                    return "2.9 Volts";
            }

            return "--";
        }
    }
}