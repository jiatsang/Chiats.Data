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
    /// 指示存取資料庫的 Transaction 模式.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class TransactionAttribute : Attribute
    {
        /// <summary>
        ///  Transaction 模式
        /// </summary>
        public readonly TransactionOption Option = TransactionOption.Required;

        /// <summary>
        /// 建構子
        /// </summary>
        /// <param name="option"></param>
        public TransactionAttribute(TransactionOption option)
        {
            this.Option = option;
        }
    }
}