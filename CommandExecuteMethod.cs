// ------------------------------------------------------------------------
// Chiats Common&Data Library V4.1.21 (2021/08)
// Chiats@Studio(http://www.chiats.com/Common)
// Design&Coding By Chia Tsang Tsai
//Copyright(C) 2005-2022 Chiats@Studio All 
// ------------------------------------------------------------------------

using Chiats.SQL;
using System;
using System.Data;
using System.Data.Common;

namespace Chiats.Data
{
    public enum CommandExecuteMethod
    {
        OpenReader, OpenDataTable, ExecuteNonQuery, ExecuteScalar
    }

    public class CommandExecutingEventArgs : EventArgs
    {
        public string Name { get; set; }
        public CommandExecuteMethod Method { get; set; }
        public string CommandText { get; set; }
        public DbParameterCollection Parameters { get; set; }
        public int ExecuteOrder { get; set; }
        public Guid ConnectionId { get; set; }
        public string TemplateId { get; set; }
        public int SPID { get; set; }
        public int ThreadId { get; set; }
    }


    public class CommandBuildingEventArgs : EventArgs
    {
        public string Name { get; set; }
        public CommandType CommandType { get; set; }
        public string CommandText { get; set; }
        public IParameterCommandBuilder ParameterBuilder { get; set; }
    }

    public class CommandExecutedEventArgs : EventArgs
    {
        public string Name { get; set; }
        public string CommandText { get; set; }
        public int ExecuteOrder { get; set; }
        public int RowsAffected { get; set; }
        /// <summary>
        /// Execute Time  TotalMilliseconds 
        /// </summary>
        public double ExecuteTime { get; set; }
        public Guid ConnectionId { get; set; }
        public string TemplateId { get; set; }

        public DbParameterCollection Parameters { get; set; }
        public int SPID { get; set; }
        public int ThreadId { get; set; }
    }

    public class CommandTransactionEventArgs : EventArgs
    {
        public string Name { get; set; }
        public string Transaction { get; set; }
        public string TemplateId { get; set; }

        public int SPID { get; set; }
        public int ThreadId { get; set; }
    }

    public class CommandExecuteExceptionEventArgs : EventArgs
    {
        public string Name { get; set; }
        public Guid ConnectionId { get; set; }
        public string TemplateId { get; set; }
        public int ExecuteOrder { get; set; }
        public Exception Exception { get; set; }

        public string CommandText { get; set; }

        public DbParameterCollection Parameters { get; set; }
        public int SPID { get; set; }
    }
}
