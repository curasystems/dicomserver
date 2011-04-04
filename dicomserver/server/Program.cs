using System;
using System.Linq.Expressions;
using System.Text;

using Dicom;
using Dicom.Imaging;
using Dicom.Imaging.LUT;
using Dicom.Imaging.Render;
using Dicom.Network;
using Dicom.Network.Server;
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

            Console.WriteLine("Listening on " + Settings.Default.ListenPort);
            Console.ReadKey();

            server.Stop();
        }
    }
}
