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
    /// �зǪ��榡�Ʀr��,�ӫإ� SQL Command ��k�ĥ� <c>String.Format</c> �ԲӽЬd�\ <see cref="String.Format(string, object)"/> ���.
    /// </summary>
    public class StringCommandBuilder : ICommandBuilder
    {
        private string sql;
        private CommandType type;

        /// <summary>
        /// �غc�l.
        /// </summary>
        /// <param name="type">SQL Command ������R�O���A</param>
        /// <param name="Command">SQL Command </param>
        public StringCommandBuilder(CommandType type, string Command)
        {
            sql = Command;
            this.type = type;
        }

        /// <summary>
        /// �غc�l.
        /// </summary>
        /// <param name="fmt">SQL Command </param>
        public StringCommandBuilder(string Command)
        {
            sql = Command;
            this.type = CommandType.Text;
        }

        #region ICommandBuilder Members

        /// <summary>
        /// �Ǧ^ Database SQL Command ������R�O.
        /// </summary>
        /// <returns>SQL Command ������R�O�r��</returns>
        public string CommandText
        {
            get { return sql; }
        }

        /// <summary>
        /// �Ǧ^ Database SQL Command �����A. �p�з� SQL Command , Table , StoredProcedure
        /// </summary>
        /// <returns>SQL Command ������R�O���A</returns>
        public CommandType CommandType
        {
            get { return type; }
        }

        #endregion ICommandBuilder Members
    }
}