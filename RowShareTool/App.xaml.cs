using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace RowShareTool
{
    public partial class App : Application
    {
        public App()
        {
            AppDomain.CurrentDomain.UnhandledException += ErrorBox.OnCurrentDomainUnhandledException;
            DispatcherUnhandledException += ErrorBox.OnDispatcherUnhandledException;
        }
    }

    [ContentProperty("DataTemplates")]
    public class TypeDataTemplateSelector : DataTemplateSelector
    {
        public TypeDataTemplateSelector()
        {
            DataTemplates = new ObservableCollection<DataTemplate>();
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public ObservableCollection<DataTemplate> DataTemplates { get; private set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            foreach (DataTemplate dt in DataTemplates.OfType<DataTemplate>().Where(dt => dt.DataType is Type))
            {
                Type type = (Type)dt.DataType;
                if (item == null)
                {
                    if (!type.IsValueType)
                        return dt;
                }
                else
                {
                    if (type.IsAssignableFrom(item.GetType()))
                        return dt;
                }
            }
            return DataTemplates.OfType<DataTemplate>().FirstOrDefault(dt => dt.DataType == null);
        }
    }
}
