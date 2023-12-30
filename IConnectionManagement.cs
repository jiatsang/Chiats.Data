// ------------------------------------------------------------------------
// Chiats Common&Data Library V4.1.21 (2021/08)
// Chiats@Studio(http://www.chiats.com/Common)
// Design&Coding By Chia Tsang Tsai
//Copyright(C) 2005-2022 Chiats@Studio All 
// ------------------------------------------------------------------------

namespace Chiats.Data
{
    /// <summary>
    /// �޲z��Ʈw�s��������
    /// </summary>
    public interface IConnectionManagement
    {
        /// <summary>
        /// �}�ҳs�u, �C�@�Ӹ�Ʈw��ڳs�u���ɾ��|�� DACTemplate TransManager and  TransScope �M�w.
        /// ��ݭn�s�u�|�q�� ConnectionDataPack.ConnectionOpen(). �]�� ConnectionOpen �����b�Q�I�s��
        /// �إ߹�ڪ��s�u.
        /// </summary>
        void ConnectionOpen(string connectionString);

        /// <summary>
        /// �q�� Connection Close(); �� DACTemplate �� InculdeTransaction �� False �ɥB �b TransScope
        /// �d�򵲧��e CommitTrans(�� TransScope.Complate() �޵o) ���������. �h�ݭn�b  close() �ɰ����� Rollback
        /// </summary>
        void Dispose();

        /// <summary>
        /// �� DACTemplate �� InculdeTransaction �� False �� , TransManager �|�I�s BeginTrans �M CommitTrans �@
        /// ���޲z Transaction ���}�l�M����. �p�G�ݭn Rollback �ɾ��h�b Close() �|�Q�I�s��.
        /// </summary>
        void BeginTrans();

        /// <summary>
        /// �� DACTemplate �� InculdeTransaction �� False �� , TransManager �|�I�s BeginTrans �M CommitTrans �@
        /// ���޲z Transaction ���}�l�M����. �p�G�ݭn Rollback �ɾ��h�b Close() �|�Q�I�s��.
        /// </summary>
        void Commit();

        /// <summary>
        /// �O�_�ҥΤ@�� Microsoft Distributed Transaction Coordinator ��Ʈw���. �ϥ� Connection �� Rollback/Commit.
        /// </summary>
        TransactionMode TransactionMode { get; }
    }
}