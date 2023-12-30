// ------------------------------------------------------------------------
// Chiats Common&Data Library V4.1.21 (2021/08)
// Chiats@Studio(http://www.chiats.com/Common)
// Design&Coding By Chia Tsang Tsai
//Copyright(C) 2005-2022 Chiats@Studio All 
// ------------------------------------------------------------------------

using System.Data;

namespace Chiats.Data

{
    /// <summary>
    /// �إߧt�� Stored Procedure �� SQL Server �I�s�{��
    /// </summary>
    public class StoredProcedureBuilder : ParameterCommandBuilder
    {
        /// <summary>
        /// �غc�l
        /// </summary>
        public StoredProcedureBuilder(string name) : base(CommandType.StoredProcedure, name) { }
    }
}