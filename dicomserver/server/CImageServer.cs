using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using Dicom;
using Dicom.Data;
using Dicom.Network;
using Dicom.Network.Client;
using Dicom.Network.Server;
using server.Properties;
using Debug = System.Diagnostics.Debug;

namespace server
{
    public class CImageServer : DcmServiceBase
    {
        public CImageServer()
        {
            LogID = "CuraSystems DICOM SCP";
            
            ImageCountOnConnection = 0;

            UseFileBuffer = false;


        }

        protected override void OnInitializeNetwork()
        {
            var ipEndpoint = Socket.RemoteEndPoint as IPEndPoint;

            if (ipEndpoint != null)
            {
                _flagAnonymousAccess = IsAnonymizedIp(ipEndpoint.Address);

                //if (ipEndpoint.Address.Equals(IPAddress.Parse("192.168.1.155")))
                //    this.ThrottleSpeed = 10;
            }

            base.OnInitializeNetwork();
        }

        private static bool IsAnonymizedIp(IPAddress ipAddress)
        {
            string[] ips = Settings.Default.AnonymizedIPs.Split(';');

            var hostEntry = LookupHostEntry(ipAddress);
             
            if( hostEntry != null )
            {
                if (IsAnonymizedHostEntry(ipAddress, hostEntry, ips))
                    return true;
            }
            
            return ips.Contains(ipAddress.ToString());
        }

        private static bool IsAnonymizedHostEntry(IPAddress ipAddress, IPHostEntry hostEntry, string[] ips)
        {
            var hostName = hostEntry.HostName;

            Console.WriteLine("Checking hostname of accessing ip {0} => {1} ({2})", ipAddress, hostName, String.Join(",", hostEntry.Aliases));

            if (ips.Any(h => h.ToLower().Trim() == hostName.ToLower().Trim()))
            {
                return true;
            }
                
            if (ips.Any(h =>  hostEntry.Aliases.Any( alias => alias.ToLower().Trim() == h.ToLower().Trim())))
            {
                return true;
            }

            return false;
        }

        public static IPHostEntry LookupHostEntry(IPAddress ipAddress)
        {
            return LookupHostEntry(ipAddress, Settings.Default.DnsLookupTimeout);
        }

        public static IPHostEntry LookupHostEntry(IPAddress ipAddress, TimeSpan timeout)
        {
            try
            {
                IAsyncResult result = Dns.BeginGetHostEntry(ipAddress, null, null);

                if (result.AsyncWaitHandle.WaitOne(timeout, false))
                {
                    // Received response within timeout limit
                    return Dns.EndGetHostEntry(result);
                }
                else
                {
                    // Error occurred, send back IP Address instead of hostname
                    Console.WriteLine("Looking up hostname of accessing ip {0} => UNKNOWN HOST (Timeout)", ipAddress);
                }
            }
            catch (Exception ex)
            {
                // Error occurred, send back IP Address instead of hostname
                Console.WriteLine("Looking up hostname of accessing ip {0} => UNKNOWN HOST ({1})", ipAddress, ex.Message);
            }

            return null;
        }

        private static bool IsAnonymizedAE(string aeTitle)
        {
            string[] aeTs = Settings.Default.AnonymizedAETs.Split(';');

            if (aeTs.Any(aet => aet.ToLower().Trim() == aeTitle.ToLower().Trim()))
            {
                return true;
            }

            return false;
        }

        protected override void OnReceiveReleaseRequest()
        {
            base.OnReceiveReleaseRequest();
        }

        protected override void OnConnectionClosed()
        {
            Timer.Stop();

            var now = DateTime.Now;

            if (ImageCountOnConnection > 0)
            {
                Console.WriteLine("{0}:Received {1} files in {2}", now, ImageCountOnConnection, Timer.Elapsed);
            }

            if (IsReceiveConnection && ImageCountOnConnection == 0)
            {
                Console.WriteLine("{0}:ERROR Received no files on storage request association...", now);
                Console.WriteLine(Associate.ToString());
                Console.WriteLine();
            }   

            if (MoveQueue.Count > 0)
            {
                MoveReceivedFilesIntoCorrectFolder();
            }

            base.OnConnectionClosed();
        }


