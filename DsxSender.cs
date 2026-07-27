using RacingDSX.Config;
using RacingDSX.DSX;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using static RacingDSX.RacingWorker;

namespace RacingDSX
{
    public class DsxSender : IDataSender
    {
        private readonly RacingWorker racingWorker;

        public DsxSender(RacingWorker racingWorker)
        {
            this.racingWorker = racingWorker;
        }

        protected static readonly JsonSerializerOptions jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };


        //Send Data to DSX
        public void Send(Packet data)
        {
            var settings = racingWorker.settings;
            var progressReporter = racingWorker.progressReporter;

            if (settings.VerboseLevel > VerboseLevel.Limited && progressReporter != null)
            {
                progressReporter.Report(new RacingReportStruct($"Converting Message to JSON"));
            }

            try
            {
                string jsonString = JsonSerializer.Serialize(data, jsonOptions);
                byte[] RequestData = Encoding.ASCII.GetBytes(jsonString);

                if (settings.VerboseLevel > VerboseLevel.Limited && progressReporter != null)
                {
                    progressReporter.Report(new RacingReportStruct($"{Encoding.ASCII.GetString(RequestData)}"));
                }

                if (settings.VerboseLevel > VerboseLevel.Limited && progressReporter != null)
                {
                    progressReporter.Report(new RacingReportStruct($"Sending Message to DSX..."));
                }

                senderClient.Send(RequestData, RequestData.Length);

                if (settings.VerboseLevel > VerboseLevel.Limited && progressReporter != null)
                {
                    progressReporter.Report(new RacingReportStruct($"Message sent to DSX"));
                }
            }
            catch (JsonException je)
            {
                if (progressReporter != null)
                {
                    progressReporter.Report(new RacingReportStruct($"JSON Serialization Error: {je.Message}"));
                }
            }
            catch (Exception e)
            {
                if (progressReporter != null)
                {
                    progressReporter.Report(new RacingReportStruct("Error Sending Message: " + e.Message));
                }

                if (e is SocketException)
                {
                    if (progressReporter != null)
                    {
                        progressReporter.Report(new RacingReportStruct("Couldn't Access Port. " + e.Message));
                    }
                    throw;
                }
                else if (e is ObjectDisposedException)
                {
                    if (progressReporter != null)
                    {
                        progressReporter.Report(new RacingReportStruct("Connection closed. Restarting..."));
                    }
                    Connect();
                }
                else
                {
                    if (progressReporter != null)
                    {
                        progressReporter.Report(new RacingReportStruct("Unknown Error: " + e.Message));
                    }
                }
            }
        }

        // Connect to DSX
        public void Connect()
        {
            var settings = racingWorker.settings;
            var progressReporter = racingWorker.progressReporter;

            senderClient = new UdpClient();
            var portNumber = settings.DSXPort;

            if (progressReporter != null)
            {
                progressReporter.Report(new RacingReportStruct("DSX is using port " + portNumber + ". Attempting to connect.."));
            }

            if (!int.TryParse(portNumber.ToString(), out int portNum))
            {
                if (progressReporter != null)
                {
                    progressReporter.Report(new RacingReportStruct($"DSX provided a non-numerical port! Using configured default ({settings.DSXPort})."));
                }
                portNum = (int)settings.DSXPort;
            }

            endPoint = new IPEndPoint(IPAddress.Loopback, portNum);

            try
            {
                senderClient.Connect(endPoint);
            }
            catch (Exception e)
            {
                if (progressReporter != null)
                {
                    progressReporter.Report(new RacingReportStruct("Error connecting: " + e.Message));

                    if (e is SocketException)
                    {
                        progressReporter.Report(new RacingReportStruct("Couldn't access port. " + e.Message));
                    }
                    else if (e is ObjectDisposedException)
                    {
                        progressReporter.Report(new RacingReportStruct("Connection object closed. Restart the application."));
                    }
                    else
                    {
                        progressReporter.Report(new RacingReportStruct("Unknown error: " + e.Message));
                    }
                }
            }
        }

        public void Stop()
        {
            if (senderClient != null)
            {
                senderClient.Close();
                senderClient.Dispose();
            }
        }

        private static UdpClient senderClient;
        private static IPEndPoint endPoint;
    }
}