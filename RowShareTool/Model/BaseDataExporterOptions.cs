using System.Text;

namespace RowShareTool.Model
{
    public abstract class BaseDataExporterOptions
    {
        public BaseDataExporterOptions()
        {
            Encoding = Encoding.UTF8;
            ContinueOnError = true;
            FolderFolderNameFormat = "{0}";
            FolderFileNameFormat = "{0}";
            ListFileNameFormat = "{0}";
            ListBlobsFolderNameFormat = "{0}_Blobs";
        }

        public Encoding Encoding { get; set; }
        public bool ContinueOnError { get; set; }
        public string FolderFolderNameFormat { get; set; }
        public string FolderFileNameFormat { get; set; }
        public string ListFileNameFormat { get; set; }
        public string ListBlobsFolderNameFormat { get; set; }
    }
}