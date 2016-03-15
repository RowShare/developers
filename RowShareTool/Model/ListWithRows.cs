using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace RowShareTool.Model
{
    public class ListWithRows
    {
        private ObservableCollection<Row> _rows;

        public ListWithRows()
        {
        }

        public ListWithRows(List list)
        {
            if (list == null)
                throw new ArgumentNullException("list");

            List = list;
            ServerUrl = List.Parent.Server.Url;
        }

        public List List { get; set; }
        public string ServerUrl { get; set; }

        public ObservableCollection<Row> Rows
        {
            get
            {
                if (_rows == null)
                {
                    if (List != null)
                    {
                        _rows = List.LoadRows();
                    }
                }
                return _rows;
            }
        }

        public List Import(Folder folder, string targetName, ImportOptionsDefinition options)
        {
            if (folder == null)
                throw new ArgumentNullException("folder");

            if (targetName == null)
                throw new ArgumentNullException("targetName");

            if (options == null)
                throw new ArgumentNullException("options");

            List newList = new List();
            if (options.ReplaceAllRows)
            {
                var existingRows = options.ExistingList.LoadRows();

                bool hasUniqueColumn = options.ExistingList.Columns.Any(c => (c.Options & ColumnOptions.IsUnique) == ColumnOptions.IsUnique);
                if (hasUniqueColumn)
                {
                    // need to delete first
                    object db = folder.Server.PostCall("row/deletebatch", null, null, existingRows.ToArray());
                }

                List<Row> addedRows = new List<Row>();
                foreach (Row row in Rows)
                {
                    row.ListId = options.ExistingList.Id;
                    row.Id = Guid.Empty;
                    Row newRow = new Row();
                    folder.Server.PostCall("row/save", newRow, null, row);
                    if (newRow.Id != Guid.Empty)
                    {
                        addedRows.Add(newRow);
                    }
                }

                if (!hasUniqueColumn)
                {
                    // ok?
                    if (addedRows.Count == Rows.Count)
                    {
                        // delete old rows
                        object db = folder.Server.PostCall("row/deletebatch", null, null, existingRows.ToArray());
                    }
                    else
                    {
                        // delete what was done
                        if (addedRows.Count > 0)
                        {
                            object db = folder.Server.PostCall("row/deletebatch", null, null, addedRows.ToArray());
                        }
                    }
                }
            }
            else if (options.ReplaceWithKey && !string.IsNullOrWhiteSpace(options.ReplaceColumn))
            {
                var existingRows = options.ExistingList.LoadRows();
                foreach (Row row in Rows)
                {
                    object key = row.GetValue(options.ReplaceColumn);
                    if (key == null)
                        continue;

                    Row existingRow= existingRows.FirstOrDefault(r => r.ValueEquals(options.ReplaceColumn, key));
                    Row updatedRow;
                    if (existingRow == null)
                    {
                        updatedRow = new Row();
                        updatedRow.ListId = newList.Id;
                    }
                    else
                    {
                        updatedRow = existingRow;
                        existingRow.Values = row.Values;
                    }

                    Row newRow = new Row();
                    folder.Server.PostCall("row/save", newRow, null, updatedRow);
                }
            }
            else
            {
                List.DisplayName = targetName;
                List.Id = Guid.Empty;
                List.FolderId = folder.Id;
                folder.Server.PostCall("list/save", newList, folder, List);
                if (newList.Id == Guid.Empty)
                    return null;

                foreach (Column column in List.Columns)
                {
                    column.ListId = newList.Id;
                    column.Id = Guid.Empty;
                    Column newColumn = new Column();
                    folder.Server.PostCall("column/save", newColumn, null, column);
                }

                foreach (Row row in Rows)
                {
                    row.ListId = newList.Id;
                    row.Id = Guid.Empty;
                    Row newRow = new Row();
                    folder.Server.PostCall("row/save", newRow, null, row);
                }
            }
            folder.Reload();
            return newList;
        }
    }
}
