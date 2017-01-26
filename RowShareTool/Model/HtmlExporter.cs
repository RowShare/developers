using CodeFluent.Runtime.Utilities;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;

namespace RowShareTool.Model
{
    public class HtmlExporter : BaseDataExporter
    {
        public HtmlExporter()
            : this(null)
        {
        }

        public HtmlExporter(HtmlExporterOptions options)
            : base(options ?? new HtmlExporterOptions())
        {
        }

        protected override bool WriteFolder(Folder folder, string targetPath, out string folderPath)
        {
            var folderFolderName = LongPath.ToValidFileName(string.Format(Options.FolderFolderNameFormat, folder.DisplayName));
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
                WriteHeader(writer, list);

                writer.Write("<tr>");
                var columns = list.Columns.OrderBy(c => c.SortOrder).ToArray();
                foreach  (var column in columns)
                {
                    writer.Write("<td>" + HtmlEncode(column.DisplayName) + "</td>");
                }
                writer.Write("</tr>");

                var rows = list.LoadRows();
                foreach (var row in rows)
                {
                    writer.Write("<tr>");
                    foreach (var column in columns)
                    {
                        string value = null;
                        if (column.DataType == ColumnDataType.Blob)
                        {
                            var blobProperties = row.GetValue<Dictionary<string, object>>(column.Name, null);
                            if (blobProperties != null)
                            {
                                var fileName = blobProperties.GetValue<string>("FileName", null);
                                var contentType = blobProperties.GetValue<string>("ContentType", null);

                                value = ExtractBlobFromRow(list, row, column, listBlobsPath);
                                if (value != null)
                                {
                                    var isImage = (column.Options & ColumnOptions.IsImage) == ColumnOptions.IsImage ||
                                                  contentType != null && contentType.StartsWith("image/");

                                    value = string.Format("<a href=\"{0}\">{1}</a>", value, isImage ? string.Format("<img src=\"{0}\" alt=\"{1}\" />", value, fileName) : fileName);
                                }
                            }
                        }
                        else
                        {
                            value = HtmlEncode(row.GetValue<string>(column.Name, null));
                        }
                        writer.Write(string.Format("<td>{0}</td>", value));
                    }
                    writer.Write("</tr>");
                }

                WriteFooter(writer);
            }
            return true;
        }

        private void WriteHeader(StreamWriter writer, List list)
        {
            writer.Write(
                "<html xmlns:x=\"urn: schemas - microsoft - com:office: excel\">" +
                "<head>" +
                "<meta http-equiv=\"Content-Type\" content=\"text/html; charset=utf-8\">" +
                "<title>" + HtmlEncode(list.DisplayName) + "</title>" +
                "<style>" +
                "td" +
                "{" +
                "	white-space:nowrap;" +
                "}" +
                "  <!-- table" +
                "  @page" +
                "     {mso-header-data:\"&CMultiplication Table\\000ADate\\: &D\\000APage &P\";" +
                "	mso-page-orientation:landscape;}" +
                "     br" +
                "     {mso-data-placement:same-cell;}" +
                "  -->" +
                "  font-" +
                "</style>" +
                "  <!--[if gte mso 9]><xml>" +
                "   <x:ExcelWorkbook>" +
                "    <x:ExcelWorksheets>" +
                "     <x:ExcelWorksheet>" +
                "      <x:Name>Sample Workbook</x:Name>" +
                "      <x:WorksheetOptions>" +
                "       <x:Print>" +
                "        <x:ValidPrinterInfo/>" +
                "       </x:Print>" +
                "      </x:WorksheetOptions>" +
                "     </x:ExcelWorksheet>" +
                "    </x:ExcelWorksheets>" +
                "   </x:ExcelWorkbook>" +
                "  </xml><![endif]-->" +
                "</head><body><table>"
                );
        }
        private void WriteFooter(StreamWriter writer)
        {
            writer.Write("</table></body></html>");
        }

        internal static string HtmlEncode(string text)
        {
            return text != null ? WebUtility.HtmlEncode(text) : null;
        }
    }
}