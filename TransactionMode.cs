// ------------------------------------------------------------------------
// Chiats Common&Data Library V4.1.21 (2021/08)
// Chiats@Studio(http://www.chiats.com/Common)
// Design&Coding By Chia Tsang Tsai
//Copyright(C) 2005-2022 Chiats@Studio All 
// ------------------------------------------------------------------------

namespace Chiats.Data
{
    /// <summary>
    /// ��Ʈw�����(Transaction)�ҫ�. �i�t�X DTC Transaction �ҫ�
    /// </summary>
    public enum TransactionMode
    {
        /// <summary>
        /// Manual Transaction Call, �o�ӿﶵ�O�n�D DACTemplate �b�������n�I�s BeginTrans, Commit/Rollback.
        ///  TODO: ��@�b TransactionScope �U�� Manual Transaction �i�H���T�B�@
        /// </summary>
        Manual,

        /// <summary>
        /// �W�� Transaction �]�w, �o�ӿﶵ�O�n�D DACTemplate ��ѵ{���ۦ�ɪ����I�s BeginTrans, Commit/Rollback
        /// </summary>
        /// <remarks>
        /// DACTemplate�|�b Dispose �ɨ� TransactionScope �����A(IsAbort)  �M�w�I�s BeginTrans, Commit/Rollback
        /// </remarks>
        Transaction,

        /// <summary>
        /// (�w�]��)�䴩 MSDTC(Microsoft Distributed Transaction Coordinator) Transaction �]�w�覡.
        /// �o�ӿﶵ�O�n�D DACTemplate �b�������ΩI�s BeginTrans, Commit/Rollback. Transaction �|��� MSDTC ����.
        /// </summary>
        DTCTransaction
    }
}