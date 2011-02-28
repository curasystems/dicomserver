using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Text;

using Dicom;
using Dicom.Data;
using Dicom.Imaging;
using Dicom.Imaging.Display;
using Dicom.Imaging.LUT;
using Dicom.Imaging.Render;
using Dicom.Network;
using Dicom.Network.Server;
using Dicom.Network.Client;
using System.Globalization;

namespace server
{
    class Program
    {
        public static int BytesPerSecondFromMBit(int mb)
        {
            return mb*1024*1024;
        }

        public static int BytesPerSecondFromKBit(int kb)
        {
            return kb * 1024;
        }


        static void Main(string[] args)
        {
            var server = new DcmServer<CImageServer>();
            server.OnDicomClientCreated = (a, s, t) => { s.ThrottleSpeed = 0; };

            server.AddPort(104,DcmSocketType.TCP);
            server.Start();

            Console.WriteLine("Listening on 104");
            Console.ReadKey();

            server.Stop();
        }
    }

    class CImageServer : DcmServiceBase
    {
        public CImageServer()
        {
            LogID = "C-Find/Move SCP";
        }

        protected override void OnInitializeNetwork()
        {
            IPEndPoint ipEndpoint = Socket.RemoteEndPoint as IPEndPoint;

            if (ipEndpoint != null)
            {
                if (ipEndpoint.Address.Equals(IPAddress.Parse("192.168.1.155")))
                    this.ThrottleSpeed = 10;
            }

            base.OnInitializeNetwork();
        }

        protected override void OnReceiveAssociateRequest(DcmAssociate association)
        {
            association.NegotiateAsyncOps = false;

            LogID = association.CallingAE;

            foreach (DcmPresContext pc in association.GetPresentationContexts())
            {
                pc.SetResult(DcmPresContextResult.RejectTransferSyntaxesNotSupported);

                HandleEchoRequests(pc);
                HandleFindRequests(pc);
                HandleMoveRequests(pc);
            }

            SendAssociateAccept(association);
        }

        private void HandleMoveRequests(DcmPresContext pc)
        {
            if (pc.AbstractSyntax == DicomUID.StudyRootQueryRetrieveInformationModelMOVE)
            {
                if (pc.HasTransfer(DicomTransferSyntax.ImplicitVRLittleEndian))
                {
                    pc.SetResult(DcmPresContextResult.Accept, DicomTransferSyntax.ImplicitVRLittleEndian);
                }
                else if (pc.HasTransfer(DicomTransferSyntax.ExplicitVRLittleEndian))
                {
                    pc.SetResult(DcmPresContextResult.Accept, DicomTransferSyntax.ExplicitVRLittleEndian);
                }
            }
        }

        private void HandleFindRequests(DcmPresContext pc)
        {
            if (pc.AbstractSyntax == DicomUID.StudyRootQueryRetrieveInformationModelFIND)
            {
                if (pc.HasTransfer(DicomTransferSyntax.ImplicitVRLittleEndian))
                {
                    pc.SetResult(DcmPresContextResult.Accept, DicomTransferSyntax.ImplicitVRLittleEndian);
                }
                else if (pc.HasTransfer(DicomTransferSyntax.ExplicitVRLittleEndian))
                {
                    pc.SetResult(DcmPresContextResult.Accept, DicomTransferSyntax.ExplicitVRLittleEndian);
                }
            }
        }

        private void HandleEchoRequests(DcmPresContext pc)
        {
            if (pc.AbstractSyntax == DicomUID.VerificationSOPClass)
            {
                if (pc.HasTransfer(DicomTransferSyntax.ImplicitVRLittleEndian))
                {
                    pc.SetResult(DcmPresContextResult.Accept, DicomTransferSyntax.ImplicitVRLittleEndian);
                }
                else if (pc.HasTransfer(DicomTransferSyntax.ExplicitVRLittleEndian))
                {
                    pc.SetResult(DcmPresContextResult.Accept, DicomTransferSyntax.ExplicitVRLittleEndian);
                }
            }
        }

        protected override void OnReceiveCEchoRequest(byte presentationID, ushort messageID, Dicom.Network.DcmPriority priority)
        {
            SendCEchoResponse(presentationID, messageID, DcmStatus.Success);
        }

