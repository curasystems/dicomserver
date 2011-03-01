﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Dicom.Data;

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

            patients = patients.OrderByDescending(p => p.Studies.Max(s => s.PerformedDateTime));

            return patients;
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

            var lName = patientNameDicomFormatted.Split(new[] { ',' });
            var firstName = "";
            var lastName = "";

            if (lName.Length == 0)
                return allMatch;

            if (lName.Length >= 2)
            {
                firstName = lName[1];
                firstName = firstName.TrimEnd('*');
                firstName = firstName.Replace('*', '%');
            }

            if (lName.Length >= 1)
            {
                lastName = lName[0];
                lastName = lastName.TrimEnd('*');
                lastName = lastName.Replace('*', '%');
            }

            return p => p.FirstName.StartsWith(firstName) && p.LastName.StartsWith(lastName);
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
