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
            series = series.Where( FilterBySeriesUid(query) );
            series = series.Where( FilterBySeriesDate(query) );
            
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

            if (valueString.EndsWith("*"))
            {
                return s => s.StudyInstanceUid.StartsWith(valueString.Trim('*'));
            }
            else
            {
                return s => s.StudyInstanceUid == valueString;
            }
        }

        private static Expression<Func<Series, bool>> FilterBySeriesUid(DcmDataset query)
        {
            Expression<Func<Series, bool>> allMatch = p => true;

            var seriesQuery = query.GetElement(DicomTags.SeriesInstanceUID);

            if (seriesQuery == null)
                return allMatch;

            var valueString = seriesQuery.GetValueString();

            if (String.IsNullOrWhiteSpace(valueString))
                return allMatch;

            if (valueString.EndsWith("*"))
            {
                return s => s.SeriesInstanceUid.StartsWith(valueString.Trim('*'));
            }
            else
            {
                return s => s.SeriesInstanceUid == valueString;
            }
        }


        private static Expression<Func<Series, bool>> FilterBySeriesDate(DcmDataset query)
        {
            Expression<Func<Series, bool>> allMatch = p => true;

            var studyQuery = query.GetElement(DicomTags.SeriesDate);

            if (studyQuery == null)
                return allMatch;

            var valueString = studyQuery.GetValueString();

            if (String.IsNullOrWhiteSpace(valueString))
                return allMatch;

            var dateTimeRange = DateTimeRangeQuery.Parse(valueString);

            return s => s.PerformedDateTime >= dateTimeRange.From && s.PerformedDateTime <= dateTimeRange.To;
        }

    }
}
