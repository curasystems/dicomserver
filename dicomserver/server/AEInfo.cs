using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dicom.Data;

namespace server
{
    class AEInfo
    {
        public AEInfo()
        {
            TransferSyntax = DicomTransferSyntax.ExplicitVRLittleEndian;
        }

        public string Name { get; set; }

        public string Ip { get; set; }

        public int Port { get; set; }

        public DicomTransferSyntax TransferSyntax { get; set; }
    }
}
