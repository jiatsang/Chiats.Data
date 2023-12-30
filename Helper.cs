// ------------------------------------------------------------------------
// Chiats Common&Data Library V4.1.21 (2021/08)
// Chiats@Studio(http://www.chiats.com/Common)
// Design&Coding By Chia Tsang Tsai
//Copyright(C) 2005-2022 Chiats@Studio All 
// ------------------------------------------------------------------------
using System;
using System.Data.Common;
using System.Data.SqlClient;

namespace Chiats.Data
{
    internal static class Helper
    {
        public static Guid GetConnectionId(DbConnection connection)
        {
            if (connection is SqlConnection cn)
            {
                return cn.ClientConnectionId;
            }
            return Guid.Empty;
        }
    }
}