        protected override void OnReceiveAssociateRequest(DcmAssociate association)
        {
            Timer = Stopwatch.StartNew();

            association.NegotiateAsyncOps = false;

            LogID = association.CallingAE;

            Console.WriteLine("{0} Received Association Request from AE:{1} -> {2}", DateTime.Now, association.CallingAE, association.CalledAE );

            _flagAnonymousAccess = _flagAnonymousAccess || IsAnonymizedAE(association.CallingAE);

            foreach (DcmPresContext pc in association.GetPresentationContexts())
            {
                HandleEchoAssociationRequest(pc);
                HandleFindAssociationRequest(pc);
                HandleMoveAssociationRequest(pc);
            }

            HandleStoreAssociationRequest(association);

            SendAssociateAccept(association);
        }

        private static void HandleStoreAssociationRequest(DcmAssociate association)
        {
            DcmAssociateProfile profile = DcmAssociateProfile.Find(association, true);
            profile.Apply(association);

            IsReceiveConnection = true;
        }
        
        private static void HandleMoveAssociationRequest(DcmPresContext pc)
        {
            if (pc.AbstractSyntax == DicomUID.StudyRootQueryRetrieveInformationModelMOVE)
            {
                AcceptStandardTransferSyntaxes(pc);
            }
        }


        private static void HandleFindAssociationRequest(DcmPresContext pc)
        {
            if (pc.AbstractSyntax == DicomUID.StudyRootQueryRetrieveInformationModelFIND)
            {
                AcceptStandardTransferSyntaxes(pc);
            }
        }

        private static void HandleEchoAssociationRequest(DcmPresContext pc)
        {
            if (pc.AbstractSyntax == DicomUID.VerificationSOPClass)
            {
                AcceptStandardTransferSyntaxes(pc);
            }
        }


        private static void AcceptStandardTransferSyntaxes(DcmPresContext pc)
        {
            if (pc.HasTransfer(DicomTransferSyntax.ImplicitVRLittleEndian))
            {
                pc.SetResult(DcmPresContextResult.Accept, DicomTransferSyntax.ImplicitVRLittleEndian);
            }
            else if (pc.HasTransfer(DicomTransferSyntax.ExplicitVRLittleEndian))
            {
                pc.SetResult(DcmPresContextResult.Accept, DicomTransferSyntax.ExplicitVRLittleEndian);
            }
            else if (pc.HasTransfer(DicomTransferSyntax.ExplicitVRBigEndian))
            {
                pc.SetResult(DcmPresContextResult.Accept, DicomTransferSyntax.ExplicitVRBigEndian);
            }
        }

        protected override void OnReceiveCEchoRequest(byte presentationID, ushort messageID, Dicom.Network.DcmPriority priority)
        {
            Trace.WriteLine(String.Format("Receive C-Echo from {0} (marked as anonymous:{1})", this.Associate.CallingAE, _flagAnonymousAccess));
            SendCEchoResponse(presentationID, messageID, DcmStatus.Success);
        }

