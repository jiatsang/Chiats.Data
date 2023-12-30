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
    [Serializable]
    public class InvalidColumnNameException : CommonException
    {
        protected InvalidColumnNameException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        /// <summary>
        /// CommonException 建構子
        /// </summary>
        /// <param name="message">字串訊息內容</param>
        public InvalidColumnNameException(string ColumnNames) : base(string.Format("Invalid Column Name {0}", ColumnNames)) { }
    }
}