using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Management;
using System.Reflection;
using System.Text;
using CodeFluent.Runtime.Utilities;

namespace RowShareTool.Utilities
{
    public class SystemInformation
    {
        private List<Asm> _loadedAssemblies;

        [Category("System")]
        public string FrameworkInstalledVersions
        {
            get
            {
                StringBuilder fxVersions = new StringBuilder();
                foreach (Version version in ConvertUtilities.FrameworkVersions)
                {
                    if (fxVersions.Length > 0)
                    {
                        fxVersions.Append(", ");
                    }
                    fxVersions.Append(version);
                }
                return fxVersions.ToString();
            }
        }

        [Category("System")]
        public string VirtualMachine
        {
            get
            {
                return SystemUtilities.GetVirtualMachineInformation();
            }
        }

        [Category("System")]
        public string BoardModel
        {
            get
            {
                return GetManagementInfo<string>("Win32_ComputerSystem", "Model", null);
            }
        }

        [Category("System")]
        public string BoardManufacturer
        {
            get
            {
                return GetManagementInfo<string>("Win32_ComputerSystem", "Manufacturer", null);
            }
        }

        [Category("System")]
        public string Processor
        {
            get
            {
                return GetManagementInfo<string>("Win32_Processor", "Name", null);
            }
        }

        //[Category("System")]
        //public string ScriptEngineVersion
        //{
        //    get
        //    {
        //        var v = ScriptManager.ScriptEngineVersion;
        //        return v != null ? v.ToString() : null;
        //    }
        //}

        [Category("System")]
        [Description("OS Version")]
        public virtual string OSVersion
        {
            get
            {
                return Environment.OSVersion.ToString();
            }
        }

        [Category("System")]
        [Description("CLR Version")]
        public virtual string ClrVersion
        {
            get
            {
                return Environment.Version.ToString();
            }
        }

        [Category("System")]
        public virtual string MachineName
        {
            get
            {
                return Environment.MachineName;
            }
        }

        [Category("Process")]
        public virtual TokenElevationType TokenElevationType
        {
            get
            {
                return SystemUtilities.GetTokenElevationType();
            }
        }

        //[Category("System")]
        //public virtual float DesktopDpiX
        //{
        //    get
        //    {
        //        return SystemParameters.DesktopDpiX;
        //    }
        //}

        //[Category("System")]
        //public virtual float DesktopDpiY
        //{
        //    get
        //    {
        //        return SystemParameters.DesktopDpiY;
        //    }
        //}

        [Category("Process")]
        [Description("CodeFluent Entities Version")]
        public string CodeFluentVersion
        {
            get
            {
                Assembly asm = typeof(JsonUtilities).Assembly;
                string version = AssemblyUtilities.GetInformationalVersion(asm);
                string conf = AssemblyUtilities.GetConfiguration(asm);
                if (string.IsNullOrWhiteSpace(conf))
                {
                    string title = AssemblyUtilities.GetTitle(asm);
                    if (title != null)
                    {
                        int pos = title.LastIndexOf('-');
                        if (pos >= 0)
                        {
                            version += " - " + title.Substring(pos + 1).Trim();
                        }
                    }
                }
                DateTime? dt = AssemblyUtilities.GetLinkerTimestamp(asm);
                if (dt.HasValue)
                {
                    version += " - Compiled " + dt.Value;
                }

                return version;
            }
        }

        [Category("Localization")]
        public string Location
        {
            get
            {
                var c = CodeFluent.Runtime.Utilities.Country.Location;
                return c!= null ? c.EnglishName : null;
            }
        }

        [Category("Localization")]
        public string Country
        {
            get
            {
                var c = CodeFluent.Runtime.Utilities.Country.Current;
                return c != null ? c.EnglishName : null;
            }
        }

        [Category("Localization")]
        [Description("UI Country")]
        public string UICountry
        {
            get
            {
                var c = CodeFluent.Runtime.Utilities.Country.UICurrent;
                return c != null ? c.EnglishName : null;
            }
        }

        [Category("Process")]
        public virtual string CurrentDirectory
        {
            get
            {
                return Environment.CurrentDirectory;
            }
        }

        [Category("Process")]
        public virtual string CommandLine
        {
            get
            {
                return Environment.CommandLine;
            }
        }

