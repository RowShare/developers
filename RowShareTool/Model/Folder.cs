using System;
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
            Server = parent as Server;
            IsRoot = Server != null;
            if (Server == null)
            {
                ParentFolder = (Folder)parent;
                Server = ParentFolder.Server;
            }
        }

        public override string DisplayName
        {
            get
            {
                if (Parent is Server)
                    return "<Root Folder>";

                return "<" + base.DisplayName + ">";
            }

            set
            {
                base.DisplayName = value;
            }
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
        public Folder ParentFolder { get; private set; }

        [JsonUtilities(IgnoreWhenSerializing = true)]
        public Folder RootFolder
        {
            get
            {
                if (Parent is Server)
                    return this;

                return ParentFolder.RootFolder;
            }
        }

        [Browsable(false)]
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

        public override void Reload()
        {
            Server.Call("folder/load/" + IdN, this, null);
            if (!HasLazyChild)
            {
                ReloadChildren();
            }
        }

        public override bool Delete()
        {
            if (IsRoot)
                return false;

            object o = Server.Call("folder/delete/" + IdN, null, this);
            if (o is bool)
                return (bool)o;

            return false;
        }

        protected override void LoadChildren()
        {
            base.LoadChildren();
            List<TreeItem> items = new List<TreeItem>();
            if (ListsIds != null)
            {
                foreach (Guid id in ListsIds)
                {
                    List list = new List(this);
                    Server.Call("list/load/" + id.ToString("N"), list, null);
                    items.Add(list);
                }
            }

            if (FoldersIds != null)
            {
                foreach (Guid id in FoldersIds)
                {
                    Folder folder = new Folder(this);
                    Server.Call("folder/load/" + id.ToString("N"), folder, null);
                    items.Add(folder);
                }
            }

            items.Sort();
            foreach (var item in items)
            {
                Children.Add(item);
            }
            OnPropertyChanged("Children");
        }

        public override int CompareTo(TreeItem other)
        {
            List list = other as List;
            if (list != null)
                return -1;

            return base.CompareTo(other);
        }
    }
}
