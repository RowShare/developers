using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using CodeFluent.Runtime.Utilities;

namespace RowShareTool.Model
{
    public class User : TreeItem
    {
        public User(Server server)
            : base(server, true)
        {
            if (server == null)
                throw new ArgumentNullException(nameof(server));
        }

        [Browsable(false)]
        [JsonUtilities(IgnoreWhenSerializing = true)]
        public new Server Parent
        {
            get
            {
                return (Server)base.Parent;
            }
        }

        public override string Url
        {
            get
            {
                return Parent.Url + "/MyAccount";
            }
        }

        [Browsable(false)]
        public override string DisplayName
        {
            get
            {
                return Email;
            }
            set
            {
                Email = value;
            }
        }

        public string Email
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

        public string NickName
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

        [JsonUtilities(IgnoreWhenSerializing = true)]
        public CultureInfo Language
        {
            get
            {
                if (Lcid <= 0)
                    return CultureInfo.GetCultureInfo(9);

                CultureInfo ci;
                ConvertUtilities.TryChangeType(Lcid, out ci);
                return ci != null ? ci : CultureInfo.GetCultureInfo(9);
            }
        }

        [Browsable(false)]
        public int Lcid
        {
            get
            {
                return GetProperty<int>();
            }
            set
            {
                SetProperty(value);
            }
        }

        public string TimeZoneId
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

        [Browsable(false)]
        [JsonUtilities(IgnoreWhenSerializing = true)]
        public Guid RootFolderId
        {
            get
            {
                return GetProperty<Guid>();
            }
            set
            {
                SetProperty(value);
            }
        }

        [DisplayName("Root Folder Id")]
        public string RootFolderIdN
        {
            get
            {
                return RootFolderId.ToString("N");
            }
        }

        protected override void LoadChildren()
        {
            base.LoadChildren();
            var items = new List<Organization>();

            Parent.Call("organization/loadall", items, this);
            items.Sort();
            foreach (var item in items)
            {
                Children.Add(item);
            }

            OnPropertyChanged(nameof(Children));
        }
    }
}
