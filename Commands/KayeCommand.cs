using System;

namespace Kaye.Commands
{
    public class KayeCommand
    {

        public String[] Args;
        public KayeCommandType Command;

        public static KayeCommand Parse(String Str)
        {
            KayeCommand Output = new KayeCommand();
            String[] Words = Str.Split(new Char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            Output.Args = new String[Words.Length > 0 ? Words.Length - 1 : 0];
            for (int i = 0; i < Words.Length - 1; i++) Output.Args[i] = Words[i + 1];
            if (Words.Length == 0)
            {
                Output.Command = KayeCommandType.None;
                return Output;
            }
            else if (Words[0].ToLower() == "import") Output.Command = KayeCommandType.Import;
            else if (Words[0].ToLower() == "export") Output.Command = KayeCommandType.Export;
            else if (Words[0].ToLower() == "reboot") Output.Command = KayeCommandType.Reboot;
            else if (Words[0].ToLower() == "live") Output.Command = KayeCommandType.Live;
            else if (Words[0].ToLower() == "reg") Output.Command = KayeCommandType.Register;
            else if (Words[0].ToLower() == "exit") Output.Command = KayeCommandType.Exit;
            else if (Words[0].ToLower() == "currentfile") Output.Command = KayeCommandType.CurrentFile;
            else if (Words[0].ToLower() == "list") Output.Command = KayeCommandType.List;
            else if (Words[0].ToLower() == "hash") Output.Command = KayeCommandType.Hash;
            else if (Words[0].ToLower() == "dictionary") Output.Command = KayeCommandType.Dictionary;
            else if (Words[0].ToLower() == "rainbow") Output.Command = KayeCommandType.Rainbow;
            else if (Words[0].ToLower() == "commands") Output.Command = KayeCommandType.Commands;
            else Output.Command = KayeCommandType.None;

            return Output;
        }

        public static String CommandTypeToString(KayeCommandType Type)
        {
            switch (Type)
            {
                case KayeCommandType.Exit:
                    return "exit";
                case KayeCommandType.Export:
                    return "export";
                case KayeCommandType.Import:
                    return "import";
                case KayeCommandType.Reboot:
                    return "reboot";
                case KayeCommandType.Register:
                    return "reg";
                case KayeCommandType.Live:
                    return "live";
                case KayeCommandType.CurrentFile:
                    return "currentfile";
                case KayeCommandType.List:
                    return "list";
                case KayeCommandType.Hash:
                    return "hash";
                case KayeCommandType.Dictionary:
                    return "dictionary";
                case KayeCommandType.Rainbow:
                    return "rainbow";
                case KayeCommandType.Commands:
                    return "commands";
                default:
                    return String.Empty;
            }
        }
    }
}