        protected override void OnReceiveCFindRequest(byte presentationID, ushort messageID, DcmPriority priority, Dicom.Data.DcmDataset query)
        {
            Trace.WriteLine(query.Dump());
 
            var database = new MedicalISDataContext();

            
            var queryLevel = query.GetString(DicomTags.QueryRetrieveLevel, null);
            
            if (queryLevel == "STUDY")
            {
                IQueryable<Study> studies = GetMatchingStudies(database, query);

                if( QueryHasPatientSpecificFilters(query) )
                {
                    var matchingPatients = GetMatchingPatients(database, query);
                    matchingPatients = matchingPatients.Take(500);

                    studies = from study in studies
                        join patient in matchingPatients
                        on study.PatientId equals patient.PatientId
                        select study;
                }

                studies = studies.Take(100);
                
                //string lStudyDate = query.GetElement(DicomTags.StudyDate).GetValueString();
                //var lStudyDateRange = lStudyDate.Split(new [] {'-'},2);

                //string lStudyStartAsString = "18000101";
                //string lStudyEndAsString = "29990101";

                //if (lStudyDateRange.Length > 0 )
                //    lStudyStartAsString = GetDateString(lStudyDateRange[0], lStudyStartAsString);

                //if( lStudyDateRange.Length > 1 )
                //    lStudyEndAsString = GetDateString(lStudyDateRange[1], lStudyEndAsString);

                //var studyFrom = DateTime.ParseExact(lStudyStartAsString, "yyyyMMdd", CultureInfo.InvariantCulture);
                //var studyTo = DateTime.ParseExact(lStudyEndAsString, "yyyyMMdd", CultureInfo.InvariantCulture);)
                
                foreach (var currentStudy in studies)
                {
                    var p = currentStudy.Patient;

                    var response = new DcmDataset();

                    // Map saved study tags to output

                    response.AddElementWithValue(DicomTags.RetrieveAETitle, "CURAPACS");
                    
                    response.AddElementWithValue(DicomTags.PatientID, p.ExternalPatientID);
                    response.AddElementWithValue(DicomTags.PatientsName, p.LastName + "^" + p.FirstName);    
                    response.AddElementWithValue(DicomTags.PatientsBirthDate, p.BirthDateTime.Value);

                    response.AddElementWithValue(DicomTags.StudyInstanceUID, currentStudy.StudyInstanceUid);
                    response.AddElementWithValue(DicomTags.AccessionNumber, currentStudy.AccessionNumber);
                    response.AddElementWithValue(DicomTags.StudyDescription, currentStudy.Description);
                    response.AddElementWithValue(DicomTags.ModalitiesInStudy, currentStudy.ModalityAggregation);

                    if (currentStudy.PerformedDateTime.HasValue)
                    { 
                        response.AddElementWithValue(DicomTags.StudyDate, currentStudy.PerformedDateTime.Value);
                        response.AddElementWithValue(DicomTags.StudyTime, currentStudy.PerformedDateTime.Value);
                    }

                    response.AddElementWithValue(DicomTags.NumberOfStudyRelatedSeries, currentStudy.Series.Count );
                    //response.AddElementWithValue(DicomTags.NumberOfStudyRelatedInstances, 1);
                    
                    SendCFindResponse(presentationID, messageID, response, DcmStatus.Pending);
                }                    
            }
            else if (queryLevel == "SERIES")
            {
               // SendCFindResponse(presentationID, messageID, response, DcmStatus.Pending);
            }

            SendCFindResponse(presentationID, messageID, DcmStatus.Success);
        }

        private bool QueryHasPatientSpecificFilters(DcmDataset query)
        {
            return query.Elements.Any(e => e.Tag.Group == 0x0010 && !String.IsNullOrWhiteSpace(query.GetElement(e.Tag).GetValueString()) );
        }

        private static IQueryable<Study> GetMatchingStudies(MedicalISDataContext database, DcmDataset query)
        {
            var studies = from s in database.Studies select s;

            studies = studies.Where(FilterByStudyDate(query));
            studies = studies.Where(FilterByAccessionNumber(query) );
            
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


        private static IQueryable<Patient> GetMatchingPatients(MedicalISDataContext database, DcmDataset query)
        {
            var patients = from p in database.Patients select p;

            patients = patients.Where( FilterByPatientsName(query) );
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
            
            var lName = patientNameDicomFormatted.Split(new[] {','});
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


        private string Get(string valueToParse, string defaultValue)
        {
            if (String.IsNullOrWhiteSpace(valueToParse))
                return defaultValue;

            return valueToParse;
        }
        
        private static string PrepareDicomName(string lPatientName)
        {
            lPatientName = lPatientName.Replace("[", "");
            lPatientName = lPatientName.Replace("]", "");
            lPatientName = lPatientName.Replace("*", "");
        
            return lPatientName;
        }

        protected override void OnReceiveCMoveRequest(byte presentationID, ushort messageID, string destinationAE, DcmPriority priority, DcmDataset dataset)
        {
            dataset.Dump();
            
            var config = new ScuConfig();

            
            var files = new List<string> {"ct.dcm "};
            
            var storeClient = new CStoreClient();
            storeClient.CallingAE = "CURAPACS";
            storeClient.CalledAE = destinationAE;

            storeClient.PreferredTransferSyntax = DicomTransferSyntax.ImplicitVRLittleEndian;

            foreach (var f in files)
            {
                storeClient.AddFile(f);
            }

            ushort totalImageCount = (ushort)files.Count;
            ushort imagesProcessed = 0;
            ushort errorCount = 0;
            ushort successCount = 0;

            storeClient.OnCStoreResponseReceived = (c, i) =>
                                                        {
                                                            if (i.Status == DcmStatus.Success)
                                                                successCount++;
                                                            else
                                                                errorCount++;

                                                            imagesProcessed++;
                                                            
                                                            SendCMoveResponse(presentationID, messageID, DcmStatus.Pending, (ushort)(totalImageCount-imagesProcessed), (ushort) successCount, 0, errorCount);
                                     };

            storeClient.OnCStoreRequestProgress = (c, i, p) =>
                                                      {
                                                          Console.WriteLine("{0}@{1}", p.BytesTransfered, c.Socket.LocalStats.ToString());
                                                      };

            storeClient.Connect("192.168.2.53", 11112, DcmSocketType.TCP);

            if (storeClient.Wait())
            {
                
            }

            storeClient.Close();

            SendCMoveResponse(presentationID, messageID, DcmStatus.Success, 0, imagesProcessed, 0, 0);
        }
    }

    [Serializable]
    public class ScuConfig
    {
        public string LocalAE = "TEST_SCU";
        public string RemoteAE = "ANY-SCP";
        public string RemoteHost = "localhost";
        public int RemotePort = 104;
        public uint MaxPdu = 16384;
        public int Timeout = 30;
        public int TransferSyntax = 0;
        public int Quality = 90;
        public bool UseTls = false;
    }

    public class Filter<T>
    {
        //public List<Expression<Func<T, Boolean>>>  Filters
        //{
        //    get
        //    {
                
        //    }
        //}
    }
}
