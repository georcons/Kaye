using System.Net;
using System.Threading;
using System.Net.Sockets;

namespace Kaye.Net
{
    public class PortScanner
    {
        public int TimeOut = 3000;

        public PortStatus Scan(IPEndPoint iep)
        {
            bool IsFinished = false;
            PortStatus status = PortStatus.Filtered;
            Thread control = new Thread(() => TimeOutVoid(ref IsFinished));
            Thread scan = new Thread(() => ScanVoid(iep, ref status, ref IsFinished));
            control.Start();
            scan.Start();
            while (!IsFinished)
            {
                // WAIT
            }
            control.Abort();
            scan.Abort();
            return status;
        }

        void TimeOutVoid(ref bool timeout)
        {
            Thread.Sleep(this.TimeOut);
            timeout = true;
        }

        void ScanVoid(IPEndPoint iep, ref PortStatus status, ref bool IsFinished)
        {
            Socket s = new Socket(iep.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                s.Connect(iep);
                status = PortStatus.Open;
                IsFinished = true;
            }
            catch (SocketException e)
            {
                if (e.SocketErrorCode == SocketError.ConnectionRefused) status = PortStatus.Closed;
                else if (e.SocketErrorCode == SocketError.TimedOut) status = PortStatus.Filtered;
                else status = PortStatus.Unknown;
                IsFinished = true;
            }
        }
    }
}