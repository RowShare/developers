using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Markup.Primitives;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Threading;
using CodeFluent.Runtime;
using CodeFluent.Runtime.Utilities;
using CodeFluent.Runtime.Windows;
using RowShareTool.Model;

namespace RowShareTool
{
    public static class WpfExtensions
    {
        private const int URLMON_OPTION_USERAGENT = 0x10000001;
        private const int GWL_STYLE = -16;
        private const int WS_MAXIMIZEBOX = 0x10000;
        private const int WS_MINIMIZEBOX = 0x20000;

        [DllImport("urlmon.dll", CharSet = CharSet.Ansi)] // NOTE: the unicode version does not work for some reason
        private static extern int UrlMkSetSessionOption(int dwOption, string pBuffer, int dwBufferLength, int dwReserved);

        public static void SetSessionUserAgent(string userAgent, bool throwOnError)
        {
            int hr = UrlMkSetSessionOption(URLMON_OPTION_USERAGENT, userAgent, userAgent.Length, 0);
            if (hr != 0 && throwOnError)
                throw new Win32Exception(hr);
        }

        [DllImport("user32.dll")]
        private extern static int GetWindowLong(IntPtr hwnd, int index);

        [DllImport("user32.dll")]
        private extern static int SetWindowLong(IntPtr hwnd, int index, int value);

        public static void MailTo(string to, string subject)
        {
            MailTo(to, subject, null);
        }

        public static void MailTo(string to, string subject, string body)
        {
            MailTo(to, subject, body, null);
        }

        public static void MailTo(string to, string subject, string body, string cc)
        {
            MailTo(to, subject, body, cc, null);
        }

        private static string EscapeMailToArg(string argument)
        {
            return argument.Replace("?", "%25").Replace("=", "%3D").Replace("&", "%26").Replace(Environment.NewLine, "%0D%0A");
        }

        public static void MailTo(string to, string subject, string body, string cc, string bcc)
        {
            var sb = new StringBuilder("mailto:");
            if (!string.IsNullOrWhiteSpace(to))
            {
                sb.Append(Uri.EscapeUriString(to));
            }

            var arguments = new Dictionary<string, string>();
            if (!string.IsNullOrWhiteSpace(subject))
            {
                arguments["subject"] = EscapeMailToArg(subject);
            }
            if (!string.IsNullOrWhiteSpace(body))
            {
                arguments["body"] = EscapeMailToArg(body);
            }
            if (!string.IsNullOrWhiteSpace(cc))
            {
                arguments["cc"] = EscapeMailToArg(cc);
            }
            if (!string.IsNullOrWhiteSpace(bcc))
            {
                arguments["bcc"] = EscapeMailToArg(bcc);
            }

            if (arguments.Count > 0)
            {
                sb.Append('?');
                foreach (var kv in arguments)
                {
                    if (sb[sb.Length - 1] != '?')
                    {
                        sb.Append('&');
                    }
                    sb.Append(kv.Key);
                    if (kv.Value != null)
                    {
                        sb.Append('=');
                        sb.Append(kv.Value);
                    }
                }
            }

            var process = new Process();
            process.StartInfo = new ProcessStartInfo(sb.ToString());
            process.Start();
        }

        public static void SetCollapsed(this UIElement element, bool collapsed)
        {
            if (element == null)
                throw new ArgumentNullException("element");

            element.Visibility = collapsed ? Visibility.Collapsed : Visibility.Visible;
        }

        public static void SetHidden(this UIElement element, bool hidden)
        {
            if (element == null)
                throw new ArgumentNullException("element");

            element.Visibility = hidden ? Visibility.Hidden : Visibility.Visible;
        }

        public static void HideMinimizeAndMaximizeBoxes(this Window window)
        {
            if (window == null)
                return;

            IntPtr hwnd = new WindowInteropHelper(window).Handle;
            SetWindowLong(hwnd, GWL_STYLE, (GetWindowLong(hwnd, GWL_STYLE) & ~WS_MAXIMIZEBOX & ~WS_MINIMIZEBOX));
        }

        public static void AddRange<T>(this Collection<T> collection, IEnumerable<T> add)
        {
            if (collection == null || add == null)
                return;

            foreach (T item in add)
            {
                collection.Add(item);
            }
        }

