// ------------------------------------------------------------------------
// Chiats Common&Data Library V4.1.21 (2021/08)
// Chiats@Studio(http://www.chiats.com/Common)
// Design&Coding By Chia Tsang Tsai
//Copyright(C) 2005-2022 Chiats@Studio All 
// ------------------------------------------------------------------------
using Chiats.SQL;
using System;

namespace Chiats.Data
{
    public class CommandBuilderEventArgs : EventArgs
    {
        public ICommandBuilder CommandBuilder { get; set; }

        public CommandBuildOptions Options { get; set; }
    }
}