using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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

        protected override void OnReceiveCFindRequest(byte presentationID, ushort messageID, DcmPriority priority, Dicom.Data.DcmDataset dataset)
        {
            Trace.WriteLine(dataset.Dump());
 
            string queryLevel = dataset.GetString(DicomTags.QueryRetrieveLevel, null);


            MedicalISDataContext MedicalIS = new MedicalISDataContext();

            string lPatientID = dataset.GetElement(DicomTags.PatientID).GetValueString();

            string lPatientName = dataset.GetElement(DicomTags.PatientsName).GetValueString();

            lPatientName = PrepareDicomName(lPatientName);

            string lFirstName = "";
            string lLastName = "";
            string[] lName = lPatientName.Split('^');

            if (lName != null)
            {
                try
                {
                    lFirstName = lName[0];
                    lLastName = lName[1];
                }
                catch (Exception ex)
                { 
                
                }
            }

          

            var patients = from p in MedicalIS.Patients
                           where p.ExternalPatientID.StartsWith(lPatientID)
                           && p.FirstName.StartsWith(lFirstName)
                           && p.LastName.StartsWith(lLastName)
                           select p;

            var response = new DcmDataset();

            if (queryLevel == "STUDY")
            {

                string lStudyDate = dataset.GetElement(DicomTags.StudyDate).GetValueString();
                string[] lStudyDateRange = lStudyDate.Split('-');

                string lStudyStart = "18000101";
                string lStudyEnd = "29990101";

                if (lStudyDateRange != null)
                {
                    try
                    {
                        lStudyStart = lStudyDateRange[0];
                        lStudyEnd = lStudyDateRange[1];
                    }
                    catch (Exception ex)
                    {

                    }
                }

                

                foreach (var currentPatient in patients)
                {

                     var studies = from study in MedicalIS.Studies
                                  where study.PatientId == currentPatient.PatientId
                                  && (study.CreatedDateTime >= DateTime.Parse(lStudyStart) && study.CreatedDateTime <= DateTime.Parse(lStudyEnd))
                                  select study;

                     if (studies != null)
                     {
                         foreach (var currentStudy in studies)
                         {
                             response = dataset;
                             response.AddElementWithValueString(DicomTags.RetrieveAETitle, "CURAPACS");
                             response.AddElementWithValueString(DicomTags.PatientID, currentPatient.ExternalPatientID);
                             response.AddElementWithValueString(DicomTags.PatientsName, currentPatient.FirstName + "^" + currentPatient.LastName);
                             
                             response.AddElementWithValue(DicomTags.NumberOfStudyRelatedSeries, 1 );
                             response.AddElementWithValue(DicomTags.NumberOfStudyRelatedInstances, 1);

                             SendCFindResponse(presentationID, messageID, response, DcmStatus.Pending);
                     
                         }
                     }

                }
            }


            //var lStudyDate = dataset.GetElement(DicomTags.StudyDate);
            //string studyDate = lStudyDate.GetValueString();
            //var lStudyTime = dataset.GetElement(DicomTags.StudyTime);
            //string studyTime = lStudyTime.GetValueString();
            //var lAccessionNumber = dataset.GetElement(DicomTags.AccessionNumber);
            //string accessionNumber = lAccessionNumber.GetValueString();
            //var lQueryRetrieve = dataset.GetElement(DicomTags.QueryRetrieveLevel);
            //string queryRetrieve = lQueryRetrieve.GetValueString();
            //var lModalitiesInStudy = dataset.GetElement(DicomTags.ModalitiesInStudy);
            //string modalitiesInStudy = lModalitiesInStudy.GetValueString();
            //var lReferringPhysician = dataset.GetElement(DicomTags.ReferringPhysiciansName);
            //string referringPhysician = lReferringPhysician.GetValueString();
            //var lStudyDescription = dataset.GetElement(DicomTags.StudyDescription);
            //string studyDescription = lStudyDescription.GetValueString();
            //var lInstitutionalDescription = dataset.GetElement(DicomTags.InstitutionName);
            //string institutionalDescription = lInstitutionalDescription.GetValueString();
            //var lPatientName = dataset.GetElement(DicomTags.PatientsName);
            //string patientName = lPatientName.GetValueString();
            //var lPatientId = dataset.GetElement(DicomTags.PatientID);
            //string patientId = lPatientId.GetValueString();
            //var lPatientsBirthdate = dataset.GetElement(DicomTags.PatientsBirthDate);
            //string patientBirthdate = lPatientsBirthdate.GetValueString();
            //var lPatientsSex = dataset.GetElement(DicomTags.PatientsSex);
            //string patientsSex = lPatientsSex.GetValueString();
            //var lStudyInstance = dataset.GetElement(DicomTags.StudyInstanceUID);
            //string studyInstance = lStudyInstance.GetValueString();
            //var lStudyId = dataset.GetElement(DicomTags.StudyID);
            //string studyId = lStudyId.GetValueString();
            

         
            

            //if ( queryLevel == "STUDY")
            //{ 




            //    var d = dataset.GetElement(DicomTags.StudyDate);
            //    var t = dataset.GetElement(DicomTags.StudyTime);

            //    //var rangeQuery = new DateTimeRangeQuery(d.GetValueString(), t.GetValueString());

                
            //    //var response = new DcmDataset(DicomTransferSyntax.ExplicitVRLittleEndian);
            //    response.AddElementWithValue( DicomTags.NumberOfStudyRelatedSeries, 1 );
            //    response.AddElementWithValue( DicomTags.NumberOfStudyRelatedInstances, 1);
                
            
                

            //    //response.StudyInstanceUID = "2.233.45345.234234.234234.234";
            //}
            else if (queryLevel == "SERIES")
            {
                SendCFindResponse(presentationID, messageID, response, DcmStatus.Pending);

                
            }

            SendCFindResponse(presentationID, messageID, DcmStatus.Success);
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
}
