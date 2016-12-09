using System;
using System.Linq;
using CodeFluent.Runtime.Utilities;

namespace RowShareTool.Model
{
    public class ListIcon : IUploadableFile
    {
        private List _list;
        private Column _column;

        public ListIcon(List list)
        {
            if (list == null)
                throw new ArgumentNullException(nameof(list));

            _list = list;

            if (!string.IsNullOrEmpty(_list.IconPath))
            {
                var segments = ConvertUtilities.SplitToList<string>(_list.IconPath, '/');
                var lastsegment = segments.LastOrDefault();
                if (lastsegment != null)
                {
                    FileName = lastsegment;
                }
            }
        }

        public string FormName { get { return "icon"; } }
        public string ContentType { get { return "application/octet-stream"; } }
        public string FileName { get; private set; }
        public string TempFilePath { get; private set; }

        public void DownloadFile()
        {
            if(_list.Parent == null || _list.Parent.Server == null)
                throw new InvalidOperationException("No parent server attached");

            TempFilePath = _list.Parent.Server.DownloadCall(_list.IconPath);
        }
    }
}