namespace RowShareTool.Model
{
    public class CsvExporterOptions : BaseDataExporterOptions
    {
        public const char DefaultSeparator = ';';

        public CsvExporterOptions()
        {
            Separator = DefaultSeparator;
            ListFileNameFormat = "{0}.csv";
        }

        public char Separator { get; set; }
    }
}