using System;
using System.Collections.Generic;
using CodeFluent.Runtime.Utilities;

namespace RowShareTool.Model
{
    public class Blob
    {
        public Blob(Dictionary<string, object> dico, Column column, Row row, Server server)
        {
            if (dico == null)
                throw new ArgumentNullException(nameof(dico));
            if (column == null)
                throw new ArgumentNullException(nameof(column));
            if (row == null)
                throw new ArgumentNullException(nameof(row));
            if (server == null)
                throw new ArgumentNullException(nameof(server));

            JsonUtilities.Apply(dico, this, new JsonUtilitiesOptions());
            Column = column;
            Row = row;
            Server = server;
        }
        public string ContentType { get; set; }
        public string FileName { get; set; }
        public string ImageUrl { get; set; }
        public long Size { get; set; }
        public DateTime LastWriteTimeUtc { get; set; }

        public Column Column { get; private set; }
        public Row Row { get; private set; }
        public Server Server { get; private set; }

        public string TempFilePath { get; private set; }

        public string ColumnName
        {
            get { return Column.Name; }
        }
        public string RowImageUrl { get { return "/blob/" + Row.IdN + Column.Index + "/" + (int)BlobUrlType.Raw + "/"; } }

        public override string ToString()
        {
            return FileName;
        }

        public void DownloadFile()
        {
            TempFilePath = Server.DownloadCall(RowImageUrl);
        }

        private enum BlobUrlType
        {
            Image = 0,
            Thumbnail = 1,
            FileExtension = 2,
            FileExtensionSmall = 3,
            Raw = 4,
        }
    }
}