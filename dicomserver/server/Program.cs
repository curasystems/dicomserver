using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

using Dicom;
using Dicom.Imaging;
using Dicom.Imaging.LUT;
using Dicom.Imaging.Render;
using Dicom.Network;
using Dicom.Network.Server;
using System.Globalization;
using dropkick.Wmi;
using server.Properties;
using dropkick.Configuration;
using dropkick.Configuration.Dsl.WinService;
    
namespace server
{
    public class Config : DropkickConfiguration
    {
        public Config()
        {
            ServiceName = "CuraSystems DICOM Service";
        }

        public string ServiceName { get; set; }    
    }

    public class ServiceDeployment : dropkick.Configuration.Dsl.Deployment<ServiceDeployment, Config>
    {
        public ServiceDeployment()
        {
            Define( settings =>
                DeploymentStepsFor( Standard, server =>
                    {
                        server.WinService(base.Settings.ServiceName)
                            .Create()
                            .WithServicePath(Assembly.GetEntryAssembly().Location)
                            .WithStartMode(ServiceStartMode.Automatic);
                    }
                ));
        }

        public dropkick.Configuration.Dsl.Role Standard { get; set; }
    }


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
