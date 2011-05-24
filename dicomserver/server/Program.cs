using System;
using System.Linq;
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
using System.ServiceProcess;
    
namespace server
{
    //public class Config : DropkickConfiguration
    //{
    //    public Config()
    //    {
    //        ServiceName = "CuraSystems DICOM Service";
    //    }

    //    public string ServiceName { get; set; }    
    //}

    //public class ServiceDeployment : dropkick.Configuration.Dsl.Deployment<ServiceDeployment, Config>
    //{
    //    public ServiceDeployment()
    //    {
    //        Define( settings =>
    //            DeploymentStepsFor( Standard, server =>
    //                {
    //                    server.WinService(base.Settings.ServiceName)
    //                        .Create()
    //                        .WithServicePath(Assembly.GetEntryAssembly().Location)
    //                        .WithStartMode(ServiceStartMode.Automatic);
    //                }
    //            ));
    //    }

    //    public dropkick.Configuration.Dsl.Role Standard { get; set; }
    //}


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
            if (args.Any(s=> s == "/console"))
            {
                var dicomService = new DicomService();
                dicomService.isRunningOnConsole = true;
                dicomService.Start(args);

                Console.ReadLine();

                dicomService.Stop();
            }
            else
            {
                ServiceBase[] ServicesToRun;

                ServicesToRun = new ServiceBase[]
                                    {
                                        new DicomService()
                                    };
                ServiceBase.Run(ServicesToRun);

            }
        }
    }
}
