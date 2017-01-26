using RowShareTool.Model;
using RowShareTool.Utilities;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace RowShareTool
{
    public partial class ExportFolder : Window
    {
        private readonly ObservableCollection<FolderImporterMessage> _messages = new ObservableCollection<FolderImporterMessage>();
        private bool _isExporting;

        public FolderExporter Exporter { get; private set; }

        public ExportFolder(FolderExporter exporter)
        {
            if (exporter == null)
                throw new ArgumentNullException(nameof(exporter));

            Closing += OnClosing;
            Loaded += OnLoaded;

            Exporter = exporter;
            InitializeComponent();
        }

        private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            DataGrid.ItemsSource = _messages;
        }

        private void OnMessage(object sender, FolderImporterEventArgs e)
        {
            if (e == null || e.Message == null)
                return;

            Dispatcher.Invoke(() =>
            {
                _messages.Add(e.Message);
            });
        }

        private void OnClosing(object sender, CancelEventArgs cancelEventArgs)
        {
            cancelEventArgs.Cancel = _isExporting;
        }

        private async void StartButton_OnClick(object sender, RoutedEventArgs e)
        {
            await StartExport();
        }

        private async Task StartExport()
        {
            _isExporting = true;
            Exporter.DataExporter.OnError += OnMessage;
            Exporter.DataExporter.OnMessage += OnMessage;
            try
            {
                _messages.Clear();

                ProgressBar.Foreground = new SolidColorBrush(Color.FromArgb(0xFF, 0x06, 0xB0, 0x25));
                ProgressBar.IsIndeterminate = true;
                ProgressBar.Visibility = Visibility.Visible;
                StartButton.IsEnabled = false;
                CleanButton.Visibility = Visibility.Collapsed;

                var path = Folder.Text;
                var result = await Task.Factory.StartNew(() => Exporter.ExportTo(path));

                ProgressBar.IsIndeterminate = false;
                StartButton.IsEnabled = true;

                if (!result)
                {
                    ProgressBar.Foreground = new SolidColorBrush(Color.FromArgb(0xFF, 0xFF, 0x00, 0x00));
                    CleanButton.Visibility = Visibility.Visible;
                }
            }
            finally
            {
                Exporter.DataExporter.OnError -= OnMessage;
                Exporter.DataExporter.OnMessage -= OnMessage;
                _isExporting = false;
            }
        }

        private void CleanButton_OnClick(object sender, RoutedEventArgs e)
        {
            _messages.Clear();
            _messages.AddRange(Exporter.DataExporter.ErrorMessages);
        }

        private void FolderButton_Click(object sender, RoutedEventArgs e)
        {
            var browser = new FolderBrowser();
            var r = browser.ShowDialog(WpfExtensions.GetWin32Window(this));
            if (r != System.Windows.Forms.DialogResult.OK)
                return;

            Folder.Text = browser.DirectoryPath;
            StartButton.IsEnabled = true;
        }
    }
}