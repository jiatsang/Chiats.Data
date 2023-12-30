// ------------------------------------------------------------------------
// Chiats Common&Data Library V4.1.21 (2021/08)
// Chiats@Studio(http://www.chiats.com/Common)
// Design&Coding By Chia Tsang Tsai
//Copyright(C) 2005-2022 Chiats@Studio All 
// ------------------------------------------------------------------------

namespace Chiats.Data
{
    /// <summary>
    /// 管理資料庫連結的介面
    /// </summary>
    public interface IConnectionManagement
    {
        /// <summary>
        /// 開啟連線, 每一個資料庫實際連線的時機會由 DACTemplate TransManager and  TransScope 決定.
        /// 當需要連線會通知 ConnectionDataPack.ConnectionOpen(). 因此 ConnectionOpen 必須在被呼叫時
        /// 建立實際的連線.
        /// </summary>
        void ConnectionOpen(string connectionString);

        /// <summary>
        /// 通知 Connection Close(); 當 DACTemplate 的 InculdeTransaction 為 False 時且 在 TransScope
        /// 範圍結束前 CommitTrans(由 TransScope.Complate() 引發) 內未能執行. 則需要在  close() 時執行實際 Rollback
        /// </summary>
        void Dispose();

        /// <summary>
        /// 當 DACTemplate 的 InculdeTransaction 為 False 時 , TransManager 會呼叫 BeginTrans 和 CommitTrans 作
        /// 為管理 Transaction 的開始和結束. 如果需要 Rollback 時機則在 Close() 會被呼叫時.
        /// </summary>
        void BeginTrans();

        /// <summary>
        /// 當 DACTemplate 的 InculdeTransaction 為 False 時 , TransManager 會呼叫 BeginTrans 和 CommitTrans 作
        /// 為管理 Transaction 的開始和結束. 如果需要 Rollback 時機則在 Close() 會被呼叫時.
        /// </summary>
        void Commit();

        /// <summary>
        /// 是否啟用一個 Microsoft Distributed Transaction Coordinator 資料庫交易. 使用 Connection 的 Rollback/Commit.
        /// </summary>
        TransactionMode TransactionMode { get; }
    }
}