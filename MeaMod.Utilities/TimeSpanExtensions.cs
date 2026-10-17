
using System;

namespace MeaMod.Utilities
{
    /// <summary>
    /// Description attribute extension for TimeSpan
    /// <para><see href="https://stackoverflow.com/a/4423615"/></para>
	/// <para>Licence: CC BY-SA 3.0</para>
    /// </summary>
    /// 
    public static class TimeSpanExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="span"></param>
        /// <returns></returns>
        public static string ToReadableAgeString(this TimeSpan span)
        {
            return string.Format("{0:0}", span.Days / 365.25);
        }

        /// <summary>
        /// TimeSpan to String with format of 1 day, 6 hours, 12 minutes, 23 seconds only showing elements if greater than 0<br />
        /// </summary>
        /// <param name="span">TimeSpan Source</param>
        /// <returns>String</returns>
        public static string ToReadableString(this TimeSpan span)
        {
            string formatted = string.Format("{0}{1}{2}{3}",
                span.Duration().Days > 0 ? string.Format("{0:0} day{1}, ", span.Days, span.Days == 1 ? string.Empty : "s") : string.Empty,
                span.Duration().Hours > 0 ? string.Format("{0:0} hour{1}, ", span.Hours, span.Hours == 1 ? string.Empty : "s") : string.Empty,
                span.Duration().Minutes > 0 ? string.Format("{0:0} minute{1}, ", span.Minutes, span.Minutes == 1 ? string.Empty : "s") : string.Empty,
                span.Duration().Seconds > 0 ? string.Format("{0:0} second{1}", span.Seconds, span.Seconds == 1 ? string.Empty : "s") : string.Empty);

            if (formatted.EndsWith(", ")) formatted = formatted.Substring(0, formatted.Length - 2);

            if (string.IsNullOrEmpty(formatted)) formatted = "0 seconds";

            return formatted;
        }

        /// <summary>
        /// TimeSpan to String with format of 00:00:00:00 only showing elements if greater than 0<br />
        /// i.e. 0 days, 0 hours, 34 minutes, 12 seconds will show as 34:12<br />
        /// i.e. 0 days, 1 hours, 34 minutes, 12 seconds will show as 01:34:12<br />
        /// i.e. 0 days, 0 hours, 0 minutes, 12 seconds will show as 12<br />
        /// i.e. 0 days, 0 hours, 0 minutes, 02 seconds will show as 02<br />
        /// i.e. 0 days, 0 hours, 0 minutes, 0 seconds will show as 00
        /// </summary>
        /// <param name="span">TimeSpan Source</param>
        /// <returns>String</returns>
        public static string ToReadableShortString(this TimeSpan span)
        {
            string formatted = string.Format("{0}{1}{2}{3}",
                span.Duration().Days > 0 ? string.Format("{0:D2}:", span.Days) : string.Empty,
                span.Duration().Hours > 0 ? string.Format("{0:D2}:", span.Hours) : string.Empty,
                span.Duration().Minutes > 0 ? string.Format("{0:D2}:", span.Minutes) : string.Empty,
                span.Duration().Seconds > 0 ? string.Format("{0:D2}", span.Seconds) : string.Empty);

            if (formatted.EndsWith(":")) formatted = formatted.Substring(0, formatted.Length - 1);

            if (string.IsNullOrEmpty(formatted)) formatted = "00";

            return formatted;
        }

        /// <summary>
        /// TimeSpan to String with format of 00:00:00:00 only showing the days and hours element if greater then 0, minute and seconds will always show<br />
        /// i.e. 0 days, 0 hours, 34 minutes, 12 seconds will show as 34:12<br />
        /// i.e. 0 days, 1 hours, 34 minutes, 12 seconds will show as 01:34:12<br />
        /// i.e. 0 days, 0 hours, 0 minutes, 12 seconds will show as 00:12<br />
        /// i.e. 0 days, 0 hours, 0 minutes, 02 seconds will show as 00:02
        /// </summary>
        /// <param name="span">TimeSpan Source</param>
        /// <returns>String</returns>
        public static string ToReadableShortStringMS(this TimeSpan span)
        {
            string formatted = string.Format("{0}{1}{2}{3}",
                span.Duration().Days > 0 ? string.Format("{0:D2}:", span.Days) : string.Empty,
                span.Duration().Hours > 0 ? string.Format("{0:D2}:", span.Hours) : string.Empty,
                string.Format("{0:D2}:", span.Minutes),
                string.Format("{0:D2}", span.Seconds));

            if (formatted.EndsWith(":")) formatted = formatted.Substring(0, formatted.Length - 1);

            if (string.IsNullOrEmpty(formatted)) formatted = "00:00";

            return formatted;
        }

