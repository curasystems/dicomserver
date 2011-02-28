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

        public static DateTimeRangeQuery Parse(string valueString)
        {
            var minValue = new DateTime(1800, 1, 1);
            var maxValue = new DateTime(9999, 1, 1);

            if (valueString.Contains('-') == false)
            {
                var from = ParseDateTime(valueString, minValue);
                var to = new DateTime(from.Year, from.Month, from.Day, 23, 59, 59, 999);

                return new DateTimeRangeQuery() { From = from, To = to };
            }
            else
            {
                var rangeValues = valueString.Split(new char[] { '-' }, 2);

                DateTime from = minValue;
                DateTime to = maxValue;

                if (rangeValues.Length == 2)
                {
                    from = ParseDateTime(rangeValues[0], from);
                    to = ParseDateTime(rangeValues[1], to);

                    if(to.Hour == 0 && to.Minute == 0 && to.Second == 0)
                        to = new DateTime(to.Year, to.Month, to.Day, 23, 59, 59, 999);
                }

                return new DateTimeRangeQuery() {From = from, To = to};
            }
        }

        private static DateTime ParseDateTime(string valueString, DateTime defaultValue)
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
    }
}
