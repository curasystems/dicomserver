using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Dicom.Data;
using System.Data.Linq.SqlClient;

namespace server
{
    public class StudyQueries
    {

        public static IQueryable<Study> GetMatchingStudies(MedicalISDataContext database, DcmDataset query)
        {
            var studies = from s in database.Studies select s;

            studies = studies.Where( FilterByStudyDate(query) );
            studies = studies.Where( FilterByAccessionNumber(query) );
            
            studies = studies.Where( FilterByModality(query) );

            studies.OrderByDescending(s => s.PerformedDateTime);

            return studies;
        }


        private static Expression<Func<Study, bool>> FilterByStudyDate(DcmDataset query)
        {
            Expression<Func<Study, bool>> allMatch = p => true;

            var studyQuery = query.GetElement(DicomTags.StudyDate);

            if (studyQuery == null)
                return allMatch;

            var valueString = studyQuery.GetValueString();

            if (String.IsNullOrWhiteSpace(valueString))
                return allMatch;

            var dateTimeRange = DateTimeRangeQuery.Parse(valueString);

            return s => s.PerformedDateTime >= dateTimeRange.From && s.PerformedDateTime <= dateTimeRange.To;
        }


        private static Expression<Func<Study, bool>> FilterByAccessionNumber(DcmDataset query)
        {
            Expression<Func<Study, bool>> allMatch = p => true;

            var studyQuery = query.GetElement(DicomTags.AccessionNumber);

            if (studyQuery == null)
                return allMatch;

            var valueString = studyQuery.GetValueString();

            if (String.IsNullOrWhiteSpace(valueString))
                return allMatch;

            if (valueString.EndsWith("*"))
                return s => s.AccessionNumber.StartsWith(valueString.Trim('*'));
            else
                return s => s.AccessionNumber == valueString;
        }


        private static Expression<Func<Study, bool>> FilterByModality(DcmDataset query)
        {
            Expression<Func<Study, bool>> allMatch = p => true;

            var studyQuery = query.GetElement(DicomTags.ModalitiesInStudy);

            if (studyQuery == null)
                return allMatch;

            var valueString = studyQuery.GetValueString();

            if (String.IsNullOrWhiteSpace(valueString))
                return allMatch;

            var modalities = valueString.Replace(@"\\", @"\").Split('\\');

            if (modalities.Length == 1)
            {
                return s => s.ModalityAggregation.Contains(modalities[0]);
            }
            else
            {
                return s => s.Series.Any( series => modalities.Contains(series.PerformedModalityType));
            }
        }
    }
}
