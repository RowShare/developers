using System;
using System.Collections.Generic;
using System.ComponentModel;
using CodeFluent.Runtime.Utilities;

namespace RowShareTool.Model
{
    public class Row : TreeItem
    {
        private DynamicObject _valuesObject;

        public Row()
            : base(null)
        {
        }

        public Row(List list)
            : base(list, false)
        {
        }

        public override string DisplayName
        {
            get
            {
                return Index.ToString();
            }
            set
            {
                Index = int.Parse(value);
            }
        }

        [Browsable(false)]
        [JsonUtilities(IgnoreWhenSerializing = true)]
        public new List Parent
        {
            get
            {
                return (List)base.Parent;
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

        public string Version
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
        public DynamicObject ValuesObject
        {
            get
            {
                if (_valuesObject == null && Parent != null)
                {
                    _valuesObject = new DynamicObject();
                    var cols = Parent.Columns;
                    if (cols != null)
                    {
                        foreach (Column column in cols)
                        {
                            // TODO atts
                            var dop = _valuesObject.AddProperty(column.DisplayName, column.Type, null);
                            object value = Values.GetValue(column.DisplayName, column.Type, null);
                            dop.SetValue(_valuesObject, value);
                        }
                    }
                }
                return _valuesObject;
            }
        }

        public Dictionary<string, object> Values
        {
            get
            {
                return GetProperty<Dictionary<string, object>>();
            }
            set
            {
                if (SetProperty(value))
                {
                    _valuesObject = null;
                    OnPropertyChanged("ValuesObject");
                }
            }
        }

        public string CreatedByEmail
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

        public string CreatedByNickName
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

        public string ModifiedByNickName
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

        public string ModifiedByEmail
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

        public DateTime CreatedDate
        {
            get
            {
                return GetProperty<DateTime>();
            }
            set
            {
                SetProperty(value);
            }
        }

        public DateTime ModifiedDate
        {
            get
            {
                return GetProperty<DateTime>();
            }
            set
            {
                SetProperty(value);
            }
        }

        public bool ValueEquals(string columnName, object value)
        {
            object colValue;
            if (!Values.TryGetValue(columnName, out colValue))
                return false;

            if (colValue == null)
                return false;

            return colValue.Equals(value);
        }

        public object GetValue(string columnName)
        {
            return GetValue(columnName, null);
        }

        public object GetValue(string columnName, object defaultValue)
        {
            if (columnName == null)
                throw new ArgumentNullException("columnName");

            object value;
            if (!Values.TryGetValue(columnName, out value))
                return defaultValue;

            return value;
        }

        public T GetValue<T>(string columnName, T defaultValue)
        {
            if (columnName == null)
                throw new ArgumentNullException("columnName");

            object value;
            if (!Values.TryGetValue(columnName, out value))
                return defaultValue;

            return ConvertUtilities.ChangeType(value, defaultValue);
        }
    }
}
