using System;
using System.IO;
using System.Net;
using System.Text;
using System.Drawing;
using System.Security.Cryptography;

using Kaye.Net;
using Kaye.Profiles;

namespace Kaye.Live
{
    public class DahuaLive
    {
        IPEndPoint Target;
        Profile[] Profiles;
        Int32 ProfileIndex = 0;
        String SerialNum = String.Empty;

        public DahuaLive(IPEndPoint Target, String SerialNum)
        {
            this.Target = Target;
            this.SerialNum = SerialNum;
        }

        public void SetProfiles(Profile[] Profiles)
        {
            this.Profiles = Profiles;
        }

        public Boolean ConfigureProfile(Int32 Channel)
        {
            Image Buffer;
            while (ProfileIndex < Profiles.Length)
            {
                if (GetLiveImage(Channel, out Buffer)) return true;
                ProfileIndex++;
            }
            ProfileIndex = 0;
            return false;
        }

        public Boolean GetLiveImage(Int32 Channel, out Image Output)
        {
            Output = ImageTools.EmptyBitmap(Color.White, new Size(1, 1));

            try
            { 
                Profile Profile = this.Profiles[ProfileIndex];
                if (Profile.Algorithm == HashAlg.DahuaMD5)
                {
                    String URL = "/cgi-bin/snapshot.cgi?channel=" + Channel.ToString();
                    String NonceReq = "GET " + URL + " HTTP/1.1\r\nHost: " + Target.Address + "\r\n\r\n\r\n";
                    TCPSender NonceSender = new TCPSender(Target.Address, Target.Port, NonceReq);
                    String NonceResponse = NonceSender.response;
                    Int32 NonceStart = NonceResponse.IndexOf("nonce=");
                    String NonceString = String.Empty;
                    for (Int32 i = NonceStart + 7; i < NonceResponse.Length && NonceResponse[i] != '"'; i++) NonceString += NonceResponse[i];
                    String two = "GET:" + URL;
                    String SHash = BitConverter.ToString(MD5.Create().ComputeHash(Encoding.ASCII.GetBytes(two))).Replace("-", "").ToLower();
                    String three = Profile.Hash.ToLower() + ":" + NonceString + ":00000001:db83b31e583b19ce:auth:" + SHash;
                    String DigestResponse = BitConverter.ToString(MD5.Create().ComputeHash(Encoding.ASCII.GetBytes(three))).Replace("-", "").ToLower();
                    String ImgReq = "GET " + URL + " HTTP/1.1\r\nHost: " + Target.Address + "\r\nAuthorization: Digest username=\"" + Profile.Username + "\", realm=\"Login to " + SerialNum + "\", nonce=\"" + NonceString + "\", uri=\"" + URL
                        + "\", response=\"" + DigestResponse + "\", qop=auth, nc=00000001, cnonce=\"db83b31e583b19ce\"\r\n\r\n\r\n";
                    TCPSender ImgSender = new TCPSender(Target.Address, Target.Port, ImgReq, true);
                    String r = ImgSender.response;
                    if (r == null) return false;
                    String[] lines = r.Split(new Char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                    Byte[] Image = new Byte[0];
                    try
                    {
                        String ContentLengthStr = String.Empty;
                        for (Int32 i = 16; i < lines[5].Length; i++) ContentLengthStr += lines[5][i];
                        Int32 ContentLength = Int32.Parse(ContentLengthStr);
                        if (ContentLength < 50) return false;
                        Image = new Byte[ContentLength];
                        for (Int32 i = ImgSender.ResponseBytes.Length - ContentLength; i < ImgSender.ResponseBytes.Length; i++) Image[i - ImgSender.ResponseBytes.Length + ContentLength] = ImgSender.ResponseBytes[i];
                    }
                    catch
                    { }
                    MemoryStream stream = new MemoryStream(Image);
                    Bitmap bmpImage = (Bitmap)Bitmap.FromStream(stream);
                    Output = ImageTools.ResizeBitmap(bmpImage, 300, 300);
                    return true;
                }

                if (Profile.Algorithm == HashAlg.None)
                {
                    WebClient Client = new WebClient();
                    Client.Credentials = new NetworkCredential(Profile.Username, Profile.Password);
                    Stream Stream = Client.OpenRead("/cgi-bin/snapshot.cgi?channel=" + Channel.ToString());
                    Bitmap Bitmap;
                    Bitmap = new Bitmap(Stream);
                    Output = Bitmap;
                    return true;
                }
            }

            catch {  }

            return false;
        }

    }
}