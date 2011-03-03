using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using Dicom.Data;

namespace server
{
    public class DateTimeRangeQuery
    {
        public DateTime From { get; set; }
        public DateTime To { get; set; }

        public static DateTimeRangeQuery Parse(string dateString, string timeString = null)
        {
            DateTimeRangeQuery query = null;

            var minValue = new DateTime(1800, 1, 1);
            var maxValue = new DateTime(9999, 1, 1);

            if (dateString.Contains('-') == false)
            {
                var from = ParseDate(dateString, minValue);
                var to = new DateTime(from.Year, from.Month, from.Day, 23, 59, 59, 999);

                query = new DateTimeRangeQuery() { From = from, To = to };
            }
            else
            {
                var rangeValues = dateString.Split(new char[] { '-' }, 2);

                DateTime from = minValue;
                DateTime to = maxValue;

                if (rangeValues.Length == 2)
                {
                    from = ParseDate(rangeValues[0], from);
                    to = ParseDate(rangeValues[1], to);

                    if(to.Hour == 0 && to.Minute == 0 && to.Second == 0)
                        to = new DateTime(to.Year, to.Month, to.Day, 23, 59, 59, 999);
                }

                query = new DateTimeRangeQuery() { From = from, To = to };
            }

            if (String.IsNullOrWhiteSpace(timeString))
                return query;

            if (timeString.Contains('-') == false)
            {
                var from = ParseTime(timeString, TimeSpan.Zero);
                var to = from;

                query.From = query.From.Add(from);
                query.To = query.To.Add(to);
            }
            else
            {
                var rangeValues = timeString.Split(new char[] { '-' }, 2);

                TimeSpan from = TimeSpan.Zero;
                TimeSpan to = new TimeSpan(0,23,59,59,999);

                query.To = new DateTime(query.To.Year, query.To.Month, query.To.Day);

                if (rangeValues.Length == 2)
                {
                    from = ParseTime(rangeValues[0], from);
                    to = ParseTime(rangeValues[1], to);

                    query.From = query.From.Add(from);
                    query.To = query.To.Add(to);                    
                }
            }

            return query;
        }

        private static DateTime ParseDate(string valueString, DateTime defaultValue)
        {
            try
            {
                if (String.IsNullOrWhiteSpace(valueString))
                    return defaultValue;

                if (valueString.Length == 8)
                {
                    return DateTime.ParseExact(valueString, "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture,
                
                                               DateTimeStyles.AssumeLocal);
                }
                else
                {
                    return DateTime.ParseExact(valueString, "yyyyMMdd hh:mm", System.Globalization.CultureInfo.InvariantCulture,
                                            DateTimeStyles.AssumeLocal);
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine("Error parsing datetime:" + valueString);
                return defaultValue;
            }
        }

        private static TimeSpan ParseTime(string valueString, TimeSpan defaultValue)
        {
            try
            {
                if (String.IsNullOrWhiteSpace(valueString))
                    return defaultValue;

                if (valueString.Length == 10)
                {
                    string paddedString = valueString.Substring(0, 2) + ":" +
                                          valueString.Substring(2, 2) + ":" +
                                          valueString.Substring(4, 2) + "." +
                                          valueString.Substring(7, 3);

                    return TimeSpan.ParseExact(valueString, @"hhmmss\.fff", System.Globalization.CultureInfo.InvariantCulture);
                }
                else 
                {
                    return TimeSpan.ParseExact(valueString, "hhmmss", System.Globalization.CultureInfo.InvariantCulture);
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine("Error parsing datetime:" + valueString);
                return defaultValue;
            }
        }
    }
}
