// ------------------------------------------------------------------------
// Chiats Common&Data Library V4.1.21 (2021/08)
// Chiats@Studio(http://www.chiats.com/Common)
// Design&Coding By Chia Tsang Tsai
//Copyright(C) 2005-2022 Chiats@Studio All 
// ------------------------------------------------------------------------

namespace Chiats.Data
{
    /// <summary>
    /// IManualTransaction 是用於程式自行決定 Transaction 方法.  由 IManualTransaction 自行管理 Begin/Commit/Rollback Transaction 使用.
    /// </summary>
    public interface IManualTransaction
    {
        /// <summary>
        /// Begin Transaction
        /// </summary>
        void BeginTrans();

        /// <summary>
        /// Commit Transaction
        /// </summary>
        void Commit();

        /// <summary>
        /// Rollback Transaction
        /// </summary>
        void Rollback();
    }
}