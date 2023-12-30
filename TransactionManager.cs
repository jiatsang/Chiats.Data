// ------------------------------------------------------------------------
// Chiats Common&Data Library V4.1.21 (2021/08)
// Chiats@Studio(http://www.chiats.com/Common)
// Design&Coding By Chia Tsang Tsai
//Copyright(C) 2005-2022 Chiats@Studio All 
// ------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Data.SqlClient;
using System.Web;
using System.Transactions;

namespace Chiats.Data
{
    /// <summary>
    /// �޲z System.Transactions.TransactionScope �������O. <br/>
    /// ���\�_�����ϥ� TransScope <br/>
    /// Transaction(Scope) �|�۰ʦb�C�@�� Threading �إߤ@�� TransactionManager ����Ӻ޲z System.Transactions.TransactionScope �������O
    /// </summary>
    /* public sealed */

    public sealed class TransactionManager
    {
        private static object object_lock = new object();

        private static int TransactionMaxID = 0;
        public readonly int TransactionID = Interlocked.Increment(ref TransactionMaxID);


        private Stack<TransactionScope> stack = new Stack<TransactionScope>();
        private Dictionary<string, IConnectionManagement> ConnectObjects = new Dictionary<string, IConnectionManagement>();
        public int ScopeCount { get { return stack.Count; } }
        
        /// <summary>
        /// �T�O�s�u����M Thread ���s.
        /// /// </summary>
        [ThreadStatic()]
        private static TransactionManager current;

        /// <summary>
        /// �u���\�� TransactionManager �ۤv�إߪ���
        /// </summary>
        private TransactionManager()
        {
        }

        /// <summary>
        /// �Ǧ^�ثe�@�Τ��� TransactionManager
        /// </summary>
        public static TransactionManager Current
        {
            get
            {
                lock (object_lock)
                {
                    if (current == null)
                    {
                        //Debug.Print($"\r\n#Check New TransactionManager {System.Threading.Thread.CurrentThread.ManagedThreadId}\r\n");
                        current = new TransactionManager();
                    }
                    return current;
                }
            }
        }

        /// <summary>
        /// ���o�̪��b  DACTemplate ����U�� ConnectionDataPack
        /// </summary>
        /// <param name="template">�ǤJ�ҭn�̪� DACTemplate ����.</param>
        /// <returns></returns>
        internal DacTemplate<TConnection, TTransaction, TCommand>.ConnectionManagement
            GetPackObject<TConnection, TTransaction, TCommand>(DacTemplate<TConnection, TTransaction, TCommand> template, bool allowNull = true, bool Initiailize = false)
        {
            lock (this)
            {
                Type ConnectionObjectType = template.GetConnectionObjectType();

                Debug.Assert(template != null);
                string template_key = $"{template.GetType().Name}.{template.Name}.{template.TransactionMode}";

                if (ConnectObjects.ContainsKey(template_key))
                {
                    var connectobject = (DacTemplate<TConnection, TTransaction, TCommand>.ConnectionManagement)ConnectObjects[template_key];
                    // ��� template �O��إߪ�, 
                    if (Initiailize)
                    {
                        template.RaiseCommandTransaction(template, new CommandTransactionEventArgs
                        {
                            Name = template.Name,
                            SPID = template.SPID,
                            ThreadId = System.Threading.Thread.CurrentThread.ManagedThreadId,
                            TemplateId = template.TemplateId,
                            Transaction = $"Connection.New(Inside:{TransactionManager.Current.ScopeCount}) ConnectionManagerID_{connectobject.ConnectionIndex}"
                        });
                        connectobject.ScopeAdd(template);
                    }
                    return connectobject;
                }

                // �]�����O�Ҧ����{�����ݭn���o ConnectionManagement Object.   
                if (!allowNull)
                {
                    // �غc ConnectionManagement ���P�ɷ|�����}�� Connection �s�u.
                    var connectobject =
                        (DacTemplate<TConnection, TTransaction, TCommand>.ConnectionManagement)
                        Activator.CreateInstance(ConnectionObjectType, template);

                    ConnectObjects.Add(template_key, connectobject);

                    template.RaiseCommandTransaction(template, new CommandTransactionEventArgs
                    {
                        Name = template.Name,
                        SPID = template.SPID,
                        ThreadId = System.Threading.Thread.CurrentThread.ManagedThreadId,
                        TemplateId = template.TemplateId,
                        Transaction = $"Connection.New(Outside:{TransactionManager.Current.ScopeCount}) ConnectionManagerID_{connectobject.ConnectionIndex}"
                    });
                    return connectobject;
                }
                return null;
            }
        }


        /// <summary>
        /// �����Ҧ������s�� Connection Object.
        /// </summary>
        /// <param name="IsComplate">���ܫD </param>
        public void CloseConnection<TConnection, TTransaction, TCommand>(DacTemplate<TConnection, TTransaction, TCommand> template)
        {
            lock (this)
            {
                if (template != null)
                {
                    string template_key = $"{template.GetType().Name}.{template.Name}.{template.TransactionMode}";
                    ConnectObjects.Remove(template_key);
                }
            }
        }
    }
}