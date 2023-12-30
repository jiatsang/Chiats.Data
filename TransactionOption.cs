// ------------------------------------------------------------------------
// Chiats Common&Data Library V4.1.21 (2021/08)
// Chiats@Studio(http://www.chiats.com/Common)
// Design&Coding By Chia Tsang Tsai
//Copyright(C) 2005-2022 Chiats@Studio All 
// ------------------------------------------------------------------------

namespace Chiats.Data
{
    /// <summary>
    /// 資料庫的 Transaction 模式.
    /// </summary>
    public enum TransactionOption
    {
        /// <summary>
        ///
        /// </summary>
        None,

        /// <summary>
        /// Shares a transaction, if one exists, and creates a new transaction if necessary.
        /// </summary>
        Required,

        /// <summary>
        /// Creates the component with a new transaction, regardless of the state of the current context.
        /// </summary>
        RequiresNew
    }
}