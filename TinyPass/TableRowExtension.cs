using Chiats.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Chiats.SQL;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Chiats.Data
{
    public static class TableRowExtension
    {

        public static TableRows LoadAll(this IDbTemplate template, ICommandBuilder CommandBuilder, int MaxCount, object parameters = null, CommandBuildOptions buildOptions = CommandBuildOptions.None)
        {
            using (var reader = template.OpenReader(CommandBuilder, parameters, buildOptions))
            {
                var columns = CreateTableRowColumns(Pass<SchemaTableRow>.QueryAll(reader.GetSchemaTable()), CommandBuilder as SelectModel);

                TableRows Rows = new TableRows(columns);
                int __MacCount = MaxCount;
                while (reader.Read())
                {
                    object[] values = new object[reader.FieldCount];
                    reader.GetValues(values);
                    TableRow row = Rows.Create(values);
                    row.ApplyChanged();

                    if (--__MacCount == 0) break; /* 限制最大筆數  */
                }
                return Rows;
            }
        }

        public static TableRows LoadAll(this IDbTemplate template, ICommandBuilder CommandBuilder, object parameters = null, CommandBuildOptions buildOptions = CommandBuildOptions.None)
        {
            using (var reader = template.OpenReader(CommandBuilder, parameters, buildOptions))
            {
                var columns = CreateTableRowColumns(Pass<SchemaTableRow>.QueryAll(reader.GetSchemaTable()), CommandBuilder as SelectModel);

                TableRows Rows = new TableRows(columns);
                while (reader.Read())
                {
                    var values = new object[reader.FieldCount];
                    reader.GetValues(values);
                    TableRow row = Rows.Create(values);
                    row.ApplyChanged();
                }
                return Rows;
            }
        }

        public static TableRow Load(this IDbTemplate template, ICommandBuilder CommandBuilder, object parameters = null, CommandBuildOptions buildOptions = CommandBuildOptions.None)
        {
            using (var reader = template.OpenReader(CommandBuilder, parameters, buildOptions))
            {
                var columns = CreateTableRowColumns(Pass<SchemaTableRow>.QueryAll(reader.GetSchemaTable()), CommandBuilder as SelectModel);
                TableRow row = null;
                if (reader.Read())
                {
                    object[] values = new object[reader.FieldCount];
                    reader.GetValues(values);
                    row = new TableRow(columns, values);
                    row.ApplyChanged();
                }
                return row;
            }
        }
        public static TableRow Load(this IDbTemplate template, string Command, object parameters = null, CommandBuildOptions buildOptions = CommandBuildOptions.None)
        {
            using (var reader = template.OpenReader(Command, parameters, buildOptions))
            {
                var columns = CreateTableRowColumns(Pass<SchemaTableRow>.QueryAll(reader.GetSchemaTable()), new SelectModel(Command));
                TableRow row = null;
                if (reader.Read())
                {
                    object[] values = new object[reader.FieldCount];
                    reader.GetValues(values);
                    row = new TableRow(columns, values);
                    row.ApplyChanged();
                }
                return row;
            }
        }

        public static async Task<TableRows> LoadAllAsync(this IDbTemplate template, ICommandBuilder CommandBuilder, object parameters = null, CommandBuildOptions buildOptions = CommandBuildOptions.None)
        {

            using (var reader = await template.OpenReaderAsync(CommandBuilder, parameters, buildOptions))
            {
                var columns = CreateTableRowColumns(Pass<SchemaTableRow>.QueryAll(reader.GetSchemaTable()), CommandBuilder as SelectModel);
                TableRows Rows = new TableRows(columns);
                while (reader.Read())
                {
                    object[] values = new object[reader.FieldCount];
                    reader.GetValues(values);
                    TableRow row = Rows.Create(values);
                    row.ApplyChanged();
                }
                return Rows;
            }
        }

        //public static (TableRows, StringBuilder) LoadAllEx(this IDbTemplate template, string CommandBuilder, object parameters = null)
        //{
        //    var sb = new StringBuilder();
        //    template.ExecuteNonQuery("SET SHOWPLAN_TEXT ON");

        //    using (var reader = template.OpenReader(CommandBuilder, parameters))
        //    {

        //        var columns = CreateTableRowColumns(Pass<SchemaTableRow>.QueryAll(reader.GetSchemaTable()), null);
        //        TableRows Rows = new TableRows(columns);

        //        while (reader.Read())
        //        {
        //            sb.Append(reader.GetString(0) + Environment.NewLine);
        //            sb.Append(Environment.NewLine);
        //        }

        //        reader.NextResult();

        //        while (reader.Read())
        //        {
        //            object[] values = new object[reader.FieldCount];
        //            reader.GetValues(values);
        //            TableRow row = Rows.Create(values);
        //            row.ApplyChanged();
        //        }
        //        template.ExecuteNonQuery("SET SHOWPLAN_TEXT OFF");
        //        return (Rows, sb);
        //    }
        //}
        public static TableRows LoadAll(this IDbTemplate template, string CommandBuilder, int max, object parameters = null)
        {
            template.ExecuteNonQuery("SET STATISTICS IO ON;SET STATISTICS TIME ON");
            template.Messages.Clear();
            try
            {
                Stopwatch sw = Stopwatch.StartNew();
                template.Messages.Add(new DbMessage { Message = $"{DateTime.Now:HH:mm:ss.ffff}Open DataReader Max={max}" });
                using (var reader = template.OpenReader(CommandBuilder, parameters))
                {
                    var columns = CreateTableRowColumns(Pass<SchemaTableRow>.QueryAll(reader.GetSchemaTable()), null);
                    TableRows Rows = new TableRows(columns);
                    int __max = max;
                    int index = 0;
                    template.Messages.Add(new DbMessage { Message = $"{DateTime.Now:HH:mm:ss.ffff} Start Loading  Time={sw.ElapsedMilliseconds:#,##0}ms" });
                    while (reader.Read())
                    {
                        object[] values = new object[reader.FieldCount];
                        reader.GetValues(values);
                        TableRow row = Rows.Create(values);
                        row.ApplyChanged();
                        index++;
                        if (max != -1 && __max-- == 0) break;
                    }
                    sw.Stop();
                    template.Messages.Add(new DbMessage { Message = $"{DateTime.Now:HH:mm:ss.ffff} Load OK  Time={sw.ElapsedMilliseconds:#,##0}ms Count={index:#,##0}筆 Columns={reader.FieldCount}" });
                    reader.NextResult();

                    return Rows;
                }
            }
            finally { template.ExecuteNonQuery("SET STATISTICS IO OFF;SET STATISTICS TIME OFF"); }
        }
        public static TableRows LoadAll(this IDbTemplate template, string CommandBuilder, object parameters = null)
        {
            return LoadAll(template, CommandBuilder, -1, parameters);
        }



        public static async Task<TableRows> LoadAllAsync(this IDbTemplate template, string CommandBuilder)
        {
            using (var reader = await template.OpenReaderAsync(CommandBuilder))
            {
                var columns = CreateTableRowColumns(Pass<SchemaTableRow>.QueryAll(reader.GetSchemaTable()), null);
                TableRows Rows = new TableRows(columns);
                while (reader.Read())
                {
                    object[] values = new object[reader.FieldCount];
                    reader.GetValues(values);
                    TableRow row = Rows.Create(values);
                    row.ApplyChanged();
                }
                return Rows;
            }
        }

        private static string GetTableNameWithSelectModel(ColumnNameAndAlias cn, SelectModel SelectSQL)
        {
            string TableName = null;

            if (cn == null) return null;
            if (SelectSQL == null) return null;
            if (cn.Alias == null && SelectSQL.Tables.Count == 0) return SelectSQL.Tables.PrimaryTable.Name;

            // 由 Select SQL 物中取得 Alias 所指的表格實際名稱.若無法對應則為 NULL 值.
            // 若為子查詢語法時, 此時會將 Alias 視為指定的 TableName.
            if (cn.Alias != null)
            {
                if (cn.Alias == SelectSQL.Tables.PrimaryAliasName)
                    // Table.Name 為 NULL 表示無法正確解析, 此時會將 Alias 視為 TableName
                    TableName = (SelectSQL.Tables.PrimaryTable.Name == null) ?
                        SelectSQL.Tables.PrimaryAliasName : SelectSQL.Tables.PrimaryTable.Name;

                else if (cn.Alias == SelectSQL.Tables.PrimaryTable.Name)
                    TableName = SelectSQL.Tables.PrimaryTable.Name;
                else
                {
                    foreach (var t in SelectSQL.Tables)
                    {
                        if (cn.Alias == t.Alias)
                            // Table.Name 為 NULL 表示無法正確解析, 此時會將 Alias 視為 TableName
                            TableName = (t.Table.Name == null) ? t.Alias : t.Table.Name;

                        else if (cn.Alias == t.Table.Name)
                            TableName = t.Table.Name;

                        if (TableName != null) break;
                    }
                }
            }
            return TableName;
        }

        private static TableRowColumns CreateTableRowColumns(IEnumerable<SchemaTableRow> SchemaTable, SelectModel selectModel)
        {
            List<TableRowColumn> NewColumns = new List<TableRowColumn>();
            int index = 0;
            int SchemaCount = SchemaTable.Count();
            foreach (var SchemaField in SchemaTable)
            {
                // ColumnSize ,NumericPrecision ,NumericScale ,DataType ,ProviderType
                ColumnNameAndAlias ColumnNameAndAlias = null;
                if (selectModel != null && SchemaCount == selectModel.Columns.Count)
                {
                    var column = selectModel.Columns.GetColumnByIndex(index);
                    if (column != null)
                    {
                        if (column.AsName == null)
                            ColumnNameAndAlias = new ColumnNameAndAlias(column.ColumnExpression);
                    }
                }
                else
                    ColumnNameAndAlias = new ColumnNameAndAlias(SchemaField.ColumnName);


                ColumnType ColumnType = ColumnTypeHelper.ConvertColumnType(SchemaField.DataTypeName);


                var TableName = GetTableNameWithSelectModel(ColumnNameAndAlias, selectModel);

                ColumnTypeInfo columnTypeInfo = new ColumnTypeInfo(ColumnType,
                     SchemaField.ColumnSize,
                     (short)SchemaField.NumericPrecision,
                     (short)SchemaField.NumericScale);

                NewColumns.Add(new TableRowColumn(ColumnNameAndAlias?.Name ?? SchemaField.ColumnName, columnTypeInfo, TableName));
                // Debug.Print($"TableColumn {ColumnNameAndAlias.Name} Alias={ColumnNameAndAlias.Alias}/{TableName} ");
                index++;
            }
            return new TableRowColumns(NewColumns);
        }

        //public static TableRows Copy(this TableRows TableRows)
        //{

        //    if (PageTable.Columns.Count == Columns.Count)
        //    {
        //        int s_index = 0;
        //        foreach (var r in PageTable.Rows)
        //        {
        //            if (s_index >= this.Rows.Count)
        //            {
        //                this.Rows.CreateNew();
        //                this.Rows[s_index].CopyFrom(r);
        //                this.Rows[s_index].ApplyChanged();
        //            }
        //            else
        //                this.Rows[s_index].CopyFrom(r);
        //            s_index++;
        //        }
        //        return true;
        //    }

        //    return false;
        //}
    }
}
