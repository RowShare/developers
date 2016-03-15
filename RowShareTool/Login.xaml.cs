using System.Windows;
using System.Windows.Input;
using RowShareTool.Model;

namespace RowShareTool
{
    public partial class Login : Window
    {
        public Login(string url)
        {
            Url = url;
            InitializeComponent();
            DataContext = LoginProvider.All;
            Title = url;
        }

        public string Url { get; private set; }
        public string Cookie { get; private set; }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (e.Key == Key.Escape)
            {
                e.Handled = true;
                Close();
            }
        }

        private void ProviderButton_Click(object sender, RoutedEventArgs e)
        {
            var button = (FrameworkElement)sender;
            var provider = (LoginProvider)button.DataContext;

            ProviderLogin dlg = new ProviderLogin(Url, provider);
            dlg.Title = provider.DisplayName + " Login on " + Url;
            dlg.Owner = this;
            if (dlg.ShowDialog().GetValueOrDefault())
            {
                if (dlg.Cookie != null)
                {
                    Cookie = dlg.Cookie;
                    DialogResult = true;
                    Close();
                }
            }
        }
    }
}
