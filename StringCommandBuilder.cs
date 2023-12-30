// ------------------------------------------------------------------------
// Chiats Common&Data Library V4.1.21 (2021/08)
// Chiats@Studio(http://www.chiats.com/Common)
// Design&Coding By Chia Tsang Tsai
//Copyright(C) 2005-2022 Chiats@Studio All 
// ------------------------------------------------------------------------

using Chiats.SQL;
using System.Data;

namespace Chiats.Data

{
    /// <summary>
    /// 標準的格式化字串,來建立 SQL Command 方法採用 <c>String.Format</c> 詳細請查閱 <see cref="String.Format(string, object)"/> 文件.
    /// </summary>
    public class StringCommandBuilder : ICommandBuilder
    {
        private string sql;
        private CommandType type;

        /// <summary>
        /// 建構子.
        /// </summary>
        /// <param name="type">SQL Command 的執行命令型態</param>
        /// <param name="Command">SQL Command </param>
        public StringCommandBuilder(CommandType type, string Command)
        {
            sql = Command;
            this.type = type;
        }

        /// <summary>
        /// 建構子.
        /// </summary>
        /// <param name="fmt">SQL Command </param>
        public StringCommandBuilder(string Command)
        {
            sql = Command;
            this.type = CommandType.Text;
        }

        #region ICommandBuilder Members

        /// <summary>
        /// 傳回 Database SQL Command 的執行命令.
        /// </summary>
        /// <returns>SQL Command 的執行命令字串</returns>
        public string CommandText
        {
            get { return sql; }
        }

        /// <summary>
        /// 傳回 Database SQL Command 的型態. 如標準 SQL Command , Table , StoredProcedure
        /// </summary>
        /// <returns>SQL Command 的執行命令型態</returns>
        public CommandType CommandType
        {
            get { return type; }
        }

        #endregion ICommandBuilder Members
    }
}