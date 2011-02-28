using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
using server.Properties;

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
            //server.OnDicomClientCreated = (a, s, t) => { s.ThrottleSpeed = 0; };

            server.AddPort(Settings.Default.ListenPort,DcmSocketType.TCP);
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
            var ipEndpoint = Socket.RemoteEndPoint as IPEndPoint;

            //if (ipEndpoint != null)
            //{
            //    if (ipEndpoint.Address.Equals(IPAddress.Parse("192.168.1.155")))
            //        this.ThrottleSpeed = 10;
            //}

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

            using( var database = new MedicalISDataContext() )
            {
                var queryLevel = query.GetString(DicomTags.QueryRetrieveLevel, null);

                if (queryLevel == "STUDY")
                {
                    IQueryable<Study> studies = StudyQueries.GetMatchingStudies(database, query);

                    if (QueryHasPatientSpecificFilters(query))
                    {
                        var matchingPatients = PatientQueries.GetMatchingPatients(database, query);
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

                        response.AddElementWithValue(DicomTags.NumberOfStudyRelatedSeries, currentStudy.Series.Count);
                        response.AddElementWithValue(DicomTags.NumberOfStudyRelatedInstances, (from s in currentStudy.Series select s.Images.Count).Sum());
                        
                        SendCFindResponse(presentationID, messageID, response, DcmStatus.Pending);
                    }
                }
                else if (queryLevel == "SERIES")
                {
                    // SendCFindResponse(presentationID, messageID, response, DcmStatus.Pending);
                }

                SendCFindResponse(presentationID, messageID, DcmStatus.Success);

            }
        }

        private bool QueryHasPatientSpecificFilters(DcmDataset query)
        {
            return query.Elements.Any(e => e.Tag.Group == 0x0010 && !String.IsNullOrWhiteSpace(query.GetElement(e.Tag).GetValueString()) );
        }

        protected override void OnReceiveCMoveRequest(byte presentationID, ushort messageID, string destinationAE, DcmPriority priority, DcmDataset query)
        {
            query.Dump();

            AEInfo recipientInfo = FindAE(destinationAE);

            if (recipientInfo == null)
            {
                SendCMoveResponse(presentationID, messageID, DcmStatus.QueryRetrieveMoveDestinationUnknown, 0, 0, 0, 0);
                return;
            }

            Console.Write("Moving to:" + recipientInfo.Name + "> ");

            using (var database = new MedicalISDataContext())
            {
                var storeClient = new CStoreClient();
                storeClient.CallingAE = "CURAPACS";
                storeClient.CalledAE = destinationAE;

                storeClient.PreferredTransferSyntax = DicomTransferSyntax.ImplicitVRLittleEndian;

                var files = GetFilePaths(database, query);

                foreach (var f in files)
                {
                    storeClient.AddFile(f);
                }

                ushort totalImageCount = (ushort)files.Count();
                ushort imagesProcessed = 0;
                ushort errorCount = 0;
                ushort successCount = 0;

                storeClient.OnCStoreResponseReceived = (c, i) =>
                                                           {

                                                               if (i.Status == DcmStatus.Success)
                                                               {
                                                                   successCount++;
                                                                   Console.Write(".");
                                                               }
                                                               else
                                                               {
                                                                   errorCount++;
                                                                   Console.Write("x");
                                                               }

                                                               imagesProcessed++;
                                                            
                                                               SendCMoveResponse(presentationID, messageID, DcmStatus.Pending, (ushort)(totalImageCount-imagesProcessed), (ushort) successCount, 0, errorCount);
                                                          };

                storeClient.OnCStoreRequestProgress = (c, i, p) =>
                                                          {
                                                              //Console.WriteLine("{0}@{1}", p.BytesTransfered, c.Socket.LocalStats.ToString());
                                                          };

                storeClient.Connect( recipientInfo.Ip, recipientInfo.Port, DcmSocketType.TCP);

                if (storeClient.Wait())
                {
                
                }

                storeClient.Close();

                SendCMoveResponse(presentationID, messageID, DcmStatus.Success, 0, imagesProcessed, 0, 0);

                Console.WriteLine("");
                Console.WriteLine("Done.");
            }

        }

        private static AEInfo FindAE(string destinationAE)
        {
            var allAEs = Settings.Default.KnownAEs.Split(';');

            foreach (var ae in allAEs)
            {
                string name = ae.Substring(0, ae.IndexOf('='));

                if (name.ToLower() == destinationAE.ToLower())
                { 
                    string address = ae.Substring(ae.IndexOf('=') + 1);

                    string ip = address;

                    if (ip.Contains(':'))
                        ip = ip.Substring(0, ip.IndexOf(':'));

                    int port = 104;

                    if (address.Contains(':'))
                        port = int.Parse(address.Substring(address.IndexOf(':') + 1));

                    return new AEInfo()
                               {
                                   Name = name,
                                   Ip = ip,
                                   Port = port
                               };
                }
            }


            return null;
        }

        private static IEnumerable<string> GetFilePaths(MedicalISDataContext database, DcmDataset query)
        {
            if (query.Elements.Any(e => e.Tag == DicomTags.StudyInstanceUID))
            {
                string studyInstanceUid = query.GetElement(DicomTags.StudyInstanceUID).GetValueString();

                var imagePaths = from i in database.Images
                               where i.Series.Study.StudyInstanceUid == studyInstanceUid
                               select Path.Combine(Settings.Default.RootPath, i.ArchivedStorageLocation);

                return imagePaths;
            }
            else
            {
                return new string[]{};
            }
        }
    }

}
