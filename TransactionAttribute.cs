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
    /// ���ܦs����Ʈw�� Transaction �Ҧ�.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class TransactionAttribute : Attribute
    {
        /// <summary>
        ///  Transaction �Ҧ�
        /// </summary>
        public readonly TransactionOption Option = TransactionOption.Required;

        /// <summary>
        /// �غc�l
        /// </summary>
        /// <param name="option"></param>
        public TransactionAttribute(TransactionOption option)
        {
            this.Option = option;
        }
    }
}