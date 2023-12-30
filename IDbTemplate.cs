// ------------------------------------------------------------------------
// Chiats Common&Data Library V4.1.21 (2021/08)
// Chiats@Studio(http://www.chiats.com/Common)
// Design&Coding By Chia Tsang Tsai
//Copyright(C) 2005-2022 Chiats@Studio All 
// ------------------------------------------------------------------------

using Chiats.SQL;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;

namespace Chiats.Data
{
    public interface IDbTemplate : IDacTemplate
    {
        /// <summary>
        /// 執行一道 SQL 指令, 並回傳 DbDataReader 物件(執行結果)
        /// </summary>
        /// <param name="type"></param>
        /// <param name="command"></param>
        /// <returns></returns>
        DbDataReader OpenReader(string command, object parameters = null, CommandBuildOptions buildFlags = CommandBuildOptions.None, CommandType type = CommandType.Text);

        /// <summary>
        ///  執行一道 SQL 指令, 並回傳 DbDataReader 物件(執行結果)
        /// </summary>
        /// <param name="command"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        Task<DbDataReader> OpenReaderAsync(string command, object parameters = null, CommandBuildOptions buildFlags = CommandBuildOptions.None, CommandType type = CommandType.Text);

        /// <summary>
        /// 執行一道 SQL 指令, 並回傳 DbDataReader 物件(執行結果)
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="buildFlags"></param>
        /// <returns></returns>
        DbDataReader OpenReader(ICommandBuilder builder, object parameters = null, CommandBuildOptions buildFlags = CommandBuildOptions.None);

        /// <summary>
        /// 執行一道 SQL 指令, 並回傳 DbDataReader 物件(執行結果)
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="buildFlags"></param>
        /// <returns></returns>
        Task<DbDataReader> OpenReaderAsync(ICommandBuilder builder, object parameters = null, CommandBuildOptions buildFlags = CommandBuildOptions.None);

        void Abort(string AbortReson = null,
            [System.Runtime.CompilerServices.CallerLineNumber] int LineNumber = 0,
            [System.Runtime.CompilerServices.CallerFilePath] string SourceFilePath = "",
            [System.Runtime.CompilerServices.CallerMemberName] string memberName = "");
    }
}