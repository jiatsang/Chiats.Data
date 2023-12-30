// ------------------------------------------------------------------------
// Chiats Common&Data Library V4.1.21 (2021/08)
// Chiats@Studio(http://www.chiats.com/Common)
// Design&Coding By Chia Tsang Tsai
// Copyright(C) 2005-2022 Chiats@Studio All 
// ------------------------------------------------------------------------

using System;
using System.Data.Common;
using System.Data;

namespace Chiats.Data
{
    /// <summary>
    /// 資料庫連線配置訊息.
    /// </summary>
    public partial class ConnectionConfiguration : MarshalByRefObject
    {
        /// <summary>
        /// 空值.
        /// </summary>
        public static ConnectionConfiguration Empty = new ConnectionConfiguration();

        private ConnectionStringBuilder connectionStringBuilder = null;
        public ConnectionConfiguration()
        {
        }

        /// <summary>
        /// 資料庫連線配置訊息建構子 ,
        /// </summary>
        /// <param name="Name">連線識別名稱</param>
        /// <param name="ConnectionString">連線字串</param>
        /// <param name="TransactionMode">Transaction Mode</param>
        /// <param name="CommandTimeout">等待命令執行的時間 (以秒為單位)。 預設值為 30 秒( -1)，0 表示永遠不 timeout </param>
        public ConnectionConfiguration(string Name, string ConnectionString, string ApplicationName, int CommandTimeout = -1, string ConnectTimeout = null)
        {
            this.Name = Name;
            this.CommandTimeout = CommandTimeout;


            connectionStringBuilder = new ConnectionStringBuilder(ConnectionString);

            if (!string.IsNullOrWhiteSpace(ConnectTimeout))
            {
                connectionStringBuilder.ConnectionTimeout = ConnectTimeout;
            }


            if (!string.IsNullOrWhiteSpace(ApplicationName))
            {
                connectionStringBuilder.ApplicationName = ApplicationName;
            }
        }

        public ConnectionConfiguration(string Name, string ConnectionString)
        {
            // Oracle 不支持 ApplicationName
            this.Name = Name;
            connectionStringBuilder = new ConnectionStringBuilder(ConnectionString);
        }

        /// <summary>
        /// 連線識別名稱
        /// </summary>
        public string Name { get; private set; }
        ///// <summary>
        ///// 連線識別名稱
        ///// </summary>
        public string Database { get { return connectionStringBuilder.InitialCatalog; } }
        public string UserID { get { return connectionStringBuilder.UserID; } }

        public string DataSource { get { return connectionStringBuilder.DataSource; } }

        /// <summary>
        /// 連線字串
        /// </summary>
        public string ConnectionString => connectionStringBuilder.ConnectionString;

        public int CommandTimeout { get; set; } = -1;
    }
}