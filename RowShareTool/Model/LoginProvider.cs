using System.Collections.Generic;

namespace RowShareTool.Model
{
    public class LoginProvider
    {
        private string _displayName;

        public string Name { get; private set; }

        public string DisplayName
        {
            get
            {
                if (_displayName == null)
                    return Name;

                return _displayName;
            }
            private set
            {
                _displayName = value;
            }
        }

        public override string ToString()
        {
            return Name;
        }

        public static IReadOnlyList<LoginProvider> All
        {
            get
            {
                List<LoginProvider> all = new List<LoginProvider>();
                all.Add(new LoginProvider { Name = "Google" });
                all.Add(new LoginProvider { Name = "Facebook" });
                all.Add(new LoginProvider { Name = "LinkedIn" });
                all.Add(new LoginProvider { Name = "Microsoft" });
                all.Add(new LoginProvider { Name = "Office365", DisplayName = "Office 365" });
                all.Add(new LoginProvider { Name = "Yahoo" });
                all.Add(new LoginProvider { Name = "Rowshare" });
                return all;
            }
        }
    }
}
