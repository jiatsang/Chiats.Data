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
    /// 管理 System.Transactions.TransactionScope 物件類別. <br/>
    /// 允許巢狀的使用 TransScope <br/>
    /// Transaction(Scope) 會自動在每一個 Threading 建立一組 TransactionManager 物件來管理 System.Transactions.TransactionScope 物件類別
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
        /// 確保連線物件和 Thread 關連.
        /// /// </summary>
        [ThreadStatic()]
        private static TransactionManager current;

        /// <summary>
        /// 只允許由 TransactionManager 自己建立物件
        /// </summary>
        private TransactionManager()
        {
        }

        /// <summary>
        /// 傳回目前作用中的 TransactionManager
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
        /// 取得依附在  DACTemplate 物件下的 ConnectionDataPack
        /// </summary>
        /// <param name="template">傳入所要依附 DACTemplate 物件.</param>
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
                    // 表示 template 是剛建立的, 
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

                // 因為不是所有的程式都需要取得 ConnectionManagement Object.   
                if (!allowNull)
                {
                    // 建構 ConnectionManagement 的同時會直接開啟 Connection 連線.
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
        /// 關閉所有的關連的 Connection Object.
        /// </summary>
        /// <param name="IsComplate">指示非 </param>
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