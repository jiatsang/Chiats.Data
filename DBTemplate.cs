// ------------------------------------------------------------------------
// Chiats Common&Data Library V4.1.21 (2021/08)
// Chiats@Studio(http://www.chiats.com/Common)
// Design&Coding By Chia Tsang Tsai
//Copyright(C) 2005-2022 Chiats@Studio All 
// ------------------------------------------------------------------------
using Chiats.SQL;
using System;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Chiats.Data
{
    /// <summary>
    /// 支援 ADO.NET 的資料存取物件.
    /// </summary>
    /// <remarks>
    /// DACTemplate 提供資料存取的基礎介面, 如需要 ADO/RDS/ODBC 或其他廠商所提供的存取介面, 你需要實作 DACTemplate ,
    /// 廠商所提供的存取介面如果符合 ADO.NET 的資料存取規格, 則可以由 DBTemplate 實作. <br/>
    /// DBTemplate 支援 SQLLOG  詳細請查閱 SqlLog 物件
    /// </remarks>
    public abstract class DbTemplate<TConnection, TTransaction, TCommand> : DacTemplate<TConnection, TTransaction, TCommand>, IDbTemplate
            where TConnection : DbConnection
            where TTransaction : DbTransaction
            where TCommand : DbCommand
    {
        /// <summary>
        /// 存放 ADO.NET 實體的 Connection(SqlConnection) 物件.
        /// </summary>
        public abstract class ConnectionClass : ConnectionManagement
        {
            
            /// <summary>
            /// ConnectionObject 建構子
            /// </summary>
            /// <param name="template">傳入建構此物件之 DACTemplate 物件</param>
            public ConnectionClass(DbTemplate<TConnection, TTransaction, TCommand> template) : base(template) { }

            /// <summary>
            ///  存放實體的 Connection(SqlConnection) 物件
            /// </summary>
            protected TConnection connection = null;

            protected DateTime startTime;

            protected object forlock = new object();

            /// <summary>
            /// 存放實體的 Transaction(SqlTransaction) 物件
            /// </summary>
            protected internal TTransaction transaction = null;

            protected internal Guid ClientConnectionId = Guid.NewGuid();

            /// <summary>
            /// 通知 Connection Close(); 當 DACTemplate 的 InculdeTransaction 為 False 時且 在 TransScope
            /// 範圍結束前 CommitTrans(由 TransScope.Complate() 引發) 內未能執行. 則需要在  close() 時執行實際 Rollback
            /// </summary>
            public override void Dispose()
            {
                lock (forlock)
                {
                    if (connection != null)
                    {

//#if NETCOREAPP2_2  || NETSTANDARD2_0 || NET5_0_OR_GREATER

//                            // Marshal.GetExceptionPointers() != IntPtr.Zero
//                            // GetExceptionPointers 只有在 .Net framework 4 and .Net Core 3.0 才有支援
//                            int ExceptionOccurred = Marshal.GetExceptionCode();
//                            if (ExceptionOccurred != 0)
//                            {
//                                // 執行期間有發生 Exception 強制 Abort                           
//                                AbortCount++;
//                                Template.RaiseCommandTransaction(Template, new CommandTransactionEventArgs
//                                {
//                                    Name = Template.Name,
//                                    TemplateId = Template.TemplateId,
//                                    SPID = Template.SPID,
//                                    Transaction = $"SetAbort AbortReson: 0x{ExceptionOccurred:X} 執行期間有發生 Exception 強制 Abort"
//                                });
//                                //Debug.Print($"\r\n#Check SetAbort [{Template.TemplateId}] 執行期間有發生 Exception 強制 Abort\r\n");
//                            }
//#else
                        // GetExceptionPointers 只有在 .Net framework 4 and .Net Core 3.0 才有支援
                        var ExceptionPointers = Marshal.GetExceptionPointers();
                        if (ExceptionPointers != IntPtr.Zero)
                        {
                            // 執行期間有發生 Exception 強制 Abort                           
                            AbortCount++;
                            Template.RaiseCommandTransaction(Template, new CommandTransactionEventArgs
                            {
                                Name = Template.Name,
                                TemplateId = Template.TemplateId,
                                SPID = Template.SPID,
                                Transaction = $"SetAbort AbortReson: 執行期間有發生 Exception 強制 Abort"
                            });
                            //Debug.Print($"\r\n#Check SetAbort [{Template.TemplateId}] 執行期間有發生 Exception 強制 Abort\r\n");
                        }
//#endif

                        var LastTemplate = Template;
                        if (ScopeRemove())
                        {
                            string action = null;
                            if (transaction != null && LastTemplate?.TransactionMode == TransactionMode.Transaction)
                            {
                                if (AbortCount == 0)
                                {
                                    action = ".Commit()";
                                    // Debug.Print($"\r\n#Check Transaction.Commit(auto) SPID:{LastTemplate.SPID}  ConnectionManagerID:{ConnectionIndex} [{LastTemplate.TemplateId}] {ClientConnectionId}  \r\n");
                                    transaction.Commit();
                                }
                                else
                                {
                                    action = ".Rollback()";
                                    // Debug.Print($"\r\n#Check Transaction.Rollback(auto) SPID:{LastTemplate.SPID}  ConnectionManagerID:{ConnectionIndex} [{LastTemplate.TemplateId}] {ClientConnectionId}  \r\n");
                                    transaction.Rollback();
                                }
                                //if (TransactionManager.HasTransaction && TransactionManager.Current.IsComplate)
                                //{
                                //    Debug.Print($"\r\n#Check Transaction.Commit(auto) SPID:{Template.SPID}  ConnectionManagerID :{connectionIndex} [{LastTemplate.TemplateId}] ClientConnectionID:{ClientConnectionId}  \r\n");
                                //    transaction.Commit();
                                //}
                                //else
                                //{
                                //    Debug.Print($"\r\n#Check Transaction.Rollback(auto) SPID:{Template.SPID}  ConnectionManagerID :{connectionIndex} [{LastTemplate.TemplateId}] ClientConnectionID:{ClientConnectionId}  \r\n");
                                //    transaction.Rollback();
                                //}
                                transaction = null;
                            }
                            connection.Close(); 
                            connection.Dispose();
                            connection = null;
                            TransactionManager.Current.CloseConnection(LastTemplate);    
                            if (LastTemplate != null)
                            {
                                LastTemplate.RaiseCommandTransaction(LastTemplate, new CommandTransactionEventArgs
                                {
                                    Name = LastTemplate.Name,
                                    SPID = SPID,
                                    ThreadId = System.Threading.Thread.CurrentThread.ManagedThreadId,
                                    TemplateId = LastTemplate.TemplateId,
                                    Transaction = $"Connection.Close(Outside:{TransactionManager.Current.ScopeCount}){action} ConnectionManagerID_{ConnectionIndex}"
                                });
                                //Debug.Print($"\r\n#Check SqlConnection.Close(outside) SPID:{SPID} ConnectionManagerID:{ConnectionIndex} Time:{startTime:yyyy-MM-dd HH:mm:ss} - {ts.TotalSeconds:#,##0.###00s} [{LastTemplate.TemplateId}] {ClientConnectionId}  \r\n");
                            }
                        }
                        else
                        {
                            if (LastTemplate != null)
                            {
                                LastTemplate.RaiseCommandTransaction(LastTemplate, new CommandTransactionEventArgs
                                {
                                    Name = LastTemplate.Name,
                                    SPID = SPID,
                                    ThreadId = System.Threading.Thread.CurrentThread.ManagedThreadId,
                                    TemplateId = LastTemplate.TemplateId,
                                    Transaction = $"Connection.Close(Inside:{TransactionManager.Current.ScopeCount}) ConnectionManagerID_{ConnectionIndex}/{LastTemplate.TemplateId} to {Template.TemplateId}"
                                });
                                //Debug.Print($"\r\n#Check SqlConnection.Close(Inside) SPID:{LastTemplate.SPID}  ConnectionManagerID:{ConnectionIndex} Time:{startTime:yyyy-MM-dd HH:mm:ss} - {ts.TotalSeconds:#,##0.###00s} [{LastTemplate.TemplateId} to {Template?.TemplateId}] {ClientConnectionId}  \r\n");
                            }
                        }
                    }
                }
            }

            /// <summary>
            /// 當 DACTemplate 的 InculdeDTCTransaction 為 False 時 , TransManager 會呼叫 BeginTrans 和 CommitTrans 作
            /// 為管理 Transaction 的開始和結束. 如果需要 Rollback 時機則在 Close() 會被呼叫時.
            /// </summary>
            public override void BeginTrans()
            {
                try
                {
                    lock (forlock)
                    {
                        if (connection != null && transaction == null)
                        {
                            transaction = (TTransaction)connection.BeginTransaction(IsolationLevel.ReadUncommitted);
                            Template.RaiseCommandTransaction(Template, new CommandTransactionEventArgs
                            {
                                Name = Template.Name,
                                SPID = SPID,
                                ThreadId = System.Threading.Thread.CurrentThread.ManagedThreadId,
                                TemplateId = Template.TemplateId,
                                Transaction = $"Transaction.Begin ConnectionManagerID_{ConnectionIndex}"
                            });

                            //Debug.Print($"\r\n#Check Transaction.BeginTransaction SPID:{Template.SPID} ConnectionManagerID:{ConnectionIndex} {Template.TemplateId} {ClientConnectionId}  \r\n");
                            // Chaos            無法覆寫較高層級隔離交易暫止的變更。
                            // ReadCommitted     在交易期間可以讀取 Volatile 資料，但不能修改該資料。
                            // ReadUncommitted   可以讀取和修改在交易期間變動性資料。
                            // RepeatableRead    可以讀取但不是會修改在交易期間變動性資料。 在交易期間，可以加入新資料。
                            // Serializable      變動性資料可以讀取但不是會修改，並在交易期間，可以加入任何新資料。
                            // Snapshot          可以讀取變動性資料。 交易修改資料之前，先驗證最初讀取後，其他交易是否已變更的資料。 如果資料已更新，會引發錯誤。 這可讓交易取得資料的先前認可的值。
                            // Unspecified       使用與指定不同的隔離等級時，但無法決定層級。 如果此值設定，則會擲回例外狀況。
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw new DatabaseConnectFailureException("SQL 連結失敗, 可能是網路異常或資料庫不存在.", ex);
                }
            }

            /// <summary>
            /// 當 DACTemplate 的 InculdeTransaction 為 False 時 , TransManager 會呼叫 BeginTrans 和 CommitTrans 作
            /// 為管理 Transaction 的開始和結束. 如果需要 Rollback 時機則在 Close() 會被呼叫時.
            /// </summary>
            public override void Commit()
            {
                try
                {
                    lock (forlock)
                    {
                        if (connection != null && transaction != null)
                        {
                            transaction.Commit();
                            Template.RaiseCommandTransaction(Template, new CommandTransactionEventArgs
                            {
                                Name = Template.Name,
                                SPID = SPID,
                                ThreadId = System.Threading.Thread.CurrentThread.ManagedThreadId,
                                TemplateId = Template.TemplateId,
                                Transaction = $"Transaction.Commit ConnectionManagerID_{ConnectionIndex}"
                            });
                            //Debug.Print($"\r\n#Check Transaction.Commit SPID:{SPID} ConnectionManagerID:{ConnectionIndex} {Template.TemplateId} {ClientConnectionId}  \r\n");
                            transaction = null;
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw new DatabaseConnectFailureException("SQL 連結失敗, 可能是網路異常或資料庫不存在.", ex);
                }
            }

            /// <summary>
            ///
            /// </summary>
            public override void Rollback()
            {
                try
                {
                    lock (forlock)
                    {
                        if (connection != null && transaction != null)
                        {
                            transaction.Rollback();
                            Template.RaiseCommandTransaction(Template, new CommandTransactionEventArgs
                            {
                                Name = Template.Name,
                                SPID = SPID,
                                ThreadId = System.Threading.Thread.CurrentThread.ManagedThreadId,
                                TemplateId = Template.TemplateId,
                                Transaction = $"Transaction.Rollback ConnectionManagerID_{ConnectionIndex}"
                            });
                            //Debug.Print($"\r\n#Check Transaction.Rollback SPID:{SPID} ConnectionManagerID:{ConnectionIndex} [{Template.TemplateId}] {ClientConnectionId}  \r\n");
                            transaction = null;
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw new DatabaseConnectFailureException("SQL 連結失敗, 可能是網路異常或資料庫不存在.", ex);
                }
            }

            /// <summary>
            /// 取得 Connection 實體物件.
            /// </summary>
            /// <returns> Connection 實體物件</returns>
            public override TConnection GetCurrentConnection()
            {
                lock (forlock)
                {
                    try
                    {
                        return connection;
                    }
                    catch (Exception ex)
                    {
                        throw new DatabaseConnectFailureException("SQL 連結失敗, 可能是網路異常或資料庫不存在.", ex);
                    }
                }
            }

            /// <summary>
            /// 取得 Transaction 實體物件. 如果不是使用 Transaction 則會回傳 null
            /// </summary>
            /// <returns> Connection 實體物件</returns>
            public override TTransaction GetCurrentTransaction()
            {
                return transaction;
            }
        }


        public static event EventHandler<CommandBuilderEventArgs> CreateCommandBuilder;

        public SqlOptions Options { get; set; }

        /// <summary>
        ///  取得 IManualTransaction 介面. 進行手動控管 Transaction  由 IManualTransaction 自行管理 Begin/Commit/Rollback Transaction 使用.
        ///  ManualTransaction 只有在 TransactionMode is Manual 時 才有作用
        /// </summary>
        /// <returns></returns>
        public override IManualTransaction Transaction
        {
            get
            {
                if (this.TransactionMode == Data.TransactionMode.Manual)
                {
                    return ConnectionObject(allowNull: false);
                }
                throw new NotSupportedException("ManualTransaction 只有在 TransactionMode is Manual 時才有作用.");
            }
        }

        public void Abort(string AbortReson = null,
            [System.Runtime.CompilerServices.CallerLineNumber] int LineNumber = 0,
            [System.Runtime.CompilerServices.CallerFilePath] string SourceFilePath = "",
            [System.Runtime.CompilerServices.CallerMemberName] string MemberName = "")
        {
            ConnectionObject(allowNull: false).SetAbort(AbortReson, MemberName, SourceFilePath, LineNumber);
        }

        /// <summary>
        /// DBTemplate 建構子
        /// </summary>
        /// <param name="name">連線名稱</param>
        protected DbTemplate(string name) : base(name) { }

        /// <summary>
        /// DBTemplate 建構子
        /// </summary>
        /// <param name="name">連線名稱</param>
        /// <param name="transactionMode">指示 OleDbTemplate 是否參與 Transaction </param>
        protected DbTemplate(string name, TransactionMode transactionMode) : base(name, transactionMode) { }

        /// <summary>
        /// DBTemplate 建構子
        /// </summary>
        /// <param name="name">連線名稱</param>
        /// <param name="transactionMode">指示 OleDbTemplate 是否參與 Transaction </param>
        /// <param name="connectionString">資料庫連線字串</param>
        protected DbTemplate(string name, TransactionMode transactionMode, string connectionString) :
            base(name, transactionMode, connectionString, null)
        { }

        

        /// <summary>
        /// ConnectionManagement 初始化程序. 通常是用來建立資料庫的連線.
        /// </summary>
        protected override void Initiailize()
        {
            // 建立ConnectionManagement 物件並啟用連線 ,
            //  ConnectionManagement 物件必要要同時建立,  以維持 ConnectionManagement Stack<ITemplate> 完整性.
            var cm = ConnectionObject(allowNull: false, Initiailize: true);
            cm.InfoMessage += ConnectionManagement_InfoMessage;
            base.Initiailize();
        }

        private void ConnectionManagement_InfoMessage(object sender, InfoMessageEventArgs e)
        {
            this.Messages.Add( new DbMessage { Message = e.Message });
        }

        /// <summary>
        /// 建立所要執行前的 TCommand 物件.
        /// </summary>
        /// <param name="commandBuilder"></param>
        /// <param name="buildFlags"></param>
        /// <returns></returns>
        protected TCommand CreateCommand(ICommandBuilder commandBuilder, object parameters, CommandBuildOptions buildFlags)
        {

            CreateCommandBuilder?.Invoke(this,
                new CommandBuilderEventArgs
                {
                    CommandBuilder = commandBuilder,
                    Options = buildFlags
                });

            TCommand cmd = ConnectionObject(allowNull: false).CreateCommand();
            IParameterCommandBuilder parameterBuilder = commandBuilder as IParameterCommandBuilder;
            IBuildExportSupport buildExportSupport = commandBuilder as IBuildExportSupport;

            if (commandBuilder is SqlModel sqlModel)
            {
                sqlModel.Options = Options;
            }

            if (buildExportSupport != null)
            {
                TemplateCompatibleAttribute templateCompatible =
                    CommonExtensions.GetCustomAttributeEx<TemplateCompatibleAttribute>(this.GetType(), true);

                ISqlBuildExport buildExport =
                    (templateCompatible != null) ? templateCompatible.BuildExport : DefaultBuildExport.SQLBuildExport; ;
                buildExportSupport.BeginBuild(buildExport, new BuildExportSupportEventArgs(GetDBInformation(), buildFlags));
            }

            cmd.Connection = ConnectionObject(allowNull: false).GetCurrentConnection();
            if (CommandTimeout != -1)
            {
                cmd.CommandTimeout = CommandTimeout;
            }

            var commandBuildingEventArgs = new CommandBuildingEventArgs
            {
                Name = this.Name,
                CommandText = commandBuilder.CommandText,
                CommandType = commandBuilder.CommandType,
                ParameterBuilder = parameterBuilder
            };

            RaiseCommandBuilding(this, commandBuildingEventArgs);
            cmd.CommandText = commandBuildingEventArgs.CommandText;
            cmd.CommandType = commandBuildingEventArgs.CommandType;

            if (this.TransactionMode == TransactionMode.Transaction || this.TransactionMode == TransactionMode.Manual)
                cmd.Transaction = ConnectionObject(allowNull: false).GetCurrentTransaction();

            if (parameterBuilder != null)
            {
                if (parameterBuilder.ParameterEnabled)
                {
                    foreach (Parameter namedParameter in parameterBuilder.Parameters)
                        cmd.Parameters.Add(ParameterConvert(namedParameter.Name, namedParameter));
                }
            }
            else
            {
                if (commandBuilder is CommandTextModel commandTextModel)
                {
                    foreach (DbParameter param in commandTextModel.Parameters)
                        cmd.Parameters.Add(ParameterConvert(param));
                }
            }

            if (parameters != null)
            {
                foreach (var p in parameters.GetType().GetProperties())
                {
                    var p_name = $"@{p.Name}";
                    if (cmd.Parameters.Contains(p_name))
                        cmd.Parameters[p_name].Value = p.GetValue(parameters, null);
                    else
                    {
                        var dp = new CommandParameter(p_name, p.GetValue(parameters, null));
                        cmd.Parameters.Add(ParameterConvert(dp));
                    }
                }
            }

            if (buildExportSupport != null) buildExportSupport.EndBuild();
            return cmd;
        }

        /// <summary>
        /// 轉換 Parameter 物件, 由標準 Parameter 轉換成 ADO.NET 所需 IDataParameter 型態
        /// </summary>
        /// <param name="name">參數名稱</param>
        /// <param name="param">參數值內容</param>
        /// <returns></returns>
        protected abstract IDataParameter ParameterConvert(string name, Parameter param);

        protected abstract IDataParameter ParameterConvert(DbParameter param);

        /// <summary>
        /// 執行 SQL Command 並回傳 DataTable
        /// </summary>
        /// <param name="cmd">SQL Command</param>
        /// <param name="table">DataTable</param>
        protected abstract void Fill(TCommand cmd, DataTable table);

        /// <summary>
        /// 執行 SQL Command 並回傳 DataSet
        /// </summary>
        /// <param name="cmd">SQL Command</param>
        /// <param name="ds">DataSet</param>
        protected abstract void Fill(TCommand cmd, DataSet ds);

#if (!DEBUG)
       [DebuggerNonUserCode]
#endif

        /// <summary>
        /// 以繼承必需實作 ExecuteNonQuery 作為 ExecuteNonQuery 執行無回傳值之資料庫存取動作
        /// </summary>
        /// <param name="builder">SQL Command Builder</param>
        /// <param name="result"></param>
        /// <returns>傳回資料庫受資料庫存取動作影響之筆數.(詳見各資料庫所提供之 ADO.NET 說明</returns>
        protected override int CommandExecuteNonQuery(ICommandBuilder builder, object parameters, ref object result)
        {
            using (ThreadTimeMark threadTime = new ThreadTimeMark())// 計算 Execute 時間
            {
                using (TCommand cmd = CreateCommand(builder, parameters, CommandBuildOptions.None))
                {
                    int executeOrder = GetExecuteNextOrder();
                    try
                    {
                        DbParameter output = null;
                        foreach (DbParameter currentParameter in cmd.Parameters)
                        {
                            if (currentParameter.Direction == ParameterDirection.ReturnValue)
                            {
                                output = currentParameter;
                            }
                        }
                        threadTime.Reset();
                        // cmd.Parameters 
                        RaiseCommandExecuting(this, new CommandExecutingEventArgs
                        {
                            Name = this.Name,
                            SPID = this.SPID,
                            ThreadId = System.Threading.Thread.CurrentThread.ManagedThreadId,
                            Method = CommandExecuteMethod.ExecuteNonQuery,
                            CommandText = cmd.CommandText,
                            Parameters = cmd.Parameters,
                            ExecuteOrder = executeOrder,
                            ConnectionId = Helper.GetConnectionId(cmd.Connection),
                            TemplateId = this.TemplateId
                        }); ;
                        int rowsAffected = 0;
                        if (!cmd.CommandText.StartsWith("--"))
                        {
                            rowsAffected = cmd.ExecuteNonQuery();
                            if (output != null) result = output.Value;
                        }

                        RaiseCommandExecuted(this, new CommandExecutedEventArgs
                        {
                            Name = this.Name,
                            SPID = this.SPID,
                            ThreadId = System.Threading.Thread.CurrentThread.ManagedThreadId,
                            CommandText = cmd.CommandText,
                            Parameters = cmd.Parameters,
                            ExecuteOrder = executeOrder,
                            RowsAffected = rowsAffected,
                            ExecuteTime = threadTime.Mark().ElapsedTime.TotalMilliseconds,
                            ConnectionId = Helper.GetConnectionId(cmd.Connection),
                            TemplateId = this.TemplateId
                        });
                        return rowsAffected;
                    }
                    catch (Exception ex)
                    {
                        if (ex is DatabaseExecuteFailureException) throw;
                        RaiseExecuteException(this, new CommandExecuteExceptionEventArgs
                        {
                            Name = this.Name,
                            ExecuteOrder = executeOrder,
                            Exception = ex,
                            TemplateId = this.TemplateId,
                            ConnectionId = Helper.GetConnectionId(cmd.Connection),
                            SPID = this.SPID,
                            CommandText = cmd.CommandText,
                            Parameters = cmd.Parameters,
                        });
                        throw new DatabaseExecuteFailureException(ex);
                    }
                }
            }
        }

#if (!DEBUG)
       [DebuggerNonUserCode]
#endif
        /// <summary>
        /// 以繼承必需實作 ExecuteNonQuery 作為 ExecuteNonQuery 執行無回傳值之資料庫存取動作
        /// </summary>
        /// <param name="builder">SQL Command Builder</param>
        /// <param name="result"></param>
        /// <returns>傳回資料庫受資料庫存取動作影響之筆數.(詳見各資料庫所提供之 ADO.NET 說明</returns>
        protected override async Task<(int, object)> CommandExecuteNonQueryAsync(ICommandBuilder builder, object parameters = null)
        {
            object result = null;
            using (ThreadTimeMark threadTime = new ThreadTimeMark())// 計算 Execute 時間
            {
                using (TCommand cmd = CreateCommand(builder, parameters, CommandBuildOptions.None))
                {
                    int executeOrder = GetExecuteNextOrder();
                    try
                    {
                        DbParameter output = null;
                        foreach (DbParameter currentParameter in cmd.Parameters)
                        {
                            if (currentParameter.Direction == ParameterDirection.ReturnValue)
                            {
                                output = currentParameter;
                            }
                        }
                        threadTime.Reset();
                        // cmd.Parameters 
                        RaiseCommandExecuting(this, new CommandExecutingEventArgs
                        {
                            Name = this.Name,
                            SPID = this.SPID,
                            ThreadId = System.Threading.Thread.CurrentThread.ManagedThreadId,
                            Method = CommandExecuteMethod.ExecuteNonQuery,
                            CommandText = cmd.CommandText,
                            Parameters = cmd.Parameters,
                            ExecuteOrder = executeOrder,
                            ConnectionId = Helper.GetConnectionId(cmd.Connection),
                            TemplateId = this.TemplateId
                        });

                        int rowsAffected = await cmd.ExecuteNonQueryAsync();

                        if (output != null) result = output.Value;
                        RaiseCommandExecuted(this, new CommandExecutedEventArgs
                        {
                            Name = this.Name,
                            SPID = this.SPID,
                            ThreadId = System.Threading.Thread.CurrentThread.ManagedThreadId,
                            CommandText = cmd.CommandText,
                            Parameters = cmd.Parameters,
                            ExecuteOrder = executeOrder,
                            RowsAffected = rowsAffected,
                            ExecuteTime = threadTime.Mark().ElapsedTime.TotalMilliseconds,
                            ConnectionId = Helper.GetConnectionId(cmd.Connection),
                            TemplateId = this.TemplateId
                        });
                        return (rowsAffected, result);
                    }
                    catch (Exception ex)
                    {
                        if (ex is DatabaseExecuteFailureException) throw;
                        RaiseExecuteException(this, new CommandExecuteExceptionEventArgs
                        {
                            Name = this.Name,
                            ExecuteOrder = executeOrder,
                            Exception = ex,
                            TemplateId = this.TemplateId,
                            ConnectionId = Helper.GetConnectionId(cmd.Connection),
                            SPID = this.SPID,
                            CommandText = cmd.CommandText,
                            Parameters = cmd.Parameters,
                        });
                        throw new DatabaseExecuteFailureException(ex);
                    }
                }
            }
        }

#if (!DEBUG)
       [DebuggerNonUserCode]
#endif

        /// <summary>
        /// 以繼承必需實作 CommandExecuteScalar 作為 ExecuteScalar 執行回傳值單一值之資料庫存取動作
        /// </summary>
        /// <typeparam name="T">回傳值型態</typeparam>
        /// <param name="builder">SQL Command Builder</param>
        /// <returns>回傳值</returns>
        protected override T CommandExecuteScalar<T>(ICommandBuilder builder, object parameters, T defValue)
        {
            using (ThreadTimeMark threadTime = new ThreadTimeMark())// 計算 Execute 時間
            {
                using (TCommand cmd = CreateCommand(builder, parameters, CommandBuildOptions.None))
                {
                    int executeOrder = GetExecuteNextOrder();
                    try
                    {
                        RaiseCommandExecuting(this, new CommandExecutingEventArgs
                        {
                            Name = this.Name,
                            SPID = this.SPID,
                            ThreadId = System.Threading.Thread.CurrentThread.ManagedThreadId,
                            Method = CommandExecuteMethod.ExecuteScalar,
                            CommandText = cmd.CommandText,
                            Parameters = cmd.Parameters,
                            ExecuteOrder = executeOrder,
                            ConnectionId = Helper.GetConnectionId(cmd.Connection),
                            TemplateId = this.TemplateId
                        });

                        object result = cmd.ExecuteScalar();
                        RaiseCommandExecuted(this, new CommandExecutedEventArgs
                        {
                            Name = this.Name,
                            SPID = this.SPID,
                            ThreadId = System.Threading.Thread.CurrentThread.ManagedThreadId,
                            CommandText = cmd.CommandText,
                            Parameters = cmd.Parameters,
                            ExecuteOrder = executeOrder,
                            ExecuteTime = threadTime.Mark().ElapsedTime.TotalMilliseconds,
                            ConnectionId = Helper.GetConnectionId(cmd.Connection),
                            TemplateId = this.TemplateId
                        });
                        if (result == null || result == DBNull.Value) return defValue;
                        var converionType = typeof(T);

                        if (converionType.IsGenericType && converionType.GetGenericTypeDefinition() == typeof(Nullable<>))
                            converionType = Nullable.GetUnderlyingType(converionType);

                        if (result.GetType() == converionType)
                            return (T)result;

                        if (converionType == typeof(Type))
                            return (T)result;

                        return (T)Convert.ChangeType(result, converionType);
                    }
                    catch (Exception ex)
                    {
                        if (ex is DatabaseExecuteFailureException) throw;

                        RaiseExecuteException(this, new CommandExecuteExceptionEventArgs
                        {
                            Name = this.Name,
                            ExecuteOrder = executeOrder,
                            Exception = ex,
                            TemplateId = this.TemplateId,
                            ConnectionId = Helper.GetConnectionId(cmd.Connection),
                            SPID = this.SPID,
                            CommandText = cmd.CommandText,
                            Parameters = cmd.Parameters,
                        });
                        throw new DatabaseExecuteFailureException(ex);
                    }
                }
            }
        }

        /// <summary>
        /// 以繼承必需實作 CommandExecuteScalar 作為 ExecuteScalar 執行回傳值單一值之資料庫存取動作
        /// </summary>
        /// <typeparam name="T">回傳值型態</typeparam>
        /// <param name="builder">SQL Command Builder</param>
        /// <returns>回傳值</returns>
        protected override async Task<T> CommandExecuteScalarAsync<T>(ICommandBuilder builder, object parameters, T defValue)
        {
            using (ThreadTimeMark threadTime = new ThreadTimeMark())// 計算 Execute 時間
            {
                using (TCommand cmd = CreateCommand(builder, parameters, CommandBuildOptions.None))
                {
                    int executeOrder = GetExecuteNextOrder();
                    try
                    {
                        RaiseCommandExecuting(this, new CommandExecutingEventArgs
                        {
                            Name = this.Name,
                            SPID = this.SPID,
                            ThreadId = System.Threading.Thread.CurrentThread.ManagedThreadId,
                            Method = CommandExecuteMethod.ExecuteScalar,
                            CommandText = cmd.CommandText,
                            Parameters = cmd.Parameters,
                            ExecuteOrder = executeOrder,
                            ConnectionId = Helper.GetConnectionId(cmd.Connection),
                            TemplateId = this.TemplateId
                        });

                        object result = await cmd.ExecuteScalarAsync();
                        RaiseCommandExecuted(this, new CommandExecutedEventArgs
                        {
                            Name = this.Name,
                            SPID = this.SPID,
                            ThreadId = System.Threading.Thread.CurrentThread.ManagedThreadId,
                            CommandText = cmd.CommandText,
                            Parameters = cmd.Parameters,
                            ExecuteOrder = executeOrder,
                            ExecuteTime = threadTime.Mark().ElapsedTime.TotalMilliseconds,
                            ConnectionId = Helper.GetConnectionId(cmd.Connection),
                            TemplateId = this.TemplateId
                        });
                        if (result == null || result == DBNull.Value) return defValue;
                        var converionType = typeof(T);

                        if (converionType.IsGenericType && converionType.GetGenericTypeDefinition() == typeof(Nullable<>))
                            converionType = Nullable.GetUnderlyingType(converionType);

                        if (result.GetType() == converionType)
                            return (T)result;

                        if (converionType == typeof(Type))
                            return (T)result;

                        return (T)Convert.ChangeType(result, converionType);
                    }
                    catch (Exception ex)
                    {
                        if (ex is DatabaseExecuteFailureException) throw;

                        RaiseExecuteException(this, new CommandExecuteExceptionEventArgs
                        {
                            Name = this.Name,
                            ExecuteOrder = executeOrder,
                            Exception = ex,
                            TemplateId = this.TemplateId,
                            ConnectionId = Helper.GetConnectionId(cmd.Connection),
                            SPID = this.SPID,
                            CommandText = cmd.CommandText,
                            Parameters = cmd.Parameters,
                        });
                        throw new DatabaseExecuteFailureException(ex);
                    }
                }
            }
        }

#if (!DEBUG)
       [DebuggerNonUserCode]
#endif

        /// <summary>
        /// 執行 SQL Command 並回傳 DataTable
        /// </summary>
        /// <param name="builder">SQL Command Builder</param>
        /// <param name="buildFlags"></param>
        /// <returns></returns>
        protected override DataTable CommandOpenDataTable(ICommandBuilder builder, CommandBuildOptions buildFlags)
        {
            DataTable resultTable = new DataTable("ResultTable");
            using (ThreadTimeMark threadTime = new ThreadTimeMark())// 計算 Execute 時間
            {
                using (TCommand cmd = CreateCommand(builder, null, buildFlags))
                {
                    int executeOrder = GetExecuteNextOrder();
                    try
                    {
                        RaiseCommandExecuting(this, new CommandExecutingEventArgs
                        {
                            Name = this.Name,
                            SPID = this.SPID,
                            ThreadId = System.Threading.Thread.CurrentThread.ManagedThreadId,
                            Method = CommandExecuteMethod.OpenDataTable,
                            CommandText = cmd.CommandText,
                            Parameters = cmd.Parameters,
                            ExecuteOrder = executeOrder,
                            ConnectionId = Helper.GetConnectionId(cmd.Connection),
                            TemplateId = this.TemplateId
                        });
                        Fill(cmd, resultTable);
                        RaiseCommandExecuted(this, new CommandExecutedEventArgs
                        {
                            Name = this.Name,
                            SPID = this.SPID,
                            ThreadId = System.Threading.Thread.CurrentThread.ManagedThreadId,
                            CommandText = cmd.CommandText,
                            Parameters = cmd.Parameters,
                            ExecuteOrder = executeOrder,
                            ExecuteTime = threadTime.Mark().ElapsedTime.TotalMilliseconds,
                            ConnectionId = Helper.GetConnectionId(cmd.Connection),
                            TemplateId = this.TemplateId
                        });
                        return resultTable;
                    }
                    catch (Exception ex)
                    {
                        if (ex is DatabaseExecuteFailureException) throw;

                        RaiseExecuteException(this, new CommandExecuteExceptionEventArgs
                        {
                            Name = this.Name,
                            ExecuteOrder = executeOrder,
                            Exception = ex,
                            TemplateId = this.TemplateId,
                            ConnectionId = Helper.GetConnectionId(cmd.Connection),
                            SPID = this.SPID,
                            CommandText = cmd.CommandText,
                            Parameters = cmd.Parameters,
                        });
                        throw new DatabaseExecuteFailureException(ex);
                    }
                }
            }
        }

#if DEBUG

        [DebuggerNonUserCode]
#endif
        protected override void CommandFillDataSet(ref DataSet ds, ICommandBuilder builder, CommandBuildOptions buildFlags)
        {
            using (ThreadTimeMark ThreadTime = new ThreadTimeMark())// 計算 Execute 時間
            {
                using (TCommand cmd = CreateCommand(builder, null, buildFlags))
                {
                    DateTime StartTime = DateTime.Now; // 計算 Execute 時間
                    if (ds == null) ds = new DataSet();
                    int TableCount = ds.Tables.Count;
                    int ExecuteOrder = GetExecuteNextOrder();
                    try
                    {
                        RaiseCommandExecuting(this, new CommandExecutingEventArgs
                        {
                            Name = this.Name,
                            SPID = this.SPID,
                            ThreadId = System.Threading.Thread.CurrentThread.ManagedThreadId,
                            Method = CommandExecuteMethod.OpenDataTable,
                            CommandText = cmd.CommandText,
                            Parameters = cmd.Parameters,
                            ExecuteOrder = ExecuteOrder,
                            ConnectionId = Helper.GetConnectionId(cmd.Connection),
                            TemplateId = this.TemplateId
                        });

                        Fill(cmd, ds);
                        RaiseCommandExecuted(this, new CommandExecutedEventArgs
                        {
                            Name = this.Name,
                            SPID = this.SPID,
                            ThreadId = System.Threading.Thread.CurrentThread.ManagedThreadId,
                            CommandText = cmd.CommandText,
                            ExecuteOrder = ExecuteOrder,
                            ExecuteTime = ThreadTime.Mark().ElapsedTime.TotalMilliseconds,
                            ConnectionId = Helper.GetConnectionId(cmd.Connection),
                            TemplateId = this.TemplateId
                        });
                    }
                    catch (Exception ex)
                    {
                        if (ex is DatabaseExecuteFailureException) throw;
                        RaiseExecuteException(this, new CommandExecuteExceptionEventArgs
                        {
                            Name = this.Name,
                            ExecuteOrder = ExecuteOrder,
                            Exception = ex,
                            TemplateId = this.TemplateId,
                            ConnectionId = Helper.GetConnectionId(cmd.Connection),
                            SPID = this.SPID,
                            CommandText = cmd.CommandText,
                            Parameters = cmd.Parameters,
                        });
                        throw new DatabaseExecuteFailureException("Create Command Failure", ex);
                    }
                }
            }
        }

#if (!DEBUG)
       [DebuggerNonUserCode]
#endif

        /// <summary>
        /// 執行 SQL Command 並回傳 DataReader
        /// </summary>
        /// <param name="builder">SQL Command Builder</param>
        /// <param name="buildFlags"></param>
        /// <returns></returns>
        public virtual DbDataReader OpenReader(ICommandBuilder builder, object parameters = null, CommandBuildOptions buildFlags = CommandBuildOptions.None)
        {
            using (ThreadTimeMark threadTime = new ThreadTimeMark())// 計算 Execute 時間
            {
                using (TCommand cmd = CreateCommand(builder, parameters, buildFlags))
                {
                    int executeOrder = GetExecuteNextOrder();
                    try
                    {
                        RaiseCommandExecuting(this, new CommandExecutingEventArgs
                        {
                            Name = this.Name,
                            SPID = this.SPID,
                            ThreadId = System.Threading.Thread.CurrentThread.ManagedThreadId,
                            Method = CommandExecuteMethod.OpenReader,
                            CommandText = cmd.CommandText,
                            Parameters = cmd.Parameters,
                            ExecuteOrder = executeOrder,
                            ConnectionId = Helper.GetConnectionId(cmd.Connection),
                            TemplateId = this.TemplateId
                        });

                        ConnectionObject(allowNull: true)?.ClearMessage();
                        var reader = cmd.ExecuteReader();
                        RaiseCommandExecuted(this, new CommandExecutedEventArgs
                        {
                            Name = this.Name,
                            SPID = this.SPID,
                            ThreadId = System.Threading.Thread.CurrentThread.ManagedThreadId,
                            CommandText = cmd.CommandText,
                            Parameters = cmd.Parameters,
                            ExecuteOrder = executeOrder,
                            ExecuteTime = threadTime.Mark().ElapsedTime.TotalMilliseconds,
                            ConnectionId = Helper.GetConnectionId(cmd.Connection),
                            TemplateId = this.TemplateId
                        });
                        return reader;
                    }
                    catch (Exception ex)
                    {
                        if (ex is DatabaseExecuteFailureException)
                            throw;
                        RaiseExecuteException(this, new CommandExecuteExceptionEventArgs
                        {
                            Name = this.Name,
                            ExecuteOrder = executeOrder,
                            Exception = ex,
                            TemplateId = this.TemplateId,
                            ConnectionId = Helper.GetConnectionId(cmd.Connection),
                            SPID = this.SPID,
                            CommandText = cmd.CommandText,
                            Parameters = cmd.Parameters,
                        });
                        throw new DatabaseExecuteFailureException(ex);
                    }
                }
            }
        }

#if (!DEBUG)
       [DebuggerNonUserCode]
#endif

        /// <summary>
        /// 執行 SQL Command 並回傳 DataReader
        /// </summary>
        /// <param name="builder">SQL Command Builder</param>
        /// <param name="buildFlags"></param>
        /// <returns></returns>
        public virtual async Task<DbDataReader> OpenReaderAsync(ICommandBuilder builder, object parameters = null, CommandBuildOptions buildFlags = CommandBuildOptions.None)
        {
            using (ThreadTimeMark threadTime = new ThreadTimeMark())// 計算 Execute 時間
            {
                using (TCommand cmd = CreateCommand(builder, null, buildFlags))
                {
                    int executeOrder = GetExecuteNextOrder();
                    try
                    {
                        RaiseCommandExecuting(this, new CommandExecutingEventArgs
                        {
                            Name = this.Name,
                            SPID = this.SPID,
                            ThreadId = System.Threading.Thread.CurrentThread.ManagedThreadId,
                            Method = CommandExecuteMethod.OpenReader,
                            CommandText = cmd.CommandText,
                            Parameters = cmd.Parameters,
                            ExecuteOrder = executeOrder,
                            ConnectionId = Helper.GetConnectionId(cmd.Connection),
                            TemplateId = this.TemplateId
                        });

                        ConnectionObject(allowNull: true)?.ClearMessage();
                        var reader = await cmd.ExecuteReaderAsync();
                        RaiseCommandExecuted(this, new CommandExecutedEventArgs
                        {
                            Name = this.Name,
                            SPID = this.SPID,
                            ThreadId = System.Threading.Thread.CurrentThread.ManagedThreadId,
                            CommandText = cmd.CommandText,
                            Parameters = cmd.Parameters,
                            ExecuteOrder = executeOrder,
                            ExecuteTime = threadTime.Mark().ElapsedTime.TotalMilliseconds,
                            ConnectionId = Helper.GetConnectionId(cmd.Connection),
                            TemplateId = this.TemplateId
                        });
                        return reader;
                    }
                    catch (Exception ex)
                    {
                        if (ex is DatabaseExecuteFailureException)
                            throw;

                        RaiseExecuteException(this, new CommandExecuteExceptionEventArgs
                        {
                            Name = this.Name,
                            ExecuteOrder = executeOrder,
                            Exception = ex,
                            TemplateId = this.TemplateId,
                            ConnectionId = Helper.GetConnectionId(cmd.Connection),
                            SPID = this.SPID,
                            CommandText = cmd.CommandText,
                            Parameters = cmd.Parameters,
                        });
                        throw new DatabaseExecuteFailureException(ex);
                    }
                }
            }
        }

#if (!DEBUG)
       [DebuggerNonUserCode]
#endif

        /// <summary>
        /// 執行 SQL Command 並回傳 DataReader
        /// </summary>
        /// <param name="type"></param>
        /// <param name="command"></param>
        /// <returns></returns>
        public virtual DbDataReader OpenReader(string command, object parameters = null, CommandBuildOptions buildFlags = CommandBuildOptions.None, CommandType type = CommandType.Text)
        {
            TCommand cmd = CreateCommand(new StringCommandBuilder(type, command), parameters, buildFlags);
            using (ThreadTimeMark threadTime = new ThreadTimeMark())// 計算 Execute 時間
            {
                int executeOrder = GetExecuteNextOrder();
                try
                {
                    //if(cmd.Connection is SqlConnection cnn)
                    //{
                    //    SqlCommand ss = new SqlCommand("SET STATISTICS IO ON", cnn, cmd.Transaction as SqlTransaction);
                    //    ss.ExecuteNonQuery();
                    //    //ss = new SqlCommand("SET STATISTICS TIME ON", cnn, cmd.Transaction as SqlTransaction);
                    //    //ss.ExecuteNonQuery();
                    //    cnn.FireInfoMessageEventOnUserErrors = true;
                    //    cnn.InfoMessage += Cnn_InfoMessage;
                    //}
                    RaiseCommandExecuting(this, new CommandExecutingEventArgs
                    {
                        Name = this.Name,
                        SPID = this.SPID,
                        ThreadId = System.Threading.Thread.CurrentThread.ManagedThreadId,
                        Method = CommandExecuteMethod.OpenReader,
                        CommandText = cmd.CommandText,
                        Parameters = cmd.Parameters,
                        ExecuteOrder = executeOrder,
                        ConnectionId = Helper.GetConnectionId(cmd.Connection),
                        TemplateId = this.TemplateId
                    });
                    return cmd.ExecuteReader();
                }
                catch (Exception ex)
                {
                    if (ex is DatabaseExecuteFailureException)
                        throw;
                    RaiseExecuteException(this, new CommandExecuteExceptionEventArgs
                    {
                        Name = this.Name,
                        ExecuteOrder = executeOrder,
                        Exception = ex,
                        TemplateId = this.TemplateId,
                        ConnectionId = Helper.GetConnectionId(cmd.Connection),
                        SPID = this.SPID,
                        CommandText = cmd.CommandText,
                        Parameters = cmd.Parameters,
                    });
                    throw new DatabaseExecuteFailureException(ex);
                }

            }
        }

        private void Cnn_InfoMessage(object sender, SqlInfoMessageEventArgs e)
        {
            Debug.Print($"###  {e.Source}");
            Debug.Print(e.Message);
            Debug.Print($"================");
            foreach(var er in e.Errors)
            {
                Debug.Print($"{er.ToString()}");
            }
            Debug.Print($"================");
        }

        /// <summary>
        /// 執行 SQL Command 並回傳 DataReader
        /// </summary>
        /// <param name="type"></param>
        /// <param name="command"></param>
        /// <returns></returns>
        public virtual async Task<DbDataReader> OpenReaderAsync(string command, object parameters = null, CommandBuildOptions buildFlags = CommandBuildOptions.None, CommandType type = CommandType.Text)
        {
            TCommand cmd = CreateCommand(new StringCommandBuilder(type, command), parameters, buildFlags);
            using (ThreadTimeMark threadTime = new ThreadTimeMark())// 計算 Execute 時間
            {
                int executeOrder = GetExecuteNextOrder();
                try
                {
                    RaiseCommandExecuting(this, new CommandExecutingEventArgs
                    {
                        Name = this.Name,
                        SPID = this.SPID,
                        ThreadId = System.Threading.Thread.CurrentThread.ManagedThreadId,
                        Method = CommandExecuteMethod.OpenReader,
                        CommandText = cmd.CommandText,
                        Parameters = cmd.Parameters,
                        ExecuteOrder = executeOrder,
                        ConnectionId = Helper.GetConnectionId(cmd.Connection),
                        TemplateId = this.TemplateId
                    });
                    return await cmd.ExecuteReaderAsync(); //new CancellationToken()
                }
                catch (Exception ex)
                {
                    if (ex is DatabaseExecuteFailureException)
                        throw;
                    RaiseExecuteException(this, new CommandExecuteExceptionEventArgs
                    {
                        Name = this.Name,
                        ExecuteOrder = executeOrder,
                        Exception = ex,
                        TemplateId = this.TemplateId,
                        ConnectionId = Helper.GetConnectionId(cmd.Connection),
                        SPID = this.SPID,
                        CommandText = cmd.CommandText,
                        Parameters = cmd.Parameters,
                    });
                    throw new DatabaseExecuteFailureException(ex);
                }
            }
        }
    }
}