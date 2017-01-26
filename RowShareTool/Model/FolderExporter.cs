using CodeFluent.Runtime.Utilities;
using System;
using System.Collections.Generic;

namespace RowShareTool.Model
{
    public class FolderExporter
    {
        public static readonly Type DefaultDataExporterType = typeof(HtmlExporter);
        private readonly List<FolderImporterMessage> _errorMessages = new List<FolderImporterMessage>();

        private IDataExporter _dataExporter;
        private Folder _folder;
        private Type _dataExporterType;

        public FolderExporter(Folder folder)
        {
            if (folder == null)
                throw new ArgumentNullException(nameof(folder));

            var sourceServer = folder.Server.Clone();
            var sourceFolders = new Folders(sourceServer);

            _folder = new Folder(sourceFolders);
            _folder.Id = folder.Id;
            _folder.Reload();
        }

        public static IEnumerable<Type> KnownExporters
        {
            get
            {
                yield return typeof(CsvExporter);
                yield return typeof(JsonExporter);
                yield return typeof(HtmlExporter);
            }
        }

        public Type DataExporterType
        {
            get
            {
                if (_dataExporterType == null)
                {
                    _dataExporterType = DefaultDataExporterType;
                }
                return _dataExporterType;
            }
            set
            {
                _dataExporterType = value;
                _dataExporter = null;
            }
        }

        public IDataExporter DataExporter
        {
            get
            {
                if (_dataExporter == null)
                {
                    try
                    {
                        _dataExporter = (IDataExporter)Activator.CreateInstance(DataExporterType);
                    }
                    catch (Exception ex)
                    {
                        ErrorBox.ShowException(ex);
                    }
                }
                return _dataExporter;
            }
            set
            {
                _dataExporter = value;
                _dataExporterType = value?.GetType();
            }
        }

        public bool ExportTo(string targetPath)
        {
            if (targetPath == null)
                throw new ArgumentNullException(nameof(targetPath));

            if (!LongPath.DirectoryExists(targetPath))
                throw new ArgumentException("Invalid path.", nameof(targetPath));

            _folder.Server.ShowMessageBoxOnError = false;
            try
            {
                DataExporter.ExportFolder(_folder, targetPath, true);
            }
            finally
            {
                _folder.Server.ShowMessageBoxOnError = true;
            }
            return DataExporter.ErrorMessages.Count == 0;
        }
    }
}