        public static bool? IsProcessingDisabled(this Dispatcher dispatcher)
        {
            if (dispatcher == null)
                return null;

            FieldInfo fi = dispatcher.GetType().GetField("_disableProcessingCount", BindingFlags.Instance | BindingFlags.NonPublic);
            if (fi == null)
                return null;

            return (int)fi.GetValue(dispatcher) > 0;
        }

        public static bool CanDispatch
        {
            get
            {
                var app = Application.Current;
                if (app == null)
                    return false;

                var disp = app.Dispatcher;
                return app != null && !app.Dispatcher.HasShutdownFinished && !app.Dispatcher.HasShutdownStarted;
            }
        }

        public static Window GetActiveWindow()
        {
            var app = Application.Current;
            if (app == null)
                return null;

            return app.Windows.OfType<Window>().FirstOrDefault(w => w.IsActive);
        }

        public static string GetProduct()
        {
            return Assembly.GetEntryAssembly().GetProduct();
        }

        public static string GetProduct(this Assembly assembly)
        {
            if (assembly == null)
                throw new ArgumentNullException("assembly");

            object[] atts = assembly.GetCustomAttributes(typeof(AssemblyProductAttribute), false);
            if (atts != null && atts.Length > 0)
                return ((AssemblyProductAttribute)atts[0]).Product;

            return null;
        }

        public static void ShowMessage(this Window window, string text)
        {
            if (window == null)
            {
                window = GetActiveWindow();
            }
            MessageBox.Show(window, text, GetProduct(), MessageBoxButton.OK);
        }

        public static void ShowError(this Window window, string text)
        {
            ShowMessage(window, text, MessageBoxImage.Error);
        }

        public static void ShowMessage(this Window window, string text, MessageBoxImage image)
        {
            if (window == null)
            {
                window = GetActiveWindow();
            }
            MessageBox.Show(window, text, GetProduct(), MessageBoxButton.OK, image);
        }

        public static MessageBoxResult ShowConfirm(this Window window, string text)
        {
            if (window == null)
            {
                window = GetActiveWindow();
            }
            return MessageBox.Show(window, text, GetProduct(), MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No);
        }

        public static MessageBoxResult ShowConfirmCancel(this Window window, string text)
        {
            if (window == null)
            {
                window = GetActiveWindow();
            }
            return MessageBox.Show(window, text, GetProduct(), MessageBoxButton.YesNoCancel, MessageBoxImage.Question, MessageBoxResult.Cancel);
        }

        public static DependencyObject GetContainerFromItem(this ItemsControl itemsControl, object item)
        {
            if (itemsControl == null)
                throw new ArgumentNullException("itemsControl");

            var container = itemsControl.ItemContainerGenerator.ContainerFromItem(item);
            if (container != null)
                return container;

            int count = itemsControl.ItemContainerGenerator.Items.Count;
            for (int i = 0; i < count; i++)
            {
                var child = itemsControl.ItemContainerGenerator.ContainerFromIndex(i) as ItemsControl;
                if (child == null)
                    continue;

                container = child.GetContainerFromItem(item);
                if (container != null)
                    return container;
            }
            return null;
        }

        public static void BringItemIntoView(this ItemsControl itemsControl, object item)
        {
            if (itemsControl == null)
                throw new ArgumentNullException("itemsControl");

            var container = itemsControl.GetContainerFromItem(item) as FrameworkElement;
            if (container != null)
            {
                container.BringIntoView();
            }
        }

        private static DependencyObject FirstVisualChild(Visual visual)
        {
            if (visual == null)
                return null;

            if (VisualTreeHelper.GetChildrenCount(visual) == 0)
                return null;
            
            return VisualTreeHelper.GetChild(visual, 0);
        }

        private static double CenteringOffset(double center, double viewport, double extent)
        {
            return Math.Min(extent - viewport, Math.Max(0, center - viewport / 2));
        }

        public static IEnumerable<DependencyObject> EnumerateVisualChildren(this DependencyObject obj)
        {
            return obj.EnumerateVisualChildren(true);
        }

        public static IEnumerable<DependencyObject> EnumerateVisualChildren(this DependencyObject obj, bool recursive)
        {
            return obj.EnumerateVisualChildren(recursive, true);
        }