        protected override void OnReceiveCFindRequest(byte presentationID, ushort messageID, DcmPriority priority, Dicom.Data.DcmDataset query)
        {
            Trace.WriteLine(String.Format("{0} Receive C-Find from {1} (marked as anonymous:{2})", DateTime.Now, this.Associate.CallingAE, _flagAnonymousAccess));
            Trace.WriteLine(query.Dump());
            
            using( var database = new MedicalISDataContext() )
            {
                var queryLevel = query.GetString(DicomTags.QueryRetrieveLevel, null);

                if (queryLevel == "PATIENT")
                {
                    IQueryable<Patient> patients = PatientQueries.GetMatchingPatients(database, query, _flagAnonymousAccess);

                    patients = patients.Take(Settings.Default.MaxNumberOfStudiesReturned);

                    foreach (var currentPatient in patients)
                    {
                        foreach (var currentStudy in currentPatient.Studies)
                        {
                            var p = currentPatient;

                            var response = new DcmDataset
                            {
                                SpecificCharacterSetEncoding = query.SpecificCharacterSetEncoding
                            };

                            // Map saved study tags to output

                            response.AddElementWithValue(DicomTags.RetrieveAETitle, "CURAPACS");
                            response.AddElementWithValue(DicomTags.QueryRetrieveLevel, "PATIENT");

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

                            if (_flagAnonymousAccess)
                                AnonymizeDatasetBasedOnStudyInfo(response);

                            SendCFindResponse(presentationID, messageID, response, DcmStatus.Pending);
                        }
                    }
                }
                else if (queryLevel == "STUDY")
                {
                    IQueryable<Study> studies = StudyQueries.GetMatchingStudies(database, query, _flagAnonymousAccess);

                    studies = studies.Take(Settings.Default.MaxNumberOfStudiesReturned);

                    foreach (var currentStudy in studies)
                    {
                        var p = currentStudy.Patient;

                        var response = new DcmDataset
                                           {
                                               SpecificCharacterSetEncoding = query.SpecificCharacterSetEncoding
                                           };

                        // Map saved study tags to output

                        response.AddElementWithValue(DicomTags.RetrieveAETitle, "CURAPACS");
                        response.AddElementWithValue(DicomTags.QueryRetrieveLevel, "STUDY");

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

                        if(_flagAnonymousAccess)
                            AnonymizeDatasetBasedOnStudyInfo(response);

                        SendCFindResponse(presentationID, messageID, response, DcmStatus.Pending);
                    }
                }
                else if (queryLevel == "SERIES")
                {
                    IQueryable<Series> series = SeriesQueries.GetMatchingSeries(database, query);

                    foreach (var currentSeries in series)
                    {
                        var response = new DcmDataset
                        {
                            SpecificCharacterSetEncoding = query.SpecificCharacterSetEncoding
                        };
                        
                        if (currentSeries.PerformedDateTime.HasValue)
                        {
                            response.AddElementWithValue(DicomTags.SeriesDate, currentSeries.PerformedDateTime.Value);
                            response.AddElementWithValue(DicomTags.SeriesTime, currentSeries.PerformedDateTime.Value);
                        }

                        response.AddElementWithValue(DicomTags.QueryRetrieveLevel, "SERIES");

                        response.AddElementWithValue(DicomTags.StudyInstanceUID, currentSeries.StudyInstanceUid);
                        response.AddElementWithValue(DicomTags.SeriesInstanceUID, currentSeries.SeriesInstanceUid);
                        response.AddElementWithValue(DicomTags.SeriesNumber, currentSeries.SeriesNumber);
                        response.AddElementWithValue(DicomTags.SeriesDescription, currentSeries.Description);
                        response.AddElementWithValue(DicomTags.Modality, currentSeries.PerformedModalityType);
                        
                        response.AddElementWithValue(DicomTags.NumberOfSeriesRelatedInstances, currentSeries.Images.Count());
                        response.AddElementWithValue(DicomTags.ReferringPhysiciansName, "");
                        response.AddElementWithValue(DicomTags.StudyCommentsRETIRED, "");

                        Trace.WriteLine("response > ");
                        Trace.WriteLine(response.Dump());

                        SendCFindResponse(presentationID, messageID, response, DcmStatus.Pending);
                    }

                }

                SendCFindResponse(presentationID, messageID, DcmStatus.Success);
            }
        }

        protected override void OnReceiveCStoreRequest(byte presentationID, ushort messageID, DicomUID affectedInstance,
            DcmPriority priority, string moveAE, ushort moveMessageID, DcmDataset dataset, string fileName)
        {
            Trace.WriteLine(String.Format("{0} Receive C-Store from {1} ", DateTime.Now, this.Associate.CallingAE));
           
            DcmStatus status = DcmStatus.Success;

            SendCStoreResponse(presentationID, messageID, affectedInstance, status);
        }


        private void MoveReceivedFilesIntoCorrectFolder()
        {
            string storageFolder = Settings.Default.StandardReceiveFolder;
            
            if (ImageCountOnConnection >= Settings.Default.BatchImportSizeLimitForSecondaryPriority)
            {
                storageFolder = Settings.Default.SecondaryPriorityStorageFolder;
            }

            while( MoveQueue.Count > 0)
            {
                string path = MoveQueue.Dequeue();

                string fileName = Path.GetFileName(path);
                string newPath = Path.Combine(storageFolder, fileName);

                File.Move(path, newPath);
            } 
        }

