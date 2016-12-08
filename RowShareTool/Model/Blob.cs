using System;
using System.Collections.Generic;
using CodeFluent.Runtime.Utilities;

namespace RowShareTool.Model
{
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