        public static IEnumerable<DependencyObject> EnumerateVisualChildren(this DependencyObject obj, bool recursive, bool sameLevelFirst)
        {
            if (obj == null)
                yield break;

            if (sameLevelFirst)
            {
                int count = VisualTreeHelper.GetChildrenCount(obj);
                var list = new List<DependencyObject>(count);
                for (int i = 0; i < count; i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(obj, i);
                    if (child == null)
                        continue;

                    yield return child;
                    if (recursive)
                    {
                        list.Add(child);
                    }
                }

                foreach (var child in list)
                {
                    foreach (DependencyObject grandChild in child.EnumerateVisualChildren(recursive, sameLevelFirst))
                    {
                        yield return grandChild;
                    }
                }
            }
            else
            {
                int count = VisualTreeHelper.GetChildrenCount(obj);
                for (int i = 0; i < count; i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(obj, i);
                    if (child == null)
                        continue;

                    yield return child;
                    if (recursive)
                    {
                        foreach (var dp in child.EnumerateVisualChildren(recursive, sameLevelFirst))
                        {
                            yield return dp;
                        }
                    }
                }
            }
        }

        public static T FindVisualChild<T>(this DependencyObject obj, Func<T, bool> where) where T : FrameworkElement
        {
            if (where == null)
                throw new ArgumentNullException("where");

            foreach (T item in obj.EnumerateVisualChildren(true, true).OfType<T>())
            {
                if (where(item))
                    return item;
            }
            return null;
        }

        public static bool IsRecursiveKeyboardFocused(this DependencyObject obj)
        {
            var ue = obj as UIElement;
            if (ue != null && ue.IsFocused)
                return true;

            foreach (var item in obj.EnumerateVisualChildren(true, true).OfType<UIElement>())
            {
                if (item.IsFocused)
                    return true;
            }
            return false;
        }

        public static T FindFocusedVisualChild<T>(this DependencyObject obj, string name) where T : FrameworkElement
        {
            foreach (T item in obj.EnumerateVisualChildren(true, true).OfType<T>())
            {
                if (item.IsFocused && (item.Name == name || name == null))
                    return item;
            }
            return null;
        }

        public static T FindFocusableVisualChild<T>(this DependencyObject obj, string name) where T : FrameworkElement
        {
            foreach (T item in obj.EnumerateVisualChildren(true, true).OfType<T>())
            {
                if (item.Focusable && (item.Name == name || name == null))
                    return item;
            }
            return null;
        }

        public static T FindVisualChild<T>(this DependencyObject obj, string name) where T : FrameworkElement
        {
            foreach (T item in obj.EnumerateVisualChildren(true, true).OfType<T>())
            {
                if (name == null)
                    return item;

                if (item.Name == name)
                    return item;
            }
            return null;
        }

        public static T GetVisualParent<T>(this DependencyObject obj) where T : DependencyObject
        {
            if (obj == null)
                return null;

            DependencyObject parent = VisualTreeHelper.GetParent(obj);
            if (parent == null)
                return null;

            if (typeof(T).IsAssignableFrom(parent.GetType()))
                return (T)parent;

            return parent.GetVisualParent<T>();
        }

        public static Visual GetVisualRoot(this DependencyObject obj)
        {
            if (obj == null)
                return null;

            if (obj is Visual)
            {
                var source = PresentationSource.FromVisual((Visual)obj);
                if (source != null)
                    return source.RootVisual;
            }
            else
            {
                var element = obj as FrameworkContentElement;
                if (element != null)
                    return element.Parent.GetVisualRoot();
            }
            return null;
        }

        public static T GetSelfOrParent<T>(this FrameworkElement source) where T : FrameworkElement
        {
            if (source == null)
                return default(T);

            if (typeof(T).IsAssignableFrom(source.GetType()))
                return (T)source;

            return (source.Parent as FrameworkElement).GetSelfOrParent<T>();
        }

        public static T GetVisualSelfOrParent<T>(this DependencyObject source) where T : DependencyObject
        {
            //Logger.TraceWithMethodName("source:" + source);
            if (source == null)
                return default(T);

            if (typeof(T).IsAssignableFrom(source.GetType()))
                return (T)source;

            if (!(source is Visual) && !(source is Visual3D))
                return default(T);

            return VisualTreeHelper.GetParent(source).GetVisualSelfOrParent<T>();
        }

        public static T GetSelectedTag<T>(this TreeView treeView)
        {
            return GetSelectedTag<T>(treeView, false);
        }

