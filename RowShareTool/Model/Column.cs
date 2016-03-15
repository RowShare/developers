using System;
using System.ComponentModel;
using CodeFluent.Runtime.Utilities;

namespace RowShareTool.Model
{
    public class Column : TreeItem
    {
        private Type _type;

        public Column()
            : base(null)
        {
        }

        public Column(List list)
            :base(list, false)
        {
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
        public Guid ListId
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
        public string ListIdN
        {
            get
            {
                return Id.ToString("N");
            }
        }

        [JsonUtilities(IgnoreWhenSerializing = true)]
        public Type Type
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(ClrType))
                {
                    _type = AssemblyUtilities.GetType(ClrType, false);
                }
                if (_type == null)
                {
                    _type = typeof(string);
                }
                return _type;
            }
        }

        public ColumnDataType DataType
        {
            get
            {
                return GetProperty<ColumnDataType>();
            }
            set
            {
                SetProperty(value);
            }
        }

        public ColumnOptions Options
        {
            get
            {
                return GetProperty<ColumnOptions>();
            }
            set
            {
                SetProperty(value);
            }
        }

        public string ClrType
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

        public bool IsReadOnly
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

        public int ColumnTypeId
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

        public int MaxDecimals
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

        public int MaxLength
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

        public int Index
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

        public int SortOrder
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
    }
}
