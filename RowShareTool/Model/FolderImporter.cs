using System;
using System.Collections.Generic;
using System.Linq;

namespace RowShareTool.Model
{
    public class FolderImporter
    {
        private readonly Folder _inputFolder;
        private readonly Folder _outputFolder;
        private readonly List<FolderImporterMessage> _errorMessages = new List<FolderImporterMessage>();

        public Folder SourceInputFolder { get; }
        public Folder SourceOutputFolder { get; }

        public IReadOnlyCollection<FolderImporterMessage> ErrorMessages
        {
            get { return _errorMessages.AsReadOnly(); }
        }

        public event EventHandler<FolderImporterEventArgs> OnError;
        public event EventHandler<FolderImporterEventArgs> OnMessage;

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

            var iServer = inputFolder.Server.Clone();
            var iFolders = new Folders(iServer);
            var iFolder = new Folder(iFolders);
            iFolder.Id = inputFolder.Id;
            iFolder.Reload();


            var oServer = outputFolder.Server.Clone();
            var oFolders = new Folders(oServer);
            var oFolder = new Folder(oFolders);
            oFolder.Id = outputFolder.Id;
            oFolder.Reload();


            _inputFolder = iFolder;
            _outputFolder = oFolder;

            SourceInputFolder = inputFolder;
            SourceOutputFolder = outputFolder;
        }

