using System;

namespace Kaye.Commands
{
    public class KayeCommandScope
    {
        KayeCommandType[] Scope = new KayeCommandType[0];

        public Boolean InScope(KayeCommandType Command)
        {
            if (Command == KayeCommandType.None) return true;
            for (Int32 i = 0; i < Scope.Length; i++) if (Command == Scope[i]) return true;
            return false;
        }

        public void Push(KayeCommandType Command)
        {
            if (this.InScope(Command)) return;
            KayeCommandType[] NewArray = new KayeCommandType[Scope.Length + 1];
            for (Int32 i = 0; i < Scope.Length; i++) NewArray[i] = Scope[i];
            NewArray[Scope.Length] = Command;
            Scope = NewArray;
        }

        public void Clear()
        {
            this.Scope = new KayeCommandType[0];
        }

        public override string ToString()
        {
            String Output = String.Empty;
            for (Int32 i = 0; i < Scope.Length; i++) Output += (KayeCommand.CommandTypeToString(Scope[i]) + (i == Scope.Length - 1 ? String.Empty : ", "));
            return Output;
        }
    }
}