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
    /// ConnectionObjectTypeAttribute 指示關連的連線物件. 此類別是撰寫資料庫連線類別使用. 系統內部元件類別.
    /// </summary>
    public class ConnectionObjectTypeAttribute : Attribute
    {
        /// <summary>
        /// 連線物件型別
        /// </summary>
        public readonly Type ConnectionObjectType;

        /// <summary>
        /// 連線物件建構子
        /// </summary>
        /// <param name="type"></param>
        public ConnectionObjectTypeAttribute(Type type)
        {
            ConnectionObjectType = type;
        }
    }
}