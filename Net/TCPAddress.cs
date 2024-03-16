using System;
using System.Net;

namespace Kaye.Net
{
    public class TCPAddress : IPEndPoint
    {
        public const Int32 DefaultPort = 80;
        public static TCPAddress None
        {
            get { return new TCPAddress(IPAddress.None, DefaultPort); }
        }

        public TCPAddress(IPAddress Address, Int32 Port) : base(Address, Port)
        { }

        public static Boolean TryParse(String Input, out TCPAddress Output)
        {
            Output = new TCPAddress(IPAddress.None, DefaultPort);

            IPAddress Address;
            Int32 Port = DefaultPort;

            String[] Parts = Input.Split(':');

            if (Parts.Length == 1)
            {
                if (!IPAddress.TryParse(Parts[0], out Address)) return false;

                Output = new TCPAddress(Address, Port);
                return true;
            }

            if (Parts.Length == 2)
            {
                if (!IPAddress.TryParse(Parts[0], out Address) || !Int32.TryParse(Parts[1], out Port)) return false;

                Output = new TCPAddress(Address, Port);
                return true;
            }

            return false;
        }
    }
}