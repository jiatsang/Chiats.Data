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
    /// �䴩 ADO.NET ����Ʀs������.
    /// </summary>
    /// <remarks>
    /// DACTemplate ���Ѹ�Ʀs������¦����, �p�ݭn ADO/RDS/ODBC �Ψ�L�t�өҴ��Ѫ��s������, �A�ݭn��@ DACTemplate ,
    /// �t�өҴ��Ѫ��s�������p�G�ŦX ADO.NET ����Ʀs���W��, �h�i�H�� DBTemplate ��@. <br/>
    /// DBTemplate �䴩 SQLLOG  �ԲӽЬd�\ SqlLog ����
    /// </remarks>
    public abstract class DbTemplate<TConnection, TTransaction, TCommand> : DacTemplate<TConnection, TTransaction, TCommand>, IDbTemplate
            where TConnection : DbConnection
            where TTransaction : DbTransaction
            where TCommand : DbCommand
    {
        /// <summary>
        /// �s�� ADO.NET ���骺 Connection(SqlConnection) ����.
        /// </summary>
        public abstract class ConnectionClass : ConnectionManagement
        {
            
            /// <summary>
            /// ConnectionObject �غc�l
            /// </summary>
            /// <param name="template">�ǤJ�غc������ DACTemplate ����</param>
            public ConnectionClass(DbTemplate<TConnection, TTransaction, TCommand> template) : base(template) { }

            /// <summary>
            ///  �s����骺 Connection(SqlConnection) ����
            /// </summary>
            protected TConnection connection = null;

            protected DateTime startTime;

            protected object forlock = new object();

            /// <summary>
            /// �s����骺 Transaction(SqlTransaction) ����
            /// </summary>
            protected internal TTransaction transaction = null;

            protected internal Guid ClientConnectionId = Guid.NewGuid();

            /// <summary>
            /// �q�� Connection Close(); �� DACTemplate �� InculdeTransaction �� False �ɥB �b TransScope
            /// �d�򵲧��e CommitTrans(�� TransScope.Complate() �޵o) ���������. �h�ݭn�b  close() �ɰ����� Rollback
            /// </summary>
            public override void Dispose()
            {
                lock (forlock)
                {
                    if (connection != null)
                    {

//#if NETCOREAPP2_2  || NETSTANDARD2_0 || NET5_0_OR_GREATER

//                            // Marshal.GetExceptionPointers() != IntPtr.Zero
//                            // GetExceptionPointers �u���b .Net framework 4 and .Net Core 3.0 �~���䴩
//                            int ExceptionOccurred = Marshal.GetExceptionCode();
//                            if (ExceptionOccurred != 0)
//                            {
//                                // ����������o�� Exception �j�� Abort                           
//                                AbortCount++;
//                                Template.RaiseCommandTransaction(Template, new CommandTransactionEventArgs
//                                {
//                                    Name = Template.Name,
//                                    TemplateId = Template.TemplateId,
//                                    SPID = Template.SPID,
//                                    Transaction = $"SetAbort AbortReson: 0x{ExceptionOccurred:X} ����������o�� Exception �j�� Abort"
//                                });
//                                //Debug.Print($"\r\n#Check SetAbort [{Template.TemplateId}] ����������o�� Exception �j�� Abort\r\n");
//                            }
//#else
                        // GetExceptionPointers �u���b .Net framework 4 and .Net Core 3.0 �~���䴩
                        var ExceptionPointers = Marshal.GetExceptionPointers();
                        if (ExceptionPointers != IntPtr.Zero)
                        {
                            // ����������o�� Exception �j�� Abort                           
                            AbortCount++;
                            Template.RaiseCommandTransaction(Template, new CommandTransactionEventArgs
                            {
                                Name = Template.Name,
                                TemplateId = Template.TemplateId,
                                SPID = Template.SPID,
                                Transaction = $"SetAbort AbortReson: ����������o�� Exception �j�� Abort"
                            });
                            //Debug.Print($"\r\n#Check SetAbort [{Template.TemplateId}] ����������o�� Exception �j�� Abort\r\n");
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
            /// �� DACTemplate �� InculdeDTCTransaction �� False �� , TransManager �|�I�s BeginTrans �M CommitTrans �@
            /// ���޲z Transaction ���}�l�M����. �p�G�ݭn Rollback �ɾ��h�b Close() �|�Q�I�s��.
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
                            // Chaos            �L�k�мg�����h�Źj������Ȥ�ܧ�C
                            // ReadCommitted     �b��������i�HŪ�� Volatile ��ơA������ק�Ӹ�ơC
                            // ReadUncommitted   �i�HŪ���M�ק�b��������ܰʩʸ�ơC
                            // RepeatableRead    �i�HŪ�������O�|�ק�b��������ܰʩʸ�ơC �b��������A�i�H�[�J�s��ơC
                            // Serializable      �ܰʩʸ�ƥi�HŪ�������O�|�ק�A�æb��������A�i�H�[�J����s��ơC
                            // Snapshot          �i�HŪ���ܰʩʸ�ơC ����ק��Ƥ��e�A�����ҳ̪�Ū����A��L����O�_�w�ܧ󪺸�ơC �p�G��Ƥw��s�A�|�޵o���~�C �o�i��������o��ƪ����e�{�i���ȡC
                            // Unspecified       �ϥλP���w���P���j�����ŮɡA���L�k�M�w�h�šC �p�G���ȳ]�w�A�h�|�Y�^�ҥ~���p�C
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw new DatabaseConnectFailureException("SQL �s������, �i��O�������`�θ�Ʈw���s�b.", ex);
                }
            }

            /// <summary>
            /// �� DACTemplate �� InculdeTransaction �� False �� , TransManager �|�I�s BeginTrans �M CommitTrans �@
            /// ���޲z Transaction ���}�l�M����. �p�G�ݭn Rollback �ɾ��h�b Close() �|�Q�I�s��.
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
                    throw new DatabaseConnectFailureException("SQL �s������, �i��O�������`�θ�Ʈw���s�b.", ex);
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
                    throw new DatabaseConnectFailureException("SQL �s������, �i��O�������`�θ�Ʈw���s�b.", ex);
                }
            }

            /// <summary>
            /// ���o Connection ���骫��.
            /// </summary>
            /// <returns> Connection ���骫��</returns>
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
                        throw new DatabaseConnectFailureException("SQL �s������, �i��O�������`�θ�Ʈw���s�b.", ex);
                    }
                }
            }

            /// <summary>
            /// ���o Transaction ���骫��. �p�G���O�ϥ� Transaction �h�|�^�� null
            /// </summary>
            /// <returns> Connection ���骫��</returns>
            public override TTransaction GetCurrentTransaction()
            {
                return transaction;
            }
        }


        public static event EventHandler<CommandBuilderEventArgs> CreateCommandBuilder;

        public SqlOptions Options { get; set; }

        /// <summary>
        ///  ���o IManualTransaction ����. �i���ʱ��� Transaction  �� IManualTransaction �ۦ�޲z Begin/Commit/Rollback Transaction �ϥ�.
        ///  ManualTransaction �u���b TransactionMode is Manual �� �~���@��
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
                throw new NotSupportedException("ManualTransaction �u���b TransactionMode is Manual �ɤ~���@��.");
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
        /// DBTemplate �غc�l
        /// </summary>
        /// <param name="name">�s�u�W��</param>
        protected DbTemplate(string name) : base(name) { }

        /// <summary>
        /// DBTemplate �غc�l
        /// </summary>
        /// <param name="name">�s�u�W��</param>
        /// <param name="transactionMode">���� OleDbTemplate �O�_�ѻP Transaction </param>
        protected DbTemplate(string name, TransactionMode transactionMode) : base(name, transactionMode) { }

        /// <summary>
        /// DBTemplate �غc�l
        /// </summary>
        /// <param name="name">�s�u�W��</param>
        /// <param name="transactionMode">���� OleDbTemplate �O�_�ѻP Transaction </param>
        /// <param name="connectionString">��Ʈw�s�u�r��</param>
        protected DbTemplate(string name, TransactionMode transactionMode, string connectionString) :
            base(name, transactionMode, connectionString, null)
        { }

        

        /// <summary>
        /// ConnectionManagement ��l�Ƶ{��. �q�`�O�Ψӫإ߸�Ʈw���s�u.
        /// </summary>
        protected override void Initiailize()
        {
            // �إ�ConnectionManagement ����ñҥγs�u ,
            //  ConnectionManagement ���󥲭n�n�P�ɫإ�,  �H���� ConnectionManagement Stack<ITemplate> �����.
            var cm = ConnectionObject(allowNull: false, Initiailize: true);
            cm.InfoMessage += ConnectionManagement_InfoMessage;
            base.Initiailize();
        }

        private void ConnectionManagement_InfoMessage(object sender, InfoMessageEventArgs e)
        {
            this.Messages.Add( new DbMessage { Message = e.Message });
        }

        /// <summary>
        /// �إߩҭn����e�� TCommand ����.
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
        /// �ഫ Parameter ����, �Ѽз� Parameter �ഫ�� ADO.NET �һ� IDataParameter ���A
        /// </summary>
        /// <param name="name">�ѼƦW��</param>
        /// <param name="param">�ѼƭȤ��e</param>
        /// <returns></returns>
        protected abstract IDataParameter ParameterConvert(string name, Parameter param);

        protected abstract IDataParameter ParameterConvert(DbParameter param);

        /// <summary>
        /// ���� SQL Command �æ^�� DataTable
        /// </summary>
        /// <param name="cmd">SQL Command</param>
        /// <param name="table">DataTable</param>
        protected abstract void Fill(TCommand cmd, DataTable table);

        /// <summary>
        /// ���� SQL Command �æ^�� DataSet
        /// </summary>
        /// <param name="cmd">SQL Command</param>
        /// <param name="ds">DataSet</param>
        protected abstract void Fill(TCommand cmd, DataSet ds);

