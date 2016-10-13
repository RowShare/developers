using System;
using System.Collections.ObjectModel;
using SoftFluent.Windows;

namespace RowShareTool.Model
{
    public class ImportOptionsDefinition : AutoObject
    {
        public ImportOptionsDefinition(List list, List existingList, string newListName)
        {
            if (list == null)
                throw new ArgumentNullException(nameof(list));

            List = list;
            ExistingList = existingList;
            NewListName = newListName;
            CreateNewList = newListName != null;
            Columns = new ObservableCollection<string>();
            Column firstUnique = null;
            foreach (Column col in list.Columns)
            {
                Columns.Add(col.DisplayName);
                if (firstUnique == null && ((col.Options & ColumnOptions.IsUnique) == ColumnOptions.IsUnique))
                {
                    firstUnique = col;
                }
            }

            if (firstUnique != null)
            {
                ReplaceColumn = firstUnique.DisplayName;
            }
        }

        public List List { get; private set; }
        public List ExistingList { get; private set; }
        public ObservableCollection<string> Columns { get; private set; }
        public string NewListName { get; private set; }

        public virtual bool ReplaceWithKey
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

        public virtual bool CreateNewList
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

        public virtual bool ReplaceAllRows
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

        public virtual string ReplaceColumn
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
    }
}