        protected override void OnReceiveDimse(byte pcid, DcmCommand command, DcmDataset dataset, DcmDimseProgress progress)
        {
            var now = DateTime.Now;

            Trace.WriteLine(String.Format("{0} Receive DIMSE {1} from {2} ", now, dataset.GetString(DicomTags.Modality, "UN"), this.Associate.CallingAE));
            ImageCountOnConnection++;

            bool isFiltered = IsFilteredOutData(dataset);

            if (isFiltered && !Settings.Default.ReceiveFilter_Keep)
                return;

            var fileName = GetFileNameForDicomDataset(dataset);
            string filePath;

            if (!isFiltered)
            {
                filePath = DefineStorageLocationOfFile(fileName);
            }
            else
            {
                filePath = Path.Combine(Settings.Default.ReceiveFilter_Folder, fileName);
            }

            Trace.WriteLine(String.Format("{0} Saving DIMSE {1} from {2} to {3} ", now, dataset.GetString(DicomTags.Modality, "UN"), this.Associate.CallingAE, filePath));
          
            var ff = new DicomFileFormat(dataset);
            ff.Save( filePath, DicomWriteOptions.Default);
        }

        private bool IsFilteredOutData(DcmDataset dataset)
        {
            if (!HasReceiveFilters)
            {
                return false;
            }

            var imageType = dataset.GetString(DicomTags.ImageType, "");

            if (IsFilteredOut(imageType, ReceiveFilter_ImageTypes))
            { 
                Trace.WriteLine(String.Format("{0} Filtered DIMSE {1} from {2} because of imageType:{3}", DateTime.Now, dataset.GetString(DicomTags.Modality, "UN"), this.Associate.CallingAE, imageType));
                return true;
            }

            var seriesDescription = dataset.GetString(DicomTags.SeriesDescription, "");

            if (IsFilteredOut(seriesDescription, ReceiveFilter_SeriesDescription))
            {
                Trace.WriteLine(String.Format("{0} Filtered DIMSE {1} from {2} because of seriesDescription:{3}", DateTime.Now, dataset.GetString(DicomTags.Modality, "UN"), this.Associate.CallingAE, seriesDescription));
                return true;
            }

            return false;
        }

        private static bool IsFilteredOut(string text, IEnumerable<Regex> ReceiveFilter_ImageTypes)
        {
            return ReceiveFilter_ImageTypes.Any(filter => filter.Match(text).Success);
        }

        private static string GetFileNameForDicomDataset(DcmDataset dataset)
        {
            return dataset.GetString(DicomTags.Modality, "UN") + "." + dataset.GetString(DicomTags.SOPInstanceUID, "UnknownSOPInstanceUID") + ".dcm";
        }

        private string DefineStorageLocationOfFile(string fileName)
        {
            bool fileMovePending = true;
            string storageFolder = Settings.Default.TemporaryReceiveFolder;
            
            if (ImageCountOnConnection >= Settings.Default.BatchImportSizeLimitForSecondaryPriority)
            {
                storageFolder = Settings.Default.SecondaryPriorityStorageFolder;
                fileMovePending = false;
            }
            
            string filePath = Path.Combine(storageFolder, fileName);

            if (fileMovePending)
            {
                MoveQueue.Enqueue(filePath);
            }

            return filePath;
        }

        private static bool QueryHasPatientSpecificFilters(DcmDataset query)
        {
            return query.Elements.Any(e => e.Tag.Group == 0x0010 && !String.IsNullOrWhiteSpace(query.GetElement(e.Tag).GetValueString()) );
        }

