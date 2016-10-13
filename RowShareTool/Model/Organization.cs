﻿using System;
using System.ComponentModel;
using CodeFluent.Runtime.Utilities;

namespace RowShareTool.Model
{
    public class Organization : TreeItem
    {
        private Folder _rootFolder;

        public Organization(User user)
            : base(user, true)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));
        }

        [Browsable(false)]
        [JsonUtilities(IgnoreWhenSerializing = true)]
        public new User Parent
        {
            get
            {
                return (User)base.Parent;
            }
        }

        [Browsable(false)]
        public Guid Id
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

        [DisplayName("Id")]
        [JsonUtilities(IgnoreWhenSerializing = true)]
        public string IdN
        {
            get
            {
                return Id.ToString("N");
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

        public override void Reload()
        {
            _rootFolder = null;
            ChildrenClear();
            LoadChildren();
        }

        protected override void LoadChildren()
        {
            base.LoadChildren();
            var folder = RootFolder;
            if (folder != null)
            {
                Children.Add(folder);
            }
            OnPropertyChanged(nameof(Children));
        }

        [Browsable(false)]
        [JsonUtilities(IgnoreWhenSerializing = true)]
        public Folder RootFolder
        {
            get
            {
                if (_rootFolder == null && RootFolderId != Guid.Empty)
                {
                    _rootFolder = new Folder(this);
                    Parent.Parent.Call("folder/load/" + RootFolderIdN, _rootFolder, null);
                }
                return _rootFolder;
            }
        }

        [Browsable(false)]
        public override string Url
        {
            get
            {
                return base.Url;
            }
        }

        public override string DisplayName
        {
            get
            {
                return Name;
            }

            set
            {
                Name = value;
            }
        }

        public string Name
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

        // only valid if user is security manager
        public int GroupCount
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

        // only valid if user is security manager
        public int MemberCount
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

        public string MetaData
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

        public override int CompareTo(TreeItem other)
        {
            var org = other as Organization;
            if (org != null)
                return -1;

            if (Name == null)
            {
                if (org.Name == null)
                    return 0;

                return 1;
            }
            else if (org.Name == null)
                return -1;

            return Name.CompareTo(org.Name);
        }
    }
}
