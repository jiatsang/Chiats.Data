// ------------------------------------------------------------------------
// Chiats Common&Data Library V4.1.21 (2021/08)
// Chiats@Studio(http://www.chiats.com/Common)
// Design&Coding By Chia Tsang Tsai
//Copyright(C) 2005-2022 Chiats@Studio All 
// ------------------------------------------------------------------------

namespace Chiats.Data
{
    /// <summary>
    /// 資料庫的交易(Transaction)模型. 可配合 DTC Transaction 模型
    /// </summary>
    public enum TransactionMode
    {
        /// <summary>
        /// Manual Transaction Call, 這個選項是要求 DACTemplate 在完全不要呼叫 BeginTrans, Commit/Rollback.
        ///  TODO: 實作在 TransactionScope 下時 Manual Transaction 可以正確運作
        /// </summary>
        Manual,

        /// <summary>
        /// 獨立 Transaction 設定, 這個選項是要求 DACTemplate 交由程式自行時直接呼叫 BeginTrans, Commit/Rollback
        /// </summary>
        /// <remarks>
        /// DACTemplate會在 Dispose 時依 TransactionScope 的狀態(IsAbort)  決定呼叫 BeginTrans, Commit/Rollback
        /// </remarks>
        Transaction,

        /// <summary>
        /// (預設值)支援 MSDTC(Microsoft Distributed Transaction Coordinator) Transaction 設定方式.
        /// 這個選項是要求 DACTemplate 在完全不用呼叫 BeginTrans, Commit/Rollback. Transaction 會交由 MSDTC 控管.
        /// </summary>
        DTCTransaction
    }
}