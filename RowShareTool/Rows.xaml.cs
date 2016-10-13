using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;
using CodeFluent.Runtime.Utilities;
using Microsoft.Win32;
using RowShareTool.Model;

namespace RowShareTool
{
    public partial class Rows : Window
    {
        private CancellationTokenSource _source;
        private ObservableCollection<Row> _rows = new ObservableCollection<Row>();

        public Rows(List list, bool showListButton)
        {
            if (list == null)
                throw new ArgumentNullException(nameof(list));

            List = list;
            InitializeComponent();
            GoToList.Visibility = showListButton ? Visibility.Visible : Visibility.Hidden;
            SetupGrid();
            DG.ItemsSource = _rows;
            Loaded += (sender, e) => OnLoaded();
            Closed += (sender, e) => OnClosed();
        }

        public List List { get; private set; }

        private void SetupGrid()
        {
            foreach (Column column in List.Columns)
            {
                var dgColumn = new DataGridTemplateColumn();
                var header = new DataGridColumnHeader();
                header.HorizontalContentAlignment = HorizontalAlignment.Center;
                header.Content = column.DisplayName;
                dgColumn.Header = header;
                dgColumn.CellTemplate = CreateColumnDataTemplate(column);
                DG.Columns.Add(dgColumn);
            }
        }

        private static DataTemplate CreateColumnDataTemplate(Column column)
        {
            var sp = new FrameworkElementFactory(typeof(StackPanel));
            sp.SetValue(StackPanel.OrientationProperty, Orientation.Vertical);

            var tb = new FrameworkElementFactory(typeof(TextBlock));
            tb.SetValue(HorizontalAlignmentProperty, HorizontalAlignment.Center);
            var tbb = new Binding("ValuesObject." + column.DisplayName);
            tbb.Mode = BindingMode.OneTime;
            tb.SetBinding(TextBlock.TextProperty, tbb);
            sp.AppendChild(tb);

            return new DataTemplate
            {
                VisualTree = sp,
            };
        }

        private void OnClosed()
        {
            if (_source != null)
            {
                _source.Cancel(false);
            }
        }

        private void OnLoaded()
        {
            _source = new CancellationTokenSource();
            var task = new Task(() => LoadRows(_source.Token), _source.Token, TaskCreationOptions.LongRunning);
            task.Start();
        }

        private void LoadRows(CancellationToken token)
        {
            var rows = List.LoadRows();
            if (token.IsCancellationRequested)
                return;

            foreach (Row row in rows)
            {
                if (token.IsCancellationRequested)
                    return;

                Dispatcher.BeginInvoke(() =>
                {
                    _rows.Add(row);
                });
            }
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
            var dlg = new SaveFileDialog();
            dlg.AddExtension = true;
            dlg.CheckPathExists = true;
            dlg.ValidateNames = true;
            dlg.DefaultExt = ".json";
            dlg.Title = "Save list and rows";
            dlg.Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*";
            dlg.FilterIndex = 1;
            dlg.RestoreDirectory = true;
            dlg.FileName = List.DisplayName;
            if (dlg.ShowDialog().GetValueOrDefault())
            {
                var lwr = new ListWithRows(List);
                string s = JsonUtilities.Serialize(lwr);
                File.WriteAllText(dlg.FileName, s, Encoding.UTF8);
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Row_Click(object sender, RoutedEventArgs e)
        {
            var fe = (FrameworkElement)e.Source;
            var row = fe.DataContext as Row;
            if (row == null)
                return;

            var dlg = new ObjectProperties(row, true);
            dlg.Title = "Row #" + row.Index;
            dlg.Owner = this;
            dlg.ShowDialog();
        }

        private void GoToList_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new ObjectProperties(List, true);
            dlg.Title = "List " + List.DisplayName;
            dlg.Owner = this;
            dlg.ShowDialog();
        }
    }
}
