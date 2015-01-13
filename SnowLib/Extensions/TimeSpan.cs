using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;
using System.Text.RegularExpressions;

namespace SnowLib.Extensions
{
    public static class TimeSpanExtension
    {
        public static string GetDescription(this TimeSpan timeSpan)
        {
            return GetDescription(timeSpan, CultureInfo.CurrentCulture);
        }

        public static string GetDescription(this TimeSpan timeSpan, CultureInfo culture)
        {
            StringBuilder sb = new StringBuilder();
            if (timeSpan.Days > 0)
            {
                sb.Append(timeSpan.Days);
                sb.Append(SnowLib.Messages.Get("TimeSpan_Day", culture));
            }
            if (timeSpan.Hours > 0)
            {
                if (sb.Length > 0)
                    sb.Append(' ');
                sb.Append(timeSpan.Hours);
                sb.Append(SnowLib.Messages.Get("TimeSpan_Hour", culture));
            }
            if (timeSpan.Minutes > 0)
            {
                if (sb.Length > 0)
                    sb.Append(' ');
                sb.Append(timeSpan.Minutes);
                sb.Append(SnowLib.Messages.Get("TimeSpan_Minute", culture));
            }
            if (timeSpan.Seconds > 0)
            {
                if (sb.Length > 0)
                    sb.Append(' ');
                sb.Append(timeSpan.Seconds);
                sb.Append(SnowLib.Messages.Get("TimeSpan_Second", culture));
            }
            return sb.ToString();
        }
        
        public static TimeSpan ParseDescription(string description)
        {
            return ParseDescription(description, CultureInfo.CurrentCulture);
        }

        public static TimeSpan ParseDescription(string description, CultureInfo culture)
        {
            Regex regex = getRegex(culture);
            Match mt = regex.Match(description);
            if (!mt.Success)
                throw new FormatException();
            long num = getTotal(mt); 
            if ((num > 0x346dc5d638865L) || (num < -922337203685477L))
                throw new OverflowException();
            return new TimeSpan(num * 0x2710L);
        }

        public static bool TryParseDescription(string description, out TimeSpan timeSpan)
        {
            return TryParseDescription(description, out timeSpan, CultureInfo.CurrentCulture);
        }

        public static bool TryParseDescription(string description, out TimeSpan timeSpan, CultureInfo culture)
        {
            Regex regex = getRegex(culture);
            Match mt = regex.Match(description);
            if (mt.Success)
            {
                long num = getTotal(mt);
                if ((num <= 0x346dc5d638865L) && (num >= -922337203685477L))
                {
                    timeSpan = new TimeSpan(num * 0x2710L);
                    return true;
                }
            }
            timeSpan = TimeSpan.Zero;
            return mt.Success;
        }

        private static long getTotal(Match mt)
        {
            int days, hours, mins, secs;
            int.TryParse(mt.Groups["days"].Value, out days);
            int.TryParse(mt.Groups["hours"].Value, out hours);
            int.TryParse(mt.Groups["mins"].Value, out mins);
            int.TryParse(mt.Groups["secs"].Value, out secs);
            long num = ((((((days * 0xe10L) * 0x18L) + (hours * 0xe10L)) + (mins * 60L)) + secs) * 0x3e8L);
            return mt.Groups["minus"].Success ? -num : num;
        }

        private static Dictionary<CultureInfo, Regex> regexes;

        private static Regex getRegex(CultureInfo culture)
        {
            if (regexes == null)
                regexes = new Dictionary<CultureInfo, Regex>();
            Regex res;
            if (!regexes.TryGetValue(culture, out res))
            {
                res = new Regex(
                    "^" +
                    "\\s*((?'minus')-)?"+
                    "\\s*((?'days'\\d+)\\s*" + SnowLib.Messages.Get("TimeSpan_Day").Replace(".","\\.")+")?" +
                    "\\s*((?'hours'\\d+)\\s*" + SnowLib.Messages.Get("TimeSpan_Hour").Replace(".", "\\.") + ")?" +
                    "\\s*((?'mins'\\d+)\\s*" + SnowLib.Messages.Get("TimeSpan_Minute").Replace(".", "\\.") + ")?" +
                    "\\s*((?'secs'\\d+)\\s*" + SnowLib.Messages.Get("TimeSpan_Second").Replace(".", "\\.") + ")?" +
                    "$");
                regexes.Add(culture, res);
            }
            return res;

        }
    }
}
