using System;
using System.Windows;
using System.Windows.Input;

namespace RowShareTool
{
    public partial class Connect : Window
    {
        public Connect(SettingsServer server)
        {
            if(server == null)
                throw new ArgumentNullException(nameof(server));

            DataContext = server;
            InitializeComponent();
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

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
