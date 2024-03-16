using System;
using System.IO;

using Kaye.Net;
using Kaye.Profiles;

namespace Kaye.Arguments
{
    public class KayeArguments
    {
        readonly public Mode Mode;
        readonly public TCPAddress Target;
        readonly public Profile[] TargetHashes;
        readonly public String SerialNum;

        public KayeArguments(Mode Mode, TCPAddress Target)
        {
            this.Mode = Mode;
            this.Target = Target;
        }

        public KayeArguments(Mode Mode, TCPAddress Target, Profile[] TargetHashes, String SerialNum)
        {
            this.Mode = Mode;
            this.Target = Target;
            this.TargetHashes = TargetHashes;
            this.SerialNum = SerialNum;
        }

        public static KayeArguments Parse(String[] Args)
        {
            Mode Mode = Mode.Broken;
            TCPAddress Target = TCPAddress.None;

            if (Args.Length == 1)
            {
                if (Args[0] == "--help") Mode = Mode.Help;
                else if (TCPAddress.TryParse(Args[0], out Target)) Mode = Mode.StealthExploit;
            }

            if (Args.Length == 2 && TCPAddress.TryParse(Args[1], out Target))
            {
                if (Args[0] == "--exploit-stealth") Mode = Mode.StealthExploit;
                if (Args[0] == "--exploit") Mode = Mode.Exploit;
                if (Args[0] == "--brute") Mode = Mode.Brute;
                if (Args[0] == "--spoof") Mode = Mode.Spoof;
            }

            if (Args.Length == 2 && Args[0] == "--hash" && File.Exists(Args[1]))
            {
                Mode = Mode.Hash;
                String[] Lines = File.ReadAllLines(Args[1]);
                String HashPrefix = "HASH:";
                String TargetPrefix = "TARGET:";
                String SNPrefix = "SERIALNUM:";
                String SerialNum = String.Empty;
                Int32 HashCount = 0, CurrentCount = 0;

                for (Int32 i = 0; i < Lines.Length; i++)
                {
                    if (Lines[i].StartsWith(HashPrefix)) HashCount++;
                    if (Lines[i].StartsWith(SNPrefix)) SerialNum = Lines[i].Substring(SNPrefix.Length);
                    if (Lines[i].StartsWith(TargetPrefix))
                    {
                        String AddressStr = Lines[i].Substring(TargetPrefix.Length);
                        TCPAddress Address;
                        if (TCPAddress.TryParse(AddressStr, out Address)) Target = Address;
                    }
                }

                Profile[] Profiles = new Profile[HashCount];

                for (Int32 i = 0; i < Lines.Length; i++)
                {
                    String[] Blocks = Lines[i].Split(':');
                    if (Blocks.Length == 4 && Blocks[0] == HashPrefix.TrimEnd(':'))
                    {
                        HashAlg Algorithm;
                        if (!Enum.TryParse(Blocks[3], out Algorithm)) Algorithm = HashAlg.Unknown;
                        Profiles[CurrentCount] = new Profile(Blocks[1], Blocks[2], Algorithm);
                        CurrentCount++;
                    }
                }

                return new KayeArguments(Mode, Target, Profiles, SerialNum);
            }

            return new KayeArguments(Mode, Target);
        }
    }
}