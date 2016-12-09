namespace RowShareTool.Model
{
    public interface IUploadableFile
    {
        string FormName { get; }
        string ContentType { get; }
        string FileName { get; }
        string TempFilePath { get; }
    }
}