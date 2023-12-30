// ------------------------------------------------------------------------
// Chiats Common&Data Library V4.1.21 (2021/08)
// Chiats@Studio(http://www.chiats.com/Common)
// Design&Coding By Chia Tsang Tsai
//Copyright(C) 2005-2022 Chiats@Studio All 
// ------------------------------------------------------------------------

using Chiats.SQL;
using System;

namespace Chiats.Data
{
    /// <summary>
    /// ���� DBTemplate ���������ݩʤ��e.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class TemplateCompatibleAttribute : Attribute
    {
        
        /// <summary>
        /// ���ܿ�X SqlModel ����r���;�, �p�����w�h�H�t�ιw�]��r���;�.
        /// </summary>
        public ISqlBuildExport BuildExport { get; private set; } 

        /// <summary>
        ///
        /// </summary>
        /// <param name="BuildExportType"></param>
        public TemplateCompatibleAttribute(Type BuildExportType)
        {
            // TODO : Write �@�Ϊ� BuildExport 
            BuildExport = System.Activator.CreateInstance(BuildExportType) as ISqlBuildExport;
        }
    }
}