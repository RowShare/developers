using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Threading;
using CodeFluent.Runtime;
using CodeFluent.Runtime.Utilities;
using CodeFluent.Runtime.Windows;
using RowShareTool.Utilities;

namespace RowShareTool
{
    public partial class ErrorBox : Window
    {
        private static volatile bool _showingUi;
        private Exception _error;

        public ErrorBox(Exception error)
            : this(error, error != null ? error.ToString() : null)
        {
        }

        public ErrorBox(Exception error, string errorDetails)
        {
            if (error == null)
                throw new ArgumentNullException(nameof(error));

            _error = error;
            InitializeComponent();
            ErrorDetails.Visibility = Visibility.Collapsed;
            ErrorDetails.Text = error.GetErrorText(null);
            if (!string.IsNullOrWhiteSpace(errorDetails))
            {
                ErrorDetails.Text += Environment.NewLine + Environment.NewLine + "-- Details -- " + Environment.NewLine + Environment.NewLine + errorDetails;
            }
            ErrorDetails.Text += Environment.NewLine + Environment.NewLine + "-- Diagnostics -- " + Environment.NewLine + Environment.NewLine + GetDebugInformation();
            Image.Source = WpfExtensions.GetStockIcon(WindowsUtilities.StockIconId.ERROR, WindowsUtilities.SHGSI.SHGSI_LARGEICON);
            Error.Text = CodeFluentRuntimeException.GetInterestingExceptionMessage(error);
        }

        public bool QuitRequested { get; set; }

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
            Close();
        }

        public static string GetDebugInformation()
        {
            var si = new SystemInformation();
            return JsonUtilities.SerializeFormatted(si);
        }

        private void Details_Click(object sender, RoutedEventArgs e)
        {
            if ((Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift)
            {
                WpfExtensions.MailTo(null, "SSB Error: " + Error.Text, GetDebugInformation() + Environment.NewLine + ErrorDetails.Text);
                return;
            }
            const string CopyName = "Copy";
            if (CopyName.Equals(Details.Content))
            {
                Clipboard.SetText(ErrorDetails.Text);
                return;
            }

            Details.Content = CopyName;
            ErrorDetails.Visibility = Visibility.Visible;
            Dispatcher.BeginInvoke(() =>
                {
                    WindowsUtilities.CenterWindow(new WindowInteropHelper(this).Handle);
                }, DispatcherPriority.Background); 
        }

        private void Quit_Click(object sender, RoutedEventArgs e)
        {
            QuitRequested = true;
            Close();
        }

        public static void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            bool quit = HandleException(e.Exception);
            e.Handled = true;
            if (quit || (!Debugger.IsAttached && e.Dispatcher.IsProcessingDisabled().GetValueOrDefault()))
            {
                Environment.Exit(0);
            }
        }

        public static void OnCurrentDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var ex = e.ExceptionObject as Exception;
            if (ex == null)
                return;

            bool quit = HandleException(ex);
            if (quit || !Debugger.IsAttached)
            {
                Environment.Exit(0);
            }
        }

        public static bool HandleException(Exception e)
        {
            if (e == null)
                return false;

            return ShowException(e);
        }

        public static bool ShowException(Exception exception, params KeyValuePair<string, object>[] context)
        {
            return ShowException(exception, (IWin32Window)null, context);
        }

        public static bool ShowException(Exception exception, IWin32Window owner, params KeyValuePair<string, object>[] context)
        {
            if (exception == null)
                throw new ArgumentNullException(nameof(exception));

            if (Debugger.IsAttached)
                return false;

            bool _quit = false;
            // this whole mess is to avoid conveys of errors that show tons of error boxes and then quit
            try
            {
                if (!_showingUi & WpfExtensions.CanDispatch)
                {
                    _showingUi = true;
                    var t = new Thread(() =>
                    {
                        try
                        {
                            var dlg = new ErrorBox(exception);
                            dlg.ShowDialog();
                            if (dlg.QuitRequested)
                            {
                                _quit = true;
                            }
                        }
                        finally
                        {
                            _showingUi = false;
                        }
                    });
                    t.Name = "ShowException";
                    t.SetApartmentState(ApartmentState.STA);
                    t.Start();
                }

                while (_showingUi)
                {
                    Thread.Sleep(500);
                }
            }
            catch
            {
                _showingUi = false;
            }
            return _quit;
        }
    }
}
