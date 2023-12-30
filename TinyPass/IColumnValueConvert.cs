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
    /// 定義 Pass Column 的欄位值轉換類別.
    /// </summary>
    public interface IColumnValueConvert
    {
        void Initiailize(object arg1, object arg2);
        object GetValue(string columnName, object value, Func<string, object> GetColumnValue);
    }
}