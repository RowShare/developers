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

        public Rows(List list)
        {
            if (list == null)
                throw new ArgumentNullException("list");

            List = list;
            InitializeComponent();
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
                DataGridTemplateColumn dgColumn = new DataGridTemplateColumn();
                DataGridColumnHeader header = new DataGridColumnHeader();
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
            Binding tbb = new Binding("ValuesObject." + column.DisplayName);
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
            Task task = new Task(() => LoadRows(_source.Token), _source.Token, TaskCreationOptions.LongRunning);
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
            SaveFileDialog dlg = new SaveFileDialog();
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
                ListWithRows lwr = new ListWithRows(List);
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
            FrameworkElement fe = (FrameworkElement)e.Source;
            var row = (Row)fe.DataContext;
            if (row == null)
                return;

            ObjectProperties dlg = new ObjectProperties(row, true);
            dlg.Title = "Row #" + row.Index;
            dlg.Owner = this;
            dlg.ShowDialog();
        }
    }
}
