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
    /// ��Ʈw�s���@�P��¦����. �y�z�s����Ʈw��¦�@�Τ���.
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    /// <item><description>TConnection:</description>��Ʈw�s�����s������.�p SqlConnection</item>
    /// <item><description>TTransaction:</description>��Ʈw�s�����������.�p SqlTransaction</item>
    /// <item><description>TCommand:</description>��Ʈw�s�����������.�p SqlCommand</item>
    /// </list>
    /// DACTemplate �D�n�O�n�޲z��Ʈw�s�u(Connection)�M���(Transaction). ���M DataAccessComponent ���O�@�_�X�@���Ҧ�.�ëD�H�@�Τ����Ӧs����Ʈw, �ӬO�H�U��Ʈw�ۤv�m���s���s����k�Ӱ��欰�]�p��h. �ҥH�򥻤W�å��W�d�p��
    /// �s����Ʈw����k.<br/>
    /// Chiats Data Access Module ��@ DBTemplate �� ADO.NET ����¦���O�Ӥ䴩��Ʈw���s���u�@. �u�n�O�ŦX ADO.NET ����¦���O���i�H����
    /// �~�ӫ�ϥ�. SQLTemplate �M OleDbTemplate �h�O���O�� Microsoft SQL Server �M OleDb �ҳ]�p���s�����O.<br/>
    /// Chiats Data Access Module �S��
    /// <list type="bullet">
    /// <item>²�ƹ��Ʈw�s�����{��. </item>
    /// <item>�۰ʳB�z���Ʈw�s�u���󪺺޲z. �s�u���󪺺޲z�|�O���@�� Thread �@�ӳs�u���󪺺޲z. ���䴩�h Thread �s�u���󪺺޲z.</item>
    /// <item>���󪺳]�p�O�H�O���즳��Ʈw�s�����󤧺믫���D�n�ؼ�, �Ҧp SQLClient �N�|�ϥ� SqlDataReader �@���^�Ǥ�����?.</item>
    /// <item>�䴩�h��Ʈw Transaction ���䴩. �P�ɤ䴩 SQL,Oracle,Informix,DB2 ���� (�����}�� MS DTC �A�� ,�Ԩ� Systtem.Tracnaction ����) </item>
    /// <item>�۰ʳB�z DBNull �M null ���ഫ�����D. �p�G�O DataTable/DataSet �h���|�۰��ഫ.</item>
    /// <item>�v���� OLEDB,SQL,Oracle ��Ʈw�s������</item>
    /// <item>�䴩 Parameter ���Ϊk, �аѦҦU��Ʈw�s�����󻡩�. �Ҧp SQLClient �� SqlParameter , Oledb �h�O OleDbParameter ����</item>
    /// <item>���\�ۦ�}�o�s����Ʈw�s������ �Ҧp ODBC,DB2,Informix ����</item>
    /// <item>�䴩 ICommandBuilder �� String Format �ӫغc CommandText(sql statement)</item>
    /// <item>�䴩 SQL Command Log �� Exception Log . ���\�ԲӰO�� �Ҧ����Ʈw���ʧ@, �]�t Transaction </item>
    /// </list>
    /// </remarks>
    public abstract partial class DacTemplate<TConnection, TTransaction, TCommand> : IDacTemplate
    {
        public List<DbMessage> Messages { get; } = new List<DbMessage>();
        public event EventHandler<CommandBuildingEventArgs> CommandBuilding;

        /// <summary>
        /// ��Ʀs�� Exception ���ƥ�
        /// </summary>
        public event EventHandler<CommandExecuteExceptionEventArgs> CommandExecuteException;

        /// <summary>
        /// ���쪺�ƥ�  ��Ʀs���e���ƥ�.
        /// </summary>
        public static event EventHandler<CommandExecutingEventArgs> CommandExecuting;
        /// <summary>
        /// ���쪺�ƥ�   ��Ʀs���������ƥ�
        /// </summary>
        public static event EventHandler<CommandExecutedEventArgs> CommandExecuted;
        /// <summary>
        /// ���쪺�ƥ� 
        /// </summary>
        public static event EventHandler<CommandTransactionEventArgs> CommandTransaction;
        
        public string Message => ConnectionObject(allowNull: true)?.Message.ToString();

        ///// <summary>
        ///// ���ܥثe�Ѧҫإߪ� ConnectionConfiguration, �p�G���O�� �Ѧ� Configuration �ҫت�  Configuration ��Null
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
        /// �x�s TransManager �һݪ���Ʈw���s�� Connection ��¦���骫��. �p Connection ����<br/>
        /// �����O�O�]�p�����g��Ʈw�s�u��¦���O�ϥ�. ���O���������O.
        /// </summary>
        /// <remarks>
        /// ConnectionObject �ĥζ}�񦡤��[�c. �L�򥻤W���\�A�ϥΤ��P����Ʈw�s����k. ADO.NET ADO OLEDB ODBC ����.
        /// public T GetConnection() -> ���\�A���A����Ʈw�s������ΧA�һݭn�����O . �p SqlConnection , OledbConnection ����.
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
            /// �M ConnectionObject ���s Template ����.
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
            /// ���o�U�@�Ӱ��檺�Ǹ�
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
            /// ConnectionObject �غc�l , �غc�|�� DACTemplate �~�Ӫ̩I�s. �ê����}�� Connection �s�u
            /// </summary>
            /// <param name="template">�ǤJ�غc������ DACTemplate ����</param>
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
            /// �]�w�s�u�����~��T.
            /// </summary>
            /// <param name="connectFailure"></param>
            /// <param name="Message"></param>
            protected void SetConnectionState(bool connectFailure, string Message)
            {
                Template.SetConnectionState(connectFailure, Message);
            }

            /// <summary>
            /// �}�ҳs�u, �C�@�Ӹ�Ʈw��ڳs�u���ɾ��|�� DACTemplate TransManager and  TransScope �M�w.
            /// ��ݭn�s�u�|�q�� ConnectionDataPack.ConnectionOpen(). �]�� ConnectionOpen �����b�Q�I�s��
            /// �إ߹�ڪ��s�u.
            /// </summary>
            public abstract void ConnectionOpen(string connectionString);

            /// <summary>
            /// �ˬd�s�u�O�_�}��
            /// </summary>
            /// <returns>�}�ҫh�^ True</returns>
            public abstract bool IsOpen();

            /// <summary>
            /// �q�� Connection Dispose(); �� DACTemplate �� InculdeTransaction �� False �ɥB �b TransScope
            /// �d�򵲧��e CommitTrans(�� TransScope.Complate() �޵o) ���������. �h�ݭn�b  close() �ɰ����� Rollback
            /// </summary>
            public abstract void Dispose();

            /// <summary>
            /// �� DACTemplate �� InculdeTransaction �� False �� , TransManager �|�I�s BeginTrans �M CommitTrans �@
            /// ���޲z Transaction ���}�l�M����. �p�G�ݭn Rollback �ɾ��h�b Close() �|�Q�I�s��.
            /// </summary>
            public abstract void BeginTrans();

            /// <summary>
            /// �� DACTemplate �� InculdeTransaction �� False �� , TransManager �|�I�s BeginTrans �M CommitTrans �@
            /// ���޲z Transaction ���}�l�M����. �p�G�ݭn Rollback �ɾ��h�b Close() �|�Q�I�s��.
            /// </summary>
            public abstract void Commit();

            /// <summary>
            ///
            /// </summary>
            public abstract void Rollback();

            /// <summary>
            /// ���o Connection ���骫��.
            /// </summary>
            /// <returns></returns>
            public abstract TConnection GetCurrentConnection();

            /// <summary>
            /// ���o Transaction ���骫��.
            /// </summary>
            /// <returns></returns>
            public abstract TTransaction GetCurrentTransaction();

            /// <summary>
            /// �إߤ@�� TCommand ���骫��.
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
            /// �O�_�ҥΤ@�Ӹ�Ʈw���
            /// </summary>
            public TransactionMode TransactionMode { get; private set; }

            #endregion IConnectionManagement Members
        }

        /// <summary>
        /// ��Ʈw�s������W��. ��C�@�Ӧs������. ���W�ٷ|�@�����o�����ѼƤ��̾�.
        /// </summary>
        public readonly string Name;

        /// <summary>
        /// �g�� App.Configure ���o�� ConnectionStringSettings
        /// </summary>
        protected readonly string ConnectionString;

        /// <summary>
        /// ��ܸӳs�u�����@�ѧO�X.
        /// </summary>
        public readonly string TemplateId = $"TemplateId_{System.Threading.Interlocked.Increment(ref TemplateMaxId):X}";
        private static int TemplateMaxId = 0;

        /// <summary>
        /// ���o�U�@�Ӱ��檺�Ǹ�
        /// </summary>
        /// <returns></returns>
        public int GetExecuteNextOrder()
        {
            int? order = this.ConnectionObject(allowNull: true)?.GetExecuteNextOrder();
            return (order == null ? 0 : (int)order);
        }

        /// <summary>
        /// �O���ثe���s�u��T..
        /// </summary>
        public struct ConnectState
        {
            private bool _connectFailure;
            private string _failureMessage;

            /// <summary>
            /// �Ǧ^�̫�@�������~�T��
            /// </summary>
            public string Message => _failureMessage;

            /// <summary>
            /// �Ǧ^�ثe�O�_���b�s�u��..
            /// </summary>
            public bool IsConnect => !_connectFailure;

            /// <summary>
            /// �]�w�s�u�����~��T.
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
        /// �s�u���A
        /// </summary>
        public ConnectState State = new ConnectState();

        /// <summary>
        /// �]�w�s�u�����~��T.
        /// </summary>
        /// <param name="connectFailure"></param>
        /// <param name="Message"></param>
        protected void SetConnectionState(bool connectFailure, string Message)
        {
            State.SetConnectionState(connectFailure, Message);
        }

        /// <summary>
        /// DACTemplate �غc�l , DACTemplate �|�̪���W�٦V Configure File ����������T.
        /// </summary>
        /// <param name="name">�s�u�W��</param>
        /// <param name="transactionMode">���� DACTemplate �O�_�ѻP Transaction </param>
        protected DacTemplate(string name, TransactionMode transactionMode) : this(name, transactionMode, null, null) { }

        /// <summary>
        /// DACTemplate �غc�l , DACTemplate �|�̪���W�٦V Configure File ����������T.
        /// </summary>
        /// <param name="name"></param>
        protected DacTemplate(string name) : this(name, TransactionMode.Transaction, null, null) { }

        /// <summary>
        /// DACTemplate �غc�l , �������w ConnectionString
        /// </summary>
        /// <param name="name">�s�u�W��</param>
        /// <param name="transactionMode">���� DACTemplate �O�_�ѻP Transaction </param>
        /// <param name="connectionString">��Ʈw�s�u�r��</param>
        protected DacTemplate(string name, TransactionMode transactionMode, string connectionString) : this(name, TransactionMode.Transaction, connectionString, null) { }

        /// <summary>
        /// DACTemplate �غc�l , �������w ConnectionString
        /// </summary>
        /// <param name="name">�s�u�W��</param>
        /// <param name="transactionMode">���� DACTemplate �O�_�ѻP Transaction </param>
        /// <param name="connectionString">��Ʈw�s�u�r��</param>
        /// <param name="lexicalDictionary">LexicalDictionary �w�q��</param>
        protected DacTemplate(string name, TransactionMode transactionMode, string connectionString, string lexicalDictionary)
        {
            this.Name = name;
            if (Name == null)
            {
                // �����w�W�ٮ�, �ݭn���@�Ӱߤ@���W��.
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
                        SetConnectionState(true, $"�b�]�w�ɤ��䤣���Ƴs�u�W�� ({name}).");
                }
                else
                    SetConnectionState(true, "���w��Ƴs�u�W��.");
            }
            //Debug.Print($"\r\n#Check Template.New {this.TemplateId}\r\n");
            Initiailize();
        }

        /// <summary>
        /// �Ǧ^���s���s�u�������O.
        /// </summary>
        /// <returns>�s�u�������O</returns>
        protected internal Type GetConnectionObjectType()
        {
            ConnectionObjectTypeAttribute connectionObject = CommonExtensions.GetCustomAttributeEx<ConnectionObjectTypeAttribute>(GetType(), true);
            if (connectionObject != null)
                return connectionObject.ConnectionObjectType;
            throw new CommonException("DACTemplate : �����w ConnectionObjectType.");
        }

        /// <summary>
        /// ��l��
        /// </summary>
        protected virtual void Initiailize() { }

        /// <summary>
        /// ���o��Ʈw�t�Ϊ�������T , ���䴩�ɷ|�^�� null
        /// </summary>
        protected virtual IDbInformation GetDBInformation() { return null; }

        private TransactionMode transactionMode = TransactionMode.Transaction;    /* �w�]�䴩 */

        /// <summary>
        /// ���o�ثe�v�s�b Connection(Transaction) ����, ConnectionManagement �O�H TransactionManager/GetPackObject �Ӻ޲z��ͩR�g��.
        /// �è� TransactionMode �ӨM�w�p����o�ثe�Ҧb���O�� Connection(Transaction) ����.
        /// </summary>
        /// <param name="allowNull">���ܤ��\�^�� NULL, ��L�s�u�����, ���j���إߤ@�Ӫ��s�u </param>
        /// <returns></returns>
        protected ConnectionManagement ConnectionObject(bool allowNull = true, bool Initiailize = false)
        {
            lock (this)
            {
                var connectionObject = (ConnectionManagement)TransactionManager.Current.GetPackObject(this, allowNull, Initiailize);
                if (connectionObject != null)
                {
                    // �}�ҳs�u��, �P�ɨM�w Transaction �M TransactionScope �p�����s.
                    if (!connectionObject.IsOpen())
                    {
                        if (TransactionMode == TransactionMode.DTCTransaction)
                        {
                            // TODO : �䴩 TransactionScope
                            connectionObject.ConnectionOpen(this.ConnectionString);
                        }
                        else
                        {
                            // TODO : �䴩�W�� TransactionScope
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
        /// ���� DACTemplate �O�_�ѻP Transaction
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
        /// ���o IManualTransaction ����. �i���ʱ��� Transaction  �� IManualTransaction �ۦ�޲z Begin/Commit/Rollback Transaction �ϥ�.
        /// ManualTransaction �u���b TransactionMode is Manual �� �~���@��
        /// </summary>
        /// <returns></returns>
        public abstract IManualTransaction Transaction { get; }

        private object _returnValue = null;

        /// <summary>
        /// ���o ExecuteNonQuery ��Ʈw�^�Ǫ����e��.
        /// </summary>
        public object ReturnValue => _returnValue;

        public int SPID
        {
            get { return ConnectionObject(allowNull: true)?.SPID ?? -1; }
        }

        /// <summary>
        /// ����L�^�ǭȤ���Ʈw�s���ʧ@.
        /// </summary>
        /// <param name="Command">SQL Command �榡�Ʀr��. <see cref="String.Format(string, object[])"/> </param>
        /// <returns>�Ǧ^��Ʈw����Ʈw�s���ʧ@�v�T������.(�Ԩ��U��Ʈw�Ҵ��Ѥ� ADO.NET ����</returns>
        public int ExecuteNonQuery(string Command, object parameters = null)
        {
            ConnectCheck();
            return CommandExecuteNonQuery(new StringCommandBuilder(CommandType.Text, Command), parameters, ref _returnValue);
        }

        /// <summary>
        /// ����L�^�ǭȤ���Ʈw�s���ʧ@.
        /// </summary>
        /// <param name="Command">SQL Command �榡�Ʀr��. <see cref="String.Format(string, object[])"/> </param>
        /// <returns>�Ǧ^��Ʈw����Ʈw�s���ʧ@�v�T������.(�Ԩ��U��Ʈw�Ҵ��Ѥ� ADO.NET ����</returns>
        public async Task<int> ExecuteNonQueryAsync(string Command, object parameters = null)
        {
            ConnectCheck();
            var (rowsAffected, result) = await CommandExecuteNonQueryAsync(new StringCommandBuilder(CommandType.Text, Command), parameters);
            return rowsAffected;
        }

        /// <summary>
        /// ����L�^�ǭȤ���Ʈw�s���ʧ@.
        /// </summary>
        /// <param name="type">���w��Ʈw����R�����A</param>
        /// <param name="command">SQL Command �榡�Ʀr��. �榡�Ʀr�� ��k�ĥ� <c>String.Format</c> �ԲӽЬd�\ <see cref="String.Format(string, object[])"/> ���. </param>
        /// <returns>�Ǧ^��Ʈw����Ʈw�s���ʧ@�v�T������.(�Ԩ��U��Ʈw�Ҵ��Ѥ� ADO.NET ����</returns>
        public int ExecuteNonQuery(CommandType type, string command, object parameters = null)
        {
            ConnectCheck();
            return CommandExecuteNonQuery(new StringCommandBuilder(type, command), parameters, ref _returnValue);
        }

        /// <summary>
        /// ����L�^�ǭȤ���Ʈw�s���ʧ@.
        /// </summary>
        /// <param name="type">���w��Ʈw����R�����A</param>
        /// <param name="command">SQL Command �榡�Ʀr��. �榡�Ʀr�� ��k�ĥ� <c>String.Format</c> �ԲӽЬd�\ <see cref="String.Format(string, object[])"/> ���. </param>
        /// <returns>�Ǧ^��Ʈw����Ʈw�s���ʧ@�v�T������.(�Ԩ��U��Ʈw�Ҵ��Ѥ� ADO.NET ����</returns>
        public async Task<int> ExecuteNonQueryAsync(CommandType type, string Command, object parameters = null)
        {
            ConnectCheck();
            var (rowsAffected, result) = await CommandExecuteNonQueryAsync(new StringCommandBuilder(type, Command), parameters);
            return rowsAffected;
        }

        /// <summary>
        /// ����L�^�ǭȤ���Ʈw�s���ʧ@.
        /// </summary>
        /// <param name="builder">�ǤJ�@�ӥi�إ� SQL Command �r�ꪺ����</param>
        /// <returns>�Ǧ^��Ʈw����Ʈw�s���ʧ@�v�T������.(�Ԩ��U��Ʈw�Ҵ��Ѥ� ADO.NET ����</returns>
        public int ExecuteNonQuery(ICommandBuilder builder, object parameters = null)
        {
            ConnectCheck();
            return CommandExecuteNonQuery(builder, parameters, ref _returnValue);
        }
        /// <summary>
        /// ����L�^�ǭȤ���Ʈw�s���ʧ@.
        /// </summary>
        /// <param name="builder">�ǤJ�@�ӥi�إ� SQL Command �r�ꪺ����</param>
        /// <returns>�Ǧ^��Ʈw����Ʈw�s���ʧ@�v�T������.(�Ԩ��U��Ʈw�Ҵ��Ѥ� ADO.NET ����</returns>
        public async Task<int> ExecuteNonQueryAsync(ICommandBuilder builder, object parameters = null)
        {
            ConnectCheck();
            var (rowsAffected, result) = await CommandExecuteNonQueryAsync(builder, parameters);
            return rowsAffected;
        }

        /// <summary>
        /// ����^�ǭȳ�@�Ȥ���Ʈw�s���ʧ@.
        /// </summary>
        /// <typeparam name="T">�^�ǭȫ��O.</typeparam>
        /// <param name="command">SQL Command �榡�Ʀr��. �榡�Ʀr�� ��k�ĥ� <c>String.Format</c> �ԲӽЬd�\ <see cref="String.Format(string, object[])"/> ���. </param>
        /// <returns>�^�ǭ�</returns>
        /// <remarks>
        /// ExecuteScale �|�̦^�ǭȸյ��ഫ�ܦ^�ǭȫ��O. �p�G���ѷ|��X�ҥ~.
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
        /// ����^�ǭȳ�@�Ȥ���Ʈw�s���ʧ@.
        /// </summary>
        /// <typeparam name="T">�^�ǭȫ��O</typeparam>
        /// <param name="type">���w��Ʈw����R�����A</param>
        /// <param name="command">SQL Command �榡�Ʀr��. �榡�Ʀr�� ��k�ĥ� <c>String.Format</c> �ԲӽЬd�\ <see cref="String.Format(string, object[])"/> ���. </param>
        /// <returns>��Ʈw���^�ǭ�.</returns>
        public T ExecuteScalar<T>(CommandType type, string command, T defaultValue = default(T))
        {
            ConnectCheck();
            return CommandExecuteScalar<T>(new StringCommandBuilder(type, command), null, defaultValue);
        }

        /// <summary>
        /// ����^�ǭȳ�@�Ȥ���Ʈw�s���ʧ@.
        /// </summary>
        /// <typeparam name="T">�^�ǭȫ��O</typeparam>
        /// <param name="type">���w��Ʈw����R�����A</param>
        /// <param name="command">SQL Command �榡�Ʀr��. �榡�Ʀr�� ��k�ĥ� <c>String.Format</c> �ԲӽЬd�\ <see cref="String.Format(string, object[])"/> ���. </param>
        /// <returns>��Ʈw���^�ǭ�.</returns>
        public async Task<T> ExecuteScalarAsync<T>(CommandType type, string command, T defaultValue = default(T))
        {
            ConnectCheck();
            return await CommandExecuteScalarAsync<T>(new StringCommandBuilder(type, command), null, defaultValue);
        }

        /// <summary>
        /// ����^�ǭȳ�@�Ȥ���Ʈw�s���ʧ@.
        /// </summary>
        /// <typeparam name="T">�^�ǭȫ��O</typeparam>
        /// <param name="builder">�ǤJ�@�ӥi�إ� SQL Command �r�ꪺ����</param>
        /// <returns>��Ʈw���^�ǭ�</returns>
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
        /// ����^�ǭ� DataTable ����Ʈw�s���ʧ@.
        /// </summary>
        /// <param name="builder">�ǤJ�@�ӥi�إ� SQL Command �r�ꪺ����</param>
        /// <param name="BuildOptions"></param>
        /// <returns>��Ʈw���^�ǭ�</returns>
        public DataTable OpenDataTable(ICommandBuilder builder, CommandBuildOptions BuildOptions = CommandBuildOptions.None)
        {
            ConnectCheck();
            return CommandOpenDataTable(builder, BuildOptions);
        }

        /// <summary>
        /// ����^�ǭ� DataTable ����Ʈw�s���ʧ@.
        /// </summary>
        /// <param name="command">SQL Command �榡�Ʀr��. �榡�Ʀr�� ��k�ĥ� <c>String.Format</c> �ԲӽЬd�\ <see cref="String.Format(string, object[])"/> ���. </param>
        /// <returns>��Ʈw���^�ǭ�</returns>
        public DataTable OpenDataTable(string command)
        {
            ConnectCheck();
            return CommandOpenDataTable(new StringCommandBuilder(CommandType.Text, command), CommandBuildOptions.None);
        }

        /// <summary>
        /// ����^�ǭ� DataTable ����Ʈw�s���ʧ@.
        /// </summary>
        /// <param name="type">���w��Ʈw����R�����A</param>
        /// <param name="command">SQL Command �榡�Ʀr��. �榡�Ʀr�� ��k�ĥ� <c>String.Format</c> �ԲӽЬd�\ <see cref="String.Format(string, object[])"/> ���. </param>
        /// <returns>��Ʈw���^�ǭ�</returns>
        public DataTable OpenDataTable(CommandType type, string command)
        {
            ConnectCheck();
            return CommandOpenDataTable(new StringCommandBuilder(type, command), CommandBuildOptions.None);
        }

        /// <summary>
        /// ����^�ǭ� DataSet ����Ʈw�s���ʧ@.
        /// </summary>
        /// <param name="builder">�ǤJ�@�ӥi�إ� SQL Command �r�ꪺ����</param>
        /// <param name="buildOptions"></param>
        /// <returns>��Ʈw���^�ǭ�</returns>
        public DataSet OpenDataSet(ICommandBuilder builder, CommandBuildOptions buildOptions = CommandBuildOptions.None)
        {
            ConnectCheck();
            var ds = new DataSet();
            CommandFillDataSet(ref ds, builder, buildOptions);
            return ds;
        }

        /// <summary>
        /// ����^�ǭ� DataSet ����Ʈw�s���ʧ@.
        /// </summary>
        /// <param name="ds">��Ʈw���^�ǭ�</param>
        /// <param name="builder">�ǤJ�@�ӥi�إ� SQL Command �r�ꪺ����</param>
        /// <param name="BuildOptions"></param>
        public void OpenDataSet(DataSet ds, ICommandBuilder builder, CommandBuildOptions BuildOptions = CommandBuildOptions.None)
        {
            ConnectCheck();
            CommandFillDataSet(ref ds, builder, BuildOptions);
        }

        /// <summary>
        ///  ����^�ǭ� DataSet ����Ʈw�s���ʧ@.
        /// </summary>
        /// <param name="command">SQL Command �榡�Ʀr��. �榡�Ʀr�� ��k�ĥ� <c>String.Format</c> �ԲӽЬd�\ <see cref="String.Format(string, object[])"/> ���. </param>
        /// <returns>��Ʈw���^�ǭ�</returns>
        public DataSet OpenDataSet(string command)
        {
            var ds = new DataSet();
            ConnectCheck();
            CommandFillDataSet(ref ds, new StringCommandBuilder(CommandType.Text, command), CommandBuildOptions.None);
            return ds;
        }

        /// <summary>
        ///  ����^�ǭ� DataSet ����Ʈw�s���ʧ@.
        /// </summary>
        /// <param name="type">���w��Ʈw����R�����A</param>
        /// <param name="command">SQL Command �榡�Ʀr��. �榡�Ʀr�� ��k�ĥ� <c>String.Format</c> �ԲӽЬd�\ <see cref="String.Format(string, object[])"/> ���. </param>
        /// <returns>��Ʈw���^�ǭ�</returns>
        public DataSet OpenDataSet(CommandType type, string command)
        {
            var ds = new DataSet();
            ConnectCheck();
            CommandFillDataSet(ref ds, new StringCommandBuilder(type, command), CommandBuildOptions.None);
            return ds;
        }
        /// <summary>
        ///  ����^�ǭ� DataSet ����Ʈw�s���ʧ@.
        /// </summary>
        /// <param name="ds">��Ʈw���^�ǭ�</param>
        /// <param name="type">���w��Ʈw����R�����A</param>
        /// <param name="command">SQL Command �榡�Ʀr��. �榡�Ʀr�� ��k�ĥ� <c>String.Format</c> �ԲӽЬd�\ <see cref="String.Format(string, object[])"/> ���. </param>
        public void OpenDataSet(DataSet ds, CommandType type, string command)
        {
            ConnectCheck();
            CommandFillDataSet(ref ds, new StringCommandBuilder(type, command), CommandBuildOptions.None);
        }

        /// <summary>
        ///  ����^�ǭ� DataSet ����Ʈw�s���ʧ@.
        /// </summary>
        /// <param name="ds">��Ʈw���^�ǭ�</param>
        /// <param name="command">SQL Command �榡�Ʀr��. �榡�Ʀr�� ��k�ĥ� <c>String.Format</c> �ԲӽЬd�\ <see cref="String.Format(string, object[])"/> ���. </param>
        public void OpenDataSet(DataSet ds, string command)
        {
            ConnectCheck();
            CommandFillDataSet(ref ds, new StringCommandBuilder(CommandType.Text, command), CommandBuildOptions.None);
        }
        /// <summary>
        /// �H�~�ӥ��ݹ�@ CommandExecuteNonQuery �@�� ExecuteNonQuery ����L�^�ǭȤ���Ʈw�s���ʧ@
        /// </summary>
        /// <param name="builder">�ǤJ�@�ӥi�إ� SQL Command �r�ꪺ����</param>
        /// <param name="result">���o  ExecuteNonQuery ��Ʈw�^�Ǫ����e��.</param>
        /// <returns>�Ǧ^��Ʈw����Ʈw�s���ʧ@�v�T������.(�Ԩ��U��Ʈw�Ҵ��Ѥ� ADO.NET ����</returns>
        protected abstract int CommandExecuteNonQuery(ICommandBuilder builder, object parameters, ref object result);


        /// <summary>
        /// �H�~�ӥ��ݹ�@ CommandExecuteNonQuery �@�� ExecuteNonQuery ����L�^�ǭȤ���Ʈw�s���ʧ@
        /// </summary>
        /// <param name="builder">�ǤJ�@�ӥi�إ� SQL Command �r�ꪺ����</param>
        /// <param name="result">���o  ExecuteNonQuery ��Ʈw�^�Ǫ����e��.</param>
        /// <returns>�Ǧ^��Ʈw����Ʈw�s���ʧ@�v�T������.(�Ԩ��U��Ʈw�Ҵ��Ѥ� ADO.NET ����</returns>
        protected abstract Task<(int, object)> CommandExecuteNonQueryAsync(ICommandBuilder builder, object parameters);

        /// <summary>
        /// �H�~�ӥ��ݹ�@ CommandExecuteScale �@�� ExecuteScale ����^�ǭȳ�@�Ȥ���Ʈw�s���ʧ@
        /// </summary>
        /// <typeparam name="T">�^�ǭȫ��A</typeparam>
        /// <param name="builder">�ǤJ�@�ӥi�إ� SQL Command �r�ꪺ����</param>
        /// <param name="defaultValue">defValue</param>
        /// <returns>�^�ǭ�</returns>

        protected abstract T CommandExecuteScalar<T>(ICommandBuilder builder, object parameters, T defaultValue);
        protected abstract Task<T> CommandExecuteScalarAsync<T>(ICommandBuilder builder, object parameters, T defaultValue);

        /// <summary>
        /// �H�~�ӥ��ݹ�@ CommandExecuteDataTable �@�� OpenDataTable ����^�ǭ� DataTable ����Ʈw�s���ʧ@
        /// </summary>
        /// <param name="builder">�ǤJ�@�ӥi�إ� SQL Command �r�ꪺ����</param>
        /// <param name="buildFlags"></param>
        /// <returns>�^�ǭ�</returns>
        protected abstract DataTable CommandOpenDataTable(ICommandBuilder builder, CommandBuildOptions buildFlags);

        /// <summary>
        /// �H�~�ӥ��ݹ�@ CommandExecuteDataSet �@�� OpenDataSet ����^�ǭ� DataSet ����Ʈw�s���ʧ@
        /// </summary>
        /// <param name="ds"></param>
        /// <param name="builder"></param>
        /// <param name="buildFlags"></param>
        protected abstract void CommandFillDataSet(ref DataSet ds, ICommandBuilder builder, CommandBuildOptions buildFlags);


    }
}