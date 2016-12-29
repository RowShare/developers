using System;

namespace RowShareTool.Model
{
    public class FolderImporterEventArgs : EventArgs
    {
        public FolderImporterEventArgs(FolderImporterMessage message)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));

            Message = message;
        }

        public FolderImporterMessage Message { get; }
    }
}
