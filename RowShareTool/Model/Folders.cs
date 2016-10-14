using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeFluent.Runtime.Utilities;

namespace RowShareTool.Model
{
    public class Folders : TreeItem
    {
        private Folder _userRootFolder;

        public Folders(Server server)
            : base(server, true)
        {
            DisplayName = "Folders";
        }

        public new Server Parent
        {
            get
            {
                return (Server)base.Parent;
            }
        }

        [Browsable(false)]
        [JsonUtilities(IgnoreWhenSerializing = true)]
        public Folder UserRootFolder
        {
            get
            {
                if (_userRootFolder == null && Parent.User != null && Parent.User.RootFolderId != Guid.Empty)
                {
                    _userRootFolder = new Folder(this);
                    Parent.Call("folder/load/" + Parent.User.RootFolderIdN, _userRootFolder, null);
                }
                return _userRootFolder;
            }
        }

        public override void Reload()
        {
            _userRootFolder = null;
            ChildrenClear();
            LoadChildren();
        }

        protected override void LoadChildren()
        {
            base.LoadChildren();
            var folder = UserRootFolder;
            if (folder != null)
            {
                Children.Add(folder);
            }

            if (Parent.User != null)
            {
                Parent.User.LazyLoadChildren();
                foreach (var org in Parent.User.Children.OfType<Organization>())
                {
                    var orgFolder = new Folder(this);
                    Parent.Call("folder/load/" + org.RootFolderIdN, orgFolder, null);
                    if (orgFolder != null)
                    {
                        Children.Add(orgFolder);
                    }
                }
            }
            OnPropertyChanged(nameof(Children));
        }
    }
}
