using System;
using System.Reflection;
using System.Windows;

namespace RowShareTool
{
    public partial class AboutWindow : Window
    {
        public AboutWindow()
        {
            InitializeComponent();
            Header.Text = "RowShare Tool V" + Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion + Environment.NewLine +
                "Copyright © 2015-" + DateTime.Now.Year + " SoftFluent S.A.S." + Environment.NewLine +
                "All rights reserved.";
        }
    }
}
