using System;

namespace Kaye.Static
{
    public class StaticContent
    {
        public const String Broken = "\n" + KayePrefix + "Wrong arguments. Check << kaye --help >>.";
        public const String Help = "\n Kaye 1.2\n" +
                                   " Usage: kaye [Mode] ([Address]:[Port])/[File Path]\n" +
                                   " DEFAULTS: \n" +
                                   " Mode: --exploit-stealth\n" + 
                                   " Port: 80\n" +
                                   " KAYE MODES: \n" +
                                   "  --exploit-stealth: Exploit scan\n" + 
                                   "  --exploit: Exploit scan with default credential tests\n" + 
                                   "  --hash: Options for breaking found hashes from file\n" +
                                   "  --brute: Brute-force attack (not implemented)\n" + 
                                   "  --spoof: Spoof attack (not implemented)\n" + 
                                   "  --help: Shows this help menu\n" + 
                                   " EXAMPLES: \n" + 
                                   " kaye 192.168.0.12\n" + 
                                   " kaye --brute 10.100.1.99:8080\n" + 
                                   " kaye --exploit 172.24.0.15:8000\n" + 
                                   " kaye --hash exported.txt";
        public const String Spoof = "\n" + KayePrefix + "Spoof attacks aren't implemented.";
        public const String Brute = "\n" + KayePrefix + "Brute-force attacks aren't implemented.";
        public const String KayePrefix = "  KAYE  ";
        public const String CommandPrefix = "  $ ";
    }
}