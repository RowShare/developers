using System;
using System.Collections.Generic;

namespace RowShareTool.Model
{
    public interface IDataExporter
    {
        event EventHandler<FolderImporterEventArgs> OnError;
        event EventHandler<FolderImporterEventArgs> OnMessage;

        IReadOnlyCollection<FolderImporterMessage> ErrorMessages { get; }

        void ExportFolder(Folder folder, string targetPath, bool recursive);
        void ExportList(List list, string targetPath);
    }
}