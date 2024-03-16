using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Net.Sockets;

namespace Kaye.Net
{
    public class TCPSender
    {
        public String response;
        public Byte[] ResponseBytes;
        String req;
        IPAddress ip;
        Int32 port;
        Thread control, connect;
        Boolean done = false;

        public TCPSender(IPAddress ip, int port, string req)
        {
            this.ip = ip;
            this.port = port;
            this.req = req;
            ThreadStart ConnectStart = new ThreadStart(ConnectVoid);
            ThreadStart ControlStart = new ThreadStart(ControlVoid);
            control = new Thread(ControlStart);
            connect = new Thread(ConnectStart);
            connect.Start();
            control.Start();
            while (!done) { }
        }

        public TCPSender(IPAddress ip, int port, string req, bool b)
        {
            if (b)
            {
                this.ip = ip;
                this.port = port;
                this.req = req;
                ThreadStart ConnectStart = new ThreadStart(ConnectVoidBytes);
                ThreadStart ControlStart = new ThreadStart(ControlVoid);
                control = new Thread(ControlStart);
                connect = new Thread(ConnectStart);
                connect.Start();
                control.Start();
                while (!done) { }
            }
        }

        void ControlVoid()
        {
            Thread.Sleep(2000);
            connect.Abort();
            this.done = true;
        }

        void ConnectVoidBytes()
        {
            var client = new TcpClient();
            client.Connect(ip, port);
            NetworkStream networkStream = client.GetStream();
            networkStream.ReadTimeout = 2000;
            var res = new StreamReader(networkStream, Encoding.UTF8);
            byte[] bytes = Encoding.ASCII.GetBytes(req);
            networkStream.Write(bytes, 0, bytes.Length);
            byte[] data = new byte[1024];
            using (MemoryStream ms = new MemoryStream())
            {

                int numBytesRead;
                while ((numBytesRead = networkStream.Read(data, 0, data.Length)) > 0)
                {
                    ms.Write(data, 0, numBytesRead);
                }
                this.ResponseBytes = ms.ToArray();
                this.response = Encoding.ASCII.GetString(this.ResponseBytes);
            }
        }

        void ConnectVoid()
        {
            try
            {
                var client = new TcpClient();
                client.Connect(ip, port);
                NetworkStream networkStream = client.GetStream();
                networkStream.ReadTimeout = 2000;
                var res = new StreamReader(networkStream, Encoding.UTF8);
                byte[] bytes = Encoding.ASCII.GetBytes(req);
                networkStream.Write(bytes, 0, bytes.Length);
                this.response = res.ReadToEnd();
            }

            catch { }
            this.done = true;
        }
    }
}