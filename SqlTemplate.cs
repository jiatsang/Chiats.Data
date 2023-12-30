// ------------------------------------------------------------------------
// Chiats Common&Data Library V4.1.21 (2021/08)
// Chiats@Studio(http://www.chiats.com/Common)
// Design&Coding By Chia Tsang Tsai
//Copyright(C) 2005-2022 Chiats@Studio All 
// ------------------------------------------------------------------------
using Chiats.SQL;
using System;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Threading;

namespace Chiats.Data
{
    /// <summary>
    /// 支援 SQL Server 2005/2008/2012 的資料存取物件.
    /// </summary>
    [ConnectionObjectType(typeof(SqlTemplate.ConnectionClass))]
    public sealed class SqlTemplate : DbTemplate<SqlConnection, SqlTransaction, SqlCommand>
    {
        /// <summary>
        /// 建構子
        /// </summary>
        /// <param name="name"></param>
        public SqlTemplate(string name) : base(name) { }

        /// <summary>
        /// 建構子
        /// </summary>
        public SqlTemplate() : base("default") { }

        /// <summary>
        /// 建構子
        /// </summary>
        /// <param name="name"></param>
        /// <param name="TransactionMode"></param>
        public SqlTemplate(string name, TransactionMode TransactionMode) : base(name, TransactionMode) { }

        /// <summary>
        /// 建構子
        /// </summary>
        /// <param name="TransactionMode"></param>
        public SqlTemplate(TransactionMode TransactionMode) : base("default", TransactionMode) { }
        /// <summary>
        /// 建構子
        /// </summary>
        /// <param name="name"></param>
        /// <param name="TransactionMode"></param>
        /// <param name="ConnectionString">資料庫連線字串</param>
        public SqlTemplate(string name, TransactionMode TransactionMode, string ConnectionString) :
            base(name, TransactionMode, ConnectionString)
        {
        }


        public SqlTemplate(string name, SqlConnectionStringBuilder ConnectionString, TransactionMode TransactionMode = TransactionMode.Transaction) :
          base(name, TransactionMode, ConnectionString.ConnectionString)
        { }

        /// <summary>
        ///
        /// </summary>
        /// <param name="StringBuilder"></param>
        /// <param name="TransactionMode"></param>
        public SqlTemplate(SqlConnectionStringBuilder StringBuilder, TransactionMode TransactionMode = TransactionMode.Transaction) : base(null, TransactionMode, StringBuilder.ConnectionString)
        { }

