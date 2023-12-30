// ------------------------------------------------------------------------
// Chiats Common&Data Library V4.1.21 (2021/08)
// Chiats@Studio(http://www.chiats.com/Common)
// Design&Coding By Chia Tsang Tsai
//Copyright(C) 2005-2022 Chiats@Studio All 
// ------------------------------------------------------------------------
using Chiats.SQL;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace Chiats.Data
{


    /// <summary>
    /// Pass ORM Extensions
    /// </summary>
    public static class PassExtensions
    {
        public static SqlModel With(this SqlModel commonModel, object parameters, CheckMode checkMode = CheckMode.CheckAndException)
        {
            // 例如 ...
            //if (Template.QueryFill(data, select.With(new { CategoryName = "Beverages" })))
            // 或是
            // if (Template.QueryFill(data, select.With("@CategoryName = 'Beverages'  ")))

            if (parameters != null && commonModel != null)
            {
                if (parameters is string sVal)
                {
                    throw new NotImplementedException();
                }
                else
                {
                    Type objType = parameters.GetType();
                    foreach (var p in objType.GetProperties())
                    {
                        string name = "@" + p.Name;
                        if (commonModel.Parameters.Contains(name))
                        {
                            commonModel.Parameters[name].Value = p.GetValue(parameters, null);
                        }
                        else
                        {
                            if (checkMode == CheckMode.CheckAndException)
                                throw new CommonException($"{parameters} Parameter Name Not Found {name}  in SqlModel {commonModel.CommandText}");
                        }
                    }
                }
            }
            return commonModel;
        }

        /// <summary>
        /// 依欄位名稱自動填滿物件類別的相同名稱的 Property 值.
        /// </summary>
        /// <param name="reader">資料來源</param>
        /// <param name="obj">物件類別</param>
        /// <param name="queryFillMode"></param>
        /// <returns></returns>
        public static bool QueryFill(this IDataReader reader, object obj, CheckMode queryFillMode)
        {
            int count = 0;
            if (obj != null)
            {

                var Method = GetQueryFillMethod(obj.GetType());
                try
                {
                    count = (int)Method.Invoke(null, new object[] { obj, reader, queryFillMode });
                    return (count != 0);
                }
                catch (TargetInvocationException ex)
                {
                    throw ex.InnerException;
                }
            }
            return false;
        }
        public static T Query<T>(this IDataReader reader, T obj, CheckMode options)
        {
            if (obj != null)
            {
                var Method = CreateQueryMethod<T>();
                try
                {
                    Method.Invoke(null, new object[] { obj, reader, options });
                }
                catch (TargetInvocationException ex)
                {
                    throw ex.InnerException;
                }
            }
            return obj;
        }

        /// <summary>
        /// 依欄位名稱自動填滿物件類別的相同名稱的 Property 值.
        /// </summary>
        /// <param name="template"></param>
        /// <param name="obj"></param>
        /// <param name="command"></param>
        /// <param name="queryFillMode"></param>
        /// <returns></returns>
        public static bool QueryFill(this IDbTemplate template, object obj, string command, CheckMode queryFillMode = CheckMode.CheckAndException)
        {
            int count = 0;
            if (obj != null)
            {
                var Method = GetQueryFillMethod(obj.GetType());
                using (var reader = template.OpenReader(command))
                {
                    if (reader.Read())
                    {
                        try
                        {
                            count = (int)Method.Invoke(null, new object[] { obj, reader, queryFillMode });
                        }
                        catch (TargetInvocationException ex)
                        {
                            throw ex.InnerException;
                        }
                        return true;
                    }
                }
            }
            return false;
        }
        private static MethodInfo CreateQueryMethod<T>()
        {
            Type passGeneric = typeof(Pass<>);
            Type passConstructed = passGeneric.MakeGenericType(typeof(T));

            MethodInfo getMethodInfo = passConstructed.GetMethod(
                "Query",
                new Type[] { typeof(T), typeof(IDataReader), typeof(CheckMode) });

            Debug.Assert(getMethodInfo != null, "Pass Must has Query(T,IDataReader,QueryFillMode)");
            return getMethodInfo;

        }

        private static MethodInfo GetQueryFillMethod(Type objType)
        {
            Type passGeneric = typeof(Pass<>);
            Type passConstructed = passGeneric.MakeGenericType(objType);

            MethodInfo getMethodInfo = passConstructed.GetMethod(
                "QueryFill",
                new Type[] { objType, typeof(IDataReader), typeof(CheckMode) });

            Debug.Assert(getMethodInfo != null, "Pass Must has QueryFill(T,IDataReader,QueryFillMode)");
            return getMethodInfo;

        }
        //public static QueryResult<T> QueryValue<T>(this IDbTemplate template, T obj, SqlModel @select, object parameters = null)
        //{
        //    using (var reader = template.OpenReader(@select, parameters))
        //    {
        //        if (reader.Read())
        //            return new QueryResult<T>(Pass<T>.Query(reader));
        //        return QueryResult<T>.Empty;
        //    }
        //}
        //public static async Task<QueryResult<T>> QueryValueAsync<T>(this IDbTemplate template, T obj, SqlModel @select, object parameters = null)
        //{
        //    using (var reader = await template.OpenReaderAsync(@select, parameters))
        //    {
        //        if (reader.Read())
        //            return new QueryResult<T>(Pass<T>.Query(reader));
        //        return QueryResult<T>.Empty;
        //    }
        //}
        //public static QueryResult<T> QueryValue<T>(this IDbTemplate template, T obj, string command, object parameters = null, CheckMode queryFillMode = CheckMode.CheckAndException)
        //{
        //    var Method = CreateQueryMethod<T>();
        //    using (var reader = template.OpenReader(command, parameters))
        //    {
        //        if (reader.Read())
        //        {
        //            try
        //            {
        //                Method.Invoke(null, new object[] { obj, reader, queryFillMode });
        //                return new QueryResult<T>(obj);
        //            }
        //            catch (TargetInvocationException ex)
        //            {
        //                throw ex.InnerException;
        //            }
        //        }
        //        return QueryResult<T>.Empty;
        //    }

        //}
        //public static async Task<QueryResult<T>> QueryValueAsync<T>(this IDbTemplate template, T obj, string command, object parameters = null, CheckMode queryFillMode = CheckMode.CheckAndException)
        //{
        //    if (obj != null)
        //    {
        //        var Method = CreateQueryMethod<T>();

        //        using (var reader = await template.OpenReaderAsync(command, parameters))
        //        {
        //            if (reader.Read())
        //            {
        //                try
        //                {
        //                    Method.Invoke(null, new object[] { obj, reader, queryFillMode });
        //                    return new QueryResult<T>(obj);
        //                }
        //                catch (TargetInvocationException ex)
        //                {
        //                    throw ex.InnerException;
        //                }
        //            }
        //        }
        //    }
        //    return QueryResult<T>.Empty;
        //}

        /// <summary>
        /// 依欄位名稱自動填滿物件類別的相同名稱的 Property 值.
        /// </summary>
        /// <param name="template"></param>
        /// <param name="obj"></param>
        /// <param name="command"></param>
        /// <param name="queryFillMode"></param>
        /// <returns></returns>
        public static T Query<T>(this IDbTemplate template, T obj, string command, object parameters = null, CheckMode queryFillMode = CheckMode.CheckAndException)
        {
            if (obj != null)
            {
                var Method = CreateQueryMethod<T>();
                using (var reader = template.OpenReader(command, parameters))
                {
                    if (reader.Read())
                    {
                        try
                        {
                            Method.Invoke(null, new object[] { obj, reader, queryFillMode });
                        }
                        catch (TargetInvocationException ex)
                        {
                            throw ex.InnerException;
                        }
                    }
                }
            }
            return obj;
        }
        public static async Task<T> QueryAsync<T>(this IDbTemplate template, T obj, string command, object parameters = null, CheckMode queryFillMode = CheckMode.CheckAndException)
        {
            if (obj != null)
            {
                var Method = CreateQueryMethod<T>();

                using (var reader = await template.OpenReaderAsync(command, parameters))
                {
                    if (reader.Read())
                    {
                        try
                        {
                            Method.Invoke(null, new object[] { obj, reader, queryFillMode });
                        }
                        catch (TargetInvocationException ex)
                        {
                            throw ex.InnerException;
                        }
                    }
                }
            }
            return obj;
        }

        private static void UpdateObjectEOF(object obj, bool is_eof)
        {
            var type = obj.GetType();
            // 因為 Anonymous Type PropertyInfo 是無法寫入變數內容.要改用 FieldInfo (<ID>i__Field)
            var f = type.GetField($"<__eof>i__Field", BindingFlags.SetField | BindingFlags.NonPublic | BindingFlags.Instance);
            f?.SetValue(obj, is_eof);
        }
        public static T Query<T>(this IDbTemplate template, T obj, SqlModel @select, object parameters = null, CheckMode queryFillMode = CheckMode.CheckAndException)
        {
            if (obj != null)
            {
                bool eof;
                var Method = CreateQueryMethod<T>();
                using (var reader = template.OpenReader(@select, parameters))
                {
                    if (eof = reader.Read())
                    {
                        try { Method.Invoke(null, new object[] { obj, reader, queryFillMode }); }
                        catch (TargetInvocationException ex) { throw ex.InnerException; }
                    }
                    UpdateObjectEOF(obj, eof);
                }
            }
            return obj;
        }
        public static async Task<T> QueryAsync<T>(this IDbTemplate template, T obj, SqlModel @select, object parameters = null, CheckMode queryFillMode = CheckMode.CheckAndException)
        {
            if (obj != null)
            {
                bool eof;
                var Method = CreateQueryMethod<T>();
                using (var reader = await template.OpenReaderAsync(@select, parameters))
                {
                    if (eof = reader.Read())
                    {
                        try { Method.Invoke(null, new object[] { obj, reader, queryFillMode }); }
                        catch (TargetInvocationException ex) { throw ex.InnerException; }
                    }
                    UpdateObjectEOF(obj, eof);
                }
            }
            return obj;
        }

        public static T Query<T>(this IDbTemplate template, SqlModel @select, object parameters = null, T defaultValue = default(T))
           where T : new()
        {
            bool eof;
            using (var reader = template.OpenReader(@select, parameters))
            {
                if (eof = reader.Read())
                    return Pass<T>.Query(reader);
            }
            return defaultValue;
        }
        public static async Task<T> QueryAsync<T>(this IDbTemplate template, SqlModel @select, object parameters = null, T defaultValue = default(T))
          where T : new()
        {
            using (var reader = await template.OpenReaderAsync(@select, parameters))
            {
                if (reader.Read()) return Pass<T>.Query(reader);
            }
            return defaultValue;
        }

        public static T Query<T>(this IDbTemplate template, string command, object parameters = null, T defaultValue = default(T))
            where T : new()
        {
            using (var reader = template.OpenReader(command, parameters))
            {
                if (reader.Read()) return Pass<T>.Query(reader);
            }
            return defaultValue;
        }

        public static async Task<T> QueryAsync<T>(this IDbTemplate template, string command, object parameters = null, T defaultValue = default(T))
            where T : new()
        {
            using (var reader = await template.OpenReaderAsync(command, parameters))
            {
                if (reader.Read()) return Pass<T>.Query(reader);
            }
            return defaultValue;
        }

        public static T Query<T>(this IDataReader reader)
             where T : new()
        {
            return Pass<T>.Query(reader);
        }

        /// <summary>
        ///  取得所有列的第一佪欄位值  
        /// </summary>
        /// <typeparam name="T">第一佪欄位值回傳的型別</typeparam>
        /// <param name="template"></param>
        /// <param name="select"></param>
        /// <param name="maxRows"></param>
        /// <returns></returns>
        public static IEnumerable<T> QueryScalarAll<T>(this IDbTemplate template, SqlModel @select, object parameters = null, int maxRows = -1)
        {
            using (var reader = template.OpenReader(@select, parameters))
            {
                int index = 0;
                List<T> list = new List<T>();
                while (reader.Read())
                {
                    // template.ExecuteScalar 
                    if (maxRows != -1 && maxRows < index++)
                        break;
                    list.Add(reader.GetValueEx<T>(0));
                }
                return list;
            }
        }

        /// <summary>
        /// 取得所有列的第一佪欄位值  
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="template"></param>
        /// <param name="command"></param>
        /// <param name="parameters"></param>
        /// <param name="maxRows"></param>
        /// <returns></returns>
        public static IEnumerable<T> QueryScalarAll<T>(this IDbTemplate template, string command, object parameters = null, int maxRows = -1)
        {
            using (var reader = template.OpenReader(command, parameters))
            {
                int index = 0;
                var list = new List<T>();
                while (reader.Read())
                {
                    if (maxRows != -1 && maxRows < index++)
                        break;
                    list.Add(reader.GetValueEx<T>(0));
                }
                return list;
            }
        }

        /// <summary>
        /// 依欄位順序時填入 相對順序的物件類別(NewObject) 的 Property Value
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="template"></param>
        /// <param name="select"></param>
        /// <param name="maxRows"></param>
        /// <returns></returns>
        public static async Task<IEnumerable<T>> QueryAllByOneAsync<T>(this IDbTemplate template, SqlModel @select, object parameters = null, int maxRows = -1)
        {
            List<T> list = new List<T>();
            using (var reader = await template.OpenReaderAsync(@select, parameters))
            {
                int index = 0;
                while (reader.Read())
                {
                    if (maxRows != -1 && maxRows < index++)
                        break;
                    list.Add(reader.GetValueEx<T>(0));
                }
                return list;
            }
        }
        /// <summary>
        /// 依欄位順序時填入 相對順序的物件類別(NewObject) 的 Property Value
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="template"></param>
        /// <param name="select"></param>
        /// <param name="maxRows"></param>
        /// <returns></returns>
        public static IEnumerable<T> QuerySequenceAll<T>(this IDbTemplate template, SqlModel @select, object parameters = null, int maxRows = -1)
        {
            using (var reader = template.OpenReader(@select, parameters))
            {
                var pass = Pass<T>.CreatePass(reader);
                List<T> rs = new List<T>();
                while (pass.Next())
                {
                    // 必須要用 yield return 依序回傳, 以確保 reader 只會在全部完成後才Call Dispose
                    rs.Add(pass.SequenceQuery());
                    if (maxRows != -1 && pass.RowIndex > maxRows) break;
                }
                return rs;
            }
        }
        /// <summary>
        /// 依欄位順序時填入 相對順序的物件類別(NewObject) 的 Property Value
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="template"></param>
        /// <param name="command"></param>
        /// <param name="maxRows"></param>
        /// <returns></returns>
        public static IEnumerable<T> QuerySequenceAll<T>(this IDbTemplate template, string command, object parameters = null, int maxRows = -1)
        {
            using (var reader = template.OpenReader(command, parameters))
            {
                var pass = Pass<T>.CreatePass(reader);
                List<T> rs = new List<T>();
                while (pass.Next())
                {
                    // 必須要用 yield return 依序回傳, 以確保 reader 只會在全部完成後才Call Dispose
                    rs.Add(pass.SequenceQuery());
                    if (maxRows != -1 && pass.RowIndex > maxRows) break;
                }
                return rs;
            }
        }
        /// <summary>
        /// 依欄位順序時填入 相對順序的物件類別(NewObject) 的 Property Value
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="template"></param>
        /// <param name="select"></param>
        /// <param name="maxRows"></param>
        /// <returns></returns>
        public static async Task<IEnumerable<T>> QuerySequenceAllAsync<T>(this IDbTemplate template, SqlModel @select, object parameters = null, int maxRows = -1)
        {
            using (var reader = await template.OpenReaderAsync(@select, parameters))
            {
                var pass = Pass<T>.CreatePass(reader);
                List<T> rs = new List<T>();
                while (pass.Next())
                {
                    // 必須要用 yield return 依序回傳, 以確保 reader 只會在全部完成後才Call Dispose
                    rs.Add(pass.SequenceQuery());
                    if (maxRows != -1 && pass.RowIndex > maxRows) break;
                }
                return rs;
            }
        }

        /// <summary>
        /// 依欄位順序時填入 相對順序的物件類別(NewObject) 的 Property Value
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="template"></param>
        /// <param name="select"></param>
        /// <param name="maxRows"></param>
        /// <returns></returns>
        public static async Task<IEnumerable<T>> QuerySequenceAllAsync<T>(this IDbTemplate template, string command, object parameters = null, int maxRows = -1)
        {
            using (var reader = await template.OpenReaderAsync(command, parameters))
            {
                var pass = Pass<T>.CreatePass(reader);
                List<T> rs = new List<T>();
                while (pass.Next())
                {
                    // 必須要用 yield return 依序回傳, 以確保 reader 只會在全部完成後才Call Dispose
                    rs.Add(pass.SequenceQuery());
                    if (maxRows != -1 && pass.RowIndex > maxRows) break;
                }
                return rs;
            }
        }
        /// <summary>
        /// 依欄位順序時填入 相對順序的物件類別(NewObject) 的 Property Value
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="template"></param>
        /// <param name="command"></param>
        /// <param name="maxRows"></param>
        /// <returns></returns>
        public static async Task<IEnumerable<T>> QueryAllBySequenceAsync<T>(this IDbTemplate template, string command, object parameters = null, int maxRows = -1)
        {
            using (var reader = await template.OpenReaderAsync(command, parameters))
            {
                var pass = Pass<T>.CreatePass(reader);
                List<T> rs = new List<T>();
                while (pass.Next())
                {
                    // 必須要用 yield return 依序回傳, 以確保 reader 只會在全部完成後才Call Dispose
                    rs.Add(pass.SequenceQuery());
                    if (maxRows != -1 && pass.RowIndex > maxRows) break;
                }
                return rs;
            }
        }

        public static IEnumerable<T> QueryAll<T>(this IDbTemplate template, T obj, SqlModel @select, object parameters = null, CheckMode checkMode = CheckMode.CheckAndException, int maxRows = -1)
        {
            using (var reader = template.OpenReader(@select, parameters))
            {
                List<T> rs = new List<T>();
                var pass = Pass<T>.CreatePass(reader);
                while (pass.Next())
                {
                    T newobj = (T)FormatterServices.GetUninitializedObject(typeof(T));
                    rs.Add(pass.Query(newobj, checkMode));
                    if (maxRows != -1 && pass.RowIndex > maxRows) break;
                }
                return rs;
            }
        }
#if NET5_0_OR_GREATER

        /// <summary>
        ///  依欄位名稱 相對的物件類別(NewObject) 中相同名稱的 Property 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="template"></param>
        /// <param name="obj"></param>
        /// <param name="select"></param>
        /// <param name="maxRows"></param>
        /// <returns></returns>
        public static async IAsyncEnumerable<T> QueryAllAsync<T>(this IDbTemplate template, T obj, SqlModel @select, object parameters = null, CheckMode checkMode = CheckMode.CheckAndException, int maxRows = -1)
        {
            using (var reader = await template.OpenReaderAsync(@select, parameters))
            {
                var pass = Pass<T>.CreatePass(reader);
                while (pass.Next())
                {
                    //rs.Add(pass.Query((T)FormatterServices.GetUninitializedObject(typeof(T)), checkMode));
                    //if (maxRows != -1 && pass.RowIndex > maxRows) break;
                    yield return pass.Query((T)FormatterServices.GetUninitializedObject(typeof(T)), checkMode);
                    if (maxRows != -1 && pass.RowIndex > maxRows) 
                        yield break;
                }
            }
        }
#else
        /// <summary>
        ///  依欄位名稱 相對的物件類別(NewObject) 中相同名稱的 Property 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="template"></param>
        /// <param name="obj"></param>
        /// <param name="select"></param>
        /// <param name="maxRows"></param>
        /// <returns></returns>
        public static async Task<IEnumerable<T>> QueryAllAsync<T>(this IDbTemplate template, T obj, SqlModel @select, object parameters = null, CheckMode checkMode = CheckMode.CheckAndException, int maxRows = -1)
        {
            using (var reader = await template.OpenReaderAsync(@select, parameters))
            {
                List<T> rs = new List<T>();
                var pass = Pass<T>.CreatePass(reader);
                while (pass.Next())
                {
                    rs.Add(pass.Query((T)FormatterServices.GetUninitializedObject(typeof(T)), checkMode));
                    if (maxRows != -1 && pass.RowIndex > maxRows) break;
                }
                return rs;
            }
        }
#endif
        /// <summary>
        /// 依欄位名稱 相對的物件類別(NewObject) 中相同名稱的 Property 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="template"></param>
        /// <param name="select"></param>
        /// <param name="maxRows"></param>
        /// <returns></returns>
        public static IEnumerable<T> QueryAll<T>(this IDbTemplate template, SqlModel @select, object parameters = null, CheckMode checkMode = CheckMode.CheckAndException, int maxRows = -1)
        {
            using (var reader = template.OpenReader(@select, parameters))
            {
                List<T> rs = new List<T>();
                var pass = Pass<T>.CreatePass(reader);
                while (pass.Next())
                {
                    // 必須要用 yield return 依序回傳, 以確保 reader 只會在全部完成後才 Call Dispose
                    rs.Add(pass.Query(checkMode));
                    if (maxRows != -1 && pass.RowIndex > maxRows) break;
                }
                return rs;
            }
        }
        public static IEnumerable<T> QueryAll<T>(this IDbTemplate template, SqlModel @select, out IEnumerable<SchemaTableRow> SchemaTableRows, object parameters = null, CheckMode checkMode = CheckMode.CheckAndException, int maxRows = -1)
        {
            using (var reader = template.OpenReader(@select, parameters))
            {
                List<T> rs = new List<T>();

                var pass = Pass<T>.CreatePass(reader);
                SchemaTableRows = Pass<T>.GetSchemaTableRows(reader);

                while (pass.Next())
                {
                    // 必須要用 yield return 依序回傳, 以確保 reader 只會在全部完成後才 Call Dispose
                    rs.Add(pass.Query(checkMode));
                    if (maxRows != -1 && pass.RowIndex > maxRows) break;
                }
                return rs;
            }
        }
#if NET5_0_OR_GREATER
        /// <summary>
        /// 依欄位名稱 相對的物件類別(NewObject) 中相同名稱的 Property 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="template"></param>
        /// <param name="select"></param>
        /// <param name="maxRows"></param>
        /// <returns></returns>
        public static async IAsyncEnumerable<T> QueryAllAsync<T>(this IDbTemplate template, SqlModel @select, object parameters = null, CheckMode checkMode = CheckMode.CheckAndException, int maxRows = -1)
            where T : new()
        {
            using (var reader = await template.OpenReaderAsync(@select, parameters))
            {
                var pass = Pass<T>.CreatePass(reader);
                while (pass.Next())
                {
                    // 必須要用 yield return 依序回傳, 以確保 reader 只會在全部完成後才 Call Dispose
                    yield return pass.Query(checkMode);
                    if (maxRows != -1 && pass.RowIndex > maxRows) yield break;
                }
            }
        }
#else
        /// <summary>
        /// 依欄位名稱 相對的物件類別(NewObject) 中相同名稱的 Property 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="template"></param>
        /// <param name="select"></param>
        /// <param name="maxRows"></param>
        /// <returns></returns>
        public static async Task<IEnumerable<T>> QueryAllAsync<T>(this IDbTemplate template, SqlModel @select, object parameters = null, CheckMode checkMode = CheckMode.CheckAndException, int maxRows = -1)
            where T : new()
        {
            using (var reader = await template.OpenReaderAsync(@select, parameters))
            {
                List<T> rs = new List<T>();
                var pass = Pass<T>.CreatePass(reader);
                while (pass.Next())
                {
                    // 必須要用 yield return 依序回傳, 以確保 reader 只會在全部完成後才 Call Dispose
                    rs.Add(pass.Query(checkMode));
                    if (maxRows != -1 && pass.RowIndex > maxRows) break;
                }
                return rs;
            }
        }
#endif


        public static IEnumerable<T> QueryAll<T>(this IDbTemplate template, T obj, string command, object parameters = null, CheckMode checkMode = CheckMode.CheckAndException, int maxRows = -1)
        {
            using (var reader = template.OpenReader(command, parameters))
            {
                List<T> rs = new List<T>();
                var pass = Pass<T>.CreatePass(reader);
                while (pass.Next())
                {
                    rs.Add(pass.Query((T)FormatterServices.GetUninitializedObject(typeof(T)), checkMode));
                    if (maxRows != -1 && pass.RowIndex > maxRows) break;
                }
                return rs;
            }
        }

        public static async Task<IEnumerable<T>> QueryAllAsync<T>(this IDbTemplate template, T obj, string command, object parameters = null, CheckMode checkMode = CheckMode.CheckAndException, int maxRows = -1)
        {
            using (var reader = await template.OpenReaderAsync(command, parameters))
            {
                List<T> rs = new List<T>();
                var pass = Pass<T>.CreatePass(reader);
                while (pass.Next())
                {
                    rs.Add(pass.Query((T)FormatterServices.GetUninitializedObject(typeof(T)), checkMode));
                    if (maxRows != -1 && pass.RowIndex > maxRows) break;
                }
                return rs;
            }
        }
        /// <summary>
        /// 依欄位名稱 相對的物件類別(NewObject) 中相同名稱的 Property 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="template"></param>
        /// <param name="command"></param>
        /// <param name="maxRows"></param>
        /// <returns></returns>
        public static IEnumerable<T> QueryAll<T>(this IDbTemplate template, string command, object parameters = null, CheckMode checkMode = CheckMode.CheckAndException, int maxRows = -1)
          where T : new()
        {
            using (var reader = template.OpenReader(command, parameters))
            {
                var pass = Pass<T>.CreatePass(reader);
                List<T> rs = new List<T>();
                while (pass.Next())
                {
                    // 必須要用 yield return 依序回傳, 以確保 reader 只會在全部完成後才 Call Dispose
                    rs.Add(pass.Query(checkMode));
                    if (maxRows != -1 && pass.RowIndex > maxRows) break;
                }
                return rs;
            }
        }

        public static async Task<IEnumerable<T>> QueryAllAsync<T>(this IDbTemplate template, string command, object parameters = null, CheckMode checkMode = CheckMode.CheckAndException, int maxRows = -1)
          where T : new()
        {
            using (var reader = await template.OpenReaderAsync(command, parameters))
            {
                var PassReader = Pass<T>.CreatePass(reader);
                List<T> rs = new List<T>();
                while (PassReader.Next())
                {
                    rs.Add(PassReader.Query(checkMode));
                    if (maxRows != -1 && PassReader.RowIndex > maxRows) break;
                }
                return rs;
            }
        }

        /// <summary>
        /// 依欄位名稱 相對的物件類別(NewObject) 中相同名稱的 Property 
        /// </summary>
        /// <param name="template"></param>
        /// <param name="select"></param>
        /// <param name="maxRows"></param>
        /// <returns></returns>
        public static IEnumerable<ExpandoObject> SequenceQueryAll(this IDbTemplate template, SqlModel @select, object parameters = null, int maxRows = -1)
        {
            using (var reader = template.OpenReader(@select, parameters))
            {
                var pass = Pass<ExpandoObject>.CreatePass(reader);
                List<ExpandoObject> rs = new List<ExpandoObject>();
                while (pass.Next())
                {
                    // 必須要用 yield return 依序回傳, 以確保 reader 只會在全部完成後才Call Dispose
                    rs.Add(pass.SequenceQuery());
                    if (maxRows != -1 && pass.RowIndex > maxRows) break;
                }
                return rs;
            }
        }
        /// <summary>
        /// 依欄位名稱 相對的物件類別(NewObject) 中相同名稱的 Property 
        /// </summary>
        /// <param name="template"></param>
        /// <param name="select"></param>
        /// <param name="maxRows"></param>
        /// <returns></returns>
        public static async Task<IEnumerable<ExpandoObject>> SequenceQueryAllAsync(this IDbTemplate template, SqlModel @select, object parameters = null, int maxRows = -1)
        {
            using (var reader = await template.OpenReaderAsync(@select, parameters))
            {
                var pass = Pass<ExpandoObject>.CreatePass(reader);
                List<ExpandoObject> rs = new List<ExpandoObject>();
                while (pass.Next())
                {
                    // 必須要用 yield return 依序回傳, 以確保 reader 只會在全部完成後才Call Dispose
                    rs.Add(pass.SequenceQuery());
                    if (maxRows != -1 && pass.RowIndex > maxRows) break;
                }
                return rs;
            }
        }

    }
}