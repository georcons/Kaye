using System;
using System.IO;

namespace Kaye.BruteForce
{
    public class Dictionary
    {
        public String Name;
        public String[] Content;
        private Int32 CurrentIndex = 0;

        public Dictionary(String Name, String[] Content)
        {
            this.Name = Name;
            this.Content = Content;
        }

        public static Dictionary FromFile(String Path)
        {
            String[] PathWords = Path.Split(new Char[] { '/', '\\' }, StringSplitOptions.RemoveEmptyEntries);
            String Name = PathWords[PathWords.Length - 1];
            String[] Content = File.ReadAllLines(Path);
            return new Dictionary(Name, Content);
        }

        public String Current()
        {
            if (CurrentIndex >= Content.Length) return Content[Content.Length - 1];
            return Content[CurrentIndex];
        }

        public String Next()
        {
            if (CurrentIndex >= Content.Length) return null;
            CurrentIndex++;
            return Content[CurrentIndex - 1];
        }

        public Int32 ItemsTested
        {
            get
            {
                return CurrentIndex;
            }
        }
    }
}