        protected override void OnReceiveCMoveRequest(byte presentationID, ushort messageID, string destinationAE, DcmPriority priority, DcmDataset query)
        {
            Console.WriteLine(String.Format("{0} Receive C-Move from {1} (marked as anonymous:{2})", DateTime.Now, this.Associate.CallingAE, _flagAnonymousAccess));
            Debug.WriteLine(query.Dump());

            AEInfo recipientInfo = FindAE(destinationAE);

            if (recipientInfo == null)
            {
                SendCMoveResponse(presentationID, messageID, DcmStatus.QueryRetrieveMoveDestinationUnknown, 0, 0, 0, 0);
                Console.Write("Moving to:" + recipientInfo.Name + "> ");

                return;
            }

            Console.Write("Moving to:" + recipientInfo.Name + "> ");

            ushort imagesProcessed = 0;
            ushort errorCount = 0;
            ushort successCount = 0;

            try
            {

                using (var database = new MedicalISDataContext())
                {
                    var storeClient = new CStoreClient();
                    storeClient.CallingAE = "CURAPACS";
                    storeClient.CalledAE = destinationAE;
                    storeClient.PreferredTransferSyntax = recipientInfo.TransferSyntax;
                    storeClient.PreloadCount = 10;
                    storeClient.LogID = "store";

                    if (Settings.Default.DebugMove)
                    Dicom.Debug.InitializeConsoleDebugLogger();

                    if (_flagAnonymousAccess)
                        storeClient.DisableFileStreaming = true;


                    var files = GetFilePaths(database, query);

                    if (Settings.Default.UseFixedFolder)
                    {
                        files = Directory.GetFiles(Settings.Default.FixedFolderPath);
                    }

                    long totalSize = 0;

                    foreach (var f in files)
                    {
                        totalSize += new FileInfo(f).Length;
                        storeClient.AddFile(f);
                    }

                    var totalImageCount = (ushort) files.Count();

                    Console.Write("#{0} files > ", totalImageCount);

                    Stopwatch timer = Stopwatch.StartNew();
                    
                    
                    storeClient.OnCStoreRequestBegin = (c, i) =>
                                                           {
                                                               if (_flagAnonymousAccess)
                                                               {
                                                                   var dataset = i.Dataset;

                                                                   AnonymizeDatasetBasedOnStudyInfo(dataset);
                                                               }
                                                           };

                    storeClient.OnCStoreRequestComplete = (c, i) =>
                                                                {
                                                                  timer.Stop();
 
                                                                  if (i.HasError)                                                                 
                                                                  {
                                                                      Console.WriteLine("Error:{0}", i.Error);
                                                                  }
                                                                  else
                                                                  {
                                                                  //    Console.WriteLine("Done");
                                                                  }

                                                                  imagesProcessed++;
                                                              };

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

                                                               };

                    storeClient.OnCStoreRequestProgress = (c, i, p) =>
                                                              {
                                                                  //Console.WriteLine("{0}@{1}", p.BytesTransfered, c.Socket.LocalStats.ToString());
                                                              };

                    storeClient.OnCStoreRequestFailed = (c, i) =>
                                                            {
                                                                Console.WriteLine("");
                                                                Console.WriteLine("Failed. " + i.Error.ToString());
                                                            };

                    storeClient.Connect(recipientInfo.Ip, recipientInfo.Port, DcmSocketType.TCP);

                    if (storeClient.Wait())
                    {

                    }
                
                    timer.Stop();

                    
                    double bytesPerSeconds = (double) totalSize/timer.Elapsed.TotalSeconds;
                    double MiBPerSecond = bytesPerSeconds/(1024.0*1024.0);

                    Console.WriteLine("");
                    Console.WriteLine("Done. {0} MiB/s (ImagesProcessed={1})", MiBPerSecond, imagesProcessed);

                    SendCMoveResponse(presentationID, messageID, DcmStatus.Success, 0, imagesProcessed, 0, 0);
                    storeClient.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("");
                Console.WriteLine("Failed. => " + ex.ToString());

                SendCMoveResponse(presentationID, messageID, DcmStatus.ProcessingFailure, 0, imagesProcessed, 0, 0);

            }
        }

