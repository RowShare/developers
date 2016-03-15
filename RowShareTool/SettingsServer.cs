using System;
using System.ComponentModel;
using CodeFluent.Runtime.Utilities;
using SoftFluent.Windows;

namespace RowShareTool
{
    public class SettingsServer : AutoObject
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

        public string Url
        {
            get
            {
                return GetProperty<string>();
            }
            set
            {
                SetProperty(value);
            }
        }

        protected override string Validate(string memberName)
        {
            string error = base.Validate(memberName);
            if (error == null)
            {
                if (memberName == "Url" || memberName == null)
                {
                    if (string.IsNullOrWhiteSpace(Url))
                    {
                        error = "Url must be specified.";
                    }
                    else
                    {

                        Uri uri;
                        if (!Uri.TryCreate(Url, UriKind.Absolute, out uri))
                        {
                            error = "Url is invalid.";
                        }
                    }
                }
            }
            return error;
        }
    }
}