        public static T GetSelectedTag<T>(this TreeView treeView, bool recursive)
        {
            if (treeView == null)
                return default(T);

            if (treeView.SelectedItem == null)
                return default(T);

            if (typeof(T).IsAssignableFrom(treeView.SelectedItem.GetType()))
                return (T)treeView.SelectedItem;

            object tag = null;
            var fe = treeView.SelectedItem as FrameworkElement;
            if (fe != null)
            {
                tag = fe.Tag;
            }

            if (tag != null && typeof(T).IsAssignableFrom(tag.GetType()))
                return (T)tag;

            if (recursive)
            {
                var ao = treeView.SelectedItem as TreeItem;
                if (ao != null)
                    return ao.Get<T>();
            }
            return default(T);
        }

        public static IEnumerable<T> GetChildren<T>(this DependencyObject obj)
        {
            if (obj == null)
                yield break;

            foreach (object item in LogicalTreeHelper.GetChildren(obj))
            {
                if (item == null)
                    continue;

                if (typeof(T).IsAssignableFrom(item.GetType()))
                    yield return (T)item;

                var dep = item as DependencyObject;
                if (dep != null)
                {
                    foreach (T child in dep.GetChildren<T>())
                    {
                        yield return child;
                    }
                }
            }
        }

        public static void SetTracing()
        {
            SetTracing(SourceLevels.Warning, null);
        }

        public static void SetTracing(SourceLevels levels, TraceListener listener)
        {
            if (listener == null)
            {
                listener = new DefaultTraceListener();
            }

            PresentationTraceSources.Refresh();
            foreach (PropertyInfo pi in typeof(PresentationTraceSources).GetProperties(BindingFlags.Static | BindingFlags.Public))
            {
                if (pi.Name == "FreezableSource")
                    continue;

                if (typeof(TraceSource).IsAssignableFrom(pi.PropertyType))
                {
                    var ts = (TraceSource)pi.GetValue(null, null);
                    ts.Listeners.Add(listener);
                    ts.Switch.Level = levels;
                }
            }
        }

        public static IEnumerable<DependencyProperty> EnumerateMarkupDependencyProperties(object element)
        {
            if (element != null)
            {
                MarkupObject markupObject = MarkupWriter.GetMarkupObjectFor(element);
                if (markupObject != null)
                {
                    foreach (MarkupProperty mp in markupObject.Properties)
                    {
                        if (mp.DependencyProperty != null)
                            yield return mp.DependencyProperty;
                    }
                }
            }
        }

        public static IEnumerable<DependencyProperty> EnumerateMarkupAttachedProperties(object element)
        {
            if (element != null)
            {
                MarkupObject markupObject = MarkupWriter.GetMarkupObjectFor(element);
                if (markupObject != null)
                {
                    foreach (MarkupProperty mp in markupObject.Properties)
                    {
                        if (mp.IsAttached)
                            yield return mp.DependencyProperty;
                    }
                }
            }
        }

        public static void RaiseMenuItemClickOnKeyGesture(this ItemsControl control, KeyEventArgs args)
        {
            RaiseMenuItemClickOnKeyGesture(control, args, true);
        }

        public static void RaiseMenuItemClickOnKeyGesture(this ItemsControl control, KeyEventArgs args, bool throwOnError)
        {
            if (args == null)
                throw new ArgumentNullException("e");

            if (control == null)
                return;

            var kgc = new KeyGestureConverter();
            foreach(var item in control.Items.OfType<MenuItem>())
            {
                if (!string.IsNullOrWhiteSpace(item.InputGestureText))
                {
                    KeyGesture gesture = null;
                    if (throwOnError)
                    {
                        gesture = kgc.ConvertFrom(item.InputGestureText) as KeyGesture;
                    }
                    else
                    {
                        try
                        {
                            gesture = kgc.ConvertFrom(item.InputGestureText) as KeyGesture;
                        }
                        catch
                        {
                        }
                    }

                    if (gesture != null && gesture.Matches(null, args))
                    {
                        //System.Diagnostics.Trace.WriteLine("MATCH item:" + item + ", key:" + e.Key + " Keyboard.Modifiers:" + Keyboard.Modifiers);
                        item.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
                        args.Handled = true;
                        return;
                    }
                }

                RaiseMenuItemClickOnKeyGesture(item, args, throwOnError);
                if (args.Handled)
                    return;
            }
        }

        public static BitmapSource GetStockIcon(WindowsUtilities.StockIconId id, WindowsUtilities.SHGSI flags)
        {
            return Imaging.CreateBitmapSourceFromHIcon(WindowsUtilities.GetStockIcon(id, flags).Handle, Int32Rect.Empty, null);
        }