        private static void AnonymizeDatasetBasedOnStudyInfo(DcmDataset dataset)
        {
            var studyDate = dataset.GetDateTime(DicomTags.StudyDate, DicomTags.StudyTime, new DateTime(1800,1,1));
            var anonymizedName = studyDate.ToString("yyyyMMdd_hhmmss");

            dataset.SetString(DicomTags.PatientsName, anonymizedName);
            dataset.SetString(DicomTags.PatientID, "");
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
                    {
                        ip = ip.Substring(0, ip.IndexOf(':'));
                    }
                    
                    int port = 104;
                    
                    if (address.Contains(':'))
                    {
                        var portPart = address.Substring(address.IndexOf(':') + 1);

                        if (portPart.Contains(','))
                            portPart = portPart.Substring(0, portPart.IndexOf(','));

                        port = int.Parse(portPart);
                    }

                    DicomTransferSyntax ts = DicomTransferSyntax.ExplicitVRLittleEndian;

                    if (address.Contains(','))
                    {
                        var tsPart = address.Substring(address.IndexOf(',') + 1);

                        ts = FindTransferSyntaxByName(tsPart);
                    }

                    return new AEInfo()
                               {
                                   Name = name,
                                   Ip = ip,
                                   Port = port,
                                   TransferSyntax = ts
                               };
                }
            }


            return null;
        }

        private static DicomTransferSyntax FindTransferSyntaxByName(string tsPart)
        {
            switch(tsPart.ToLower().Trim())
            {
                case "implicitvrlittleendian":
                    return DicomTransferSyntax.ImplicitVRLittleEndian;
                case "explicitvrlittleendian":
                    return DicomTransferSyntax.ExplicitVRLittleEndian;
                case "explicitvrbigendian":
                    return DicomTransferSyntax.ExplicitVRBigEndian;
                case "jpeg2000lossless":
                    return DicomTransferSyntax.JPEG2000Lossless;
                case "jpeg2000lossy":
                    return DicomTransferSyntax.JPEG2000Lossy;
                case "jpeglslossless":
                    return DicomTransferSyntax.JPEGLSLossless;
                case "jpeglsnearlossless":
                    return DicomTransferSyntax.JPEGLSNearLossless;
                case "jpegprocess1":
                    return DicomTransferSyntax.JPEGProcess1;
                case "jpegprocess14":
                    return DicomTransferSyntax.JPEGProcess14;
                case "jpegprocess14sv1":
                    return DicomTransferSyntax.JPEGProcess14SV1;
                case "jpegprocess2_4":
                    return DicomTransferSyntax.JPEGProcess2_4;
                default:
                    return DicomTransferSyntax.ExplicitVRLittleEndian;
            }
        }

        private static IEnumerable<string> GetFilePaths(MedicalISDataContext database, DcmDataset query)
        {
            if ( ! String.IsNullOrWhiteSpace(query.GetString(DicomTags.SeriesInstanceUID, "")) )
            {
                string seriesInstanceUid = query.GetString(DicomTags.SeriesInstanceUID,"");

                var imagePaths = from i in database.Images
                                 where i.SeriesInstanceUid == seriesInstanceUid
                                 select Path.Combine(Settings.Default.RootPath, i.ArchivedStorageLocation);

                return imagePaths;
            }
            else if ( !String.IsNullOrWhiteSpace(query.GetString(DicomTags.StudyInstanceUID, "")))
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
    
        private bool _flagAnonymousAccess = false;
        private int ImageCountOnConnection;
        private Stopwatch Timer;
        private Queue<string> MoveQueue = new Queue<string>();
        private static bool IsReceiveConnection;

        public bool HasReceiveFilters
        {
            get
            {
                return
                    ReceiveFilter_ImageTypes != null && ReceiveFilter_SeriesDescription != null;
            }
        }

        public void BuildReceiveFilters()
        {
            ReceiveFilter_ImageTypes = BuildFilters(Settings.Default.ReceiveFilter_ImageType);
            ReceiveFilter_SeriesDescription = BuildFilters(Settings.Default.ReceiveFilter_SeriesDescription);
        }

        private List<Regex>  BuildFilters(string concatenatedFilterString)
        {
            var filterExpressions = new List<Regex>();

            if (!String.IsNullOrWhiteSpace(concatenatedFilterString))
            {
                var filters = concatenatedFilterString.Split(';');

                foreach (var filter in filters)
                {
                    var expression = new Regex(Settings.Default.ReceiveFilter_ImageType,
                                               RegexOptions.Compiled | RegexOptions.Singleline);

                    filterExpressions.Add(expression);
                }
            }

            return filterExpressions;
        }

        private List<Regex> ReceiveFilter_ImageTypes;
        private List<Regex> ReceiveFilter_SeriesDescription;
    }
}