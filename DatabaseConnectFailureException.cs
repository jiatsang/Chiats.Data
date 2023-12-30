// ------------------------------------------------------------------------
// Chiats Common&Data Library V4.1.21 (2021/08)
// Chiats@Studio(http://www.chiats.com/Common)
// Design&Coding By Chia Tsang Tsai
//Copyright(C) 2005-2022 Chiats@Studio All 
// ------------------------------------------------------------------------

using System;
using System.Runtime.Serialization;

namespace Chiats.Data
{
    /// <summary>
    /// 資料庫連線異常.
    /// </summary>
    /// <remarks>
    /// 顯示訊息為 "資料庫連結失敗，請查詢詳細訊息或連絡系統管理員。" 更多資訊請查詢詳資料清單.
    /// </remarks>
    [Serializable]
    public class DatabaseConnectFailureException : CommonException
    {
        public DatabaseConnectFailureException(SerializationInfo info, StreamingContext context) :
            base(info, context)
        { }

        /// <summary>
        /// 資料庫連線異常 建構子
        /// </summary>
        /// <param name="message">資料庫連線異常額外訊息</param>
        public DatabaseConnectFailureException(string message)
            : base("資料庫連線異常({0})，請查詢詳細訊息或連絡系統管理員。", message)
        {
        }

        /// <summary>
        /// 資料庫連線異常 建構子
        /// </summary>
        /// <param name="innerException">傳入原始引發例外物件.</param>
        public DatabaseConnectFailureException(Exception innerException)
            : base(innerException, "資料庫連線異常，請查詢詳細訊息或連絡系統管理員。")
        {
        }

        /// <summary>
        /// 資料庫連線異常 建構子
        /// </summary>
        /// <param name="message">更多資訊</param>
        /// <param name="innerException">傳入原始引發例外物件.</param>
        public DatabaseConnectFailureException(string message, Exception innerException)
            : base(innerException, "{0}，請查詢詳細訊息或連絡系統管理員。", message)
        {
            this.MoreMessage = message;
        }
    }

    /// <summary>
    /// 資料庫連線異常.
    /// </summary>
    /// <remarks>
    /// 顯示訊息為 "資料庫連結失敗，請查詢詳細訊息或連絡系統管理員。" 更多資訊請查詢詳資料清單.
    /// </remarks>
    [Serializable]
    public class DatabaseExecuteFailureException : CommonException
    {
        public DatabaseExecuteFailureException(SerializationInfo info, StreamingContext context) :
            base(info, context)
        { }

        /// <summary>
        /// 資料庫執行異常 建構子
        /// </summary>
        /// <param name="message">更多資訊</param>
        /// <param name="innerException">傳入原始引發例外物件.</param>
        public DatabaseExecuteFailureException(string message, Exception innerException)
            : base(innerException, "資料庫執行異常({0})，請查詢詳細訊息或連絡系統管理員。", message)
        {
            this.MoreMessage = innerException.Message;
        }

        /// <summary>
        /// 資料庫執行異常 建構子
        /// </summary>
        /// <param name="innerException">異常物件</param>
        public DatabaseExecuteFailureException(Exception innerException)
            : base(innerException, "資料庫執行異常({0})，請查詢詳細訊息或連絡系統管理員。", innerException.Message)
        {
            this.MoreMessage = "";
        }
    }
}