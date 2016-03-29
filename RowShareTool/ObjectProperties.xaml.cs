using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using CodeFluent.Runtime.Utilities;

namespace RowShareTool
{
    public partial class ObjectProperties : Window
    {
        private EventHandler _extraClick;

        public ObjectProperties(object obj, bool readOnly)
            : this(obj, readOnly, null)
        {
        }

        public ObjectProperties(object obj, bool readOnly, Action action)
        {
            if (obj == null)
                throw new ArgumentNullException("obj");

            DataContext = obj;
            InitializeComponent();
            if (action != null)
            {
                Loaded += (sender, e) => OnLoaded(action);
            }
            Title = obj + " Properties";
            if (obj != null)
            {
                var roa = AssemblyUtilities.GetAttribute<ReadOnlyAttribute>(obj.GetType());
                PGrid.IsReadOnly = roa != null && roa.IsReadOnly;
            }

            if (readOnly)
            {
                PGrid.IsReadOnly = true;
            }

            PGrid.SelectedObject = obj;
            if (PGrid.IsReadOnly)
            {
                Cancel.Content = "Close";
                OK.Visibility = Visibility.Hidden;
            }
        }

        private void OnLoaded(Action action)
        {
            var task = new Task(() => action(), TaskCreationOptions.LongRunning);
            task.Start();
        }

        public void EnableExtra(string text, EventHandler onClick)
        {
            if (text == null)
                throw new ArgumentNullException("text");

            if (onClick == null)
                throw new ArgumentNullException("onClick");

            Extra.Content = text;
            Extra.Visibility = Visibility.Visible;
            _extraClick = onClick;
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

        private void Extra_Click(object sender, RoutedEventArgs e)
        {
            if (_extraClick != null)
            {
                _extraClick(sender, e);
            }
        }
    }
}
