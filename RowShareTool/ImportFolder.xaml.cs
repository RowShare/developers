using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using RowShareTool.Model;

namespace RowShareTool
{
    /// <summary>
    /// Interaction logic for ImportFolder.xaml
    /// </summary>
    public partial class ImportFolder : Window
    {
        private readonly FolderImporter _importer;
        private readonly ObservableCollection<FolderImporterMessage> _messages = new ObservableCollection<FolderImporterMessage>();

        public ImportFolder(FolderImporter importer)
        {
            if (importer == null)
                throw new ArgumentNullException(nameof(importer));

            Closing += OnClosing;
            Loaded += OnLoaded;

            _importer = importer;
            _importer.OnError += OnMessage;
            _importer.OnMessage += OnMessage;
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
            cancelEventArgs.Cancel = StartButton.IsEnabled == false;
        }

        private async void StartButton_OnClick(object sender, RoutedEventArgs e)
        {
            _messages.Clear();

            ProgressBar.Foreground = new SolidColorBrush(Color.FromArgb(0xFF, 0x06, 0xB0, 0x25));
            ProgressBar.IsIndeterminate = true;
            ProgressBar.Visibility = Visibility.Visible;
            StartButton.IsEnabled = false;
            CleanButton.Visibility = Visibility.Collapsed;

            var result = await Task.Factory.StartNew(() => _importer.CopyAllContent());

            ProgressBar.IsIndeterminate = false;
            StartButton.IsEnabled = true;

            if (!result)
            {
                ProgressBar.Foreground = new SolidColorBrush(Color.FromArgb(0xFF, 0xFF, 0x00, 0x00));
                CleanButton.Visibility = Visibility.Visible;
            }
        }

        private void CleanButton_OnClick(object sender, RoutedEventArgs e)
        {
            _messages.Clear();
            _messages.AddRange(_importer.ErrorMessages);
        }
    }
}
