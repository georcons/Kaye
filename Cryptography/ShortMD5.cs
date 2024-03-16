using System;
using System.Text;
using System.Security.Cryptography;

namespace Kaye.Cryptography
{
    public class ShortMD5
    {
        static MD5 Md5Obj;

        public static void Load()
        {
            Md5Obj = MD5.Create();
        }

        private static Byte[] Hash(Byte[] Input)
        {
            return MD5.Create().ComputeHash(Input);
        }

        public static bool HashTest(String Word, Byte[] Hashed)
        {
            Byte[] Input = Encoding.ASCII.GetBytes(Word);
            Byte[] h = Hash(Input), c = new Byte[8];
            Boolean result = true;
            for (Int32 i = 0; i < 8 && result; i++)
            {
                c[i] = (Byte)((h[2 * i] + h[2 * i + 1]) % 62);
                if (c[i] >= 0 && c[i] <= 9) c[i] += 48;
                else if (c[i] >= 10 && c[i] <= 35) c[i] += 55;
                else c[i] += 61;
                if (c[i] != Hashed[i]) result = false;
            }
            return result;
        }
    }
}