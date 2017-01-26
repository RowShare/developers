using CodeFluent.Runtime.Utilities;
using System.IO;
using System.Linq;

namespace RowShareTool.Model
{
    public class CsvExporter : BaseDataExporter
    {
        public CsvExporter()
            : this(null)
        {
        }

        public CsvExporter(CsvExporterOptions options)
            : base(options ?? new CsvExporterOptions())
        {
        }

        public new CsvExporterOptions Options
        {
            get
            {
                return (CsvExporterOptions)base.Options;
            }
        }

        protected override bool WriteFolder(Folder folder, string targetPath, out string folderPath)
        {
            var folderFolderName = LongPath.ToValidFileName(folder.DisplayName);
            folderPath = LongPath.Combine(targetPath, folderFolderName);
            return true;
        }

        protected override bool WriteList(List list, string targetPath, out string listPath)
        {
            var listFileName = LongPath.ToValidFileName(string.Format(Options.ListFileNameFormat, list.DisplayName));
            var listBlobsPath = LongPath.Combine(targetPath, LongPath.ToValidFileName(string.Format(Options.ListBlobsFolderNameFormat, list.DisplayName)));

            listPath = LongPath.Combine(targetPath, listFileName);
            using (var writer = new StreamWriter(listPath, false, Options.Encoding))
            {
                var columns = list.Columns.OrderBy(c => c.SortOrder).ToArray();
                var sep = false;
                foreach (var column in columns)
                {
                    if (sep)
                    {
                        writer.Write(Options.Separator);
                    }
                    else
                    {
                        sep = true;
                    }
                    writer.Write(CsvEncode(column.DisplayName));
                }
                writer.WriteLine();

                var rows = list.LoadRows();
                foreach (var row in rows)
                {
                    sep = false;
                    foreach (var column in columns)
                    {
                        if (sep)
                        {
                            writer.Write(Options.Separator);
                        }
                        else
                        {
                            sep = true;
                        }

                        object value = null;
                        if (column.DataType == ColumnDataType.Blob)
                        {
                            if (list.Parent != null && list.Parent.Server != null)
                            {
                                // we can download the blob
                                value = ExtractBlobFromRow(list, row, column, listBlobsPath);
                            }
                        }
                        else
                        {
                            value = row.GetValue(column.Name);
                        }

                        if (value != null)
                        {
                            writer.Write(value);
                        }
                    }
                    writer.WriteLine();
                }
            }
            return true;
        }

        internal static string CsvEncode(string text)
        {
            if (text == null)
                return null;

            if (text.IndexOf('"') >= 0 || text.IndexOf(';') >= 0)
                return "\"" + text.Replace("\"", "\"\"") + "\"";

            return text;
        }
    }
}