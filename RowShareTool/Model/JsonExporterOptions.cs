using CodeFluent.Runtime.Utilities;

namespace RowShareTool.Model
{
    public class JsonExporterOptions : BaseDataExporterOptions
    {
        public JsonExporterOptions()
        {
            FolderFileNameFormat = "{0}.js";
            ListFileNameFormat = "{0}.js";
            SerializationOptions = JsonSerializationOptions.Default;
        }

        public JsonSerializationOptions SerializationOptions { get; set; }
    }
}