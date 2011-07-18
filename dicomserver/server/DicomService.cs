using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using Dicom.Network.Server;
using server.Properties;
using Dicom.Network;

namespace server
{
    partial class DicomService : ServiceBase
    {
        public DicomService()
        {
            InitializeComponent();
        }

        public void Start(string[] args)
        {
            Console.WriteLine("started on console...");
            this.OnStart(args);
        }

        public void Stop()
        {
            this.OnStop();
        }

        protected override void OnStart(string[] args)
        {
            server = new DcmServer<CImageServer>();
        
            //server.OnDicomClientCreated = (a, s, t) => { s.ThrottleSpeed = 0; };
            
            ConfigureServerToListenOnAllPorts();

            server.Start();

            LogStartup();
        }

        void LogStartup()
        {
            if (isRunningOnConsole)
                Console.WriteLine("Listening on " + Settings.Default.ListenPorts);
            else
                Trace.WriteLine("Listening on " + Settings.Default.ListenPorts);
        }

        void ConfigureServerToListenOnAllPorts()
        {
            var listenPorts = GetListenPorts();
            
            foreach( var p in listenPorts )
                server.AddPort(p, DcmSocketType.TCP);
        }

        static IEnumerable<ushort> GetListenPorts()
        {
            var setting = Settings.Default.ListenPorts ?? "104";
            return from p in setting.Split(',', ';') select UInt16.Parse(p);
        }

        protected override void OnStop()
        {
            server.Stop();
        }

        DcmServer<CImageServer> server;
        public bool isRunningOnConsole = false;
    }
}
