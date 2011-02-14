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

            //if (ipEndpoint != null)
            //{
            //    if (ipEndpoint.Address.Equals(IPAddress.Parse("192.168.2.53")) )
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

        protected override void OnReceiveCFindRequest(byte presentationID, ushort messageID, DcmPriority priority, Dicom.Data.DcmDataset dataset)
        {
            Trace.WriteLine(dataset.Dump());

            string queryLevel = dataset.GetString(DicomTags.QueryRetrieveLevel, null);

            var file = new DicomFileFormat();
            //file.Load("Ct.dcm", DicomTags.PixelData, DicomReadOptions.None);
            file.Load("Ct.dcm", DicomReadOptions.None);

            

            //var pixelData = new DcmPixelData(file.Dataset);
            //var frameData = pixelData.GetFrameDataU16(0);

            //var data = new GrayscalePixelDataU16(pixelData.ImageWidth, pixelData.ImageHeight, frameData);
            //var lut = WindowLevel.FromDataset(file.Dataset);

            //var g = new ImageGraphic(data);
            //var i = g.RenderImage( new ( pixelData.MinimumDataValue, pixelData.MaximumDataValue, lut[0] ));
            //i.Save("test.jpg");
            

            var response = file.Dataset;
            response.Remove(DicomTags.PixelData);
            response.AddElementWithValueString(DicomTags.RetrieveAETitle, "CURAPACS");

            if ( queryLevel == "STUDY")
            { 
                var d = dataset.GetElement(DicomTags.StudyDate);
                var t = dataset.GetElement(DicomTags.StudyTime);

                //var rangeQuery = new DateTimeRangeQuery(d.GetValueString(), t.GetValueString());

                
                //var response = new DcmDataset(DicomTransferSyntax.ExplicitVRLittleEndian);
                response.AddElementWithValue( DicomTags.NumberOfStudyRelatedSeries, 1 );
                response.AddElementWithValue( DicomTags.NumberOfStudyRelatedInstances, 1);
            
                SendCFindResponse(presentationID, messageID, response, DcmStatus.Pending);

                //response.StudyInstanceUID = "2.233.45345.234234.234234.234";
            }
            else if (queryLevel == "SERIES")
            {
                SendCFindResponse(presentationID, messageID, response, DcmStatus.Pending);

                
            }

            SendCFindResponse(presentationID, messageID, DcmStatus.Success);
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
