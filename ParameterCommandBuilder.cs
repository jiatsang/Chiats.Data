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
    /// �إߧt�� Parameter SQL Command �I�s�{��. �����O�O���g��Ʈw�s�u���O�ϥ�. ���O���������O.
    /// </summary>
    /// <remarks>
    /// ParameterCommandBuilder ���\�A�ϥ� Parameter �ӫإ� SQL ?�z.
    /// </remarks>
    public abstract class ParameterCommandBuilder : ICommandBuilder, IParameterCommandBuilder
    {
        /// <summary>
        /// ParameterCommandBuilder �غc�l
        /// </summary>
        public readonly NamedCollection<Parameter> Parameters = new NamedCollection<Parameter>();

        /// <summary>
        /// �غc�l
        /// </summary>
        /// <param name="command">SQL Command</param>
        protected ParameterCommandBuilder(string command)
            : this(System.Data.CommandType.Text, command) { }

        /// <summary>
        /// �غc�l
        /// </summary>
        /// <param name="commandType">���w��Ʈw����R�����A <see cref="System.Data.CommandType"/></param>
        /// <param name="command">SQL Command �榡�Ʀr��. �榡�Ʀr�� ��k�ĥ� <c>String.Format</c> �ԲӽЬd�\ <see cref="String.Format(string, object[])"/> ���. </param>
        protected ParameterCommandBuilder(System.Data.CommandType commandType, string command)
        {
            this.CommandText = command;
            this.CommandType = commandType;
        }

        #region ICommandBuilder Members

        /// <summary>
        /// �Ǧ^ Database SQL Command ������R�O.
        /// </summary>
        /// <returns>SQL Command ������R�O�r��</returns>
        public string CommandText { get; private set; }

        /// <summary>
        /// �Ǧ^ Database SQL Command �����A. �p�з� SQL Command , Table , StoredProcedure
        /// </summary>
        /// <returns>SQL Command ������R�O���A</returns>
        public System.Data.CommandType CommandType { get; private set; }

        #endregion ICommandBuilder Members

        #region IParameterCommandBuilder Members

        bool IParameterCommandBuilder.ParameterEnabled => (Parameters.Count > 0);

        Parameter[] IParameterCommandBuilder.Parameters => Parameters.ToArray();

        #endregion IParameterCommandBuilder Members
    }
}