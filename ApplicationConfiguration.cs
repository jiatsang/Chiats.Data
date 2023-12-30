// ------------------------------------------------------------------------
// Chiats Common&Data Library V4.1.21 (2021/08)
// Chiats@Studio(http://www.chiats.com/Common)
// Design&Coding By Chia Tsang Tsai
// Copyright(C) 2005-2022 Chiats@Studio All 
// ------------------------------------------------------------------------

using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime;
using System.Xml.Linq;

namespace Chiats.Data
{
    /// <summary>
    /// Application Configuration
    /// </summary>
    public static class ApplicationConfiguration
    {
        private static Dictionary<string, ConnectionConfiguration> Connections = new Dictionary<string, ConnectionConfiguration>();
        private static Dictionary<string, string>Settings = new Dictionary<string, string>();

        /// <summary>
        /// 回傳目前資料庫連線的名稱集合
        /// </summary>
        public static ICollection<string> ConnectionKeys { get { return Connections.Keys; } }

        /// <summary>
        /// 取得資料庫連線配置檔.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static ConnectionConfiguration Query(string name)
        {
            if (Connections.ContainsKey(name))
                return Connections[name];
            return null;
        }
        public static string GetSetting(string name)
        {
            if (Settings.ContainsKey(name))
                return Settings[name];
            return null;
        }


        /// <summary>
        ///
        /// </summary>
        /// <param name="Configuration"></param>
        public static void AddOrReplace(ConnectionConfiguration Configuration)
        {
            if (Connections.ContainsKey(Configuration.Name))
                Connections[Configuration.Name] = Configuration;
            else
                Connections.Add(Configuration.Name, Configuration);
        }
        public static string Database { get; private set; }
        public static string Server { get; private set; }
        public static void Initialize(string configure_file = null)
        {
            XDocument doc;
            if (configure_file == null)
                doc = XDocument.Load("configure.xml");
            else
            {
                doc = XDocument.Load(configure_file);
            }

            IEnumerable<XElement> vars = doc.Root.Element("vars")?.Elements("var");

            foreach (XElement element in vars)
            {
                string name = element.Attribute("name")?.Value;
                if (!string.IsNullOrWhiteSpace(name) && !Settings.ContainsKey(name))
                {
                    Settings.Add(name, element.Attribute("value")?.Value);
                }
            }

            Database = "None";
            Server = "";

            IEnumerable<XElement> connections = doc.Root.Element("connections")?.Elements("connection");
            foreach (XElement element in connections)
            {
                string name = element.Attribute("name")?.Value;
                string cs = element.Attribute("connection-string")?.Value;
                string connection_timeout = element.Attribute("connection-timeout")?.Value;
                string command_timeout = element.Attribute("command-timeout")?.Value;

                if (!string.IsNullOrWhiteSpace(name) && !string.IsNullOrWhiteSpace(cs))
                {
                    var ConnectionConfiguration = new ConnectionConfiguration(name, cs, System.AppDomain.CurrentDomain.FriendlyName);
                    if (name == "default")
                    {
                        Database = ConnectionConfiguration.Database;
                        Server = ConnectionConfiguration.DataSource;
                    }
                    AddOrReplace(ConnectionConfiguration);
                    // Debug.Print($"Create ConnectionConfiguration {name}-{cs}");
                }
            }
        }
    }

    
}