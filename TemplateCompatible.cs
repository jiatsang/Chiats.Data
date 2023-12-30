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
    /// 指示 DBTemplate 的相關之屬性內容.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class TemplateCompatibleAttribute : Attribute
    {
        
        /// <summary>
        /// 指示輸出 SqlModel 的文字產生器, 如未指定則以系統預設文字產生器.
        /// </summary>
        public ISqlBuildExport BuildExport { get; private set; } 

        /// <summary>
        ///
        /// </summary>
        /// <param name="BuildExportType"></param>
        public TemplateCompatibleAttribute(Type BuildExportType)
        {
            // TODO : Write 共用的 BuildExport 
            BuildExport = System.Activator.CreateInstance(BuildExportType) as ISqlBuildExport;
        }
    }
}