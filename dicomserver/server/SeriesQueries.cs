using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Dicom.Data;
using System.Data.Linq.SqlClient;

namespace server
{
    public class SeriesQueries
    {
        public static IQueryable<Series> GetMatchingSeries(MedicalISDataContext database, DcmDataset query)
        {
            var series = from s in database.Series select s;

            series = series.Where( FilterByStudyUid(query) );
            
            series.OrderBy(s => s.PerformedDateTime);

            return series;
        }

        private static Expression<Func<Series, bool>> FilterByStudyUid(DcmDataset query)
        {
            Expression<Func<Series, bool>> allMatch = p => true;

            var seriesQuery = query.GetElement(DicomTags.StudyInstanceUID);

            if (seriesQuery == null)
                return allMatch;

            var valueString = seriesQuery.GetValueString();

            if (String.IsNullOrWhiteSpace(valueString))
                return allMatch;

            return s => s.StudyInstanceUid.StartsWith(valueString.Trim('*'));
        }
    }
}
