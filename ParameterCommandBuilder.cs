// ------------------------------------------------------------------------
// Chiats Common&Data Library V4.1.21 (2021/08)
// Chiats@Studio(http://www.chiats.com/Common)
// Design&Coding By Chia Tsang Tsai
//Copyright(C) 2005-2022 Chiats@Studio All 
// ------------------------------------------------------------------------

using Chiats.SQL;

namespace Chiats.Data
{
    /// <summary>
    /// 建立含有 Parameter SQL Command 呼叫程序. 此類別是撰寫資料庫連線類別使用. 不是直接的類別.
    /// </summary>
    /// <remarks>
    /// ParameterCommandBuilder 允許你使用 Parameter 來建立 SQL ?述.
    /// </remarks>
    public abstract class ParameterCommandBuilder : ICommandBuilder, IParameterCommandBuilder
    {
        /// <summary>
        /// ParameterCommandBuilder 建構子
        /// </summary>
        public readonly NamedCollection<Parameter> Parameters = new NamedCollection<Parameter>();

        /// <summary>
        /// 建構子
        /// </summary>
        /// <param name="command">SQL Command</param>
        protected ParameterCommandBuilder(string command)
            : this(System.Data.CommandType.Text, command) { }

        /// <summary>
        /// 建構子
        /// </summary>
        /// <param name="commandType">指定資料庫執行命的型態 <see cref="System.Data.CommandType"/></param>
        /// <param name="command">SQL Command 格式化字串. 格式化字串 方法採用 <c>String.Format</c> 詳細請查閱 <see cref="String.Format(string, object[])"/> 文件. </param>
        protected ParameterCommandBuilder(System.Data.CommandType commandType, string command)
        {
            this.CommandText = command;
            this.CommandType = commandType;
        }

        #region ICommandBuilder Members

        /// <summary>
        /// 傳回 Database SQL Command 的執行命令.
        /// </summary>
        /// <returns>SQL Command 的執行命令字串</returns>
        public string CommandText { get; private set; }

        /// <summary>
        /// 傳回 Database SQL Command 的型態. 如標準 SQL Command , Table , StoredProcedure
        /// </summary>
        /// <returns>SQL Command 的執行命令型態</returns>
        public System.Data.CommandType CommandType { get; private set; }

        #endregion ICommandBuilder Members

        #region IParameterCommandBuilder Members

        bool IParameterCommandBuilder.ParameterEnabled => (Parameters.Count > 0);

        Parameter[] IParameterCommandBuilder.Parameters => Parameters.ToArray();

        #endregion IParameterCommandBuilder Members
    }
}