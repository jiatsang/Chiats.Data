// ------------------------------------------------------------------------
// Chiats Common&Data Library V4.1.21 (2021/08)
// Chiats@Studio(http://www.chiats.com/Common)
// Design&Coding By Chia Tsang Tsai
//Copyright(C) 2005-2022 Chiats@Studio All 
// ------------------------------------------------------------------------
using Chiats.Data;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
namespace Chiats.Data
{

    /// <summary>
    /// Pass ORM
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public static class Pass<T>
    {
        /// <summary>
        /// 預設不支援  Field 的解析
        /// </summary>
        public static bool SupportField { get; set; } = false;

        public interface ITinyPass
        {
            T Query(CheckMode QueryFillMode = CheckMode.NoCheck);
            int QueryFill(T NewObject, CheckMode QueryFillMode);

            T Query(T NewObject, CheckMode QueryFillMode);

            T SequenceQuery();
            int RowIndex { get; }
            bool Next();
            int GetIndex(string columnName);
            string GetColumnName(int columnIndex);
            object GetColumnValue(int columnIndex);
        }

        private abstract class TinyPass : ITinyPass
        {
            private Type PassType { get; set; }
            private PropertyInfo[] AllPropertyInfo { get; set; }
            private FieldInfo[] AllFieldInfo { get; set; }
            private MethodInfo __init { get; set; }
            protected IColumnValueConvert ColumnValueConvert { get; set; }
            public int RowIndex { get; set; }

            protected TinyPass()
            {
                this.PassType = typeof(T);

                this.AllPropertyInfo = PassType.GetProperties();
                this.AllFieldInfo = PassType.GetFields();

                this.__init = PassType.GetMethod("__init",
                    BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            }

            protected T CreateNew()
            {
                return Activator.CreateInstance<T>();
            }

            private bool ColumnNameCheck(MemberInfo info, ref string filedName)
            {
                if (info.GetCustomAttribute<IgnoreColumnAttribute>(true) == null)
                {
                    var columnPass = info.GetCustomAttribute<ColumnPassAttribute>(true);
                    if (columnPass != null)
                    {
                        filedName = columnPass.ColumnName ?? info.Name;
                        if (columnPass.ValueConvertType != null)
                        {
                            ColumnValueConvert = System.Activator.CreateInstance(columnPass.ValueConvertType) as IColumnValueConvert;
                            if (columnPass.Arg1 != null || columnPass.Arg2 != null)
                            {
                                ColumnValueConvert.Initiailize(columnPass.Arg1, columnPass.Arg2);
                            }
                        }
                        return true;
                    }
                    ColumnValueConvert = null;
                    filedName = info.Name;
                    return true;
                }
                ColumnValueConvert = null;
                return false;
            }

            private bool Update(PropertyInfo p, object NewObject, object ObjectValue)
            {
                if (p.CanWrite)
                {
                    p.SetValue(NewObject, ObjectValue.ChangeTypeEx(p.PropertyType), null);
                    return true;
                }
                else
                {
                    // 因為 Anonymous Type PropertyInfo 是無法寫入變數內容.要改用 FieldInfo (<ID>i__Field)
                    var f = PassType.GetField($"<{p.Name}>i__Field", BindingFlags.SetField | BindingFlags.NonPublic | BindingFlags.Instance);
                    if (f != null)
                    {
                        f.SetValue(NewObject, ObjectValue.ChangeTypeEx(f.FieldType));
                        return true;
                    }
                }
                return false;
            }

            private bool UpdateByName(PropertyInfo p, string ColumnName, object NewObject)
            {
                if (GetColumnValue(ColumnName, out object ObjectValue))
                    return Update(p, NewObject, ObjectValue);
                return false;
            }

            private bool UpdateByName(FieldInfo f, string ColumnName, object NewObject)
            {
                if (GetColumnValue(ColumnName, out object ObjectValue))
                {
                    f.SetValue(NewObject, ObjectValue.ChangeTypeEx(f.FieldType));
                    return true;
                }
                return false;
            }

#if (!DEBUG)
         [System.Diagnostics.DebuggerNonUserCode]
#endif
            /// <summary>
            /// 依欄位名稱相同時填入欄位值
            /// </summary>
            /// <param name="NewObject"></param>
            /// <returns></returns>
            public int QueryFill(T NewObject, CheckMode QueryFillMode)
            {
                List<string> InvalidColumns = new List<string>();
                int field_count = 0;
                string FiledName = null;

                foreach (PropertyInfo p in AllPropertyInfo)
                {
                    FiledName = null;

                    // 不處理 GenericType/ __ 開頭的變數
                    if (p.PropertyType.IsGenericType || p.Name.StartsWith("__")) continue;

                    if (ColumnNameCheck(p, ref FiledName))
                    {
                        if (UpdateByName(p, FiledName, NewObject))
                            field_count++;
                        else
                        {
                            if (FiledName == null || p.Name == FiledName)
                                InvalidColumns.Add(p.Name);
                            else
                                InvalidColumns.Add(p.Name + "->" + FiledName);
                        }
                    }
                }

                if (Pass<T>.SupportField)
                {
                    foreach (FieldInfo __field in AllFieldInfo)
                    {
                        // 不處理 Static/GenericType/ __ 開頭的變數
                        if (__field.IsStatic || __field.FieldType.IsGenericType || __field.Name.StartsWith("__")) continue;

                        FiledName = null;
                        if (ColumnNameCheck(__field, ref FiledName))
                        {
                            if (UpdateByName(__field, FiledName, NewObject))
                                field_count++;
                            else
                            {
                                if (FiledName == null || __field.Name == FiledName)
                                    InvalidColumns.Add(__field.Name);
                                else
                                    InvalidColumns.Add(__field.Name + "->" + FiledName);
                            }
                        }
                    }
                }

                if (InvalidColumns.Count > 0 && QueryFillMode == CheckMode.CheckAndException)
                {
                    StringBuilder sb = new StringBuilder();
                    foreach (var InvalidColumn in InvalidColumns) sb.Append($"{InvalidColumn},");
                    throw new InvalidColumnNameException(sb.ToString());
                }

                if (this.__init != null)
                {
                    __init.Invoke(NewObject, null);
                }
                return field_count;
            }


            public T Query(T NewObject, CheckMode QueryFillMode)
            {
                QueryFill(NewObject, QueryFillMode);
                return NewObject;
            }

            /// <summary>
            /// 依欄位順序時填入 相對順序的物件類別(NewObject) 的 Property Value
            /// </summary>
            /// <param name="NewObject"></param>            
            /// <returns></returns>
            public int SequenceQueryFill(T NewObject)
            {
                int field_count = 0;
                int index = 0;
                if (NewObject is IDictionary<string, object> eu)
                {
                    do
                    {
                        string name = this.GetColumnName(index);
                        if (name == null)
                            return index;
                        object value = this.GetColumnValue(index);
                        eu.Add(name, value);
                        index++;
                    }
                    while (true);

                }
                else
                {
                    foreach (PropertyInfo p in AllPropertyInfo)
                    {
                        string ColumnName = GetColumnName(index);
                        if (ColumnName != null)
                        {
                            if (UpdateByName(p, ColumnName, NewObject))
                                field_count++;
                        }
                        index++;
                    }
                }

                return field_count;
            }
            public T Query(CheckMode QueryFillMode = CheckMode.NoCheck)
            {
                T NewObject = CreateNew();
                QueryFill(NewObject, QueryFillMode);
                return NewObject;
            }
            /// <summary>
            /// 依欄位順序時填入 相對順序的物件類別(NewObject) 的 Property Value
            /// </summary>
            /// <param name="QueryFillMode"></param>
            /// <returns></returns>
            public T SequenceQuery()
            {
                T NewObject = CreateNew();
                SequenceQueryFill(NewObject);
                return NewObject;
            }
            private bool GetColumnValue(string ColumnName, out object _val)
            {
                if (GetDataValue(ColumnName, out _val))
                {
                    if (ColumnValueConvert != null)
                        _val = ColumnValueConvert.GetValue(ColumnName, _val, (string columnName) =>
                        {
                            int ColumnIndex = GetIndex(columnName);
                            if (ColumnIndex != -1)
                                return GetColumnValue(ColumnIndex);
                            else
                                return null;
                        });

                    else if (_val is DateTime && typeof(T) == typeof(string))
                    {
                        _val = ((DateTime)_val).ToString("yyyy-MM-dd HH:mm:ss");
                    }
                    return true;
                }
                return false;
            }
            protected abstract bool GetDataValue(string ColumnName, out object val);
            public abstract bool Next();
            /// <summary>
            /// 取得欄位名稱所在的索引位置
            /// </summary>
            /// <param name="columnName"></param>
            /// <returns></returns>
            public abstract int GetIndex(string columnName);
            /// <summary>
            /// 取得指定位置的欄位名稱, 不在在時回傳 null
            /// </summary>
            /// <param name="columnIndex"></param>
            /// <returns></returns>
            public abstract string GetColumnName(int columnIndex);

            /// <summary>
            /// 取得指定位置的欄位值內容, 不存在時回傳 null
            /// </summary>
            /// <param name="columnIndex"></param>
            /// <returns></returns>
            public abstract object GetColumnValue(int columnIndex);

            public abstract void Dispose();
        }

        private class DataReaderPass : TinyPass
        {
            private IDataReader Reader { get; set; }
            private Dictionary<string, int> FieldIndex { get; } = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            public DataReaderPass(IDataReader reader)
            {
                for (int i = 0; i < reader.FieldCount; i++)
                    FieldIndex.Add(reader.GetName(i), i);

                this.Reader = reader;
            }
            /// <summary>
            /// 取得指定位置的欄位名稱, 不在在時回傳 null
            /// </summary>
            /// <param name="columnIndex"></param>
            /// <returns></returns>
            public override string GetColumnName(int columnIndex)
            {
                if (columnIndex >= 0 && columnIndex < Reader.FieldCount)
                    return Reader.GetName(columnIndex);
                return null;
            }

            public override int GetIndex(string columnName)
            {
                if (FieldIndex.ContainsKey(columnName))
                {
                    return FieldIndex[columnName];
                }
                return -1;
            }

            /// <summary>
            /// 取得指定位置的欄位值內容, 不存在時回傳 null
            /// </summary>
            /// <param name="columnIndex"></param>
            /// <returns></returns>
            public override object GetColumnValue(int columnIndex)
            {
                return Reader.GetValue(columnIndex);
            }

            protected override bool GetDataValue(string ColumnName, out object val)
            {
                val = null;
                int index = GetIndex(ColumnName);
                if (index != -1)
                    val = Reader.GetValue(index);
                return val != null;
            }

            public override bool Next()
            {
                RowIndex++;
                return Reader.Read();
            }

            public override void Dispose()
            {
            }
        }

        private class DataCollectionPass : TinyPass
        {
            // private IEnumerable<KeyValuePair<String, StringValues>> Values { get; set; }
            private Dictionary<string, int> FieldIndex { get; set; } = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            private List<StringValues> Values { get; set; } = new List<StringValues>();
            public DataCollectionPass(IEnumerable<KeyValuePair<String, StringValues>> Values)
            {
                int index = 0;
                foreach (var val in Values)
                {
                    FieldIndex.Add(val.Key, index++);
                    this.Values.Add(val.Value); ;
                }
            }

            public override void Dispose()
            {
                this.Values = null;
                this.FieldIndex = null;
            }
            public override int GetIndex(string columnName)
            {
                if (FieldIndex.ContainsKey(columnName))
                    return FieldIndex[columnName];
                return -1;
            }
            /// <summary>
            /// 取得指定位置的欄位名稱, 不在在時回傳 null
            /// </summary>
            /// <param name="columnIndex"></param>
            /// <returns></returns>
            public override string GetColumnName(int columnIndex)
            {
                foreach (var n in FieldIndex)
                {
                    if (n.Value == columnIndex)
                        return n.Key;
                }
                return null;
            }

            /// <summary>
            ///  取得指定位置的欄位值內容, 不存在時回傳 null
            /// </summary>
            /// <param name="columnIndex"></param>
            /// <returns></returns>
            public override object GetColumnValue(int columnIndex)
            {
                return Values[columnIndex][0];
            }

            protected override bool GetDataValue(string ColumnName, out object val)
            {
                val = null;
                int index = GetIndex(ColumnName);
                if (index != -1)
                    val = GetColumnValue(index);
                return val != null;
            }

            public override bool Next()
            {
                RowIndex++;
                return false;  // no next
            }
        }

        private class DataTablePass : TinyPass
        {
            private DataTable table;

            public DataTablePass(DataTable table, int startIndex)
            {
                this.table = table;
                this.RowIndex = startIndex;
            }

            public override int GetIndex(string columnName)
            {
                if (table.Columns.Contains(columnName))
                {
                    for (var i = 0; i < table.Columns.Count; i++)
                        if (string.Compare(columnName, table.Columns[i].ColumnName, StringComparison.OrdinalIgnoreCase) == 0)
                            return i;
                }
                return -1;
            }
            /// <summary>
            /// 取得指定位置的欄位名稱, 不在在時回傳 null
            /// </summary>
            /// <param name="columnIndex"></param>
            /// <returns></returns>
            public override string GetColumnName(int columnIndex)
            {
                if (columnIndex >= 0 && columnIndex < table.Columns.Count)
                    return table.Columns[columnIndex].ColumnName;
                return null;
            }
            /// <summary>
            ///  取得指定位置的欄位值內容, 不存在時回傳 null
            /// </summary>
            /// <param name="columnIndex"></param>
            /// <returns></returns>
            public override object GetColumnValue(int columnIndex)
            {
                return table.Rows[RowIndex].GetValueEx(columnIndex);
            }

            protected override bool GetDataValue(string ColumnName, out object val)
            {
                if (RowIndex >= 0 && RowIndex < table.Rows.Count)
                {
                    if (table.Columns.Contains(ColumnName))
                    {
                        val = table.Rows[RowIndex].GetValueEx(ColumnName);
                        return true;
                    }
                }
                val = null;
                return false;
            }

            public override bool Next()
            {
                RowIndex++;
                return (table != null) ? RowIndex < table.Rows.Count : false;
            }

            public override void Dispose()
            {
                this.table.Dispose();
            }
        }

#if NET5_0_OR_GREATER
        /// <summary>
        ///  取得資料表內容轉成 List&lt;T&gt; 物件, 依欄位名稱, 但不含 Alias 名稱  T 為內含欄位名稱的類別定義.
        /// </summary>
        /// <param name="table"></param>
        /// <param name="startIndex"></param>
        /// <param name="maxRows"></param>
        /// <returns></returns>
        public static async IAsyncEnumerable<T> QueryAllAsync(DataTable table, int startIndex = 0, int maxRows = -1)
        {
            var pass = new DataTablePass(table, startIndex - 1);
            while (pass.Next())
            {
                yield return pass.Query();
                if (maxRows != -1 && pass.RowIndex > maxRows) yield break;
            }
        }
        //public static async IAsyncEnumerable<SchemaTableRow> GetSchemaTableRowsAsync(IDataReader reader)
        //{
        //    var ps = typeof(T).GetProperties();
        //    Func<string, bool> Find = (columnName) => (from p in ps where p.Name == columnName select p).Count() != 0;
        //    var rows = await Pass<SchemaTableRow>.QueryAllAsync(reader.GetSchemaTable());
        //    return (from r in rows where Find(r.ColumnName) select r);
        //}
#endif
        /// <summary>
        ///  取得資料表內容轉成 List&lt;T&gt; 物件, 依欄位名稱, 但不含 Alias 名稱  T 為內含欄位名稱的類別定義.
        /// </summary>
        /// <param name="table"></param>
        /// <param name="startIndex"></param>
        /// <param name="maxRows"></param>
        /// <returns></returns>
        public static IEnumerable<T> QueryAll(DataTable table, int startIndex = 0, int maxRows = -1)
        {
            var pass = new DataTablePass(table, startIndex - 1);
            while (pass.Next())
            {
                yield return pass.Query();
                if (maxRows != -1 && pass.RowIndex > maxRows) yield break;
            }
        }

        public static IEnumerable<SchemaTableRow> GetSchemaTableRows(IDataReader reader)
        {
            var ps = typeof(T).GetProperties();
            Func<string, bool> Find = (columnName) => (from p in ps where p.Name == columnName select p).Count() != 0;
            var rows = Pass<SchemaTableRow>.QueryAll(reader.GetSchemaTable());
            return (from r in rows where Find(r.ColumnName) select r);
        }




        public static T Query(DataTable table, int startIndex)
        {
            return CreatePass(table, startIndex - 1).Query();
        }
        public static T Query(IEnumerable<KeyValuePair<String, StringValues>> QueryOrFrom)
        {
            return CreatePass(QueryOrFrom).Query();
        }

        public static Dictionary<string, T> QueryAllEx(IDataReader reader, string primaryKey, StringComparer Comparer = null, int MaxRows = -1)
        {
            return QueryAllEx(CreatePass(reader), primaryKey, Comparer, MaxRows);  // return QueryAllEx(new DataReaderPass(reader), primaryKey, Comparer, MaxRows);
        }

        public static ITinyPass CreatePass(IDataReader reader)
        {
            return new DataReaderPass(reader);
        }
        public static ITinyPass CreatePass(IEnumerable<KeyValuePair<String, StringValues>> Values)
        {
            return new DataCollectionPass(Values);
        }
        public static ITinyPass CreatePass(DataTable table, int startIndex)
        {
            return new DataTablePass(table, startIndex);
        }

        /// <summary>
        /// 取得資料表內容轉成 Dictionary&lt;string,T&gt; 物件, 依欄位名稱, 但不含 Alias 名稱  T 為內含欄位名稱的類別定義.
        /// </summary>
        /// <param name="pass"></param>
        /// <param name="primaryKey"></param>
        /// <param name="comparer"></param>
        /// <param name="maxRows"></param>
        /// <returns></returns>
        private static Dictionary<string, T> QueryAllEx(ITinyPass pass, string primaryKey, StringComparer comparer = null, int maxRows = -1)
        {
            Dictionary<string, T> dictionary = null;
            if (comparer != null)
                dictionary = new Dictionary<string, T>(comparer);
            else
                dictionary = new Dictionary<string, T>(StringComparer.OrdinalIgnoreCase);
            int primaryKeyIndex = pass.GetIndex(primaryKey);
            if (primaryKeyIndex != -1)
            {
                while (pass.Next())
                {
                    string key = pass.GetColumnValue(primaryKeyIndex)?.ChangeType<string>();

                    if (!dictionary.ContainsKey(key))
                        dictionary.Add(key, pass.Query());
                    if (maxRows != -1 && pass.RowIndex > maxRows) break;
                }
            }
            else
            {
                throw new CommonException("Invalid primaryKey in QueryAllEx");
            }
            return dictionary;
        }

#if (!DEBUG)
         [System.Diagnostics.DebuggerNonUserCode]
#endif
        public static int QueryFill(T newObject, IDataReader reader, CheckMode queryFillMode)
        {
            return CreatePass(reader).QueryFill(newObject, queryFillMode);
        }

        private static bool property_setvalue(object newObject, string name, object val)
        {
            // 有指定  __count  更新其值為 更新欄位數            ;
            var target = (from p in newObject.GetType().GetProperties() where p.Name == name select p).FirstOrDefault();
            if (target != null)
            {
                if (target.CanWrite)
                {
                    target.SetValue(newObject, val.ChangeTypeEx(target.PropertyType), null);
                    return true;
                }
                else
                {
                    // 因為 Anonymous Type PropertyInfo 是無法寫入變數內容.要改用 FieldInfo (<ID>i__Field)
                    var f = newObject.GetType().GetField($"<{name}>i__Field", BindingFlags.SetField | BindingFlags.NonPublic | BindingFlags.Instance);
                    if (f != null)
                    {
                        f.SetValue(newObject, val.ChangeTypeEx(target.PropertyType));
                        return true;
                    }
                }
            }
            return false;
        }

        public static T Query(T newObject, IDataReader reader, CheckMode queryFillMode)
        {
            int count = CreatePass(reader).QueryFill(newObject, queryFillMode);
            // 有指定  __count  更新其值為 更新欄位數
            property_setvalue(newObject, "__count", count);
            return newObject;
        }

        public static T Query(IDataReader reader)
        {
            return CreatePass(reader).Query();
        }
    }
}