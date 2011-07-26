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

        public static IQueryable<Study> GetMatchingStudies(MedicalISDataContext database, DcmDataset query, bool isAnonymousQuery )
        {
            var studies = from s in database.Studies select s;

            if (!isAnonymousQuery)
                studies = studies.Where( FilterByPatientsName(query) );
            
            studies = studies.Where( FilterByPatientsId(query) );
            studies = studies.Where( FilterByPatientsBirthDate(query) );

            studies = studies.Where( FilterByStudyUid(query) );
            studies = studies.Where( FilterByStudyDate(query) );
            studies = studies.Where( FilterByAccessionNumber(query) );
            
            studies = studies.Where( FilterByModality(query) );

            studies.OrderBy(s => s.PerformedDateTime);

            return studies;
        }

        private static Expression<Func<Study, bool>> FilterByPatientsName(DcmDataset query)
        {
            Expression<Func<Study, bool>> allMatch = p => true;

            var patientNameQuery = query.GetElement(DicomTags.PatientsName);

            if (patientNameQuery == null)
                return allMatch;

            var patientNameDicomFormatted = patientNameQuery.GetValueString();

            if (String.IsNullOrWhiteSpace(patientNameDicomFormatted))
                return allMatch;

            string[] lName;

            if (patientNameDicomFormatted.Contains("[^]"))
                lName = patientNameDicomFormatted.Split(new[] {"[^]"}, StringSplitOptions.None);
            else if (patientNameDicomFormatted.Contains("^"))
                lName = patientNameDicomFormatted.Split(new[] { "^" }, StringSplitOptions.None);
            else 
                lName = patientNameDicomFormatted.Split(new[] {Properties.Settings.Default.PatientNameSplitCharacterForFind});
            
            var firstName = "";
            var lastName = "";

            if (lName.Length == 0)
                return allMatch;

            if (lName.Length >= 2)
            {
                firstName = lName[1];
                firstName = firstName.TrimEnd('*').Trim();
                firstName = firstName.Replace('*', '%');
            }

            if (lName.Length >= 1)
            {
                lastName = lName[0];
                lastName = lastName.TrimEnd('*').Trim();
                lastName = lastName.Replace('*', '%');
            }

            if( !firstName.StartsWith("\""))
                firstName += "%";
            if (!lastName.StartsWith("\""))
                lastName += "%";

            return s => SqlMethods.Like(s.Patient.FirstName, firstName) && SqlMethods.Like(s.Patient.LastName, lastName);
        }


        private static Expression<Func<Study, bool>> FilterByPatientsBirthDate(DcmDataset query)
        {
            Expression<Func<Study, bool>> allMatch = p => true;

            var patientQuery = query.GetElement(DicomTags.PatientsBirthDate);

            if (patientQuery == null)
                return allMatch;

            var valueString = patientQuery.GetValueString();

            if (String.IsNullOrWhiteSpace(valueString))
                return allMatch;

            var dateTimeRange = DateTimeRangeQuery.Parse(valueString);

            return s => s.Patient.BirthDateTime >= dateTimeRange.From && s.Patient.BirthDateTime <= dateTimeRange.To;
        }


        private static Expression<Func<Study, bool>> FilterByPatientsId(DcmDataset query)
        {
            Expression<Func<Study, bool>> allMatch = p => true;

            var studyQuery = query.GetElement(DicomTags.PatientID);

            if (studyQuery == null)
                return allMatch;

            var valueString = studyQuery.GetValueString();

            if (String.IsNullOrWhiteSpace(valueString))
                return allMatch;

            if (valueString.EndsWith("*"))
                return s => s.Patient.ExternalPatientID.StartsWith(valueString.Trim('*'));
            else
                return s => s.Patient.ExternalPatientID == valueString;
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

            var dateTimeRange = DateTimeRangeQuery.Parse(valueString, query.GetString(DicomTags.StudyTime, null));

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


        private static Expression<Func<Study, bool>> FilterByStudyUid(DcmDataset query)
        {
            Expression<Func<Study, bool>> allMatch = p => true;

            var studyQuery = query.GetElement(DicomTags.StudyInstanceUID);

            if (studyQuery == null)
                return allMatch;

            var valueString = studyQuery.GetValueString();

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
    }
}
