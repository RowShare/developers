using System;
using System.Collections.Generic;
using System.ComponentModel;
using CodeFluent.Runtime.Utilities;

namespace RowShareTool
{
    public class Settings : Serializable<Settings>
    {
        private List<SettingsServer> _servers;

        public Settings()
        {
            _servers = new List<SettingsServer>();
        }

        [Browsable(false)]
        public SettingsServer[] Servers
        {
            get
            {
                return _servers.ToArray();
            }
            set
            {
                if (value != null)
                {
                    _servers = new List<SettingsServer>(value);
                }
                else
                {
                    _servers = new List<SettingsServer>();
                }
            }
        }

        public bool AddServer(SettingsServer server)
        {
            if (server == null)
                throw new ArgumentNullException("server");

            SettingsServer existing = GetServer(server.Url);
            if (existing != null)
                return false;

            _servers.Add(server);
            return true;
        }

        public SettingsServer GetServer(string url)
        {
            if (url == null)
                throw new ArgumentNullException("url");

            return _servers.Find(s => s.Url.EqualsIgnoreCase(url));
        }

        public bool RemoveServer(SettingsServer server)
        {
            if (server == null)
                throw new ArgumentNullException("server");

            SettingsServer existing = GetServer(server.Url);
            if (existing == null)
                return false;

            bool b = _servers.Remove(existing);
            return b;
        }
    }
}
