using CodeFluent.Runtime.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RowShareTool.Model
{
    public abstract class BaseDataExporter : IDataExporter
    {
        protected readonly List<FolderImporterMessage> _errorMessages = new List<FolderImporterMessage>();

        public event EventHandler<FolderImporterEventArgs> OnError;
        public event EventHandler<FolderImporterEventArgs> OnMessage;

        public BaseDataExporter(BaseDataExporterOptions options)
        {
            if (options == null)
                throw new ArgumentNullException(nameof(options));

            Options = options;
        }

        public BaseDataExporterOptions Options { get; private set; }

        public IReadOnlyCollection<FolderImporterMessage> ErrorMessages => _errorMessages.AsReadOnly();

        public virtual void ExportFolder(Folder folder, string targetPath, bool recursive)
        {
            if (folder == null)
                throw new ArgumentNullException(nameof(folder));

            if (targetPath == null)
                throw new ArgumentNullException(nameof(targetPath));

            if (!LongPath.DirectoryExists(targetPath))
                throw new ArgumentException("Invalid path.", nameof(targetPath));

            _errorMessages.Clear();
            ExportFolderContent(folder, targetPath, recursive);
        }

        public virtual void ExportList(List list, string targetPath)
        {
            if (list == null)
                throw new ArgumentNullException(nameof(list));

            if (targetPath == null)
                throw new ArgumentNullException(nameof(targetPath));

            if (!LongPath.DirectoryExists(targetPath))
                throw new ArgumentException("Invalid path.", nameof(targetPath));

            _errorMessages.Clear();
            string listPath;
            WriteList(list, targetPath, out listPath);
        }

        protected virtual void ExportFolderContent(Folder folder, string targetPath, bool recursive)
        {
            string folderExportPath;
            try
            {

                if (!WriteFolder(folder, targetPath, out folderExportPath))
                {
                    AddMessage("Folder skipped", folderId: folder.IdN, folderName: folder.DisplayName);
                    return;
                }

                if (folderExportPath == null)
                {
                    // it means it will not be hierarchical
                    folderExportPath = targetPath;
                }

                LongPath.DirectoryCreate(folderExportPath);
                AddMessage("Folder created", folderId: folder.IdN, folderName: folder.DisplayName);
            }
            catch (Exception ex)
            {
                AddError(ex, folder.IdN, folder.DisplayName);
                return;
            }

            folder.LazyLoadChildren();
            if (recursive)
            {
                foreach (var childFolder in folder.Children.OfType<Folder>())
                {
                    ExportFolderContent(childFolder, folderExportPath, true);
                    if (_errorMessages.Count > 0 && !Options.ContinueOnError)
                    {
                        AddMessage("Folder processing failed", folderId: folder.IdN, folderName: folder.DisplayName);
                        return;
                    }
                }
            }

            foreach (var childList in folder.Children.OfType<List>())
            {
                try
                {
                    string listPath;
                    var msg = WriteList(childList, folderExportPath, out listPath) ? "created" : "skipped";
                    AddMessage("List " + msg,
                            folderId: childList.Parent.IdN,
                            folderName: childList.Parent.DisplayName,
                            listId: childList.IdN,
                            listName: childList.DisplayName);
                }
                catch (Exception ex)
                {
                    AddError(ex,
                         folderId: childList.Parent.IdN,
                         folderName: childList.Parent.DisplayName,
                         listId: childList.IdN,
                         listName: childList.DisplayName);

                    if (!Options.ContinueOnError)
                    {
                        AddMessage("Folder processing failed", folderId: folder.IdN, folderName: folder.DisplayName);
                        return;
                    }
                }
            }

            AddMessage("Folder processed", folderId: folder.IdN, folderName: folder.DisplayName);
        }

        protected abstract bool WriteFolder(Folder folder, string targetPath, out string folderPath);
        protected abstract bool WriteList(List list, string targetPath, out string listPath);

        protected void AddError(Exception exception, string folderId = null, string folderName = null, string listId = null, string listName = null, string rowId = null, string rowName = null, string blobId = null, string blobName = null)
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

        protected void AddMessage(string eventName, string folderId = null, string folderName = null, string listId = null, string listName = null, string rowId = null, string rowName = null, string blobId = null, string blobName = null)
        {
            var message = new FolderImporterMessage();
            message.EventName = eventName;
            message.FolderId = folderId;
            message.FolderName = folderName;
            message.ListId = listId;
            message.ListName = listName;
            message.RowId = rowId;
            message.RowName = rowName;
            message.BlobId = blobId;
            message.BlobName = blobName;
            OnMessage?.Invoke(this, new FolderImporterEventArgs(message));
        }

        protected virtual string ExtractBlobFromRow(List list, Row row, Column column, string listBlobsPath)
        {
            if (list == null)
                throw new ArgumentNullException(nameof(list));

            if (row == null)
                throw new ArgumentNullException(nameof(row));

            if (column == null)
                throw new ArgumentNullException(nameof(column));

            if (listBlobsPath == null)
                throw new ArgumentNullException(nameof(listBlobsPath));

            var blobProperties = row.GetValue<Dictionary<string, object>>(column.Name, null);
            if (blobProperties == null)
                return null;

            var blob = new Blob(blobProperties, column, row, list.Parent.Server);
            if (blob == null)
                return null;

            blob.DownloadFile();
            if (blob.TempFilePath == null)
                return null;

            LongPath.DirectoryCreate(listBlobsPath);

            var blobFilePath = GetUniqueBlobFilePath(blob, listBlobsPath);
            LongPath.FileCopy(blob.TempFilePath, blobFilePath, true);

            AddMessage("Blob extracted",
                   folderId: list.Parent.IdN,
                   folderName: list.Parent.DisplayName,
                   listId: list.IdN,
                   listName: list.DisplayName,
                   rowId: row.IdN,
                   rowName: row.DisplayName,
                   blobId: blob.ImageUrl,
                   blobName: ((IUploadableFile)blob).FormName);

            return blobFilePath;
        }

        private string GetUniqueBlobFilePath(Blob blob, string blobsPath)
        {
            var i = 0;
            do
            {
                string path;
                if (i == 0)
                {
                    path = LongPath.Combine(blobsPath, blob.FileName);
                }
                else
                {
                    path = LongPath.Combine(blobsPath, string.Format("{0} ({1}){2}", blob.FileNameWithoutExtension, i, blob.FileExtension));
                }

                if (!LongPath.FileExists(path))
                    return path;

                i++;
            }
            while (true);
        }
    }
}