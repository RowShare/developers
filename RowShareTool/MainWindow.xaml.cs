using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using CodeFluent.Runtime.Utilities;
using Microsoft.Win32;
using RowShareTool.Model;

namespace RowShareTool
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            PG.DefaultCategoryName = "General";
            Settings = Settings.DeserializeFromConfiguration();

            var list = new ObservableCollection<Server>();
            foreach (var s in Settings.Servers)
            {
                Server server = new Server();
                server.DisplayName = s.Url;
                server.Cookie = s.Cookie;
                list.Add(server);
            }
            LoadTreeView(list);
        }

        public Settings Settings { get; private set; }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void LoadTreeView(ObservableCollection<Server> servers)
        {
            TV.ItemsSource = servers;
        }

        private void AddServerToTreeView(SettingsServer settingsServer)
        {
            Server server = new Server();
            server.DisplayName = settingsServer.Url;
            server.Cookie = settingsServer.Cookie;

            var servers = (ObservableCollection<Server>) TV.ItemsSource;
            servers.Add(server);
        }

        private void RemoveServerToTreeView(SettingsServer settingsServer)
        {
            var servers = (ObservableCollection<Server>)TV.ItemsSource;
            Server server = servers.FirstOrDefault(s => s.Url.EqualsIgnoreCase(settingsServer.Url));
            servers.Remove(server);
        }

        private void TV_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            PG.SelectedObject = e.NewValue;
        }

        protected override void OnPreviewMouseRightButtonDown(MouseButtonEventArgs e)
        {
            TreeViewItem treeViewItem = (e.OriginalSource as DependencyObject).GetVisualSelfOrParent<TreeViewItem>();
            if (treeViewItem != null)
            {
                treeViewItem.Focus();
                e.Handled = true;
                return;
            }
        }

        private void TreeViewBrowse_Click(object sender, RoutedEventArgs e)
        {
            var item = TV.GetSelectedTag<TreeItem>();
            if (item != null)
            {
                WpfExtensions.Browse(item.Url);
                return;
            }
        }

        private void TV_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            var list = TV.SelectedItem as List;
            var server = TV.SelectedItem as Server;
            var folder = TV.SelectedItem as Folder;
            TreeViewEditRows.SetCollapsed(list == null);
            TreeViewImportList.SetCollapsed(folder == null);
            TreeViewDelete.IsEnabled = server != null || list != null || (folder != null && !folder.IsRoot);

            TreeViewLogin.SetCollapsed(server == null);
            TreeViewLogout.SetCollapsed(server == null);
            TreeViewLogin.IsEnabled = server == null || server.Cookie == null;
            TreeViewLogout.IsEnabled = !TreeViewLogin.IsEnabled;
        }

        private void TreeViewEditRows_Click(object sender, RoutedEventArgs e)
        {
            var list = TV.GetSelectedTag<List>();
            if (list == null)
                return;

            Rows dlg = new Rows(list);
            dlg.Title = list.DisplayName;
            dlg.Owner = this;
            dlg.ShowDialog();
        }

        private void FileMenu_Opened(object sender, RoutedEventArgs e)
        {
        }

        private void TreeViewImportList_Click(object sender, RoutedEventArgs e)
        {
            var folder = TV.GetSelectedTag<Folder>();
            if (folder == null)
                return;

            OpenFileDialog dlg = new OpenFileDialog();
            dlg.AddExtension = true;
            dlg.CheckPathExists = true;
            dlg.CheckPathExists = true;
            dlg.DefaultExt = ".json";
            dlg.RestoreDirectory = true;
            dlg.Title = "Choose a list to import";
            dlg.Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*";
            dlg.FilterIndex = 1;
            if (dlg.ShowDialog(this).GetValueOrDefault())
            {
                string s = File.ReadAllText(dlg.FileName, Encoding.UTF8);
                ListWithRows lwr = JsonUtilities.Deserialize<ListWithRows>(s);
                if (lwr.List != null)
                {
                    string name = lwr.List.DisplayName;
                    bool showOptions = false;
                    if (folder.HasLazyChild)
                    {
                        folder.LazyLoadChildren();
                    }
                    List list = folder.Children.OfType<List>().FirstOrDefault(l => l.DisplayName.EqualsIgnoreCase(name));
                    List existingList = list;
                    int i = 1;
                    while (list != null)
                    {
                        showOptions = true;
                        name = lwr.List.DisplayName + " - Copy(" + i + ")";
                        list = folder.Children.OfType<List>().FirstOrDefault(l => l.DisplayName.EqualsIgnoreCase(name));
                        i++;
                    }

                    ImportOptionsDefinition def = new ImportOptionsDefinition(lwr.List, existingList, name);
                    if (showOptions)
                    {
                        ImportOptions options = new ImportOptions(def);
                        options.Owner = this;
                        if (!options.ShowDialog().GetValueOrDefault())
                            return;
                    }

                    lwr.Import(folder, name, def);
                    this.ShowMessage("Ok");
                }
            }
        }

        private void TreeViewRefresh_Click(object sender, RoutedEventArgs e)
        {
            var item = TV.SelectedItem as TreeItem;
            if (item != null)
            {
                item.Reload();
            }
        }

        private void TreeViewDelete_Click(object sender, RoutedEventArgs e)
        {
            var item = TV.SelectedItem as TreeItem;
            if (item != null)
            {
                if (this.ShowConfirm("Delete " + item.GetType().Name + " '" + item.DisplayName + "'?") == MessageBoxResult.No)
                    return;

                Server server = item as Server;
                if (server != null)
                {
                    var settingsServer = new SettingsServer {Url = server.Url};
                    RemoveServerToTreeView(settingsServer);
                    Settings.RemoveServer(settingsServer);
                    Settings.SerializeToConfiguration();
                }
                else if (item.Delete())
                {
                    if (item.Parent != null)
                    {
                        item.Parent.Reload();
                    }
                }
            }
        }

        private void TreeViewLogin_Click(object sender, RoutedEventArgs e)
        {
            var server = TV.SelectedItem as Server;
            if (server == null)
                return;

            Login login = new Login(server.Url);
            login.Owner = this;
            if (login.ShowDialog().GetValueOrDefault())
            {
                server.Cookie = login.Cookie;
                Settings.GetServer(server.Url).Cookie = login.Cookie;
                Settings.SerializeToConfiguration();
                server.Reload();
            }
        }

        private void TreeViewLogout_Click(object sender, RoutedEventArgs e)
        {
            var server = TV.SelectedItem as Server;
            if (server == null)
                return;

            server.Cookie = null;
            Settings.GetServer(server.Url).Cookie = null;
            Settings.SerializeToConfiguration();
            server.Reload();
        }

        private void Connect_Click(object sender, RoutedEventArgs e)
        {
            var server = new SettingsServer();
            var dlg = new Connect(server);
            dlg.Owner = this;
            if (dlg.ShowDialog().GetValueOrDefault())
            {
                Settings.AddServer(server);
                Settings.SerializeToConfiguration();
                AddServerToTreeView(server);
            }
        }
    }
}
