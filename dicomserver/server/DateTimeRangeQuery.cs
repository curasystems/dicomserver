using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dicom.Data;

namespace server
{
    public class DateTimeRangeQuery
    {
        public DateTimeRangeQuery(string date) : this(date,null)
        {
        }

        public DateTimeRangeQuery(string date, string time)
        {
            ParseDate(date);
            ParseTime(time);
        }

        private void ParseDate(string date)
        {
            throw new NotImplementedException();
        }
        
        private void ParseTime(string time)
        {
            throw new NotImplementedException();
        }

        public DateTime? From { get; set; }
        public DateTime? To { get; set; }

        public static implicit operator DateTimeRangeQuery(string valueAsString)
        {
            return new DateTimeRangeQuery(valueAsString);
        }
    }
}
