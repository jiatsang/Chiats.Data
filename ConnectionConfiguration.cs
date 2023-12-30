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
    /// ��Ʈw�s�u�t�m�T��.
    /// </summary>
    public partial class ConnectionConfiguration : MarshalByRefObject
    {
        /// <summary>
        /// �ŭ�.
        /// </summary>
        public static ConnectionConfiguration Empty = new ConnectionConfiguration();

        private ConnectionStringBuilder connectionStringBuilder = null;
        public ConnectionConfiguration()
        {
        }

        /// <summary>
        /// ��Ʈw�s�u�t�m�T���غc�l ,
        /// </summary>
        /// <param name="Name">�s�u�ѧO�W��</param>
        /// <param name="ConnectionString">�s�u�r��</param>
        /// <param name="TransactionMode">Transaction Mode</param>
        /// <param name="CommandTimeout">���ݩR�O���檺�ɶ� (�H�����)�C �w�]�Ȭ� 30 ��( -1)�A0 ��ܥû��� timeout </param>
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
            // Oracle ����� ApplicationName
            this.Name = Name;
            connectionStringBuilder = new ConnectionStringBuilder(ConnectionString);
        }

        /// <summary>
        /// �s�u�ѧO�W��
        /// </summary>
        public string Name { get; private set; }
        ///// <summary>
        ///// �s�u�ѧO�W��
        ///// </summary>
        public string Database { get { return connectionStringBuilder.InitialCatalog; } }
        public string UserID { get { return connectionStringBuilder.UserID; } }

        public string DataSource { get { return connectionStringBuilder.DataSource; } }

        /// <summary>
        /// �s�u�r��
        /// </summary>
        public string ConnectionString => connectionStringBuilder.ConnectionString;

        public int CommandTimeout { get; set; } = -1;
    }
}