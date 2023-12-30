// ------------------------------------------------------------------------
// Chiats Common&Data Library V4.1.21 (2021/08)
// Chiats@Studio(http://www.chiats.com/Common)
// Design&Coding By Chia Tsang Tsai
//Copyright(C) 2005-2022 Chiats@Studio All 
// ------------------------------------------------------------------------

namespace Chiats.Data
{
    /// <summary>
    /// �޲z System.Transactions.TransactionScope �������O.
    /// </summary>
    public interface ITransaction
    {
        /// <summary>
        /// ���� Transaction ����. �æb TransactionScope ������. �~�|�i�� Rollback Transaction.
        /// </summary>
        void Abort();

        /// <summary>
        /// �Ǧ^ Transaction �Q Abort ����. ���� Abort �� �^�ǭȬ� 0
        /// </summary>
        int AbortCount { get; }

        /// <summary>
        /// ���� Transaction ����. �æb TransactionScope ������. �~�|�i�� Rollback Transaction.
        /// </summary>
        /// <param name="autoThrowException"></param>
        void Abort(bool autoThrowException);
    }
}