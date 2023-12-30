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
    ///
    /// </summary>
    [Serializable]
    public class TranstionException : CommonException
    {
        /// <summary>
        /// TranstionAbortException «Øºc¤l.
        /// </summary>
        internal TranstionException(string Message)
            : base("Transtion Exception : {0}", Message)
        {
        }
    }
}