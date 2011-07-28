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
            if (args.Length < 3)
            {
                Console.WriteLine("cmove <serverae>@<serverip>:<port> <studyuid> <targetae>");
                return;
            }

            var mover = new DicomStudyMover(args[0]);
            mover.TransferStudy( args[1], args[2] );
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

        private static IPEndPoint target = new IPEndPoint( IPAddress.Parse("10.32.34.37"), 10105);
        private static DcmServer<CStoreService> _storageServer;
    }
}