        /// <summary>
        /// 存放  ADO.NET 實體的 Connection(SqlConnection) 物件.
        /// </summary>
        public new class ConnectionClass : DbTemplate<SqlConnection, SqlTransaction, SqlCommand>.ConnectionClass
        {

            /// <summary>
            /// 存放  ADO.NET 實體的 Connection(SqlConnection) 物件.
            /// </summary>
            /// <param name="template"></param>
            public ConnectionClass(DbTemplate<SqlConnection, SqlTransaction, SqlCommand> template) : base(template) { }

            /// <summary>
            /// 開啟連線, 每一個資料庫實際連線的時機會由 DACTemplate TransManager and  TransScope 決定.
            /// 當需要連線會通知 ConnectionDataPack.ConnectionOpen(). 因此 ConnectionOpen 必須在被呼叫時
            /// 建立實際的連線.
            /// </summary>
            public override void ConnectionOpen(string connectionString)
            {
                if (!Template.State.IsConnect)
                    throw new DatabaseConnectFailureException(Template.State.Message);
                try
                {
                    if (connection == null)
                    {
                        connection = new SqlConnection { ConnectionString = connectionString };
                        ConnectionIndex = Interlocked.Increment(ref connectionMax);

                        startTime = DateTime.Now;

                        connection.Open();
                        this.ClientConnectionId = connection.ClientConnectionId;

                        Debug.Print($"Connection Time: {(DateTime.Now - startTime).TotalMilliseconds:#,##0.000}ms // 連結資料庫花費時間");

                        // 當啟用 FireInfoMessageEventOnUserErrors 時, ADO.NET 就不會引發 Exception
                        connection.InfoMessage += Connection_InfoMessage;
                        // connection.FireInfoMessageEventOnUserErrors = true;
                        // Debug.Print($"#Check SqlConnection.Open ConnectionManagerID:{ConnectionIndex} Time:{startTime:yyyy-MM-dd HH:mm:ss} [{Template.TemplateId}] {connectionString}");

                        using (var cc = connection.CreateCommand())
                        {
                            cc.CommandText = "select @@SPID";
                            this.SPID = cc.ExecuteScalar().ChangeType<int>();
                        }

                        Template.RaiseCommandTransaction(Template, new CommandTransactionEventArgs
                        {
                            Name = Template.Name,
                            SPID = SPID,
                            ThreadId = System.Threading.Thread.CurrentThread.ManagedThreadId,
                            TemplateId = Template.TemplateId,
                            Transaction = $"Connection.Open ConnectionManagerID_{ConnectionIndex} "
                        });
                        //Debug.Print($"#Check SqlConnection.Open SPID:{SPID} ConnectionManagerID:{ConnectionIndex} Time:{startTime:yyyy-MM-dd HH:mm:ss} [{Template.TemplateId}]");

                        if (this.TransactionMode == TransactionMode.Transaction)
                        {
                            transaction = connection.BeginTransaction(IsolationLevel.ReadUncommitted);
                            Template.RaiseCommandTransaction(Template, new CommandTransactionEventArgs
                            {
                                Name = Template.Name,
                                SPID = SPID,
                                ThreadId = System.Threading.Thread.CurrentThread.ManagedThreadId,
                                TemplateId = Template.TemplateId,
                                Transaction = $"Transaction.Begin(Auto) ConnectionManagerID_{ConnectionIndex}"
                            });
                            //Debug.Print($"#Check Transaction.Begin(auto) SPID:{SPID} ConnectionManagerID:{ConnectionIndex} [{Template.TemplateId}] {ClientConnectionId}");
                        }
                        else
                        {
                            transaction = null;
                        }
                    }
                }
                catch (CommonException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    SetConnectionState(true, ex.Message);
                    if (connection != null)
                    {
                        SqlConnectionStringBuilder sb = new SqlConnectionStringBuilder(connection.ConnectionString);
                        sb.Password = "*****"; // mask Password in Exception
                        var cs = sb.ConnectionString;
                        throw new DatabaseConnectFailureException($"SQL 連結失敗({ex.Message}) => {cs}", ex);
                    }
                    else
                    {
                        throw new DatabaseConnectFailureException($"SQL 連結失敗({ex.Message})", ex);
                    }
                }
            }

            private void Connection_InfoMessage(object sender, SqlInfoMessageEventArgs e)
            {
                this.RaiseInfoMessage(new InfoMessageEventArgs { Message = e.Message, Source = e.Source });
            }

            /// <summary>
            /// 檢查連線是否開啟
            /// </summary>
            /// <returns>開啟則回 True</returns>
            public override bool IsOpen()
            {
                return (connection != null);
            }

            /// <summary>
            /// 建立一個 TCommand 實體物件.
            /// </summary>
            /// <returns></returns>
            public override SqlCommand CreateCommand()
            {
                return new SqlCommand();
            }
        }

        /// <summary>
        /// Fill to DataTable
        /// </summary>
        /// <param name="command"></param>
        /// <param name="table"></param>
        protected override void Fill(SqlCommand command, DataTable table)
        {
            SqlDataAdapter adapter = new SqlDataAdapter(command);
            adapter.Fill(table);
        }
        /// <summary>
        /// Fill to DataTable
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="ds"></param>
        protected override void Fill(SqlCommand cmd, DataSet ds)
        {
            SqlDataAdapter adapter = new SqlDataAdapter(cmd);
            adapter.Fill(ds);
        }

        private IDbInformation DbInformation = null;

        /// <summary>
        ///
        /// </summary>
        protected override IDbInformation GetDBInformation()
        {
            if (DbInformation == null)
            {
                DbInformation = new SqlDbInformation(this);
            }
            return DbInformation;
        }

        /// <summary>
        /// Parameter Convert
        /// </summary>
        /// <param name="name"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        protected override IDataParameter ParameterConvert(string name, Parameter param)
        {

            switch (param.Value)
            {
                case null:
                    return new SqlParameter(name, DBNull.Value);
                case DateTime datetime:
                    var p = new SqlParameter(name, SqlDbType.DateTime2);
                    p.Value = param.Value;
                    return p;
                default:
                    return new SqlParameter(name, param.Value);
            }
        }

        protected override IDataParameter ParameterConvert(DbParameter param)
        {
            SqlParameter sp = param.Value == null ?
                new SqlParameter(param.ParameterName, DBNull.Value) :
                new SqlParameter(param.ParameterName, param.Value);

            sp.DbType = param.DbType;
            sp.Direction = param.Direction;

            if (param.Size != 0) sp.Size = param.Size;

            sp.IsNullable = param.IsNullable;

            return sp;
        }
    }
}