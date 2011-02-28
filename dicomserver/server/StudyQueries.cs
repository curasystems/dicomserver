using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Dicom.Data;

namespace server
{
    public class StudyQueries
    {

        public static IQueryable<Study> GetMatchingStudies(MedicalISDataContext database, DcmDataset query)
        {
            var studies = from s in database.Studies select s;

            studies = studies.Where(FilterByStudyDate(query));
            studies = studies.Where(FilterByAccessionNumber(query));

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

        private string Get(string valueToParse, string defaultValue)
        {
            if (String.IsNullOrWhiteSpace(valueToParse))
                return defaultValue;

            return valueToParse;
        }
        
    }
}
