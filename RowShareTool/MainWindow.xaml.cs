using CodeFluent.Runtime.Utilities;
using Microsoft.Win32;
using RowShareTool.Model;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace RowShareTool
{
    public partial class MainWindow : Window
    {
        private Folder _copyFolder;

        public MainWindow()
        {
            InitializeComponent();
            PG.DefaultCategoryName = "General";
            Settings = Settings.DeserializeFromConfiguration();

            var list = new ObservableCollection<Server>();
            foreach (var s in Settings.Servers)
            {
                var server = new Server();
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
            var server = new Server();
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
            var login = TV.GetSelectedTag<Model.Login>();
            if (login != null)
            {
                Login(login.Parent);
            }
        }

        protected override void OnPreviewMouseRightButtonDown(MouseButtonEventArgs e)
        {
            var treeViewItem = (e.OriginalSource as DependencyObject).GetVisualSelfOrParent<TreeViewItem>();
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
            TreeViewDelete.IsEnabled = server != null || list != null || (folder != null && !folder.IsRoot && (folder.Options & FolderOptions.Undeletable) != FolderOptions.Undeletable);
            TreeViewCopyFrom.IsEnabled = folder != null;
            TreeViewCopyTo.IsEnabled = folder != null && _copyFolder != null && folder.Server.CompareTo(_copyFolder.Server) != 0;
            TreeViewExportTo.IsEnabled = folder != null;

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

            var dlg = new Rows(list, false);
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

            var dlg = new OpenFileDialog();
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
                var lwr = JsonUtilities.Deserialize<ListWithRows>(s);
                if (lwr.List != null)
                {
                    string name = lwr.List.DisplayName;
                    bool showOptions = false;
                    if (folder.HasLazyChild)
                    {
                        folder.LazyLoadChildren();
                    }

                    var list = folder.Children.OfType<List>().FirstOrDefault(l => l.DisplayName.EqualsIgnoreCase(name));
                    List existingList = list;
                    int i = 1;
                    while (list != null)
                    {
                        showOptions = true;
                        name = lwr.List.DisplayName + " - Copy(" + i + ")";
                        list = folder.Children.OfType<List>().FirstOrDefault(l => l.DisplayName.EqualsIgnoreCase(name));
                        i++;
                    }

                    var def = new ImportOptionsDefinition(lwr.List, existingList, name);
                    if (showOptions)
                    {
                        var options = new ImportOptions(def);
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

        private void TreeViewCopyFrom_Click(object sender, RoutedEventArgs e)
        {
            var folder = TV.GetSelectedTag<Folder>();
            if (folder == null)
                return;

            _copyFolder = folder;
        }

        private void TreeViewCopyTo_Click(object sender, RoutedEventArgs e)
        {
            if (_copyFolder == null)
                return;

            var folder = TV.GetSelectedTag<Folder>();
            if (folder == null)
                return;

            if (this.ShowConfirm("Are you sure you want to copy folder '" + _copyFolder.DisplayName + "' from server '" + _copyFolder.Server.DisplayName + "' to folder '" + folder.DisplayName  +"' ?") == MessageBoxResult.No)
                return;

            var importer = new FolderImporter(_copyFolder, folder);
            var popup = new ImportFolder(importer);
            popup.ShowDialog();
        }

        private void TreeViewExportTo_Click(object sender, RoutedEventArgs e)
        {
            var folder = TV.GetSelectedTag<Folder>();
            if (folder == null)
                return;

            var exporter = new FolderExporter(folder);
            var popup = new ExportFolder(exporter);
            popup.ShowDialog();
        }

        private void TreeViewDelete_Click(object sender, RoutedEventArgs e)
        {
            var item = TV.SelectedItem as TreeItem;
            if (item != null)
            {
                if (this.ShowConfirm("Delete " + item.GetType().Name + " '" + item.DisplayName + "'?") == MessageBoxResult.No)
                    return;

                var server = item as Server;
                if (server != null)
                {
                    var settingsServer = new SettingsServer {Url = server.Url};
                    RemoveServerToTreeView(settingsServer);
                    Settings.RemoveServer(settingsServer);
                    Settings.SerializeToConfiguration();
                }
                else
                {
                    if (item.Delete())
                    {
                        if (item.Parent != null)
                        {
                            item.Parent.Reload();
                        }
                    }
                    else
                    {
                        this.ShowError("Operation failed.");
                    }
                }
            }
        }

        private void Login(Server server)
        {
            if (server == null)
                return;

            var login = new Login(server.Url);
            login.Owner = this;
            if (login.ShowDialog().GetValueOrDefault())
            {
                server.Cookie = login.Cookie;
                Settings.GetServer(server.Url).Cookie = login.Cookie;
                Settings.SerializeToConfiguration();
                server.Reload();
            }
        }

        private void TreeViewLogin_Click(object sender, RoutedEventArgs e)
        {
            Login(TV.SelectedItem as Server);
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
            server.Url = "https://www.rowshare.com";
            var dlg = new Connect(server);
            dlg.Owner = this;
            if (dlg.ShowDialog().GetValueOrDefault())
            {
                Settings.AddServer(server);
                Settings.SerializeToConfiguration();
                AddServerToTreeView(server);
            }
        }

        private void TreeViewOpenList_Click(object sender, RoutedEventArgs e)
        {
            var server = TV.GetSelectedTag<Server>(true);
            if (server == null)
                return;

            var list = new List();
            var dlg = new OpenList(list);
            dlg.Owner = this;
            if (dlg.ShowDialog().GetValueOrDefault())
            {
                var serverList = server.LoadList(list.IdN);
                if (serverList != null && serverList.Id != Guid.Empty)
                {
                    var dlg2 = new Rows(serverList, true);
                    dlg2.Title = serverList.DisplayName;
                    dlg2.Owner = this;
                    dlg2.ShowDialog();
                }
                else
                {
                    this.ShowError("An error occurred while trying to read the list " + list.IdN + ".");
                }
            }
        }

        private void About_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new AboutWindow();
            dlg.Owner = this;
            dlg.ShowDialog();
        }
    }
}
