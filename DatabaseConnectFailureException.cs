// ------------------------------------------------------------------------
// Chiats Common&Data Library V4.1.21 (2021/08)
// Chiats@Studio(http://www.chiats.com/Common)
// Design&Coding By Chia Tsang Tsai
//Copyright(C) 2005-2022 Chiats@Studio All 
// ------------------------------------------------------------------------

using System;
using System.Runtime.Serialization;

namespace Chiats.Data
{
    /// <summary>
    /// ��Ʈw�s�u���`.
    /// </summary>
    /// <remarks>
    /// ��ܰT���� "��Ʈw�s�����ѡA�Ьd�߸ԲӰT���γs���t�κ޲z���C" ��h��T�Ьd�߸Ը�ƲM��.
    /// </remarks>
    [Serializable]
    public class DatabaseConnectFailureException : CommonException
    {
        public DatabaseConnectFailureException(SerializationInfo info, StreamingContext context) :
            base(info, context)
        { }

        /// <summary>
        /// ��Ʈw�s�u���` �غc�l
        /// </summary>
        /// <param name="message">��Ʈw�s�u���`�B�~�T��</param>
        public DatabaseConnectFailureException(string message)
            : base("��Ʈw�s�u���`({0})�A�Ьd�߸ԲӰT���γs���t�κ޲z���C", message)
        {
        }

        /// <summary>
        /// ��Ʈw�s�u���` �غc�l
        /// </summary>
        /// <param name="innerException">�ǤJ��l�޵o�ҥ~����.</param>
        public DatabaseConnectFailureException(Exception innerException)
            : base(innerException, "��Ʈw�s�u���`�A�Ьd�߸ԲӰT���γs���t�κ޲z���C")
        {
        }

        /// <summary>
        /// ��Ʈw�s�u���` �غc�l
        /// </summary>
        /// <param name="message">��h��T</param>
        /// <param name="innerException">�ǤJ��l�޵o�ҥ~����.</param>
        public DatabaseConnectFailureException(string message, Exception innerException)
            : base(innerException, "{0}�A�Ьd�߸ԲӰT���γs���t�κ޲z���C", message)
        {
            this.MoreMessage = message;
        }
    }

    /// <summary>
    /// ��Ʈw�s�u���`.
    /// </summary>
    /// <remarks>
    /// ��ܰT���� "��Ʈw�s�����ѡA�Ьd�߸ԲӰT���γs���t�κ޲z���C" ��h��T�Ьd�߸Ը�ƲM��.
    /// </remarks>
    [Serializable]
    public class DatabaseExecuteFailureException : CommonException
    {
        public DatabaseExecuteFailureException(SerializationInfo info, StreamingContext context) :
            base(info, context)
        { }

        /// <summary>
        /// ��Ʈw���沧�` �غc�l
        /// </summary>
        /// <param name="message">��h��T</param>
        /// <param name="innerException">�ǤJ��l�޵o�ҥ~����.</param>
        public DatabaseExecuteFailureException(string message, Exception innerException)
            : base(innerException, "��Ʈw���沧�`({0})�A�Ьd�߸ԲӰT���γs���t�κ޲z���C", message)
        {
            this.MoreMessage = innerException.Message;
        }

        /// <summary>
        /// ��Ʈw���沧�` �غc�l
        /// </summary>
        /// <param name="innerException">���`����</param>
        public DatabaseExecuteFailureException(Exception innerException)
            : base(innerException, "��Ʈw���沧�`({0})�A�Ьd�߸ԲӰT���γs���t�κ޲z���C", innerException.Message)
        {
            this.MoreMessage = "";
        }
    }
}