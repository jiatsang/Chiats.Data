// ------------------------------------------------------------------------
// Chiats Common&Data Library V4.1.21 (2021/08)
// Chiats@Studio(http://www.chiats.com/Common)
// Design&Coding By Chia Tsang Tsai
//Copyright(C) 2005-2022 Chiats@Studio All 
// ------------------------------------------------------------------------

using Chiats.SQL;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace Chiats.Data
{

    public struct DbMessage
    {
        public string Message;
        public override string ToString()
        {
            return Message;
        }
    }
    /// <summary>
    /// 支援資料存取的基礎介面
    /// </summary>
    public interface IDacTemplate : IDisposable 
    {
        List<DbMessage> Messages { get; } 
        /// <summary>
        /// 取得目前資料庫的交易(Transaction)模型.
        /// </summary>
        TransactionMode TransactionMode { get; }

        /// <summary>
        /// 取得目前資料庫的手動之交易模型介面. 此為在 TransactionMode 為 Manual 時有效.
        /// </summary>
        /// <returns></returns>
        IManualTransaction Transaction { get; }

        /// <summary>
        /// 取得目前資料庫最後的執行結果.
        /// </summary>
        object ReturnValue { get; }

        int SPID { get; }

        /// <summary>
        /// 執行一道 SQL 指令, 並回傳單一值(執行結果)
        /// </summary>
        /// <typeparam name="T">回傳值型別</typeparam>
        /// <param name="builder">SQL 指令</param>
        ///  <param name="defaultValue">defaultValue</param>
        /// <returns>回傳的單一值(執行結果)</returns>
        T ExecuteScalar<T>(ICommandBuilder builder, T defaultValue = default(T));

        T ExecuteScalar<T>(ICommandBuilder builder, object parameters, T defaultValue = default(T));

        /// <summary>
        /// 執行一道 SQL 指令, 並回傳單一值(執行結果)
        /// </summary>
        /// <typeparam name="T">回傳值型別</typeparam>
        /// <param name="builder">SQL 指令</param>
        ///  <param name="defaultValue">defaultValue</param>
        /// <returns>回傳的單一值(執行結果)</returns>
        Task<T> ExecuteScalarAsync<T>(ICommandBuilder builder, T defaultValue = default(T));

        Task<T> ExecuteScalarAsync<T>(ICommandBuilder builder, object parameters, T defaultValue = default(T));

        /// <summary>
        /// 執行一道 SQL 指令, 並回傳單一值(執行結果)
        /// </summary>
        /// <typeparam name="T">回傳值型別</typeparam>
        /// <param name="command">SQL 指令</param>
        ///  <param name="defaultValue">defaultValue</param>
        /// <returns>回傳的單一值(執行結果</returns>
        T ExecuteScalar<T>(string command, T defaultValue = default(T));

        T ExecuteScalar<T>(string command, object parameters, T defaultValue = default(T));

        Task<T> ExecuteScalarAsync<T>(string command, T defaultValue = default(T));

        Task<T> ExecuteScalarAsync<T>(string command, object parameters, T defaultValue = default(T));

        /// <summary>
        /// 執行一道 SQL 指令, 並回傳單一值(執行結果)
        /// </summary>
        /// <typeparam name="T">回傳值型別</typeparam>
        /// <param name="type">SQL 指令型別</param>
        /// <param name="command">SQL 指令</param>
        /// <param name="defaultValue">defaultValue</param>
        /// <returns></returns>
        T ExecuteScalar<T>(CommandType type, string command, T defaultValue = default(T));

        /// <summary>
        /// 執行一道 SQL 指令, 並回傳單一值(執行結果)
        /// </summary>
        /// <typeparam name="T">回傳值型別</typeparam>
        /// <param name="type">SQL 指令型別</param>
        /// <param name="command">SQL 指令</param>
        /// <param name="defaultValue">defaultValue</param>
        /// <returns></returns>
        Task<T> ExecuteScalarAsync<T>(CommandType type, string command, T defaultValue = default(T));


        /// <summary>
        /// 執行一道 SQL 指令, 並回傳 DataTable 表格物件(執行結果)
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="buildOptions"></param>
        /// <returns></returns>
        DataTable OpenDataTable(ICommandBuilder builder, CommandBuildOptions buildOptions = CommandBuildOptions.None);

        /// <summary>
        /// 執行一道 SQL 指令, 並回傳 DataTable 表格物件(執行結果)
        /// </summary>
        /// <param name="command">SQL 指令</param>
        /// <returns></returns>
        DataTable OpenDataTable(string command);

        /// <summary>
        /// 執行一道 SQL 指令, 並回傳 DataTable 表格物件(執行結果)
        /// </summary>
        /// <param name="type">SQL 指令型別</param>
        /// <param name="command">SQL 指令</param>
        /// <returns></returns>
        DataTable OpenDataTable(CommandType type, string command);

        /// <summary>
        /// 執行一道 SQL 指令, 無回傳值
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        int ExecuteNonQuery(ICommandBuilder builder, object parameters = null);

        Task<int> ExecuteNonQueryAsync(ICommandBuilder builder,object parameters = null);

        /// <summary>
        /// 執行一道 SQL 指令, 無回傳值
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        int ExecuteNonQuery(string command, object parameters = null);

        Task<int> ExecuteNonQueryAsync(string command, object parameters = null);

        /// <summary>
        /// 執行一道 SQL 指令, 無回傳值
        /// </summary>
        /// <param name="type"></param>
        /// <param name="command"></param>
        /// <returns></returns>
        int ExecuteNonQuery(CommandType type, string command, object parameters = null);

        Task<int> ExecuteNonQueryAsync(CommandType type, string command, object parameters = null);

        /// <summary>
        /// 執行一道 SQL 指令, 並回傳 DataSet 表格物件(執行結果)
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="buildOptions"></param>
        /// <returns></returns>
        DataSet OpenDataSet(ICommandBuilder builder, CommandBuildOptions buildOptions = CommandBuildOptions.None);

        /// <summary>
        /// 執行一道 SQL 指令, 並回傳 DataSet 表格物件(執行結果)
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        DataSet OpenDataSet(string command);

        /// <summary>
        /// 執行一道 SQL 指令, 並回傳 DataSet 表格物件(執行結果)
        /// </summary>
        /// <param name="type"></param>
        /// <param name="command"></param>
        /// <returns></returns>
        DataSet OpenDataSet(CommandType type, string command);

        /// <summary>
        /// 執行一道 SQL 指令, 並回傳 DataSet 表格物件(執行結果)
        /// </summary>
        /// <param name="ds"></param>
        /// <param name="builder"></param>
        /// <param name="buildOptions"></param>
        void OpenDataSet(DataSet ds, ICommandBuilder builder, CommandBuildOptions buildOptions = CommandBuildOptions.None);

        /// <summary>
        /// 執行一道 SQL 指令, 並回傳 DataSet 表格物件(執行結果)
        /// </summary>
        /// <param name="ds"></param>
        /// <param name="type"></param>
        /// <param name="command"></param>
        void OpenDataSet(DataSet ds, CommandType type, string command);

        /// <summary>
        /// 執行一道 SQL 指令, 並回傳 DataSet 表格物件(執行結果)
        /// </summary>
        /// <param name="ds"></param>
        /// <param name="command"></param>
        void OpenDataSet(DataSet ds, string command);
    }
}