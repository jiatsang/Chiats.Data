// ------------------------------------------------------------------------
// Chiats Common&Data Library V4.1.21 (2021/08)
// Chiats@Studio(http://www.chiats.com/Common)
// Design&Coding By Chia Tsang Tsai
//Copyright(C) 2005-2022 Chiats@Studio All 
// ------------------------------------------------------------------------

using Chiats.SQL;
using System;
using System.Collections.Generic;

namespace Chiats.Data
{
    /// <summary>
    /// SQL Server 2005/2008 取得資料庫系統資訊.  (表格/欄位/資料型別)
    /// </summary>
    public class SqlDbInformation : IDbInformation
    {
        private class TableInfoCache : BufferCache<string, TableInfo>
        {
            // private SelectModel QueryTableModel = new SelectModel("select name,column_id,system_type_id,max_length,precision,scale from sys.columns where object_id=(select object_id from sys.tables where name=@Name)");
            private SqlDbInformation SqlDBInfo = null;
            private Dictionary<int, string> SqlTypeInfoList = new Dictionary<int, string>();
            private readonly object _lockObject = new object();

            public TableInfoCache(SqlDbInformation sqldbInfo)
                : base(100)
            {
                this.SqlDBInfo = sqldbInfo;
            }

            protected override TableInfo QueryNewValue(string key)
            {
                TableInfo tableInfo = new TableInfo();
                lock (SqlTypeInfoList)
                {
                    if (SqlTypeInfoList.Count == 0)
                    {
                        // select name,system_type_id,user_type_id,max_length,precision,scale from sys.types
                        // TODO: 未支援 User Defined Type
                        using (var read = SqlDBInfo._template.OpenReader("select name,system_type_id from sys.types"))
                        {
                            while (read.Read())
                            {
                                var name = read.GetValueEx<string>("name");
                                var systemTypeId = read.GetValueEx<int>("system_type_id");
                                if (!SqlTypeInfoList.ContainsKey(systemTypeId))
                                {
                                    SqlTypeInfoList.Add(systemTypeId, name);
                                }
                            }
                        }
                    }
                }
                // TODO: 當使用 SelectModel 時會產生 _template 在重組 SQL 時 Options 遺失而無法取得 AliasName 因為 DefaultBuilderExport 是共用的
                var sql = $"select name,column_id,system_type_id,max_length,precision,scale from sys.columns where object_id=(select object_id from sys.tables where name=@key)";
                // QueryTableModel.ParameterMode = ParameterMode.Parameter;
                // QueryTableModel.Parameters["@Name"].Value = key;
                using (var read = SqlDBInfo._template.OpenReader(sql, new { key }))
                {
                    while (read.Read())
                    {
                        lock (_lockObject)
                        {
                            var name = read.GetValueEx<string>("name");
                            var datatype = read.GetValueEx<int>("system_type_id");
                            var size = read.GetValueEx<int>("max_length");
                            var precision = read.GetValueEx<short>("precision");
                            var scale = read.GetValueEx<short>("scale");
                            var columnType = ColumnType.Auto;
                            var typename = SqlTypeInfoList[datatype];
                            if (!string.IsNullOrEmpty(typename))
                            {
                                columnType = ColumnTypeHelper.ConvertColumnType(typename);
                            }
                            tableInfo.Columns.Add(new ColumnInfo(name, columnType, size, precision, scale, true));
                        }
                    }
                    return tableInfo;
                }
            }
        }

        private readonly IDbTemplate _template = null;
        private readonly TableInfoCache _tableCache = null;

        public SqlDbInformation(IDbTemplate template)
        {
            this._template = template;
            _tableCache = new TableInfoCache(this);
        }

        public TableInfo QueryTableInfo(string name)
        {
            return _tableCache.Get(name);
        }
    }

