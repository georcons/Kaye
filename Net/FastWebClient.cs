using System;
using System.Net;

namespace Kaye.Net
{
    public class FastWebClient : WebClient
    {
        public Int32 TimeOut = 2000;

        protected override WebRequest GetWebRequest(Uri uri)
        {
            WebRequest w = base.GetWebRequest(uri);
            w.Timeout = TimeOut;
            return w;
        }
    }
}
