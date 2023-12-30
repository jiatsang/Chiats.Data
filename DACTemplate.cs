// ------------------------------------------------------------------------
// Chiats Common&Data Library V4.1.21 (2021/08)
// Chiats@Studio(http://www.chiats.com/Common)
// Design&Coding By Chia Tsang Tsai
//Copyright(C) 2005-2022 Chiats@Studio All 
// ------------------------------------------------------------------------

using Chiats.SQL;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace Chiats.Data
{

    public sealed class InfoMessageEventArgs : EventArgs
    {
        public string Message { get; set; }
        public string Source { get; set; }
        public override string ToString()
        {
            return Message;
        }
    }

    /// <summary>
    /// 資料庫存取共同基礎介面. 描述存取資料庫基礎共用介面.
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    /// <item><description>TConnection:</description>資料庫存取的連結物件.如 SqlConnection</item>
    /// <item><description>TTransaction:</description>資料庫存取的交易物件.如 SqlTransaction</item>
    /// <item><description>TCommand:</description>資料庫存取的交易物件.如 SqlCommand</item>
    /// </list>
    /// DACTemplate 主要是要管理資料庫連線(Connection)和交易(Transaction). 它和 DataAccessComponent 類別一起合作之模式.並非以共用介面來存取資料庫, 而是以各資料庫自己皂的連結存取方法來執行為設計原則. 所以基本上並未規範如何
    /// 存取資料庫的方法.<br/>
    /// Chiats Data Access Module 實作 DBTemplate 給 ADO.NET 的基礎類別來支援資料庫的存取工作. 只要是符合 ADO.NET 的基礎類別均可以直接
    /// 繼承後使用. SQLTemplate 和 OleDbTemplate 則是分別對 Microsoft SQL Server 和 OleDb 所設計的存取類別.<br/>
    /// Chiats Data Access Module 特色
    /// <list type="bullet">
    /// <item>簡化對資料庫存取之程式. </item>
    /// <item>自動處理對資料庫連線物件的管理. 連線物件的管理會保持一個 Thread 一個連線物件的管理. 不支援多 Thread 連線物件的管理.</item>
    /// <item>元件的設計是以保持原有資料庫存取元件之精神為主要目標, 例如 SQLClient 就會使用 SqlDataReader 作為回傳之物件型?.</item>
    /// <item>支援多資料庫 Transaction 的支援. 同時支援 SQL,Oracle,Informix,DB2 等等 (必須開啟 MS DTC 服務 ,詳見 Systtem.Tracnaction 說明) </item>
    /// <item>自動處理 DBNull 和 null 值轉換的問題. 如果是 DataTable/DataSet 則不會自動轉換.</item>
    /// <item>己提供 OLEDB,SQL,Oracle 資料庫存取元件</item>
    /// <item>支援 Parameter 的用法, 請參考各資料庫存取元件說明. 例如 SQLClient 用 SqlParameter , Oledb 則是 OleDbParameter 等等</item>
    /// <item>允許自行開發新的資料庫存取元件 例如 ODBC,DB2,Informix 等等</item>
    /// <item>支援 ICommandBuilder 或 String Format 來建構 CommandText(sql statement)</item>
    /// <item>支援 SQL Command Log 及 Exception Log . 允許詳細記錄 所有對資料庫的動作, 包含 Transaction </item>
    /// </list>
    /// </remarks>
    public abstract partial class DacTemplate<TConnection, TTransaction, TCommand> : IDacTemplate
    {
        public List<DbMessage> Messages { get; } = new List<DbMessage>();
        public event EventHandler<CommandBuildingEventArgs> CommandBuilding;

        /// <summary>
        /// 資料存取 Exception 的事件
        /// </summary>
        public event EventHandler<CommandExecuteExceptionEventArgs> CommandExecuteException;

        /// <summary>
        /// 全域的事件  資料存取前的事件.
        /// </summary>
        public static event EventHandler<CommandExecutingEventArgs> CommandExecuting;
        /// <summary>
        /// 全域的事件   資料存取完成的事件
        /// </summary>
        public static event EventHandler<CommandExecutedEventArgs> CommandExecuted;
        /// <summary>
        /// 全域的事件 
        /// </summary>
        public static event EventHandler<CommandTransactionEventArgs> CommandTransaction;
        
        public string Message => ConnectionObject(allowNull: true)?.Message.ToString();

        ///// <summary>
        ///// 指示目前參考建立的 ConnectionConfiguration, 如果不是由 參考 Configuration 所建的  Configuration 為Null
        ///// </summary>
        public ConnectionConfiguration CurrentConfiguration { get; private set; }

        /// <summary>
        ///  Gets or sets the wait time before terminating the attempt to execute a command and generating an error.
        /// </summary>
        public int CommandTimeout { get; set; }
        public int ConnectionIndex
        {
            get
            {
                return ConnectionObject(allowNull: true)?.ConnectionIndex ?? -1;
            }
        }

        public void Dispose()
        {
            ConnectionObject(allowNull: true)?.Dispose();
        }


        public override string ToString()
        {
            return $"{this.TemplateId}";
        }

        protected void RaiseCommandBuilding(object sender, CommandBuildingEventArgs e)
        {
            CommandBuilding?.Invoke(sender, e);
        }

        protected void RaiseCommandExecuting(object sender, CommandExecutingEventArgs e)
        {
            CommandExecuting?.Invoke(sender, e);
        }

        protected void RaiseCommandExecuted(object sender, CommandExecutedEventArgs e)
        {
            CommandExecuted?.Invoke(sender, e);
        }

        public void RaiseCommandTransaction(object sender, CommandTransactionEventArgs e)
        {
            CommandTransaction?.Invoke(sender, e);
        }

        protected void RaiseExecuteException(object sender, CommandExecuteExceptionEventArgs e)
        {
            CommandExecuteException?.Invoke(sender, e);
        }

        /// <summary>
        /// 儲存 TransManager 所需的資料庫關連的 Connection 基礎實體物件. 如 Connection 物件<br/>
        /// 本類別是設計給撰寫資料庫連線基礎類別使用. 不是直接的類別.
        /// </summary>
        /// <remarks>
        /// ConnectionObject 採用開放式之架構. 他基本上允許你使用不同的資料庫連結方法. ADO.NET ADO OLEDB ODBC 等等.
        /// public T GetConnection() -> 允許你為你的資料庫連結物件用你所需要的類別 . 如 SqlConnection , OledbConnection 等等.
        /// <example>
        /// <code>
        /// [Transaction(TransactionOption.Required)]
        /// public class MyDataAccess : DataAccessComponent
        /// {
        ///    public static SQLTemplate SQLTemplate = new SQLTemplate("local-sql", false);
        ///       public void DoSelectData()
        ///    {
        ///      string result = SQLTemplate.ExecuteScale(string)("select name from Table4Test where id=123");
        ///      result = SQLTemplate.ExecuteScale&lt;string&gt;("select name from Table4Test where id=1");
        ///    }
        /// }
        /// </code>
        /// </example>
        /// </remarks>
        public abstract class ConnectionManagement : IConnectionManagement, IManualTransaction
        {
            public event EventHandler<InfoMessageEventArgs> InfoMessage;
            protected void RaiseInfoMessage(InfoMessageEventArgs e)
            {
                InfoMessage?.Invoke(this, e);
            }
            /// <summary>
            /// 和 ConnectionObject 關連 Template 物件.
            /// </summary>
            public DacTemplate<TConnection, TTransaction, TCommand> Template { get; private set; }
            public Stack<DacTemplate<TConnection, TTransaction, TCommand>> TemplateStack = new Stack<DacTemplate<TConnection, TTransaction, TCommand>>();

            private static int ConnectionMaxId = 0;
            public readonly int InstanceId = System.Threading.Interlocked.Increment(ref ConnectionMaxId);
            public int SPID { get; protected set; } = -1;

            private int _executeNextOrder = 0;

            protected static int connectionMax = 0;
            public int ConnectionIndex { get; protected set; } = 0;
            /// <summary>
            /// 取得下一個執行的序號
            /// </summary>
            /// <returns></returns>
            public int GetExecuteNextOrder()
            {
                return System.Threading.Interlocked.Increment(ref _executeNextOrder);
            }

            public int AbortCount { get; protected set; } = 0;

            public void SetAbort(string AbortReson, string memberName, string SourceFilePath, int LineNumber)
            {

                Template.RaiseCommandTransaction(Template, new CommandTransactionEventArgs
                {
                    Name = Template.Name,
                    TemplateId = Template.TemplateId,
                    SPID = Template.SPID,
                    ThreadId = System.Threading.Thread.CurrentThread.ManagedThreadId,
                    Transaction = $"SetAbort AbortReson: {AbortReson}"
                });

                //Debug.Print($"\r\n#Check SetAbort [{Template.TemplateId}] {AbortReson} {memberName}  Source:{SourceFilePath} Line:{LineNumber}\r\n");
                AbortCount++;
            }

            /// <summary>
            /// ConnectionObject 建構子 , 建構會由 DACTemplate 繼承者呼叫. 並直接開啟 Connection 連線
            /// </summary>
            /// <param name="template">傳入建構此物件之 DACTemplate 物件</param>
            protected ConnectionManagement(DacTemplate<TConnection, TTransaction, TCommand> template)
            {
                Template = template;
                TransactionMode = template.transactionMode;
            }

            public void ScopeAdd(DacTemplate<TConnection, TTransaction, TCommand> template)
            {
                lock (TemplateStack)
                {
                    TemplateStack.Push(Template);
                    Template = template;
                }
            }
            public bool ScopeRemove()
            {
                lock (TemplateStack)
                {
                    Template = (TemplateStack.Count > 0) ? TemplateStack.Pop() : null;
                    return (Template == null);
                }
            }

            /// <summary>
            /// 設定連線的錯誤資訊.
            /// </summary>
            /// <param name="connectFailure"></param>
            /// <param name="Message"></param>
            protected void SetConnectionState(bool connectFailure, string Message)
            {
                Template.SetConnectionState(connectFailure, Message);
            }

            /// <summary>
            /// 開啟連線, 每一個資料庫實際連線的時機會由 DACTemplate TransManager and  TransScope 決定.
            /// 當需要連線會通知 ConnectionDataPack.ConnectionOpen(). 因此 ConnectionOpen 必須在被呼叫時
            /// 建立實際的連線.
            /// </summary>
            public abstract void ConnectionOpen(string connectionString);

            /// <summary>
            /// 檢查連線是否開啟
            /// </summary>
            /// <returns>開啟則回 True</returns>
            public abstract bool IsOpen();

            /// <summary>
            /// 通知 Connection Dispose(); 當 DACTemplate 的 InculdeTransaction 為 False 時且 在 TransScope
            /// 範圍結束前 CommitTrans(由 TransScope.Complate() 引發) 內未能執行. 則需要在  close() 時執行實際 Rollback
            /// </summary>
            public abstract void Dispose();

            /// <summary>
            /// 當 DACTemplate 的 InculdeTransaction 為 False 時 , TransManager 會呼叫 BeginTrans 和 CommitTrans 作
            /// 為管理 Transaction 的開始和結束. 如果需要 Rollback 時機則在 Close() 會被呼叫時.
            /// </summary>
            public abstract void BeginTrans();

            /// <summary>
            /// 當 DACTemplate 的 InculdeTransaction 為 False 時 , TransManager 會呼叫 BeginTrans 和 CommitTrans 作
            /// 為管理 Transaction 的開始和結束. 如果需要 Rollback 時機則在 Close() 會被呼叫時.
            /// </summary>
            public abstract void Commit();

            /// <summary>
            ///
            /// </summary>
            public abstract void Rollback();

            /// <summary>
            /// 取得 Connection 實體物件.
            /// </summary>
            /// <returns></returns>
            public abstract TConnection GetCurrentConnection();

            /// <summary>
            /// 取得 Transaction 實體物件.
            /// </summary>
            /// <returns></returns>
            public abstract TTransaction GetCurrentTransaction();

            /// <summary>
            /// 建立一個 TCommand 實體物件.
            /// </summary>
            /// <returns></returns>
            public abstract TCommand CreateCommand();

            public StringBuilder Message { get; } = new StringBuilder();

            public void ClearMessage()
            {
                if (Message.Length > 0)
                    Message.Remove(0, Message.Length);
            }

            #region IConnectionManagement Members

            /// <summary>
            /// 是否啟用一個資料庫交易
            /// </summary>
            public TransactionMode TransactionMode { get; private set; }

            #endregion IConnectionManagement Members
        }

        /// <summary>
        /// 資料庫存取物件名稱. 對每一個存取物件. 此名稱會作為取得相關參數之依據.
        /// </summary>
        public readonly string Name;

        /// <summary>
        /// 經由 App.Configure 取得的 ConnectionStringSettings
        /// </summary>
        protected readonly string ConnectionString;

        /// <summary>
        /// 表示該連線的維一識別碼.
        /// </summary>
        public readonly string TemplateId = $"TemplateId_{System.Threading.Interlocked.Increment(ref TemplateMaxId):X}";
        private static int TemplateMaxId = 0;

        /// <summary>
        /// 取得下一個執行的序號
        /// </summary>
        /// <returns></returns>
        public int GetExecuteNextOrder()
        {
            int? order = this.ConnectionObject(allowNull: true)?.GetExecuteNextOrder();
            return (order == null ? 0 : (int)order);
        }

        /// <summary>
        /// 記錄目前的連線資訊..
        /// </summary>
        public struct ConnectState
        {
            private bool _connectFailure;
            private string _failureMessage;

            /// <summary>
            /// 傳回最後一次的錯誤訊息
            /// </summary>
            public string Message => _failureMessage;

            /// <summary>
            /// 傳回目前是否正在連線中..
            /// </summary>
            public bool IsConnect => !_connectFailure;

            /// <summary>
            /// 設定連線的錯誤資訊.
            /// </summary>
            /// <param name="connectFailure"></param>
            /// <param name="Message"></param>
            internal void SetConnectionState(bool connectFailure, string Message)
            {
                this._connectFailure = connectFailure;
                this._failureMessage = Message;
            }
        }

        /// <summary>
        /// 連線狀態
        /// </summary>
        public ConnectState State = new ConnectState();

        /// <summary>
        /// 設定連線的錯誤資訊.
        /// </summary>
        /// <param name="connectFailure"></param>
        /// <param name="Message"></param>
        protected void SetConnectionState(bool connectFailure, string Message)
        {
            State.SetConnectionState(connectFailure, Message);
        }

        /// <summary>
        /// DACTemplate 建構子 , DACTemplate 會依物件名稱向 Configure File 索取相關資訊.
        /// </summary>
        /// <param name="name">連線名稱</param>
        /// <param name="transactionMode">指示 DACTemplate 是否參與 Transaction </param>
        protected DacTemplate(string name, TransactionMode transactionMode) : this(name, transactionMode, null, null) { }

        /// <summary>
        /// DACTemplate 建構子 , DACTemplate 會依物件名稱向 Configure File 索取相關資訊.
        /// </summary>
        /// <param name="name"></param>
        protected DacTemplate(string name) : this(name, TransactionMode.Transaction, null, null) { }

        /// <summary>
        /// DACTemplate 建構子 , 直接指定 ConnectionString
        /// </summary>
        /// <param name="name">連線名稱</param>
        /// <param name="transactionMode">指示 DACTemplate 是否參與 Transaction </param>
        /// <param name="connectionString">資料庫連線字串</param>
        protected DacTemplate(string name, TransactionMode transactionMode, string connectionString) : this(name, TransactionMode.Transaction, connectionString, null) { }

        /// <summary>
        /// DACTemplate 建構子 , 直接指定 ConnectionString
        /// </summary>
        /// <param name="name">連線名稱</param>
        /// <param name="transactionMode">指示 DACTemplate 是否參與 Transaction </param>
        /// <param name="connectionString">資料庫連線字串</param>
        /// <param name="lexicalDictionary">LexicalDictionary 定義檔</param>
        protected DacTemplate(string name, TransactionMode transactionMode, string connectionString, string lexicalDictionary)
        {
            this.Name = name;
            if (Name == null)
            {
                // 未指定名稱時, 需要給一個唯一的名稱.
                Name = TemplateId.ToString();
            }
            this.transactionMode = transactionMode;

            if (connectionString != null)
            {
                this.ConnectionString = connectionString;
            }
            else
            {
                if (!string.IsNullOrEmpty(Name))
                {
                    CurrentConfiguration = ApplicationConfiguration.Query(Name);
                    if (CurrentConfiguration?.Name != null)
                    {
                        this.CommandTimeout = CurrentConfiguration.CommandTimeout;
                        this.ConnectionString = CurrentConfiguration.ConnectionString;
                    }
                    else
                        SetConnectionState(true, $"在設定檔中找不到資料連線名稱 ({name}).");
                }
                else
                    SetConnectionState(true, "指定資料連線名稱.");
            }
            //Debug.Print($"\r\n#Check Template.New {this.TemplateId}\r\n");
            Initiailize();
        }

        /// <summary>
        /// 傳回關連的連線物件類別.
        /// </summary>
        /// <returns>連線物件類別</returns>
        protected internal Type GetConnectionObjectType()
        {
            ConnectionObjectTypeAttribute connectionObject = CommonExtensions.GetCustomAttributeEx<ConnectionObjectTypeAttribute>(GetType(), true);
            if (connectionObject != null)
                return connectionObject.ConnectionObjectType;
            throw new CommonException("DACTemplate : 未指定 ConnectionObjectType.");
        }

        /// <summary>
        /// 初始化
        /// </summary>
        protected virtual void Initiailize() { }

        /// <summary>
        /// 取得資料庫系統的相關資訊 , 未支援時會回傳 null
        /// </summary>
        protected virtual IDbInformation GetDBInformation() { return null; }

        private TransactionMode transactionMode = TransactionMode.Transaction;    /* 預設支援 */

        /// <summary>
        /// 取得目前己存在 Connection(Transaction) 物件, ConnectionManagement 是以 TransactionManager/GetPackObject 來管理其生命週期.
        /// 並依 TransactionMode 來決定如何取得目前所在類別的 Connection(Transaction) 物件.
        /// </summary>
        /// <param name="allowNull">指示允許回傳 NULL, 當無連線物件時, 不強迫建立一個的連線 </param>
        /// <returns></returns>
        protected ConnectionManagement ConnectionObject(bool allowNull = true, bool Initiailize = false)
        {
            lock (this)
            {
                var connectionObject = (ConnectionManagement)TransactionManager.Current.GetPackObject(this, allowNull, Initiailize);
                if (connectionObject != null)
                {
                    // 開啟連線時, 同時決定 Transaction 和 TransactionScope 如何關連.
                    if (!connectionObject.IsOpen())
                    {
                        if (TransactionMode == TransactionMode.DTCTransaction)
                        {
                            // TODO : 支援 TransactionScope
                            connectionObject.ConnectionOpen(this.ConnectionString);
                        }
                        else
                        {
                            // TODO : 支援獨立 TransactionScope
                            //using (System.Transactions.TransactionScope SuppressScope =
                            //    new System.Transactions.TransactionScope(TransactionScopeOption.Suppress))
                            //{
                            connectionObject.ConnectionOpen(this.ConnectionString);
                            //}
                        }
                    }
                }
                return connectionObject;
            }
        }

        /// <summary>
        /// 指示 DACTemplate 是否參與 Transaction
        /// </summary>
        /// <remarks>
        /// </remarks>
        public TransactionMode TransactionMode => transactionMode;

        private void ConnectCheck()
        {
            if (!State.IsConnect)
                throw new DatabaseConnectFailureException(State.Message, null);
            _returnValue = null;

            ConnectionObject(allowNull: true)?.ClearMessage();
        }

        /// <summary>
        /// 取得 IManualTransaction 介面. 進行手動控管 Transaction  由 IManualTransaction 自行管理 Begin/Commit/Rollback Transaction 使用.
        /// ManualTransaction 只有在 TransactionMode is Manual 時 才有作用
        /// </summary>
        /// <returns></returns>
        public abstract IManualTransaction Transaction { get; }

        private object _returnValue = null;

        /// <summary>
        /// 取得 ExecuteNonQuery 資料庫回傳的內容值.
        /// </summary>
        public object ReturnValue => _returnValue;

        public int SPID
        {
            get { return ConnectionObject(allowNull: true)?.SPID ?? -1; }
        }

        /// <summary>
        /// 執行無回傳值之資料庫存取動作.
        /// </summary>
        /// <param name="Command">SQL Command 格式化字串. <see cref="String.Format(string, object[])"/> </param>
        /// <returns>傳回資料庫受資料庫存取動作影響之筆數.(詳見各資料庫所提供之 ADO.NET 說明</returns>
        public int ExecuteNonQuery(string Command, object parameters = null)
        {
            ConnectCheck();
            return CommandExecuteNonQuery(new StringCommandBuilder(CommandType.Text, Command), parameters, ref _returnValue);
        }

        /// <summary>
        /// 執行無回傳值之資料庫存取動作.
        /// </summary>
        /// <param name="Command">SQL Command 格式化字串. <see cref="String.Format(string, object[])"/> </param>
        /// <returns>傳回資料庫受資料庫存取動作影響之筆數.(詳見各資料庫所提供之 ADO.NET 說明</returns>
        public async Task<int> ExecuteNonQueryAsync(string Command, object parameters = null)
        {
            ConnectCheck();
            var (rowsAffected, result) = await CommandExecuteNonQueryAsync(new StringCommandBuilder(CommandType.Text, Command), parameters);
            return rowsAffected;
        }

        /// <summary>
        /// 執行無回傳值之資料庫存取動作.
        /// </summary>
        /// <param name="type">指定資料庫執行命的型態</param>
        /// <param name="command">SQL Command 格式化字串. 格式化字串 方法採用 <c>String.Format</c> 詳細請查閱 <see cref="String.Format(string, object[])"/> 文件. </param>
        /// <returns>傳回資料庫受資料庫存取動作影響之筆數.(詳見各資料庫所提供之 ADO.NET 說明</returns>
        public int ExecuteNonQuery(CommandType type, string command, object parameters = null)
        {
            ConnectCheck();
            return CommandExecuteNonQuery(new StringCommandBuilder(type, command), parameters, ref _returnValue);
        }

        /// <summary>
        /// 執行無回傳值之資料庫存取動作.
        /// </summary>
        /// <param name="type">指定資料庫執行命的型態</param>
        /// <param name="command">SQL Command 格式化字串. 格式化字串 方法採用 <c>String.Format</c> 詳細請查閱 <see cref="String.Format(string, object[])"/> 文件. </param>
        /// <returns>傳回資料庫受資料庫存取動作影響之筆數.(詳見各資料庫所提供之 ADO.NET 說明</returns>
        public async Task<int> ExecuteNonQueryAsync(CommandType type, string Command, object parameters = null)
        {
            ConnectCheck();
            var (rowsAffected, result) = await CommandExecuteNonQueryAsync(new StringCommandBuilder(type, Command), parameters);
            return rowsAffected;
        }

        /// <summary>
        /// 執行無回傳值之資料庫存取動作.
        /// </summary>
        /// <param name="builder">傳入一個可建立 SQL Command 字串的物件</param>
        /// <returns>傳回資料庫受資料庫存取動作影響之筆數.(詳見各資料庫所提供之 ADO.NET 說明</returns>
        public int ExecuteNonQuery(ICommandBuilder builder, object parameters = null)
        {
            ConnectCheck();
            return CommandExecuteNonQuery(builder, parameters, ref _returnValue);
        }
        /// <summary>
        /// 執行無回傳值之資料庫存取動作.
        /// </summary>
        /// <param name="builder">傳入一個可建立 SQL Command 字串的物件</param>
        /// <returns>傳回資料庫受資料庫存取動作影響之筆數.(詳見各資料庫所提供之 ADO.NET 說明</returns>
        public async Task<int> ExecuteNonQueryAsync(ICommandBuilder builder, object parameters = null)
        {
            ConnectCheck();
            var (rowsAffected, result) = await CommandExecuteNonQueryAsync(builder, parameters);
            return rowsAffected;
        }

        /// <summary>
        /// 執行回傳值單一值之資料庫存取動作.
        /// </summary>
        /// <typeparam name="T">回傳值型別.</typeparam>
        /// <param name="command">SQL Command 格式化字串. 格式化字串 方法採用 <c>String.Format</c> 詳細請查閱 <see cref="String.Format(string, object[])"/> 文件. </param>
        /// <returns>回傳值</returns>
        /// <remarks>
        /// ExecuteScale 會依回傳值試著轉換至回傳值型別. 如果失敗會丟出例外.
        /// </remarks>
        public T ExecuteScalar<T>(string command, T defaultValue = default(T))
        {
            ConnectCheck();
            return CommandExecuteScalar<T>(new StringCommandBuilder(CommandType.Text, command), null, defaultValue);
        }

        public T ExecuteScalar<T>(string command, object parameters, T defaultValue = default(T))
        {
            ConnectCheck();
            return CommandExecuteScalar<T>(new StringCommandBuilder(CommandType.Text, command), parameters, defaultValue);
        }

        public async Task<T> ExecuteScalarAsync<T>(string command, T defaultValue = default(T))
        {
            ConnectCheck();
            return await CommandExecuteScalarAsync<T>(new StringCommandBuilder(CommandType.Text, command), null, defaultValue);
        }
        public async Task<T> ExecuteScalarAsync<T>(string command, object parameters, T defaultValue = default(T))
        {
            ConnectCheck();
            return await CommandExecuteScalarAsync<T>(new StringCommandBuilder(CommandType.Text, command), parameters, defaultValue);
        }


        /// <summary>
        /// 執行回傳值單一值之資料庫存取動作.
        /// </summary>
        /// <typeparam name="T">回傳值型別</typeparam>
        /// <param name="type">指定資料庫執行命的型態</param>
        /// <param name="command">SQL Command 格式化字串. 格式化字串 方法採用 <c>String.Format</c> 詳細請查閱 <see cref="String.Format(string, object[])"/> 文件. </param>
        /// <returns>資料庫的回傳值.</returns>
        public T ExecuteScalar<T>(CommandType type, string command, T defaultValue = default(T))
        {
            ConnectCheck();
            return CommandExecuteScalar<T>(new StringCommandBuilder(type, command), null, defaultValue);
        }

        /// <summary>
        /// 執行回傳值單一值之資料庫存取動作.
        /// </summary>
        /// <typeparam name="T">回傳值型別</typeparam>
        /// <param name="type">指定資料庫執行命的型態</param>
        /// <param name="command">SQL Command 格式化字串. 格式化字串 方法採用 <c>String.Format</c> 詳細請查閱 <see cref="String.Format(string, object[])"/> 文件. </param>
        /// <returns>資料庫的回傳值.</returns>
        public async Task<T> ExecuteScalarAsync<T>(CommandType type, string command, T defaultValue = default(T))
        {
            ConnectCheck();
            return await CommandExecuteScalarAsync<T>(new StringCommandBuilder(type, command), null, defaultValue);
        }

        /// <summary>
        /// 執行回傳值單一值之資料庫存取動作.
        /// </summary>
        /// <typeparam name="T">回傳值型別</typeparam>
        /// <param name="builder">傳入一個可建立 SQL Command 字串的物件</param>
        /// <returns>資料庫的回傳值</returns>
        public T ExecuteScalar<T>(ICommandBuilder builder, T defaultValue = default(T))
        {
            ConnectCheck();
            return CommandExecuteScalar<T>(builder, null, defaultValue);
        }

        public T ExecuteScalar<T>(ICommandBuilder builder, object parameters, T defaultValue = default(T))
        {
            ConnectCheck();
            return CommandExecuteScalar<T>(builder, parameters, defaultValue);
        }


        public async Task<T> ExecuteScalarAsync<T>(ICommandBuilder builder, T defaultValue = default(T))
        {
            ConnectCheck();
            return await CommandExecuteScalarAsync<T>(builder, null, defaultValue);
        }

        public async Task<T> ExecuteScalarAsync<T>(ICommandBuilder builder, object parameters, T defaultValue = default(T))
        {
            ConnectCheck();
            return await CommandExecuteScalarAsync<T>(builder, parameters, defaultValue);
        }

        /// <summary>
        /// 執行回傳值 DataTable 之資料庫存取動作.
        /// </summary>
        /// <param name="builder">傳入一個可建立 SQL Command 字串的物件</param>
        /// <param name="BuildOptions"></param>
        /// <returns>資料庫的回傳值</returns>
        public DataTable OpenDataTable(ICommandBuilder builder, CommandBuildOptions BuildOptions = CommandBuildOptions.None)
        {
            ConnectCheck();
            return CommandOpenDataTable(builder, BuildOptions);
        }

        /// <summary>
        /// 執行回傳值 DataTable 之資料庫存取動作.
        /// </summary>
        /// <param name="command">SQL Command 格式化字串. 格式化字串 方法採用 <c>String.Format</c> 詳細請查閱 <see cref="String.Format(string, object[])"/> 文件. </param>
        /// <returns>資料庫的回傳值</returns>
        public DataTable OpenDataTable(string command)
        {
            ConnectCheck();
            return CommandOpenDataTable(new StringCommandBuilder(CommandType.Text, command), CommandBuildOptions.None);
        }

        /// <summary>
        /// 執行回傳值 DataTable 之資料庫存取動作.
        /// </summary>
        /// <param name="type">指定資料庫執行命的型態</param>
        /// <param name="command">SQL Command 格式化字串. 格式化字串 方法採用 <c>String.Format</c> 詳細請查閱 <see cref="String.Format(string, object[])"/> 文件. </param>
        /// <returns>資料庫的回傳值</returns>
        public DataTable OpenDataTable(CommandType type, string command)
        {
            ConnectCheck();
            return CommandOpenDataTable(new StringCommandBuilder(type, command), CommandBuildOptions.None);
        }

        /// <summary>
        /// 執行回傳值 DataSet 之資料庫存取動作.
        /// </summary>
        /// <param name="builder">傳入一個可建立 SQL Command 字串的物件</param>
        /// <param name="buildOptions"></param>
        /// <returns>資料庫的回傳值</returns>
        public DataSet OpenDataSet(ICommandBuilder builder, CommandBuildOptions buildOptions = CommandBuildOptions.None)
        {
            ConnectCheck();
            var ds = new DataSet();
            CommandFillDataSet(ref ds, builder, buildOptions);
            return ds;
        }

        /// <summary>
        /// 執行回傳值 DataSet 之資料庫存取動作.
        /// </summary>
        /// <param name="ds">資料庫的回傳值</param>
        /// <param name="builder">傳入一個可建立 SQL Command 字串的物件</param>
        /// <param name="BuildOptions"></param>
        public void OpenDataSet(DataSet ds, ICommandBuilder builder, CommandBuildOptions BuildOptions = CommandBuildOptions.None)
        {
            ConnectCheck();
            CommandFillDataSet(ref ds, builder, BuildOptions);
        }

        /// <summary>
        ///  執行回傳值 DataSet 之資料庫存取動作.
        /// </summary>
        /// <param name="command">SQL Command 格式化字串. 格式化字串 方法採用 <c>String.Format</c> 詳細請查閱 <see cref="String.Format(string, object[])"/> 文件. </param>
        /// <returns>資料庫的回傳值</returns>
        public DataSet OpenDataSet(string command)
        {
            var ds = new DataSet();
            ConnectCheck();
            CommandFillDataSet(ref ds, new StringCommandBuilder(CommandType.Text, command), CommandBuildOptions.None);
            return ds;
        }

        /// <summary>
        ///  執行回傳值 DataSet 之資料庫存取動作.
        /// </summary>
        /// <param name="type">指定資料庫執行命的型態</param>
        /// <param name="command">SQL Command 格式化字串. 格式化字串 方法採用 <c>String.Format</c> 詳細請查閱 <see cref="String.Format(string, object[])"/> 文件. </param>
        /// <returns>資料庫的回傳值</returns>
        public DataSet OpenDataSet(CommandType type, string command)
        {
            var ds = new DataSet();
            ConnectCheck();
            CommandFillDataSet(ref ds, new StringCommandBuilder(type, command), CommandBuildOptions.None);
            return ds;
        }
        /// <summary>
        ///  執行回傳值 DataSet 之資料庫存取動作.
        /// </summary>
        /// <param name="ds">資料庫的回傳值</param>
        /// <param name="type">指定資料庫執行命的型態</param>
        /// <param name="command">SQL Command 格式化字串. 格式化字串 方法採用 <c>String.Format</c> 詳細請查閱 <see cref="String.Format(string, object[])"/> 文件. </param>
        public void OpenDataSet(DataSet ds, CommandType type, string command)
        {
            ConnectCheck();
            CommandFillDataSet(ref ds, new StringCommandBuilder(type, command), CommandBuildOptions.None);
        }

        /// <summary>
        ///  執行回傳值 DataSet 之資料庫存取動作.
        /// </summary>
        /// <param name="ds">資料庫的回傳值</param>
        /// <param name="command">SQL Command 格式化字串. 格式化字串 方法採用 <c>String.Format</c> 詳細請查閱 <see cref="String.Format(string, object[])"/> 文件. </param>
        public void OpenDataSet(DataSet ds, string command)
        {
            ConnectCheck();
            CommandFillDataSet(ref ds, new StringCommandBuilder(CommandType.Text, command), CommandBuildOptions.None);
        }
        /// <summary>
        /// 以繼承必需實作 CommandExecuteNonQuery 作為 ExecuteNonQuery 執行無回傳值之資料庫存取動作
        /// </summary>
        /// <param name="builder">傳入一個可建立 SQL Command 字串的物件</param>
        /// <param name="result">取得  ExecuteNonQuery 資料庫回傳的內容值.</param>
        /// <returns>傳回資料庫受資料庫存取動作影響之筆數.(詳見各資料庫所提供之 ADO.NET 說明</returns>
        protected abstract int CommandExecuteNonQuery(ICommandBuilder builder, object parameters, ref object result);


        /// <summary>
        /// 以繼承必需實作 CommandExecuteNonQuery 作為 ExecuteNonQuery 執行無回傳值之資料庫存取動作
        /// </summary>
        /// <param name="builder">傳入一個可建立 SQL Command 字串的物件</param>
        /// <param name="result">取得  ExecuteNonQuery 資料庫回傳的內容值.</param>
        /// <returns>傳回資料庫受資料庫存取動作影響之筆數.(詳見各資料庫所提供之 ADO.NET 說明</returns>
        protected abstract Task<(int, object)> CommandExecuteNonQueryAsync(ICommandBuilder builder, object parameters);

        /// <summary>
        /// 以繼承必需實作 CommandExecuteScale 作為 ExecuteScale 執行回傳值單一值之資料庫存取動作
        /// </summary>
        /// <typeparam name="T">回傳值型態</typeparam>
        /// <param name="builder">傳入一個可建立 SQL Command 字串的物件</param>
        /// <param name="defaultValue">defValue</param>
        /// <returns>回傳值</returns>

        protected abstract T CommandExecuteScalar<T>(ICommandBuilder builder, object parameters, T defaultValue);
        protected abstract Task<T> CommandExecuteScalarAsync<T>(ICommandBuilder builder, object parameters, T defaultValue);

        /// <summary>
        /// 以繼承必需實作 CommandExecuteDataTable 作為 OpenDataTable 執行回傳值 DataTable 之資料庫存取動作
        /// </summary>
        /// <param name="builder">傳入一個可建立 SQL Command 字串的物件</param>
        /// <param name="buildFlags"></param>
        /// <returns>回傳值</returns>
        protected abstract DataTable CommandOpenDataTable(ICommandBuilder builder, CommandBuildOptions buildFlags);

        /// <summary>
        /// 以繼承必需實作 CommandExecuteDataSet 作為 OpenDataSet 執行回傳值 DataSet 之資料庫存取動作
        /// </summary>
        /// <param name="ds"></param>
        /// <param name="builder"></param>
        /// <param name="buildFlags"></param>
        protected abstract void CommandFillDataSet(ref DataSet ds, ICommandBuilder builder, CommandBuildOptions buildFlags);


    }
}