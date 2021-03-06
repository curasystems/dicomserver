﻿using System;
using System.Net;
using Dicom.Network;
using Dicom.Network.Client;

namespace cmove
{
    public class DicomStudyMover
    {
        const string _callingAE = "CURAPACS";
        string _serverAE = "CURAPACS";
        IPEndPoint _serverAddress;

        public DicomStudyMover(string serverAddress)
        {
            ParseServerAddress(serverAddress);
        }

        void ParseServerAddress(string address)
        {
            string[] parts = address.Split(new[] {'@'}, 2);

            if (parts.Length != 2)
            {
                Console.WriteLine("Server Address must be in format <serverae>@<serverip>:<port>");

                throw new InvalidOperationException();
            }

            _serverAE = parts[0];
            _serverAddress = ParseIPAddress(parts[1]);
        }

        static IPEndPoint ParseIPAddress(string address)
        {
            string[] parts = address.Split(new[] {':'}, 2);

            if (parts.Length == 1)
            {
                return new IPEndPoint(IPAddress.Parse(parts[0]), 104);
            }
            else
            {
                return new IPEndPoint(IPAddress.Parse(parts[0]), UInt16.Parse(parts[1]));
            }
        }

        public void TransferStudy(string studyUid, string targetAE)
        {
            Console.WriteLine("Initiating C-MOVE of {0} from {1} to {2}...", studyUid, _serverAddress, targetAE);

            var moveClient = new CMoveClient();
            moveClient.DestinationAE = targetAE;
            moveClient.CallingAE = _callingAE;
            moveClient.CalledAE = _serverAE;

            moveClient.AddQuery(DcmQueryRetrieveLevel.Study, studyUid);
            
            moveClient.OnCMoveResponse += (query, dataset, status, remain, complete, warning, failure) =>
                {
                    if (status == DcmStatus.Pending)
                    {
                        double progress = (double) complete / (double)(complete + remain);

                        Console.WriteLine("%{4} - {0}/{1} (warn:{2}, fail:{3})", complete, remain, warning, failure, GetAsciiArtForProgress(progress, 10));

                        Console.CursorVisible = false;
                        Console.CursorLeft = 0;
                        Console.CursorTop -= 1;
                    }
                    else if (status == DcmStatus.Success)
                    {
                        Console.WriteLine("{0}/{1} (warn:{2}, fail:{3})", complete, remain, warning, failure);
                        Console.WriteLine("Completed.");
                    }
                    else
                        Console.WriteLine("Status:{0}", status);
                };

            moveClient.Connect(_serverAddress.Address.ToString(), _serverAddress.Port, DcmSocketType.TCP);
            moveClient.Wait();

            Console.CursorVisible = true;
        }

        private object GetAsciiArtForProgress(double progress, int hashCount, char doneChar = '#', char notDoneChar = '_')
        {
            int hashesToShow = (int) Math.Floor(progress*hashCount);

            return new String(doneChar, hashesToShow) + new string(notDoneChar, hashCount - hashesToShow);
        }
    }
}