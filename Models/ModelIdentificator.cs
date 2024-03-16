using System;
using System.Net;

using Kaye.Net;

namespace Kaye.Models
{
    public class ModelIdentificator
    {
        Model Model;
        string Directory, TextInDirectory, STextInDirectory = String.Empty;

        public ModelIdentificator(Model Model, string Directory, string TextInDirectory)
        {
            this.Model = Model;
            this.Directory = Directory;
            this.TextInDirectory = TextInDirectory;
        }

        public ModelIdentificator(Model Model, string Directory, string TextInDirectory, string STextInDirectory)
        {
            this.Model = Model;
            this.Directory = Directory;
            this.TextInDirectory = TextInDirectory;
            this.STextInDirectory = STextInDirectory;
        }

        public bool Check(IPEndPoint iep)
        {
            try
            {
                HTTPSender http = new HTTPSender(iep.Address, iep.Port);
                string response = http.DownloadString(this.Directory);
                int Code = HTTPSender.CodeFromResponse(response);
                if (Code != 200 && Code != 401) return false;
                if ((response.IndexOf(this.TextInDirectory) == -1))
                {
                    if (STextInDirectory != String.Empty)
                    {
                        if (response.IndexOf(this.STextInDirectory) == -1) return false;
                        return true;
                    }

                    return false;
                }
                return true;
            }
            catch { return false; }
        }

        public bool Check(IPEndPoint iep, out int Code)
        {
            Code = 0;
            try
            {
                HTTPSender http = new HTTPSender(iep.Address, iep.Port);
                string response = http.DownloadString(this.Directory);
                Code = HTTPSender.CodeFromResponse(response);
                if (Code != 200 && Code != 401) return false;
                if (response.IndexOf(this.TextInDirectory) == -1) return false;
                return true;
            }
            catch (Exception e) { Console.WriteLine(e); return false; }
        }

        public void AsyncCheck(IPEndPoint iep, ref bool Scan, bool[] Done, bool[] Model, int index)
        {
            while (!Scan)
            {
                // WAIT
            }
            bool result = this.Check(iep);
            Model[index] = result;
            Done[index] = true;
        }
    }
}