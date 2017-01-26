using CodeFluent.Runtime.Utilities;
using System.IO;

namespace RowShareTool.Model
{
    public class JsonExporter : BaseDataExporter
    {
        public JsonExporter()
            : this(null)
        {
        }

        public JsonExporter(JsonExporterOptions options)
            : base(options ?? new JsonExporterOptions())
        {
        }

        public new JsonExporterOptions Options
        {
            get
            {
                return (JsonExporterOptions)base.Options;
            }
        }

        protected override bool WriteFolder(Folder folder, string targetPath, out string folderPath)
        {
            var folderFolderName = LongPath.ToValidFileName(folder.DisplayName);
            var folderFileName = LongPath.Combine(targetPath, string.Format(Options.FolderFileNameFormat, folder.DisplayName));

            using (var writer = new StreamWriter(folderFileName, false, Options.Encoding))
            {
                JsonUtilities.Serialize(writer, folder, Options.SerializationOptions);
            }

            folderPath = LongPath.Combine(targetPath, folderFolderName);
            return true;
        }

        protected override bool WriteList(List list, string targetPath, out string listPath)
        {
            var listFileName = LongPath.ToValidFileName(string.Format(Options.ListFileNameFormat, list.DisplayName));
            var listBlobsPath = LongPath.Combine(targetPath, LongPath.ToValidFileName(string.Format(Options.ListBlobsFolderNameFormat, list.DisplayName)));

            var lwr = new ListWithRows(list);
            foreach (var row in lwr.Rows)
            {
                foreach (var column in list.Columns)
                {
                    if (column.DataType == ColumnDataType.Blob)
                    {
                        row.Values[column.Name] = ExtractBlobFromRow(list, row, column, listBlobsPath);
                    }
                }
            }

            listPath = LongPath.Combine(targetPath, listFileName);
            using (var writer = new StreamWriter(listPath, false, Options.Encoding))
            {
                JsonUtilities.Serialize(writer, lwr, Options.SerializationOptions);
            }
            return true;
        }
    }
}