// ------------------------------------------------------------------------
// Chiats Common&Data Library V4.1.21 (2021/08)
// Chiats@Studio(http://www.chiats.com/Common)
// Design&Coding By Chia Tsang Tsai
// Copyright(C) 2005-2022 Chiats@Studio All 
// ------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Text;

namespace Chiats.Data
{

    public class ConnectionStringBuilder
    {
        private Dictionary<string, string> currentValues = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        public string ConnectionString
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                foreach (var key in currentValues.Keys)
                {
                    string val = currentValues[key];
                    if (sb.Length != 0) sb.Append(';');
                    sb.Append($"{key}={val}");
                }
                return sb.ToString();
            }
        }
        public ConnectionStringBuilder(string ConnectionString)
        {
            string[] values = ConnectionString.Split(';');
            foreach (var val in values)
            {
                int index = val.IndexOf('=');
                if (index != -1)
                {
                    string name = val.Substring(0, index);
                    string nval = val.Substring(index + 1);
                    if (!currentValues.ContainsKey(name))
                    {
                        currentValues.Add(name, nval);
                    }
                }
            }
        }
        private string GetValue(string Key = null, /*[System.Runtime.CompilerServices.CallerMemberName]*/ string PropertyName = null)
        {
            string namekey = Key ?? PropertyName;
            if (currentValues.ContainsKey(namekey))
                return currentValues[namekey];
            return null;
        }
        private void SetValue(string Value, string Key = null, /*[System.Runtime.CompilerServices.CallerMemberName]*/ string PropertyName = null)
        {
            string namekey = Key ?? PropertyName;
            if (currentValues.ContainsKey(namekey))
                currentValues[namekey] = Value;
            else
                currentValues.Add(namekey, Value);
        }

        public string UserID { get { return GetValue("User ID"); } set { SetValue(value, "User ID"); } }
        public string Password { get { return GetValue(); } set { SetValue(value); } }
        public string DataSource { get { return GetValue("Data Source"); } set { SetValue(value, "Data Source"); } }
        public string InitialCatalog { get { return GetValue("Initial Catalog"); } set { SetValue(value, "Initial Catalog"); } }

        public string ConnectionTimeout { get { return GetValue("Connect Timeout"); } set { SetValue(value, "Connect Timeout"); } }
        public string ApplicationName { get { return GetValue("Application Name"); } set { SetValue(value, "Application Name"); } }
    }
}