    /// <summary>
    /// 資料快取物件管理類別.
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TItem"></typeparam>
    public abstract class BufferCache<TKey, TItem>
        where TKey : IEquatable<TKey>
        where TItem : class
    {
        private class CacheValue<T>
        {
            public CacheValue(T val)
            {
                AccessCount = 1;
                AccessTime = DateTime.Now;
                Value = val;
            }

            public int AccessCount;             //  存取次數
            public DateTime AccessTime;         //  最後存取時間.
            public T Value;                     //  快取值
        }

        private class CacheValueAccessCountCompare<T> : IComparer<CacheValue<T>>
        {
            public int Compare(CacheValue<T> x, CacheValue<T> y)
            {
                if (Math.Abs(x.AccessCount - y.AccessCount) < 2)
                {
                    // 次數少於 N 次時, 則比較時間
                    if (x.AccessTime < y.AccessTime)
                    {
                        return -1;
                    }
                    return x.AccessCount - y.AccessCount;  // 時間相同以次數為基準.
                }
                return 1;
            }
        }

        private int _cacheHitCount;
        private int _cacheClearCount;

        private object _lockObject = new object();
        private int capacityMax = 100;
        private Dictionary<TKey, CacheValue<TItem>> cacheData = null;

        /// <summary>
        /// 清除所有快取內容
        /// </summary>
        public void ClearAll()
        {
            lock (_lockObject)
            {
                cacheData.Clear();
                _cacheHitCount = 0;
            }
        }

        /// <summary>
        /// 快取命中數字
        /// </summary>
        public int CacheHitCount
        {
            get { return _cacheHitCount; }
        }

        /// <summary>
        /// 快取容量大小
        /// </summary>
        public int CapacityMax
        {
            get { return capacityMax; }
        }

        /// <summary>
        /// 快取中的數量
        /// </summary>
        public int Count
        {
            get { return cacheData.Count; }
        }

        /// <summary>
        /// 快取物件之建構子
        /// </summary>
        /// <param name="comparer"></param>
        public BufferCache(IEqualityComparer<TKey> comparer) : this(100, comparer) { }

        /// <summary>
        /// 快取物件之建構子 , CapacityMax 限制可用範圍 100 ~ 5000
        /// </summary>
        /// <param name="CapacityMax"></param>
        public BufferCache(int CapacityMax) : this(CapacityMax, null) { }

        /// <summary>
        /// 快取物件之建構子 , CapacityMax 限制可用範圍 100 ~ 5000
        /// </summary>
        /// <param name="CapacityMax"></param>
        /// <param name="comparer"></param>
        public BufferCache(int CapacityMax, IEqualityComparer<TKey> comparer)
        {
            if (CapacityMax < 100) CapacityMax = 100;
            if (CapacityMax > 5000) CapacityMax = 5000;
            this.capacityMax = CapacityMax;  // CapacityMax 限制可用範圍 100 ~ 5000

            this._cacheClearCount = (this.capacityMax * 10) / 100; // 10% 數量
            this._cacheHitCount = 0;

            // 100  -> 10
            // 5000 -> 500
            // TODO: 依新修正演算法 公開可用參數.
            if (comparer != null)
                cacheData = new Dictionary<TKey, CacheValue<TItem>>(capacityMax, comparer);
            else
                cacheData = new Dictionary<TKey, CacheValue<TItem>>(CapacityMax);
        }

        /// <summary>
        /// 取得物件.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public TItem Get(TKey key)
        {
            if (cacheData.ContainsKey(key))
            {
                CacheValue<TItem> CacheValue = cacheData[key];
                _cacheHitCount++;
                CacheValue.AccessCount++;
                CacheValue.AccessTime = DateTime.Now;
                return CacheValue.Value;
            }

            if (cacheData.Count > capacityMax) ClearCacheOut();
            TItem Value = QueryNewValue(key);  // 無值則回傳 NULL
            if (Value != null)
            {
                cacheData.Add(key, new CacheValue<TItem>(Value));
            }
            return Value;
        }

        private void ClearCacheOut()
        {
            lock (_lockObject)
            {
                // TODO: 修正演算法 in ClearCacheOut

                DateTime CurrentNow = DateTime.Now;

                SortedList<CacheValue<TItem>, TKey> sortlist =
                    new SortedList<CacheValue<TItem>, TKey>(cacheData.Count,
                        new CacheValueAccessCountCompare<TItem>());

                foreach (TKey key in cacheData.Keys)
                {
                    CacheValue<TItem> Val = cacheData[key];
                    // 超過 60 秒未存取者 AccessCount 減半.
                    if ((CurrentNow - Val.AccessTime).TotalSeconds > (60))
                    {
                        Val.AccessCount /= 2;
                    }
                    sortlist.Add(Val, key);
                }
                // 清除前 10% 數量 cacheClearCount
                for (int i = 0; i < _cacheClearCount; i++)
                {
                    cacheData.Remove(sortlist[sortlist.Keys[i]]);
                }
            }
        }

        /// <summary>
        /// 快取中是否包含該物件
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool ContainsKey(TKey key) { return cacheData.ContainsKey(key); }

        /// <summary>
        /// 當快取指定物件不存在快取時會呼叫 QueryNewValue 以取得新的物件.但如 指定物件不存在時則回傳 null
        /// </summary>
        /// <param name="key"></param>
        /// <returns>無效的 KEY 值則回傳 NULL</returns>
        protected abstract TItem QueryNewValue(TKey key);
    }
}