using System.Windows;
using System.Windows.Input;
using System.Windows.Navigation;
using CodeFluent.Runtime.Utilities;
using RowShareTool.Model;

namespace RowShareTool
{
    public partial class ProviderLogin : Window
    {
        public ProviderLogin(string url, LoginProvider provider)
        {
            Url = url;
            Provider = provider;
            InitializeComponent();
            Loaded += ProviderLogin_Loaded;
            WB.Navigated += WB_Navigated;
        }

        public string Url { get; private set; }
        public LoginProvider Provider { get; private set; }
        public string Cookie { get; private set; }

        private void ProviderLogin_Loaded(object sender, RoutedEventArgs e)
        {
            WB.Navigate(Url + "/clientlogin.aspx?provider=" + Provider.Name);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (e.Key == Key.Escape)
            {
                e.Handled = true;
                Close();
            }
        }

        private void WB_Navigated(object sender, NavigationEventArgs e)
        {
            dynamic doc = WB.Document;
            string cookie = doc.Cookie;
            var coll = new NameValueCollectionCollection(cookie);
            Cookie = ConvertUtilities.Nullify(coll.GetValue<string>(Server.CookieName, null), true);
            if (Cookie != null)
            {
                DialogResult = true;
                Close();
            }
        }
    }
}
