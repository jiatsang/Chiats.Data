// ------------------------------------------------------------------------
// Chiats Common&Data Library V4.1.21 (2021/08)
// Chiats@Studio(http://www.chiats.com/Common)
// Design&Coding By Chia Tsang Tsai
//Copyright(C) 2005-2022 Chiats@Studio All 
// ------------------------------------------------------------------------

using System;

namespace Chiats.Data
{
    /// <summary>
    /// ConnectionObjectTypeAttribute �������s���s�u����. �����O�O���g��Ʈw�s�u���O�ϥ�. �t�Τ����������O.
    /// </summary>
    public class ConnectionObjectTypeAttribute : Attribute
    {
        /// <summary>
        /// �s�u���󫬧O
        /// </summary>
        public readonly Type ConnectionObjectType;

        /// <summary>
        /// �s�u����غc�l
        /// </summary>
        /// <param name="type"></param>
        public ConnectionObjectTypeAttribute(Type type)
        {
            ConnectionObjectType = type;
        }
    }
}