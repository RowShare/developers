using System;
using System.Collections.Generic;
using System.Linq;
using CodeFluent.Runtime.Utilities;

namespace RowShareTool.Model
{
    public class FolderImporter
    {
        private readonly Folder _inputFolder;
        private readonly Folder _outputFolder;
        public List<FolderImporterError> ErrorMessages { get; set; } = new List<FolderImporterError>();

        public FolderImporter(Folder inputFolder, Folder outputFolder)
        {
            if (inputFolder == null)
                throw new ArgumentNullException(nameof(inputFolder));

            if (outputFolder == null)
                throw new ArgumentNullException(nameof(outputFolder));

            if (inputFolder == outputFolder)
                throw new ArgumentException("output folder must be different from input folder", nameof(outputFolder));

            if (inputFolder.Server == null)
                throw new ArgumentException("inputFolder has an invalid server property", nameof(inputFolder));

            if (outputFolder.Server == null)
                throw new ArgumentException("outputFolder has an invalid server property", nameof(outputFolder));

            if (inputFolder.Server.CompareTo(outputFolder.Server) == 0)
                throw new ArgumentException("outputFolder must have a different than inputFolder", nameof(outputFolder));

            _inputFolder = inputFolder;
            _outputFolder = outputFolder;
        }

        public bool CopyAllContent()
        {
            ErrorMessages.Clear();

            bool success = true;

            _inputFolder.Server.ShowMessageBoxOnError = false;
            _outputFolder.Server.ShowMessageBoxOnError = false;

            try
            {
                CopyFolderContent(_inputFolder, _outputFolder);

                _outputFolder.ReloadChildren();
                _outputFolder.LazyLoadChildren();
            }
            finally
            {
                _inputFolder.Server.ShowMessageBoxOnError = true;
                _outputFolder.Server.ShowMessageBoxOnError = true;
            }

            return ErrorMessages.Count == 0;
        }

        private void CopyFolderContent(Folder inputFolder, Folder outputFolder)
        {
            inputFolder.LazyLoadChildren();
            foreach (var childList in inputFolder.Children.OfType<List>())
            {
                var newList = CopyList(childList, outputFolder);
            }

            inputFolder.LazyLoadChildren();
            foreach (var childFolder in inputFolder.Children.OfType<Folder>())
            {
                var newFolder = CopyFolder(childFolder, outputFolder);
                CopyFolderContent(childFolder, newFolder);
            }
        }

        private Folder CopyFolder(Folder inputFolder, Folder targetFolder)
        {
            targetFolder.LazyLoadChildren();
            var outputFolder =
                targetFolder.Children.OfType<Folder>()
                    .FirstOrDefault(f => f.DisplayName.EqualsIgnoreCase(inputFolder.DisplayName));

            if (outputFolder == null)
            {
                outputFolder = new Folder(targetFolder);

                var server = outputFolder.Server;
                var data = new
                {
                    ParentId = targetFolder.IdN,
                    DisplayName = inputFolder.DisplayName,
                    MetaData = inputFolder.MetaData
                };
                server.PostCall("folder/save", outputFolder, targetFolder, data);
            }
            return outputFolder;
        }

        private List CopyList(List inputList, Folder targetFolder)
        {
            targetFolder.LazyLoadChildren();
            var outputList =
                targetFolder.Children.OfType<List>()
                    .FirstOrDefault(f => f.DisplayName.EqualsIgnoreCase(inputList.DisplayName));

            if (outputList == null)
            {
                outputList = new List(targetFolder);

                var server = targetFolder.Server;
                var data = new
                {
                    FolderId = targetFolder.IdN,
                    AccessMode = inputList.AccessMode,
                    Lcid = inputList.Lcid,
                    Description = inputList.Description,
                    DisplayName = inputList.DisplayName,
                    //Options = inputList.Options,
                    //OneRowMaximumPerUser = inputList.OneRowMaximumPerUser,
                    //AllowPublic = inputList.AllowPublic,
                    //ShowTotals = inputList.ShowTotals,
                    Summary = inputList.Summary,
                    //ColorOrDefault = inputList.ColorOrDefault,
                    CategoryId = inputList.CategoryId,
                    //ColorOrDefault = inputList.ColorOrDefault,
                };
                server.PostCall("list/save", outputList, targetFolder, data);

                CopyColumns(inputList, outputList);
                outputList.Reload();

                try
                {
                    CopyRows(inputList, outputList);
                }
                catch (Exception ex)
                {
                    AddError(
                        ex,
                        listId: inputList.IdN,
                        listName: inputList.DisplayName);
                }
                outputList.Reload();
            }
            return outputList;
        }

