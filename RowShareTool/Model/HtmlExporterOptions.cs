namespace RowShareTool.Model
{
    public class HtmlExporterOptions : BaseDataExporterOptions
    {
        public HtmlExporterOptions()
        {
            ListFileNameFormat = "{0}.html";
            ListBlobsFolderNameFormat = "{0}_Files";
        }
    }
}