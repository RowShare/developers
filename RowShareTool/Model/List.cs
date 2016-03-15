using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using CodeFluent.Runtime.Utilities;

namespace RowShareTool.Model
{
    public class List : TreeItem
    {
        private ObservableCollection<Column> _columns;

        public List()
            :base(null)
        {
        }

        public List(Folder folder)
            : base(folder, false)
        {
            if (folder == null)
                throw new ArgumentNullException("folder");

            FolderId = folder.Id;
        }

        [Browsable(false)]
        [JsonUtilities(IgnoreWhenSerializing = true)]
        public new Folder Parent
        {
            get
            {
                return (Folder)base.Parent;
            }
        }

        public override string Url
        {
            get
            {
                if (Parent == null || Parent.Server == null || Parent.Server.Url == null)
                    return null;

                return Parent.Server.Url + "/t/" + IdN;
            }
        }

        [Browsable(false)]
        public Guid FolderId
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

        [DisplayName("Folder Id")]
        [JsonUtilities(IgnoreWhenSerializing = true)]
        public string FolderIdN
        {
            get
            {
                return FolderId.ToString("N");
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

        [JsonUtilities(IgnoreWhenSerializing = true)]
        public CultureInfo Language
        {
            get
            {
                if (Lcid <= 0)
                    return null;

                try
                {
                    return new CultureInfo(Lcid);
                }
                catch
                {
                    return null;
                }
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

        public int ReportCount
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

        public ListAccessMode AccessMode
        {
            get
            {
                return GetProperty<ListAccessMode>();
            }
            set
            {
                SetProperty(value);
            }
        }

        public int CategoryId
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

        [Browsable(false)]
        public int ColumnCount
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

        // not updated yet
        //public int RowCount
        //{
        //    get
        //    {
        //        return GetProperty<int>();
        //    }
        //    set
        //    {
        //        SetProperty(value);
        //    }
        //}

        public bool CanWrite
        {
            get
            {
                return GetProperty<bool>();
            }
            set
            {
                SetProperty(value);
            }
        }

        public bool Owned
        {
            get
            {
                return GetProperty<bool>();
            }
            set
            {
                SetProperty(value);
            }
        }

        public bool Concurrency
        {
            get
            {
                return GetProperty<bool>();
            }
            set
            {
                SetProperty(value);
            }
        }

        public string Description
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

        public ObservableCollection<Column> Columns
        {
            get
            {
                if (_columns == null)
                {
                    _columns = new ObservableCollection<Column>();
                    if (Parent != null && Parent.Server != null)
                    {
                        Parent.Server.Call("column/loadforparent/" + IdN, _columns, this);
                    }
                }
                return _columns;
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

        public override bool Delete()
        {
            object o = Parent.Server.PostCall("list/delete", null, new { Id = IdN });
            if (o is bool)
                return (bool)o;

            return false;
        }

        public ObservableCollection<Row> LoadRows()
        {
            ObservableCollection<Row> rows = new ObservableCollection<Row>();
            if (Parent != null && Parent.Server != null)
            {
                Parent.Server.Call("row/loadforparent/" + IdN, rows, this);
            }
            return rows;
        }

        public override int CompareTo(TreeItem other)
        {
            Folder folder = other as Folder;
            if (folder != null)
                return 1;

            return base.CompareTo(other);
        }
    }
}