        public bool CopyAllContent()
        {
            _errorMessages.Clear();

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
            foreach (var childFolder in inputFolder.Children.OfType<Folder>())
            {
                var newFolder = CopyFolder(childFolder, outputFolder);
                CopyFolderContent(childFolder, newFolder);
            }

            inputFolder.LazyLoadChildren();
            foreach (var childList in inputFolder.Children.OfType<List>())
            {
                try
                {
                    CopyList(childList, outputFolder);
                }
                catch (Exception ex)
                {
                    AddError(
                        ex,
                        folderId: childList.Parent.IdN,
                        folderName: childList.Parent.DisplayName,
                        listId: childList.IdN,
                        listName: childList.DisplayName);
                }
            }

            AddMessage("Folder processed",
                folderId: inputFolder.IdN,
                folderName: inputFolder.DisplayName);
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

                AddMessage("Folder created",
                    folderId: inputFolder.IdN,
                    folderName: inputFolder.DisplayName);
            }
            else
            {
                AddMessage("Folder creation skipped",
                    folderId: inputFolder.IdN,
                    folderName: inputFolder.DisplayName);
            }
            return outputFolder;
        }

        private void CopyList(List inputList, Folder targetFolder)
        {
            targetFolder.LazyLoadChildren();
            var outputList =
                targetFolder.Children.OfType<List>()
                    .FirstOrDefault(f => f.DisplayName.EqualsIgnoreCase(inputList.DisplayName));

            if (outputList == null)
            {
                AddMessage("List saving",
                    folderId: inputList.Parent.IdN,
                    folderName: inputList.Parent.DisplayName,
                    listId: inputList.IdN,
                    listName: inputList.DisplayName);

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
                    OneRowMaximumPerUser = inputList.OneRowMaximumPerUser,
                    AllowPublic = inputList.AllowPublic,
                    ShowTotals = inputList.ShowTotals,
                    Summary = inputList.Summary,
                    ColorOrDefault = inputList.Color,
                    CategoryId = inputList.CategoryId,
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
                        folderId: inputList.Parent.IdN,
                        folderName: inputList.Parent.DisplayName,
                        listId: inputList.IdN,
                        listName: inputList.DisplayName);
                }
                outputList.Reload();

                CopyListIcon(inputList, outputList);

                AddMessage("List saved",
                    folderId: inputList.Parent.IdN,
                    folderName: inputList.Parent.DisplayName,
                    listId: inputList.IdN,
                    listName: inputList.DisplayName);
            }
            else
            {
                AddMessage("List skipped",
                    folderId: inputList.Parent.IdN,
                    folderName: inputList.Parent.DisplayName,
                    listId: inputList.IdN,
                    listName: inputList.DisplayName);
            }
        }

        private void CopyListIcon(List inputList, List targetList)
        {
            try
            {
                var inputServer = inputList.Parent.Server;
                var outputServer = targetList.Parent.Server;
                if (string.IsNullOrEmpty(inputList.IconPath))
                    return;

                var listIcon = new ListIcon(inputList);
                listIcon.DownloadFile();

                outputServer.PostBlobCall("list/save/" + targetList.IdN + "/icon", null, null, null, listIcon);
            }
            catch (Exception ex)
            {
                AddError(
                    ex,
                    folderId: inputList.Parent.IdN,
                    folderName: inputList.Parent.DisplayName,
                    listId: inputList.IdN,
                    listName: inputList.DisplayName);
            }
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
                var mandatoryBlobs = new List<Blob>();

                foreach (var column in inputList.Columns)
                {
                    if (column.IsReadOnly || (column.Options & ColumnOptions.IsComputed) == ColumnOptions.IsComputed)
                        continue;

                    if (column.DataType == ColumnDataType.Blob)
                    {
                        var dico = inputRow.GetValue<Dictionary<string, object>>(column.Name, null);
                        if (dico != null)
                        {
                            var blob = new Blob(dico, column, inputRow, inputServer);
                            bool isMandatoryBlob = (column.Options & ColumnOptions.IsMandatory) == ColumnOptions.IsMandatory;
                            if (isMandatoryBlob)
                            {
                                mandatoryBlobs.Add(blob);
                            }
                            else
                            {
                                blobs.Add(blob);
                            }
                        }
                        continue;
                    }

                    values[column.Name] = inputRow.GetValue(column.Name);
                }
                var outputRow = new Row(targetList);
                var newRowData = new
                {
                    ListId = targetList.IdN,
                    Values = values,
                };

                try
                {
                    if (mandatoryBlobs.Count == 0)
                    {
                        outputServer.PostCall("row/save", outputRow, targetList, newRowData);
                    }
                    else
                    {
                        foreach (var blob in mandatoryBlobs)
                        {
                            blob.DownloadFile();
                        }

                        outputServer.PostBlobCall("row/save", outputRow, targetList, newRowData, mandatoryBlobs);
                    }
                }
                catch (Exception ex)
                {
                    AddError(
                        ex,
                        folderId: inputList.Parent.IdN,
                        folderName: inputList.Parent.DisplayName,
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
                            folderId: inputList.Parent.IdN,
                            folderName: inputList.Parent.DisplayName,
                            listId: inputList.IdN,
                            listName: inputList.DisplayName,
                            rowId: inputRow.IdN,
                            rowName: inputRow.DisplayName,
                            blobId: blob.ImageUrl,
                            blobName: ((IUploadableFile)blob).FormName);
                    }
                }
            }
        }

        private void AddError(Exception ex, string folderId = null, string folderName = null, string listId = null, string listName = null, string rowId = null, string rowName = null, string blobId = null, string blobName = null)
        {
            var error = new FolderImporterMessage();
            error.EventName = "Error";
            error.FolderId = folderId;
            error.FolderName = folderName;
            error.ListId = listId;
            error.ListName = listName;
            error.RowId = rowId;
            error.RowName = rowName;
            error.BlobId = blobId;
            error.BlobName = blobName;
            if (ex != null)
            {
                error.ExceptionMessage = ex.GetErrorText();
            }

            _errorMessages.Add(error);

            var handler = OnError;
            if (handler != null)
            {
                handler(this, new FolderImporterEventArgs(error));
            }
        }

        private void AddMessage(string eventName, string folderId = null, string folderName = null, string listId = null, string listName = null)
        {
            var error = new FolderImporterMessage();
            error.EventName = eventName;
            error.FolderId = folderId;
            error.FolderName = folderName;
            error.ListId = listId;
            error.ListName = listName;

            var handler = OnMessage;
            if (handler != null)
            {
                handler(this, new FolderImporterEventArgs(error));
            }
        }
    }

    public class FolderImporterMessage
    {
        public string EventName { get; set; }
        public string FolderId { get; set; }
        public string FolderName { get; set; }
        public string ListId { get; set; }
        public string ListName { get; set; }
        public string RowId { get; set; }
        public string RowName { get; set; }
        public string BlobId { get; set; }
        public string BlobName { get; set; }
        public string ExceptionMessage { get; set; }
    }

    public class FolderImporterEventArgs : EventArgs
    {
        public FolderImporterMessage Message { get; }

        public FolderImporterEventArgs(FolderImporterMessage message)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));
            Message = message;
        }
    }
}