        private void CopyColumns(List inputList, List targetList)
        {
            var server = targetList.Parent.Server;

            foreach (var inputColumn in inputList.Columns.OrderBy(c => c.SortOrder))
            {
                var column = new Column();
                var data = new
                {
                    ListId = targetList.IdN,
                    Name = inputColumn.Name,
                    DisplayName = inputColumn.DisplayName,
                    SubName = inputColumn.SubName,
                    Format = inputColumn.Format,
                    MinValue = inputColumn.MinValue,
                    MaxValue = inputColumn.MaxValue,
                    MaxDecimals = inputColumn.MaxDecimals,
                    MaxLength = inputColumn.MaxLength,
                    //Mask = inputColumn.Mask,
                    LookupListColumnName = inputColumn.LookupListColumnName,
                    LookupValues = inputColumn.LookupValues,
                    SortOrder = inputColumn.SortOrder,
                    DataType = inputColumn.DataType,
                    Renderer = inputColumn.Renderer,
                    Options = inputColumn.Options,
                    DefaultValue = inputColumn.DefaultValue,
                    Description = inputColumn.Description,
                    MetaData = inputColumn.MetaData,
                    LookupListId = inputColumn.LookupListId,
                };
                server.PostCall("column/save", column, targetList, data);

                targetList.Columns.Add(column);
            }
        }

        private void CopyRows(List inputList, List targetList)
        {
            var outputServer = targetList.Parent.Server;
            var inputServer = inputList.Parent.Server;

            var inputRows = inputList.LoadRows();

            foreach (var inputRow in inputRows.OrderBy(r => r.Index))
            {
                var values = new Dictionary<string, object>();
                var blobs = new List<Blob>();

                foreach (var column in inputList.Columns)
                {
                    if (column.IsReadOnly || (column.Options & ColumnOptions.IsComputed) == ColumnOptions.IsComputed)
                        continue;

                    if (column.DataType == ColumnDataType.Blob)
                    {
                        var dico = inputRow.GetValue<Dictionary<string, object>>(column.Name, null);
                        if (dico != null)
                        {
                            blobs.Add(new Blob(dico, column.Name, inputServer));
                        }
                        continue;
                    }

                    values[column.Name] = inputRow.GetValue(column.Name);
                }
                var outputRow = new Row(targetList);
                var newRowData = new
                {
                    Id = (string)null,
                    ListId = targetList.IdN,
                    Values = values,
                };

                try
                {
                    outputServer.PostCall("row/save", outputRow, targetList, newRowData);
                }
                catch (Exception ex)
                {
                    AddError(
                        ex,
                        listId: inputList.IdN,
                        listName: inputList.DisplayName,
                        rowId: inputRow.IdN,
                        rowName: inputRow.DisplayName);
                    continue;
                }


                var rowData = new
                {
                    Id = outputRow.IdN,
                    Version = "0000000000000000",
                    Values = new Dictionary<string, object>(),
                };

                foreach (var blob in blobs)
                {
                    try
                    {
                        blob.DownloadFile();

                        outputServer.PostBlobCall("row/save", null, null, rowData, blob);
                    }
                    catch (Exception ex)
                    {
                        AddError(
                            ex,
                            listId: inputList.IdN,
                            listName: inputList.DisplayName,
                            rowId: inputRow.IdN,
                            rowName: inputRow.DisplayName,
                            blobId: blob.ImageUrl,
                            blobName: blob.ColumnName);
                    }
                }
            }
        }

        private void AddError(Exception ex, string listId = null, string listName = null, string rowId = null, string rowName = null,
            string blobId = null, string blobName = null)
        {
            var error = new FolderImporterError();
            error.ListId = listId;
            error.ListName = listName;
            error.RowId = rowId;
            error.RowName = rowName;
            error.BlobId = blobId;
            error.BlobName = blobName;
            error.ExceptionMessage = ex.GetErrorText();

            ErrorMessages.Add(error);
        }
    }

    public class FolderImporterError
    {
        public string ListId { get; set; }
        public string ListName { get; set; }
        public string RowId { get; set; }
        public string RowName { get; set; }
        public string BlobId { get; set; }
        public string BlobName { get; set; }
        public string ExceptionMessage { get; set; }
    }

    public class Blob
    {
        public Blob(Dictionary<string, object> dico, string columnName, Server server)
        {
            if (dico == null)
                throw new ArgumentNullException(nameof(dico));
            if (columnName == null)
                throw new ArgumentNullException(nameof(columnName));
            if (server == null)
                throw new ArgumentNullException(nameof(server));

            JsonUtilities.Apply(dico, this, new JsonUtilitiesOptions());
            ColumnName = columnName;
            Server = server;
        }
        public string ContentType { get; set; }
        public string FileName { get; set; }
        public string ImageUrl { get; set; }
        public long Size { get; set; }
        public DateTime LastWriteTimeUtc { get; set; }

        public string ColumnName { get; private set; }
        public Server Server { get; private set; }

        public string TempFilePath { get; private set; }

        public override string ToString()
        {
            return FileName;
        }

        public void DownloadFile()
        {
            TempFilePath = Server.DownloadCall(ImageUrl);
        }
    }
}