        [Category("System")]
        [Description("64-bit")]
        public virtual bool Is64BitOperatingSystem
        {
            get
            {
                return Environment.Is64BitOperatingSystem;
            }
        }

        [Category("Process")]
        [Description("64-bit")]
        public virtual bool Is64BitProcess
        {
            get
            {
                return Environment.Is64BitProcess;
            }
        }


        [Category("System")]
        public virtual int ProcessorCount
        {
            get
            {
                return Environment.ProcessorCount;
            }
        }

        [Category("User")]
        [Description("Domain")]
        public virtual string UserDomainName
        {
            get
            {
                return Environment.UserDomainName;
            }
        }

        [Category("User")]
        [Description("Name")]
        public virtual string UserName
        {
            get
            {
                return Environment.UserName;
            }
        }

        [Category("Process")]
        public virtual string AssemblyInformationalVersion
        {
            get
            {
                return AssemblyUtilities.GetInformationalVersion(Assembly.GetExecutingAssembly());
            }
        }

        [Category("Process")]
        public virtual string AssemblyConfiguration
        {
            get
            {
                return AssemblyUtilities.GetConfiguration(Assembly.GetExecutingAssembly());
            }
        }

        [Category("Process")]
        [Description("Assembly Compile Date")]
        public virtual DateTime? AssemblyLinkerTimestamp
        {
            get
            {
                return AssemblyUtilities.GetLinkerTimestamp(Assembly.GetExecutingAssembly());
            }
        }

        [Category("Process")]
        public Asm[] LoadedAssemblies
        {
            get
            {
                if (_loadedAssemblies == null)
                {
                    _loadedAssemblies = new List<Asm>();
                    foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
                    {
                        Asm a = new Asm(asm);
                        if (string.IsNullOrWhiteSpace(a.Location))
                            continue;

                        _loadedAssemblies.Add(a);
                    }
                    _loadedAssemblies.Sort();
                }
                return _loadedAssemblies.ToArray();
            }
        }

        public class Asm : IComparable, IComparable<Asm>
        {
            public string Name { get; private set; }
            public string Location { get; private set; }
            public string Version { get; private set; }
            public string InformationalVersion { get; private set; }
            public DateTime? CompileDate { get; private set; }

            public Asm(Assembly asm)
            {
                Name = asm.FullName;
                if (!AssemblyUtilities.IsDynamic(asm))
                {
                    try
                    {
                        Location = asm.Location;
                    }
                    catch
                    {
                        // do nothing
                    }
                }

                if (string.IsNullOrEmpty(Location))
                {
                    Location = " ";
                }
                AssemblyFileVersionAttribute vatt = AssemblyUtilities.GetAttribute<AssemblyFileVersionAttribute>(asm);
                if (vatt != null)
                {
                    Version = vatt.Version;
                }
                else
                {
                    AssemblyVersionAttribute att = AssemblyUtilities.GetAttribute<AssemblyVersionAttribute>(asm);
                    if (att != null)
                    {
                        Version = att.Version;
                    }
                }
                InformationalVersion = AssemblyUtilities.GetInformationalVersion(asm);
                if (string.IsNullOrEmpty(Version))
                {
                    Version = InformationalVersion;
                }
                CompileDate = AssemblyUtilities.GetLinkerTimestamp(asm);
            }

            public override string ToString()
            {
                return Name;
            }

            int IComparable.CompareTo(object obj)
            {
                return CompareTo(obj as Asm);
            }

            public int CompareTo(Asm other)
            {
                if (other == null)
                    throw new ArgumentNullException("other");

                return Name.CompareTo(other.Name);
            }
        }

        public static T GetManagementInfo<T>(string className, string propertyName, T defaultValue)
        {
            if (className == null)
                throw new ArgumentNullException("className");

            if (propertyName == null)
                throw new ArgumentNullException("propertyName");

            try
            {
                foreach (ManagementObject mo in new ManagementObjectSearcher(new WqlObjectQuery("select * from " + className)).Get())
                {
                    foreach (PropertyData data in mo.Properties)
                    {
                        if (data == null || data.Name == null)
                            continue;

                        if (data.Name.EqualsIgnoreCase(propertyName))
                            return ConvertUtilities.ChangeType(data.Value, defaultValue);
                    }
                }
            }
            catch
            {
                // do nothing
            }
            return defaultValue;
        }
    }
}
