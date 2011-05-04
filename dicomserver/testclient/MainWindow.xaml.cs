using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

//using Dicom;
//using Dicom.Network;

//using Dicom.Data;

namespace testclient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            var client = new Dicom.Network.Client.CFindStudyClient
                             {
                                 CallingAE = "CURATEST", CalledAE = "CURAPACS"
                             };

            var q = new Dicom.Network.Client.CFindStudyQuery();
                        //{
                        //    StudyDate = new DcmDateRange(DateTime.Now.Subtract(TimeSpan.FromDays(1)))
                        //};

            client.AddQuery( q );
            client.Connect("127.0.0.1", 10105, Dicom.Network.DcmSocketType.TCP);
            
            client.OnCFindResponse = (oq, r) =>
                                         {
                                             listBox1.Items.Add(r);
                                         };
            client.OnCFindComplete = (oq, r) =>
                                         {
                                             
                                         };


        }
    }
}
