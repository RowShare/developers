using System.ComponentModel;

namespace RowShareTool.Model
{
    public class Login : TreeItem
    {
        public Login(Server server)
            : base(server, false)
        {
            DisplayName = "Login ...";
        }

        [Browsable(false)]
        public new Server Parent
        {
            get
            {
                return (Server)base.Parent;
            }
        }

        [Browsable(false)]
        public override string DisplayName
        {
            get
            {
                return base.DisplayName;
            }

            set
            {
                base.DisplayName = value;
            }
        }

        [Browsable(false)]
        public override string Url
        {
            get
            {
                return base.Url;
            }
        }
    }
}