        public static string GetErrorText(this Exception exception, JsonUtilitiesOptions options)
        {
            if (exception == null)
                return null;

            if (options == null)
            {
                options = new JsonUtilitiesOptions();
            }

            string error = CodeFluentRuntimeException.GetAllMessages(exception);
            string extra = null;
            var we = exception as WebException;
            if (we != null && we.Response != null)
            {
                Stream stream = we.Response.GetResponseStream();
                if (stream != null && stream.CanRead)
                {
                    using (var reader = new StreamReader(stream))
                    {
                        extra = reader.ReadToEnd();
                        if (we.Response.Headers[HttpResponseHeader.ContentType] == "application/json" && extra != null)
                        {
                            options.ThrowExceptions = false;
                            var dic = (Dictionary<string, object>)JsonUtilities.Deserialize(extra, null, options);
                            object ex;
                            if (dic.TryGetValue("Exception", out ex))
                            {
                                if (ex != null)
                                {
                                    extra = string.Format("{0}", ex);
                                }
                                else
                                {
                                    // try without deserialization
                                    options.SerializationOptions &= ~JsonSerializationOptions.UseISerializable;
                                    var dic2 = (Dictionary<string, object>)JsonUtilities.Deserialize(extra, null, options);
                                    if (dic2.TryGetValue("Exception", out ex) && ex is IDictionary<string, object>)
                                    {
                                        var dicex = (IDictionary<string, object>)ex;
                                        var sb = new StringBuilder(Environment.NewLine);
                                        sb.AppendLine("Source: " + dicex.GetValue<string>("Source", null));
                                        sb.AppendLine("Message: " + dicex.GetValue<string>("Message", null));
                                        sb.AppendLine("Class: " + dicex.GetValue<string>("ClassName", null));
                                        sb.AppendLine(dicex.GetValue<string>("StackTraceString", null));
                                        extra = sb.ToString();
                                    }
                                    else
                                    {
                                        if (dic.TryGetValue("FullMessage", out ex))
                                        {
                                            extra = string.Format("{0}", ex);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            if (extra != null)
                return error + Environment.NewLine + extra;

            return error;
        }

        public static void Browse(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return;

            WindowsUtilities.OpenDefaultBrowser(url);
        }

        public static bool EqualsIgnoreCase(this string thisString, string text)
        {
            return EqualsIgnoreCase(thisString, text, false);
        }

        public static bool EqualsIgnoreCase(this string thisString, string text, bool trim)
        {
            if (trim)
            {
                thisString = ConvertUtilities.Nullify(thisString, true);
                text = ConvertUtilities.Nullify(text, true);
            }

            if (thisString == null)
                return text == null;

            if (text == null)
                return false;

            if (thisString.Length != text.Length)
                return false;

            return string.Compare(thisString, text, StringComparison.OrdinalIgnoreCase) == 0;
        }

        public static object GetValue(this IDictionary<string, string> dictionary, string key, Type type, object defaultValue)
        {
            if (key == null)
                throw new ArgumentNullException("key");

            object def = ConvertUtilities.ChangeType(defaultValue, type);
            if (dictionary == null)
                return def;

            string o;
            if (!dictionary.TryGetValue(key, out o))
                return def;

            return ConvertUtilities.ChangeType(o, type, def);
        }

        public static object GetValue(this IDictionary<string, object> dictionary, string key, Type type, object defaultValue)
        {
            if (key == null)
                throw new ArgumentNullException("key");

            object def = ConvertUtilities.ChangeType(defaultValue, type);
            if (dictionary == null)
                return def;

            object o;
            if (!dictionary.TryGetValue(key, out o))
                return def;

            return ConvertUtilities.ChangeType(o, type, def);
        }

        public static T GetValue<T>(this IDictionary<string, object> dictionary, string key, T defaultValue)
        {
            if (key == null)
                throw new ArgumentNullException("key");

            if (dictionary == null)
                return defaultValue;

            object o;
            if (!dictionary.TryGetValue(key, out o))
                return defaultValue;

            return ConvertUtilities.ChangeType(o, defaultValue);
        }

        public static T GetValue<T>(this IDictionary<string, string> dictionary, string key, T defaultValue)
        {
            if (key == null)
                throw new ArgumentNullException("key");

            if (dictionary == null)
                return defaultValue;

            string o;
            if (!dictionary.TryGetValue(key, out o))
                return defaultValue;

            return ConvertUtilities.ChangeType(o, defaultValue);
        }
    }
}
