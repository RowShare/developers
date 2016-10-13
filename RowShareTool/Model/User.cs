using System;
using System.ComponentModel;
using CodeFluent.Runtime.Utilities;

namespace RowShareTool.Model
{
    public class User : TreeItem
    {
        public User(Server server)
            : base(server)
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
    }
}
