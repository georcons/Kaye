using System;
using System.Net;
using System.Text;
using System.Threading;
using System.Net.Sockets;

namespace Kaye.Net
{
    public class HTTPSender
    {
        private IPAddress IPAddress;
        private int Port;
        private string Hostname;

        public HTTPSender(IPAddress IPAddress, int Port, string Hostname)
        {
            this.IPAddress = IPAddress;
            this.Port = Port;
            this.Hostname = Hostname;
        }

        public HTTPSender(IPAddress IPAddress, int Port) : this(IPAddress, Port, IPAddress.ToString())
        { }

        public HTTPSender(IPEndPoint iep) : this(iep.Address, iep.Port)
        { }

        public byte[] DownloadBytes(string directory)
        {
            byte[] output = new byte[0];
            Socket s = new Socket(IPAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            IPEndPoint iep = new IPEndPoint(this.IPAddress, this.Port);
            bool TimedOut = false, Connected = false;
            Thread ConnectThread = new Thread(() =>
            {
                try
                {
                    s.Connect(iep);
                    Connected = true;
                }
                catch { TimedOut = true; }
            });
            Thread TimeOutThread = new Thread(() =>
            {
                Thread.Sleep(1000);
                if (!Connected) TimedOut = true;
            });
            ConnectThread.Start();
            TimeOutThread.Start();
            while (!Connected && !TimedOut)
            {
                // WAIT
            }
            if (TimedOut) return new byte[0];
            s.ReceiveTimeout = 700;
            string request = "GET " + (directory.StartsWith("/") ? directory : "/" + directory) + " HTTP/1.1\r\nHost: " + Hostname + "\r\nConnection: close\r\n\r\n";
            s.Send(Encoding.ASCII.GetBytes(request));
            int i;
            byte[] buffer = new byte[(int)Math.Pow(2, 16)];
            try
            {
                while ((i = s.Receive(buffer)) != 0)
                {
                    byte[] NewOutput = new byte[output.Length + i];
                    for (int j = 0; j < output.Length; j++) NewOutput[j] = output[j];
                    for (int j = output.Length; j < NewOutput.Length; j++) NewOutput[j] = buffer[j - output.Length];
                    output = NewOutput;
                }
            }
            catch { }
            return output;
        }

        public string DownloadString(string directory)
        {
            byte[] Bytes = this.DownloadBytes(directory);
            return Encoding.UTF8.GetString(Bytes).Replace(((char)7).ToString(), " ");
        }

        public int DownloadCode(string directory)
        {
            string Str = this.DownloadString(directory);
            string[] Parts = Str.Split(' ');
            int output = 0;
            if (Parts.Length < 2 && !int.TryParse(Parts[1], out output)) return -1;
            return output;
        }

        public static int CodeFromResponse(string response)
        {
            string[] Parts = response.Split(' ');
            int output = -1;
            if (Parts.Length < 2 || !int.TryParse(Parts[1], out output)) return -1;
            return output;
        }
    }
}