#if (!DEBUG)
       [DebuggerNonUserCode]
#endif

        /// <summary>
        /// �H�~�ӥ��ݹ�@ ExecuteNonQuery �@�� ExecuteNonQuery ����L�^�ǭȤ���Ʈw�s���ʧ@
        /// </summary>
        /// <param name="builder">SQL Command Builder</param>
        /// <param name="result"></param>
        /// <returns>�Ǧ^��Ʈw����Ʈw�s���ʧ@�v�T������.(�Ԩ��U��Ʈw�Ҵ��Ѥ� ADO.NET ����</returns>
        protected override int CommandExecuteNonQuery(ICommandBuilder builder, object parameters, ref object result)
        {
            using (ThreadTimeMark threadTime = new ThreadTimeMark())// �p�� Execute �ɶ�
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
        /// �H�~�ӥ��ݹ�@ ExecuteNonQuery �@�� ExecuteNonQuery ����L�^�ǭȤ���Ʈw�s���ʧ@
        /// </summary>
        /// <param name="builder">SQL Command Builder</param>
        /// <param name="result"></param>
        /// <returns>�Ǧ^��Ʈw����Ʈw�s���ʧ@�v�T������.(�Ԩ��U��Ʈw�Ҵ��Ѥ� ADO.NET ����</returns>
        protected override async Task<(int, object)> CommandExecuteNonQueryAsync(ICommandBuilder builder, object parameters = null)
        {
            object result = null;
            using (ThreadTimeMark threadTime = new ThreadTimeMark())// �p�� Execute �ɶ�
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
        /// �H�~�ӥ��ݹ�@ CommandExecuteScalar �@�� ExecuteScalar ����^�ǭȳ�@�Ȥ���Ʈw�s���ʧ@
        /// </summary>
        /// <typeparam name="T">�^�ǭȫ��A</typeparam>
        /// <param name="builder">SQL Command Builder</param>
        /// <returns>�^�ǭ�</returns>
        protected override T CommandExecuteScalar<T>(ICommandBuilder builder, object parameters, T defValue)
        {
            using (ThreadTimeMark threadTime = new ThreadTimeMark())// �p�� Execute �ɶ�
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
        /// �H�~�ӥ��ݹ�@ CommandExecuteScalar �@�� ExecuteScalar ����^�ǭȳ�@�Ȥ���Ʈw�s���ʧ@
        /// </summary>
        /// <typeparam name="T">�^�ǭȫ��A</typeparam>
        /// <param name="builder">SQL Command Builder</param>
        /// <returns>�^�ǭ�</returns>
        protected override async Task<T> CommandExecuteScalarAsync<T>(ICommandBuilder builder, object parameters, T defValue)
        {
            using (ThreadTimeMark threadTime = new ThreadTimeMark())// �p�� Execute �ɶ�
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
        /// ���� SQL Command �æ^�� DataTable
        /// </summary>
        /// <param name="builder">SQL Command Builder</param>
        /// <param name="buildFlags"></param>
        /// <returns></returns>
        protected override DataTable CommandOpenDataTable(ICommandBuilder builder, CommandBuildOptions buildFlags)
        {
            DataTable resultTable = new DataTable("ResultTable");
            using (ThreadTimeMark threadTime = new ThreadTimeMark())// �p�� Execute �ɶ�
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
            using (ThreadTimeMark ThreadTime = new ThreadTimeMark())// �p�� Execute �ɶ�
            {
                using (TCommand cmd = CreateCommand(builder, null, buildFlags))
                {
                    DateTime StartTime = DateTime.Now; // �p�� Execute �ɶ�
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
        /// ���� SQL Command �æ^�� DataReader
        /// </summary>
        /// <param name="builder">SQL Command Builder</param>
        /// <param name="buildFlags"></param>
        /// <returns></returns>
        public virtual DbDataReader OpenReader(ICommandBuilder builder, object parameters = null, CommandBuildOptions buildFlags = CommandBuildOptions.None)
        {
            using (ThreadTimeMark threadTime = new ThreadTimeMark())// �p�� Execute �ɶ�
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
        /// ���� SQL Command �æ^�� DataReader
        /// </summary>
        /// <param name="builder">SQL Command Builder</param>
        /// <param name="buildFlags"></param>
        /// <returns></returns>
        public virtual async Task<DbDataReader> OpenReaderAsync(ICommandBuilder builder, object parameters = null, CommandBuildOptions buildFlags = CommandBuildOptions.None)
        {
            using (ThreadTimeMark threadTime = new ThreadTimeMark())// �p�� Execute �ɶ�
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
        /// ���� SQL Command �æ^�� DataReader
        /// </summary>
        /// <param name="type"></param>
        /// <param name="command"></param>
        /// <returns></returns>
        public virtual DbDataReader OpenReader(string command, object parameters = null, CommandBuildOptions buildFlags = CommandBuildOptions.None, CommandType type = CommandType.Text)
        {
            TCommand cmd = CreateCommand(new StringCommandBuilder(type, command), parameters, buildFlags);
            using (ThreadTimeMark threadTime = new ThreadTimeMark())// �p�� Execute �ɶ�
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
        /// ���� SQL Command �æ^�� DataReader
        /// </summary>
        /// <param name="type"></param>
        /// <param name="command"></param>
        /// <returns></returns>
        public virtual async Task<DbDataReader> OpenReaderAsync(string command, object parameters = null, CommandBuildOptions buildFlags = CommandBuildOptions.None, CommandType type = CommandType.Text)
        {
            TCommand cmd = CreateCommand(new StringCommandBuilder(type, command), parameters, buildFlags);
            using (ThreadTimeMark threadTime = new ThreadTimeMark())// �p�� Execute �ɶ�
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