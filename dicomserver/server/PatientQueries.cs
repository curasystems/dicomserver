using System;
using System.Collections.Generic;
using System.Data.Linq.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Dicom.Data;
using server.Properties;

namespace server
{
    public class PatientQueries
    {
        public static IQueryable<Patient> GetMatchingPatients(MedicalISDataContext database, DcmDataset query, bool anonymousOnly = false)
        {
            var patients = from p in database.Patients select p;

            if( anonymousOnly == false )
                patients = patients.Where(FilterByPatientsName(query));

            patients = patients.Where(FilterByPatientsId(query));
            patients = patients.Where(FilterByPatientsBirthDate(query));
            patients = patients.Where(FilterByStudyPerformedDate(query));
            patients = patients.Where(FilterByStudyModalityDate(query));

            patients = patients.OrderByDescending(p => p.Studies.Max(s => s.PerformedDateTime));

            return patients;
        }
        private static Expression<Func<Patient, bool>> FilterByStudyPerformedDate(DcmDataset query)
        {
            Expression<Func<Patient, bool>> allMatch = p => true;

            var studyQuery = query.GetElement(DicomTags.StudyDate);

            if (studyQuery == null)
                return allMatch;

            var valueString = studyQuery.GetValueString();

            if (String.IsNullOrWhiteSpace(valueString))
                return allMatch;

            var dateTimeRange = DateTimeRangeQuery.Parse(valueString, query.GetString(DicomTags.StudyTime, null));

            return
                p =>
                p.Studies.Any(s => s.PerformedDateTime >= dateTimeRange.From && s.PerformedDateTime <= dateTimeRange.To);
        }


        private static Expression<Func<Patient, bool>> FilterByStudyModalityDate(DcmDataset query)
        {
            Expression<Func<Patient, bool>> allMatch = p => true;

            var studyQuery = query.GetElement(DicomTags.ModalitiesInStudy);

            if (studyQuery == null)
                return allMatch;

            var valueString = studyQuery.GetValueString();

            if (String.IsNullOrWhiteSpace(valueString))
                return allMatch;

            var modalities = valueString.Replace(@"\\", @"\").Split('\\');

            if (String.IsNullOrWhiteSpace(valueString))
                return allMatch;

            if (modalities.Length == 1)
            {
                return p => p.Studies.Any(s => s.ModalityAggregation.Contains(modalities[0]));
            }
            else
            {
                return p => p.Studies.Any( s => s.Series.Any( series => modalities.Contains(series.PerformedModalityType)));
                
            }
        }

        private static Expression<Func<Patient, bool>> FilterByPatientsName(DcmDataset query)
        {
            Expression<Func<Patient, bool>> allMatch = p => true;

            var patientNameQuery = query.GetElement(DicomTags.PatientsName);

            if (patientNameQuery == null)
                return allMatch;

            var patientNameDicomFormatted = patientNameQuery.GetValueString();

            if (String.IsNullOrWhiteSpace(patientNameDicomFormatted))
                return allMatch;

            string[] lName;

            if (patientNameDicomFormatted.Contains("[^]"))
                lName = patientNameDicomFormatted.Split(new[] { "[^]" }, StringSplitOptions.None);
            else if (patientNameDicomFormatted.Contains("^"))
                lName = patientNameDicomFormatted.Split(new[] { "^" }, StringSplitOptions.None);
            else
                lName = patientNameDicomFormatted.Split(new[] { Properties.Settings.Default.PatientNameSplitCharacterForFind });
         

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

            if (!firstName.StartsWith("\""))
                firstName += "%";
            if (!lastName.StartsWith("\""))
                lastName += "%";

            return p => SqlMethods.Like(p.FirstName, firstName) && SqlMethods.Like(p.LastName, lastName);
        }

        private static Expression<Func<Patient, bool>> FilterByPatientsId(DcmDataset query)
        {
            Expression<Func<Patient, bool>> allMatch = p => true;

            var patientQuery = query.GetElement(DicomTags.PatientID);

            if (patientQuery == null)
                return allMatch;

            var valueString = patientQuery.GetValueString();

            if (String.IsNullOrWhiteSpace(valueString))
                return allMatch;

            if (valueString.EndsWith("*"))
                return p => p.ExternalPatientID.StartsWith(valueString.Trim('*'));
            else
                return p => p.ExternalPatientID == valueString;
        }

        private static Expression<Func<Patient, bool>> FilterByPatientsBirthDate(DcmDataset query)
        {
            Expression<Func<Patient, bool>> allMatch = p => true;

            var patientQuery = query.GetElement(DicomTags.PatientsBirthDate);

            if (patientQuery == null)
                return allMatch;

            var valueString = patientQuery.GetValueString();

            if (String.IsNullOrWhiteSpace(valueString))
                return allMatch;

            var dateTimeRange = DateTimeRangeQuery.Parse(valueString);

            return p => p.BirthDateTime >= dateTimeRange.From && p.BirthDateTime <= dateTimeRange.To;
        }


    }
}
