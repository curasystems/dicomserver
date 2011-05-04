using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using Dicom.Network;
using Dicom.Network.Server;

namespace cmove
{
    class Program
    {
        static void Main(string[] args)
        {
            var seriesUidArg = args.FirstOrDefault(a => a.StartsWith("/seriesuid="));

            StartStorageServer(10108);

            //if( !String.IsNullOrEmpty(seriesUidArg) )
            {
                var seriesUid = "1.3.12.2.1107.5.2.32.35322.2010082109092032370711447.0.0.0";

                if( !String.IsNullOrEmpty(seriesUidArg) )
                    seriesUid = seriesUidArg.Split('=')[1];

                TransferSeries(seriesUid);
            }

            StopStorageServer();
        }

        private static void StopStorageServer()
        {
            _storageServer.Stop();
        }

        private static void StartStorageServer(int port)
        {
            _storageServer = new Dicom.Network.Server.DcmServer<CStoreService>();
            _storageServer.AddPort(port, DcmSocketType.TCP);
            _storageServer.OnDicomClientCreated = (s, c, t) =>
            {
                c.UseFileBuffer = false;
                //c.OnCStoreRequestProgress +=
                //    (client, pcid, command, dataset, progress) =>
                //        {
                //         //   Console.WriteLine("{0} {1}%",progress.BytesTransfered,
                //         //                     (double)progress.BytesTransfered/progress.EstimatedBytesTotal);

                //        };
            };
            _storageServer.Start();
        }

        private static void TransferSeries(string seriesUid)
        {
            var moveClient = new Dicom.Network.Client.CMoveClient();
            moveClient.DestinationAE = callingAE;
            moveClient.CallingAE = callingAE;
            moveClient.CalledAE = calledAE;
            moveClient.AddQuery(DcmQueryRetrieveLevel.Series, seriesUid);
            
            moveClient.OnCMoveResponse += (query, dataset, status, remain, complete, warning, failure) =>
                                              {
                                                  if( status == DcmStatus.Pending )
                                                        Console.WriteLine("{0}/{1} (warn:{2}, fail:{3})", complete, remain, warning,failure);
                                                  else if( status == DcmStatus.Success )
                                                  {
                                                      Console.WriteLine("{0}/{1} (warn:{2}, fail:{3})", complete, remain, warning,failure);
                                                      Console.WriteLine("Completed.");
                                                  }
                                                  else
                                                      Console.WriteLine("Status:{0}", status);
                                              };

            moveClient.Connect(target.Address.ToString(), target.Port, DcmSocketType.TCP);
            moveClient.Wait();
        }

        private static IPEndPoint target = new IPEndPoint( IPAddress.Parse("10.32.34.37"), 10105);
        private static string callingAE = "CURACLIENT";
        private static string calledAE = "CURAPACS";
        private static DcmServer<CStoreService> _storageServer;
    }
}
