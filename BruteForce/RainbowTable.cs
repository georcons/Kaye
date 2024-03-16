using System;

namespace Kaye.BruteForce
{
    public class RainbowTable
    {
        public String Name;
        public String[] Content;
        public Char Separator;

        public RainbowTable(String Name, String[] Content, Char Separator)
        {
            this.Name = Name;
            this.Content = Content;
            this.Separator = Separator;
        }
    }
}