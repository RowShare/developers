using System;
using System.Net;

namespace RowShareTool.Utilities
{
    public class CookieWebClient : WebClient
    {
        public CookieWebClient()
        {
            Cookies = new CookieContainer();
        }

        public CookieContainer Cookies { get; private set; }

        protected override WebRequest GetWebRequest(Uri address)
        {
            HttpWebRequest request = (HttpWebRequest)base.GetWebRequest(address);
            request.CookieContainer = Cookies;
            return request;
        }
    }
}
