using System;
using System.Windows;
using System.Windows.Input;
using RowShareTool.Model;

namespace RowShareTool
{
    public partial class OpenList : Window
    {
        public OpenList(List list)
        {
            if(list == null)
                throw new ArgumentNullException(nameof(list));

            DataContext = list;
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
