using System.ComponentModel;
using CodeFluent.Runtime.Utilities;

namespace RowShareTool
{
    public class SettingsServer
    {
        internal string Cookie
        {
            get
            {
                return CryptedCookie != null ? SecurityUtilities.UnprotectData(CryptedCookie, false) : null;
            }
            set
            {
                CryptedCookie = value != null ? SecurityUtilities.ProtectData(value, false) : null;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public string CryptedCookie { get; set; }

        public string Url { get; set; }
    }
}
