// ------------------------------------------------------------------------
// Chiats Common&Data Library V4.1.21 (2021/08)
// Chiats@Studio(http://www.chiats.com/Common)
// Design&Coding By Chia Tsang Tsai
//Copyright(C) 2005-2022 Chiats@Studio All 
// ------------------------------------------------------------------------
using System;

namespace Chiats.Data
{
    /// <summary>
    /// 資料庫存取. 找不到指定參數異常
    /// </summary>
    /// <remarks>
    /// 顯示訊息為 "資料庫存取失敗，請查詢詳細訊息或連絡系統管理員。" 更多資訊請查詢詳資料清單.
    /// </remarks>
    [Serializable]
    public class ParamNotFoundException : CommonException
    {
        /// <summary>
        /// 找不到指定參數異常 建構子
        /// </summary>
        /// <param name="name">參數名稱</param>
        public ParamNotFoundException(string name)
            : base("資料庫存取失敗，請查詢詳細訊息或連絡系統管理員。")
        {
            this.MoreMessage = $"找不到指定參數名稱 {name} ";
        }
    }
}