﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using CodeFluent.Runtime.Utilities;

namespace RowShareTool.Model
{
    public class Folder : TreeItem
    {
        public Folder(TreeItem parent)
            : base(parent, true)
        {
            Folders = parent as Folders;
            if (Folders == null)
            {
                ParentFolder = (Folder)parent;
                Folders = ParentFolder.Folders;
            }

            Server = Folders.Parent;
            IsRoot = parent is Folders;
        }

        public override string Url
        {
            get
            {
                return Server.Url + "/MyTables.html?parentFolderId=" + IdN + "#tab-my-tables";
            }
        }

        [Browsable(false)]
        [JsonUtilities(IgnoreWhenSerializing = true)]
        public bool IsRoot { get; private set; }

        [Browsable(false)]
        [JsonUtilities(IgnoreWhenSerializing = true)]
        public Server Server { get; private set; }

        [Browsable(false)]
        [JsonUtilities(IgnoreWhenSerializing = true)]
        public Folders Folders { get; private set; }

        [Browsable(false)]
        [JsonUtilities(IgnoreWhenSerializing = true)]
        public Folder ParentFolder { get; private set; }

        [JsonUtilities(IgnoreWhenSerializing = true)]
        public Folder RootFolder
        {
            get
            {
                if (Parent is Folders)
                    return this;

                return ParentFolder.RootFolder;
            }
        }

        [Browsable(false)]
        [JsonUtilities(IgnoreWhenSerializing = true)]
        public Guid[] ListsIds
        {
            get
            {
                return GetProperty<Guid[]>();
            }
            set
            {
                if (SetProperty(value))
                {
                    ChildrenClear();
                }
            }
        }

        [Browsable(false)]
        [JsonUtilities(IgnoreWhenSerializing = true)]
        public Guid[] FoldersIds
        {
            get
            {
                return GetProperty<Guid[]>();
            }
            set
            {
                if (SetProperty(value))
                {
                    ChildrenClear();
                }
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

        public FolderOptions Options
        {
            get
            {
                return GetProperty<FolderOptions>();
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

        public override void Reload()
        {
            Server.Call("folder/load/" + IdN, this, null);
            ReloadChildren();
        }

        public override bool Delete()
        {
            if (IsRoot)
                return false;

            object o = Server.PostCall("folder/delete", null, new { Id = IdN });
            if (o is bool)
                return (bool)o;

            return false;
        }

        protected override void LoadChildren()
        {
            base.LoadChildren();
            var items = new List<TreeItem>();
            if (ListsIds != null)
            {
                foreach (Guid id in ListsIds)
                {
                    var list = new List(this);
                    Server.Call("list/load/" + id.ToString("N"), list, null);
                    items.Add(list);
                }
            }

            if (FoldersIds != null)
            {
                foreach (Guid id in FoldersIds)
                {
                    var folder = new Folder(this);
                    Server.Call("folder/load/" + id.ToString("N"), folder, null);
                    items.Add(folder);
                }
            }

            items.Sort();
            foreach (var item in items)
            {
                Children.Add(item);
            }
            OnPropertyChanged(nameof(Children));
        }

        public override int CompareTo(TreeItem other)
        {
            var list = other as List;
            if (list != null)
                return -1;

            return base.CompareTo(other);
        }
    }
}
