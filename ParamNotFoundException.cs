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
    /// ��Ʈw�s��. �䤣����w�ѼƲ��`
    /// </summary>
    /// <remarks>
    /// ��ܰT���� "��Ʈw�s�����ѡA�Ьd�߸ԲӰT���γs���t�κ޲z���C" ��h��T�Ьd�߸Ը�ƲM��.
    /// </remarks>
    [Serializable]
    public class ParamNotFoundException : CommonException
    {
        /// <summary>
        /// �䤣����w�ѼƲ��` �غc�l
        /// </summary>
        /// <param name="name">�ѼƦW��</param>
        public ParamNotFoundException(string name)
            : base("��Ʈw�s�����ѡA�Ьd�߸ԲӰT���γs���t�κ޲z���C")
        {
            this.MoreMessage = $"�䤣����w�ѼƦW�� {name} ";
        }
    }
}