using System;
using System.Text;
using System.Security.Cryptography;

namespace Kaye.Cryptography
{
    public class DahuaMD5
    {
        public static Boolean HashTest(String Word, String Username, String SerialNumber, Byte[] Hash)
        {
            String ToHash = Username + ":Login to " + SerialNumber + ":" + Word;
            Byte[] ToHashBytes = Encoding.ASCII.GetBytes(ToHash);
            Byte[] Hashed = MD5.Create().ComputeHash(ToHashBytes);
            Boolean Output = Hash.Length == Hashed.Length;
            for (Int32 i = 0; i < Hashed.Length && Output; i++) Output = Hash[i] == Hashed[i];
            return Output;
        }
    }
}