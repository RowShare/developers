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

        public event EventHandler<FolderImporterEventArgs> OnError;
        public event EventHandler<FolderImporterEventArgs> OnMessage;

        public Folder SourceFolder { get; }
        public Folder TargetFolder { get; }
        public IReadOnlyCollection<FolderImporterMessage> ErrorMessages => _errorMessages.AsReadOnly();

        public FolderImporter(Folder sourceFolder, Folder targetFolder)
        {
            if (sourceFolder == null)
                throw new ArgumentNullException(nameof(sourceFolder));

            if (targetFolder == null)
                throw new ArgumentNullException(nameof(targetFolder));

            if (sourceFolder == targetFolder)
                throw new ArgumentException(null, nameof(targetFolder));

            SourceFolder = sourceFolder;
            TargetFolder = targetFolder;

            var sourceServer = sourceFolder.Server.Clone();
            var sourceFolders = new Folders(sourceServer);
            var iFolder = new Folder(sourceFolders);
            iFolder.Id = sourceFolder.Id;
            iFolder.Reload();

            var targetServer = targetFolder.Server.Clone();
            var targetFolders = new Folders(targetServer);
            var oFolder = new Folder(targetFolders);
            oFolder.Id = targetFolder.Id;
            oFolder.Reload();

            _inputFolder = iFolder;
            _outputFolder = oFolder;
        }

        public bool CopyAllContent()
        {
            _errorMessages.Clear();

            _inputFolder.Server.ShowMessageBoxOnError = false;
            _outputFolder.Server.ShowMessageBoxOnError = false;

            try
            {
                CopyContent(_inputFolder, _outputFolder);

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

        private void CopyContent(Folder inputFolder, Folder outputFolder)
        {
            inputFolder.LazyLoadChildren();
            foreach (var childFolder in inputFolder.Children.OfType<Folder>())
            {
                var newFolder = CopyFolder(childFolder, outputFolder);
                CopyContent(childFolder, newFolder);
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

            AddMessage("Folder processed", folderId: inputFolder.IdN, folderName: inputFolder.DisplayName);
        }

        private Folder CopyFolder(Folder inputFolder, Folder targetFolder)
        {
            targetFolder.LazyLoadChildren();
            var outputFolder = targetFolder.Children.OfType<Folder>().FirstOrDefault(f => f.DisplayName.EqualsIgnoreCase(inputFolder.DisplayName));

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
                AddMessage("Folder created", folderId: inputFolder.IdN, folderName: inputFolder.DisplayName);
            }
            else
            {
                AddMessage("Folder creation skipped", folderId: inputFolder.IdN, folderName: inputFolder.DisplayName);
            }
            return outputFolder;
        }

        private void CopyList(List inputList, Folder targetFolder)
        {
            targetFolder.LazyLoadChildren();

            var outputList = targetFolder.Children.OfType<List>().FirstOrDefault(f => f.DisplayName.EqualsIgnoreCase(inputList.DisplayName));
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
                catch (Exception e)
                {
                    AddError(
                        e,
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
            catch (Exception e)
            {
                AddError(
                    e,
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
                    catch (Exception e)
                    {
                        AddError(
                            e,
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

        private void AddError(Exception exception, string folderId = null, string folderName = null, string listId = null, string listName = null, string rowId = null, string rowName = null, string blobId = null, string blobName = null)
        {
            var message = new FolderImporterMessage();
            message.EventName = "Error";
            message.FolderId = folderId;
            message.FolderName = folderName;
            message.ListId = listId;
            message.ListName = listName;
            message.RowId = rowId;
            message.RowName = rowName;
            message.BlobId = blobId;
            message.BlobName = blobName;
            if (exception != null)
            {
                message.ExceptionMessage = exception.GetErrorText();
            }

            _errorMessages.Add(message);

            OnError?.Invoke(this, new FolderImporterEventArgs(message));
        }

        private void AddMessage(string eventName, string folderId = null, string folderName = null, string listId = null, string listName = null)
        {
            var message = new FolderImporterMessage();
            message.EventName = eventName;
            message.FolderId = folderId;
            message.FolderName = folderName;
            message.ListId = listId;
            message.ListName = listName;
            OnMessage?.Invoke(this, new FolderImporterEventArgs(message));
        }
    }
}