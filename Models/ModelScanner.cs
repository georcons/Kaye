using System.Net;
using System.Threading;

namespace Kaye.Models
{
    public class ModelScanner
    {
        ModelIdentificator HiSilicon, Dahua, NVMS9000, NVMS1000, Hipcam;

        public ModelScanner()
        {
            this.HiSilicon = new ModelIdentificator(Model.HiSilicon, "/config.js", "cabAddress");
            this.Dahua = new ModelIdentificator(Model.Dahua, "/cgi-bin/snap.jpg", "Login to", "Device_CGI");
            this.NVMS9000 = new ModelIdentificator(Model.NVMS9000, "/server.js", "ServerType=10");
            this.NVMS1000 = new ModelIdentificator(Model.NVMS1000, "/server.js", "Cross Web Server");
            this.Hipcam = new ModelIdentificator(Model.Hipcam, "/web/", "Welcome");
        }

        public Model Scan(IPEndPoint iep)
        {
            int httpCode = 0;
            if (HiSilicon.Check(iep, out httpCode)) return Model.HiSilicon;
            if (httpCode == -1) return Model.None;
            bool[] Checked = new bool[4];
            bool[] ResultPool = new bool[4];
            bool Start = false;
            Thread DahuaThread = new Thread(() => Dahua.AsyncCheck(iep, ref Start, Checked, ResultPool, 0));
            Thread NVMS9000Thread = new Thread(() => NVMS9000.AsyncCheck(iep, ref Start, Checked, ResultPool, 1));
            Thread NVMS1000Thread = new Thread(() => NVMS1000.AsyncCheck(iep, ref Start, Checked, ResultPool, 2));
            Thread HipcamThread = new Thread(() => Hipcam.AsyncCheck(iep, ref Start, Checked, ResultPool, 3));
            DahuaThread.Start();
            NVMS1000Thread.Start();
            NVMS9000Thread.Start();
            HipcamThread.Start();
            Start = true;
            bool CheckDone = false, Found = false;
            while (!CheckDone && !Found)
            {
                CheckDone = true;
                for (int i = 0; i < Checked.Length; i++) CheckDone = Checked[i] && CheckDone;
                for (int i = 0; i < ResultPool.Length; i++) Found = Found || ResultPool[i];
            }
            if (ResultPool[0]) return Model.Dahua;
            if (ResultPool[1]) return Model.NVMS9000;
            if (ResultPool[2]) return Model.NVMS1000;
            if (ResultPool[3]) return Model.Hipcam;
            return Model.None;
        }
    }
}