        /// <summary>
        /// TimeSpan to String with format of days and hours without minutes only showing the days and hours element if greater then 0, minute and seconds will only show if days and hours is lower than 0<br />
        /// i.e. 0 days, 0 hours, 34 minutes, 12 seconds will show as 34:12<br />
        /// i.e. 0 days, 1 hours, 34 minutes, 12 seconds will show as 1 hour<br />
        /// i.e. 2 days, 1 hours, 34 minutes, 12 seconds will show as 2 days<br />  
        /// i.e. 0 days, 0 hours, 0 minutes, 12 seconds will show as 00:12<br />
        /// i.e. 0 days, 0 hours, 0 minutes, 02 seconds will show as 00:02
        /// </summary>
        /// <param name="span">TimeSpan Source</param>
        /// <returns>String</returns>
        public static string ToReadableStringDaysHoursMS(this TimeSpan span)
        {

            string formatted = span switch
            {
                TimeSpan { TotalDays: >= 1 } => string.Format("{0:0} day{1}", span.Days, span.Days == 1 ? string.Empty : "s"),
                TimeSpan { TotalHours: >= 1 } => string.Format("{0:0} hour{1}", span.Hours, span.Hours == 1 ? string.Empty : "s"),
                _ => string.Format("{0}{1}",
                    string.Format("{0:D2}:", span.Minutes),
                    string.Format("{0:D2}", span.Seconds))
            };


            if (formatted.EndsWith(":")) formatted = formatted.Substring(0, formatted.Length - 1);

            if (string.IsNullOrEmpty(formatted)) formatted = "00:00";

            return formatted;
        }

        /// <summary>
        /// TimeSpan to String with format of 00:00:00:00 only showing the days element if greater then 0, hours, minutes and seconds will always show<br />
        /// i.e. 0 days, 0 hours, 34 minutes, 12 seconds will show as 00:34:12<br />
        /// i.e. 0 days, 1 hours, 34 minutes, 12 seconds will show as 01:34:12<br />
        /// i.e. 0 days, 0 hours, 0 minutes, 12 seconds will show as 00:00:12<br />
        /// i.e. 0 days, 0 hours, 0 minutes, 02 seconds will show as 00:00:02
        /// </summary>
        /// <param name="span">TimeSpan Source</param>
        /// <returns>String</returns>
        public static string ToReadableShortStringHMS(this TimeSpan span)
        {
            string formatted = string.Format("{0}{1}{2}{3}",
                span.Duration().Days > 0 ? string.Format("{0:D2}:", span.Days) : string.Empty,
                string.Format("{0:D2}:", span.Hours),
                string.Format("{0:D2}:", span.Minutes),
                string.Format("{0:D2}", span.Seconds));

            if (formatted.EndsWith(":")) formatted = formatted.Substring(0, formatted.Length - 1);

            if (string.IsNullOrEmpty(formatted)) formatted = "00:00:00";

            return formatted;
        }

        /// <summary>
        /// TimeSpan to String with format of 00:00:00:00 days, hours, minutes and seconds will always show<br />
        /// i.e. 1 days, 0 hours, 34 minutes, 12 seconds will show as 1:00:34:12<br />
        /// i.e. 0 days, 1 hours, 34 minutes, 12 seconds will show as 0:01:34:12<br />
        /// i.e. 0 days, 0 hours, 0 minutes, 12 seconds will show as 0:00:00:12<br />
        /// i.e. 0 days, 0 hours, 0 minutes, 02 seconds will show as 0:00:00:02
        /// </summary>
        /// <param name="span">TimeSpan Source</param>
        /// <returns>String</returns>
        public static string ToReadableShortStringDHMS(this TimeSpan span)
        {
            string formatted = string.Format("{0}{1}{2}{3}",
                string.Format("{0:0}:", span.Days),
                string.Format("{0:D2}:", span.Hours),
                string.Format("{0:D2}:", span.Minutes),
                string.Format("{0:D2}", span.Seconds));

            if (formatted.EndsWith(":")) formatted = formatted.Substring(0, formatted.Length - 1);

            if (string.IsNullOrEmpty(formatted)) formatted = "0:00:00:00";

            return formatted;
        }


        /// <summary>
        /// TimeSpan to String using the 1 day ago or 12 hours ago or 15 minutes ago style
        /// <para><see href="https://gist.github.com/Rychu-Pawel/fefb89e21b764e97e4993ff517ff0129"/></para>
        /// </summary>
        /// <param name="span">TimeSpan Source</param>
        /// <param name="justNowSecondsThreshold">Just Now Seconds Threshold</param>
        /// <returns></returns>
        public static string ToReadableAgoString(this TimeSpan span, double justNowSecondsThreshold)
        {
            if (span.TotalSeconds < justNowSecondsThreshold)
                return "just now";

            return span switch
            {
                TimeSpan { TotalDays: >= 1 } => string.Format("{0:0} day{1} ago", span.Days, span.Days == 1 ? string.Empty : "s"),
                TimeSpan { TotalHours: >= 1 } => string.Format("{0:0} hour{1} ago", span.Hours, span.Hours == 1 ? string.Empty : "s"),
                TimeSpan { TotalMinutes: >= 1 } => string.Format("{0:0} minute{1} ago", span.Minutes, span.Minutes == 1 ? string.Empty : "s"),
                _ => string.Format("{0:0} second{1} ago", span.Seconds, span.Seconds == 1 ? string.Empty : "s")
            };
        }
    }
}
