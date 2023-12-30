// ------------------------------------------------------------------------
// Chiats Common&Data Library V4.1.21 (2021/08)
// Chiats@Studio(http://www.chiats.com/Common)
// Design&Coding By Chia Tsang Tsai
//Copyright(C) 2005-2022 Chiats@Studio All 
// ------------------------------------------------------------------------

namespace Chiats.Data
{
    /// <summary>
    /// 管理 System.Transactions.TransactionScope 物件類別.
    /// </summary>
    public interface ITransaction
    {
        /// <summary>
        /// 指示 Transaction 失敗. 並在 TransactionScope 結束時. 才會進行 Rollback Transaction.
        /// </summary>
        void Abort();

        /// <summary>
        /// 傳回 Transaction 被 Abort 次數. 未曾 Abort 時 回傳值為 0
        /// </summary>
        int AbortCount { get; }

        /// <summary>
        /// 指示 Transaction 失敗. 並在 TransactionScope 結束時. 才會進行 Rollback Transaction.
        /// </summary>
        /// <param name="autoThrowException"></param>
        void Abort(bool autoThrowException);
    }
}