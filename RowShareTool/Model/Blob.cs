using System;
using System.Collections.Generic;
using CodeFluent.Runtime.Utilities;

namespace RowShareTool.Model
{
    public class Blob : IUploadableFile
    {
        private Server _server;
        private Column _column;
        private Row _row;

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
            _column = column;
            _row = row;
            _server = server;
        }

        public string ContentType { get; set; }
        public string FileName { get; set; }
        public string ImageUrl { get; set; }
        public long Size { get; set; }
        public DateTime LastWriteTimeUtc { get; set; }

        public string TempFilePath { get; private set; }
        public string RowImageUrl { get { return "/blob/" + _row.IdN + _column.Index + "/" + (int)BlobUrlType.Raw + "/"; } }

        string IUploadableFile.FormName
        {
            get { return _column.Name; }
        }

        public override string ToString()
        {
            return FileName;
        }

        public void DownloadFile()
        {
            TempFilePath = _server.DownloadCall(RowImageUrl);
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