// Copyright (c) 2012, Andreas Grimme (http://andreas-grimme.gmxhome.de/)
// 
// This file is part of sidi-util.
// 
// sidi-util is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// sidi-util is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public License
// along with sidi-util. If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SQLite;
using System.Data;
using System.Data.Common;
using System.Reflection;
using System.IO;
using System.Linq;
using Sidi.Util;
using System.Linq.Expressions;
using System.Threading;
using Sidi.Extensions;
using Sidi.IO;

namespace Sidi.Persistence
{
    public class CollectionBase
    {
        protected SharedConnection m_connection;

        public SharedConnection SharedConnection
        {
            get
            {
                return m_connection;
            }
        }
    }
    
    public class Collection<T> : CollectionBase, ICollection<T>, IDisposable where T : new()
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        LPath path;
        string table;
        SQLiteCommand insert;
        SQLiteCommand select;
        SQLiteCommand getDataByRowId;
        SQLiteCommand updateDataByRowId;
        SQLiteCommand deleteByRowId;
        SQLiteCommand containsQuery;

        MemberInfo rowIdMember;
        string rowIdColumn;
        MemberInfo[] members;

        Sidi.Collections.LruCache<long, T> cache;

        public SQLiteConnection Connection
        {
            get { return m_connection.Connection; }
        }

        public string Table
        {
            get { return table; }
        }

        bool IsRowId(MemberInfo f)
        {
            object[] a = f.GetCustomAttributes(typeof(RowId), true);
            return a.Length > 0;
        }

        bool IsDataField(MemberInfo f)
        {
            object[] a = f.GetCustomAttributes(typeof(Data), true);
            return a.Length > 0;
        }

        bool IsIndexed(MemberInfo f)
        {
            object[] a = f.GetCustomAttributes(typeof(Indexed), true);
            return a.Length > 0;
        }

        bool IsAutoIncrement(MemberInfo f)
        {
            object[] a = f.GetCustomAttributes(typeof(AutoIncrement), true);
            return a.Length > 0;
        }

        bool IsUnique(MemberInfo f)
        {
            object[] a = f.GetCustomAttributes(typeof(Unique), true);
            return a.Length > 0;
        }

        public Collection(LPath a_path)
        : this(a_path, typeof(T).Name)
        {
        }

        public Collection(LPath a_path, string a_table)
        {
            Init(a_path, a_table);
        }

        public Collection(SharedConnection connection)
        {
            Init(connection, typeof(T).Name);
        }

        public Collection(SharedConnection connection, string a_table)
        {
            Init(connection, a_table);
        }

        public void SetParameters(SQLiteCommand command, T parameters)
        {
            SetFieldParams(command, parameters);
            SetRowIdParam(command, GetRowId(parameters));
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
        public SQLiteCommand CreateCommand(string sql)
        {
            var command = Connection.CreateCommand();
            AddFieldParams(command);
            AddRowIdParam(command);
            sql = sql.Replace("@table", table);
            command.CommandText = sql;
            return command;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
        public SQLiteCommand CreateCommand(string sql, params object[] parameters)
        {
            var command = Connection.CreateCommand();
            for (int i = 0; i < parameters.Length; ++i)
            {
                var p = command.CreateParameter();
                p.Value = parameters[i];
                command.Parameters.Add(p);
                p.ParameterName = "@param" + i.ToString();
                sql = sql.Replace("{" + i.ToString() + "}", p.ParameterName);
            }

            sql = sql.Replace("@table", table);
            command.CommandText = sql;
            return command;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        void Init(LPath a_path, string a_table)
        {
            path = a_path;
            SQLiteConnectionStringBuilder b = new SQLiteConnectionStringBuilder();
            b.DataSource = path;
            b.DateTimeFormat = SQLiteDateFormats.ISO8601;
            b.UseUTF16Encoding = true;
            b.DefaultTimeout = Int32.MaxValue;
            bool create = !path.IsFile;
            if (create)
            {
                path.EnsureParentDirectoryExists();
            }

            var c = new SQLiteConnection(b.ToString());
            c.Open();
            using (var sc = new SharedConnection(c))
            {
                Init(sc, a_table);
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
        void Init(SharedConnection connection, string a_table)
        {
            this.m_connection = new SharedConnection(connection);

            rowIdMember = typeof(T).GetMembers().FirstOrDefault(x => IsRowId(x));
            if (rowIdMember == null)
            {
                throw new InvalidDataException("Need [RowId] attribute.");
            }
            rowIdColumn = "oid";

            members = typeof(T).GetMembers().Where(x => IsDataField(x)).ToArray();

            table = a_table;

            using (var t = this.BeginTransaction())
            {
                bool create = false;
                create |= !TableExists(table);
                if (create)
                {
                    CreateTable();
                }
                else
                {
                    if (!CheckTableSchema())
                    {
                        throw new System.InvalidCastException();
                    }
                }
                t.Commit();
            }

            this.insert = Connection.CreateCommand();
            insert.CommandText = String.Format("insert or replace into {0} ({1}) values({2})",
                table,
                Members.Select(x => x.Name).Join(", "),
                Members.Select(x => "@" + x.Name).Join(", ")
                );
            AddFieldParams(insert);

            this.select = Connection.CreateCommand();
            select.CommandText = String.Format("select {0} from {1} where @query", rowIdColumn, table);
            select.Parameters.Add(new SQLiteParameter("query"));

            this.containsQuery = Connection.CreateCommand();
            containsQuery.CommandText = String.Format("select {0} from {1} where {2}",
                rowIdColumn, table,
                String.Join(" AND ", Members.Select(x => String.Format("{0} = @{0}", x.Name)).ToArray())
                );
            AddFieldParams(containsQuery);

            this.getDataByRowId = Connection.CreateCommand();
            getDataByRowId.CommandText =
                String.Format("select {0} from {1} where {2} = @{2}",
                SelectFieldList, table, rowIdColumn);
            AddRowIdParam(getDataByRowId);

            this.updateDataByRowId = Connection.CreateCommand();
            updateDataByRowId.CommandText = String.Format(
                "update {0} set {1} where {2} = @{2}",
                table,
                FieldAssignments,
                rowIdColumn);
            AddFieldParams(updateDataByRowId);
            AddRowIdParam(updateDataByRowId);

            this.deleteByRowId = Connection.CreateCommand();
            deleteByRowId.CommandText = String.Format(
                "delete from {0} where {1} = @{1}",
                table,
                rowIdColumn);
            AddRowIdParam(deleteByRowId);

            cache = new Sidi.Collections.LruCache<long, T>(1024, new Sidi.Collections.LruCache<long, T>.ProvideValue(delegate(long key)
            {
                return Get(key);
            }));
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
        void CreateTable()
        {
            string sql = String.Format("create table {0} ({1})", table, FieldDefinition);
            using (var c = Connection.CreateCommand())
            {
                c.CommandText = sql;
                ExecuteNonQuery(c);
            }
            CreateIndex();
        }

        void CreateIndex()
        {
            foreach (MemberInfo i in Members)
            {
                if (IsIndexed(i))
                {
                    Indexed indexed = (Indexed)i.GetCustomAttributes(typeof(Indexed), false)[0];
                    var fields = indexed.Fields;
                    if (fields == null)
                    {
                        fields = new[] { i.Name };
                    }
                    CreateIndex(fields, indexed.Unique || IsUnique(i));
                }
            }

            foreach (var index in typeof(T).GetCustomAttributes(typeof(Indexed), true).Cast<Indexed>())
            {
                CreateIndex(index.Fields, index.Unique);
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
        void CreateIndex(string[] fields, bool unique)
        {
            using (var c = Connection.CreateCommand())
            {
                c.CommandText = String.Format(
                    "create {2} index if not exists {0}_{1} on {0} ({3})",
                    table,
                    fields.Join("_"),
                    unique ? "unique" : String.Empty,
                    fields.Join(", "));
                ExecuteNonQuery(c);
            }
        }

        void ExecuteNonQuery(SQLiteCommand c)
        {
            log.Info(c.CommandText);
            c.ExecuteNonQuery();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
        bool TableExists(string table)
        {
            try
            {
                using (var c = Connection.CreateCommand())
                {
                    c.CommandText = String.Format("select * from {0} limit 1", table);
                    c.ExecuteScalar();
                    return true;
                }
            }
            catch (SQLiteException)
            {
                return false;
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
        bool CheckTableSchema()
        {
            foreach (MemberInfo i in Members)
            {
                using (var c = Connection.CreateCommand())
                {
                    c.CommandText = String.Format("select {0} from {1} limit 1", i.Name, table);
                    try
                    {
                        c.ExecuteScalar();
                    }
                    catch (SQLiteException)
                    {
                        log.WarnFormat("{0} not found in table {1}. Trying to alter table.", i.Name, table);
                        c.CommandText = "alter table {0} add column {1}".F(
                            table, i.Name);
                        try
                        {
                            c.ExecuteNonQuery();
                        }
                        catch (SQLiteException)
                        {
                            return false;
                        }
                        log.InfoFormat("Column {0} added to table {1}", i.Name, table);
                    }
                }
            }
            return true;
        }

        void AddFieldParams(SQLiteCommand cmd)
        {
            foreach (MemberInfo i in Members)
            {
                cmd.Parameters.Add(new SQLiteParameter(i.Name));
            }
        }

        void AddRowIdParam(SQLiteCommand cmd)
        {
            cmd.Parameters.Add(new SQLiteParameter(rowIdColumn));
        }

        string FieldAssignments
        {
            get
            {
                string[] names = Members.Select(x=>String.Format("{0} = @{0}", x.Name)).ToArray();
                return String.Join(", ", names);
            }
        }

        MemberInfo[] Members
        {
            get
            {
                return members;
            }
        }

        string FieldDefinition
        {
            get
            {
                string[] names = Members.Select(x => GetFieldDefinition(x)).ToArray();
                return String.Join(", ", names);
            }
        }

        string GetFieldDefinition(MemberInfo m)
        {
            if (IsRowId(m))
            {
                return m.Name + " INTEGER PRIMARY KEY";
            }
            else
            {
                return m.Name;
            }
        }

        string SelectFieldList
        {
            get
            {
                string[] names = Members.Select(x=>x.Name).ToArray();
                return rowIdColumn + ", " + String.Join(", ", names);
            }
        }

        public SQLiteTransaction BeginTransaction()
        {
            return Connection.BeginTransaction();
        }

        /// <summary>
        /// Selects a subset of items from the collection
        /// </summary>
        /// <param name="query">Part of an SQL statement after the "where" keyword.</param>
        /// <returns></returns>
        public IList<T> Select(string query)
        {
            return DoSelect(query);
        }

        /// <summary>
        /// Selects a subset of items from the collection.
        /// </summary>
        /// <param name="query">complete SQL select statement returning row ids</param>
        /// <returns>List of found items</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
        public IList<T> Query(string query)
        {
            using (var command = Connection.CreateCommand())
            {
                command.CommandText = query;
                return Query(command);
            }
        }

        static string GetOperator(ExpressionType nodeType)
        {
            switch (nodeType)
            {
                case ExpressionType.Equal:
                    return "=";
                case ExpressionType.GreaterThan:
                    return ">";
                case ExpressionType.GreaterThanOrEqual:
                    return ">=";
                case ExpressionType.LessThan:
                    return "<";
                case ExpressionType.LessThanOrEqual:
                    return "<=";
                case ExpressionType.AndAlso:
                    return "AND";
                case ExpressionType.NotEqual:
                    return "!=";
                default:
                    throw new NotImplementedException(nodeType.ToString());
            }
        }

        static string SqlConstant(object o)
        {
            var t = o.GetType();
            if (t == typeof(int) || t == typeof(long))
            {
                return o.ToString();
            }
            else if (t == typeof(DateTime))
            {
                return ((DateTime)o).ToString("yyyy-MM-dd HH:mm:ss.fffffff").Quote();
            }
            else
            {
                return o.ToString().Quote();
            }
        }

        public static string SqlPredicate(Expression e)
        {
            try
            {
                var o = Expression.Lambda(e).Compile().DynamicInvoke();
                return SqlConstant(o);
            }
            catch
            {
            }
            
            if (e is BinaryExpression)
            {
                var be = (BinaryExpression)e;
                var op = GetOperator(be.NodeType);
                return String.Format("({0} {1} {2})", SqlPredicate(be.Left), op, SqlPredicate(be.Right));
            }
            else if (e is MemberExpression)
            {
                var me = (MemberExpression)e;
                if (me.Expression.NodeType == ExpressionType.Constant)
                {
                    var c = (ConstantExpression) me.Expression;
                    var v = me.Member.GetValue(c.Value);
                    return v.ToString().Quote();
                }
                else
                {
                    return me.Member.Name;
                }
            }
            else if (e is ConstantExpression)
            {
                var ce = (ConstantExpression)e;
                return SqlConstant(ce.Value);
            }
            else
            {
                throw new NotImplementedException(e.GetType().ToString());
            }
        }

        public IList<T> Query(Expression<Func<T, bool>> predicate)
        {
            using (var command = CreateCommand("select {0} from {1} where {2}".F(rowIdColumn, table, SqlPredicate(predicate.Body))))
            {
                log.Info(command);
                return Query(command);
            }
        }

        public T Find(Expression<Func<T, bool>> predicate)
        {
            using (var command = CreateCommand("select {0} from {1} where {2} limit 1".F(rowIdColumn, table, SqlPredicate(predicate.Body))))
            {
                return Find(command);
            }
        }

        /// <summary>
        /// Finds an element that matches the condition
        /// </summary>
        /// <param name="query">part of SQL statement after the "where" keyword</param>
        /// <returns>Found object</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
        public T Find(string query)
        {
            List<long> ids = new List<long>();
            using (var select = Connection.CreateCommand())
            {
                select.CommandText = String.Format("select {0} from {1} where {2}", rowIdColumn, table, query);
                return Find(select);
            }
        }

        T Find(SQLiteCommand select)
        {
            using (var reader = (SQLiteDataReader)select.ExecuteReader())
            {
                while (reader.Read())
                {
                    return this[(long)reader[0]];
                }
                return default(T);
            }
        }

        public T Find(string query, string paramName, object param)
        {
            using (var select = CreateCommand(String.Format("select {0} from @table where {1}", rowIdColumn, query)))
            {
                SQLiteParameter p = new SQLiteParameter(paramName);
                select.Parameters.Add(p);
                select.Parameters[paramName].Value = param;
                return Find(select);
            }
        }

        /// <summary>
        /// Executes a SQL select and returns a list of objects
        /// </summary>
        /// <param name="query">part of SQL select statement after "where"</param>
        /// <returns></returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
        IList<T> DoSelect(string query)
        {
            using (var select = Connection.CreateCommand())
            {
                select.CommandText = String.Format("select {0} from {1} where {2}", "oid", table, query);
                return Query(select);
            }
        }

        public IList<T> Query(SQLiteCommand select)
        {
            using (var reader = select.ExecuteReader())
            {
                return new ResultProxy<T>(this, reader.Cast<DbDataRecord>().Select(r => (long)r[0]).ToList());
            }
        }

        public T this[long key]
        {
            get
            {
                return cache[key];
            }
            set
            {
                Set(key, value);
                cache.Reset(key);
            }
        }

        /// <summary>
        /// Flushes the internal cache
        /// </summary>
        public void Flush()
        {
            cache.Clear();
        }

        T Get(long key)
        {
            SetRowIdParam(getDataByRowId, key);
            using (var reader = getDataByRowId.ExecuteReader())
            {
                reader.Read();
                return FromReader(reader);
            }
        }

        void Set(long key, T item)
        {
            SetFieldParams(updateDataByRowId, item);
            SetRowIdParam(updateDataByRowId, key);
            updateDataByRowId.ExecuteNonQuery();
        }

        public void Update(T item)
        {
            SetFieldParams(updateDataByRowId, item);
            SetRowIdParam(updateDataByRowId, GetRowId(item));
            updateDataByRowId.ExecuteNonQuery();
        }

        void SetFieldParams(SQLiteCommand cmd, T item)
        {
            foreach (MemberInfo i in Members)
            {
                object v = i.GetValue(item);
                if (FieldType(i).Equals(typeof(String)))
                {
                    if (v == null)
                    {
                        v = String.Empty;
                    }
                }
                if (IsAutoIncrement(i) && v is long && (long)v == 0)
                {
                    v = null;
                }
                cmd.Parameters[i.Name].Value = v;
            }
        }

        Type FieldType(MemberInfo member)
        {
            if (member is FieldInfo)
            {
                return ((FieldInfo)member).FieldType;
            }
            else if (member is PropertyInfo)
            {
                return ((PropertyInfo)member).PropertyType;
            }
            else
            {
                throw new InvalidOperationException();
            }
        }

        T FromReader(SQLiteDataReader reader)
        {
            T item = new T();
            int index = 0;
            SetValue(rowIdMember, item, reader, index);
            ++index;
            foreach (MemberInfo i in Members)
            {
                SetValue(i, item, reader, index);

                ++index;
            }
            return item;
        }

        void SetValue(MemberInfo i, T item, SQLiteDataReader reader, int index)
        {
            try
            {
                var mt = i.GetMemberType();

                if (reader.IsDBNull(index))
                {
                    i.SetValue(item, null);
                }
                else if (mt == typeof(string))
                {
                    i.SetValue(item, reader.GetString(index));
                }
                else if (mt == typeof(DateTime))
                {
                    i.SetValue(item, reader.GetDateTime(index));
                }
                else if (mt == typeof(bool))
                {
                    i.SetValue(item, reader.GetBoolean(index));
                }
                else if (mt == typeof(decimal))
                {
                    i.SetValue(item, reader.GetDecimal(index));
                }
                else if (mt == typeof(short))
                {
                    i.SetValue(item, reader.GetInt16(index));
                }
                else if (mt == typeof(int))
                {
                    i.SetValue(item, reader.GetInt32(index));
                }
                else if (mt == typeof(long))
                {
                    i.SetValue(item, reader.GetInt64(index));
                }
                else if (mt == typeof(Guid))
                {
                    i.SetValue(item, reader.GetGuid(index));
                }
                else if (reader.GetFieldType(index) == typeof(string))
                {
                    // FieldType must be constructible from string
                    var stringConstructor = mt.GetConstructor(new Type[] { typeof(string) });
                    var value = stringConstructor.Invoke(new object[] { reader.GetString(index) });
                    i.SetValue(item, value);
                }
                else
                {
                    i.SetValue(item, reader.GetValue(index));
                }
            }
            catch (Exception ex)
            {
                log.Warn(String.Format("{0}", i.GetMemberType()), ex);
                throw;
            }
        }

        #region ICollection<T> Members

        public void Add(T item)
        {
            SetFieldParams(insert, item);
            insert.ExecuteNonQuery();
        }

        public void AddRange(IEnumerable<T> range)
        {
            using (var t = this.BeginTransaction())
            {
                foreach (var i in range)
                {
                    Add(i);
                }
                t.Commit();
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
        public void Clear()
        {
            using (var clear = Connection.CreateCommand())
            {
                clear.CommandText = String.Format("delete from {0}", table);
                clear.ExecuteNonQuery();
            }
        }

        public bool Contains(T item)
        {
            SetFieldParams(containsQuery, item);
            IList<T> result = Query(containsQuery);
            return result.Count > 0;
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            foreach (T i in this)
            {
                array[arrayIndex++] = i;
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
        public int Count
        {
            get
            {
                using (var c = Connection.CreateCommand())
                {
                    c.CommandText = String.Format("select count(*) from {0}", table);
                    object result = c.ExecuteScalar();
                    return (int)(long)result;
                }
            }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool Remove(T item)
        {
            SetRowIdParam(deleteByRowId, GetRowId(item));
            int count = deleteByRowId.ExecuteNonQuery();
            return count > 0;
        }

        long GetRowId(T item)
        {
            return (long) rowIdMember.GetValue(item);
        }

        void SetRowIdParam(SQLiteCommand cmd, long key)
        {
            cmd.Parameters[rowIdColumn].Value = key;
        }

        #endregion

        #region IEnumerable<T> Members

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
        public IEnumerator<T> GetEnumerator()
        {
            using (var all = Connection.CreateCommand())
            {
                all.CommandText = String.Format("select {0} from {1};", this.rowIdColumn, this.table);
                return Query(all).GetEnumerator();
            }
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            throw new Exception("The method or operation is not implemented.");
        }

        #endregion

        public static Collection<T> UserSetting()
        {
            return new Collection<T>(Paths.GetLocalApplicationDataDirectory(typeof(T)).CatName(".sqlite"));
        }

        /// <summary>
        /// Executes an SQL statement.
        /// </summary>
        /// <param name="sql"></param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
        private void Sql(string sql)
        {
            using (var c = Connection.CreateCommand())
            {
                c.CommandText = sql;
                c.ExecuteNonQuery();
            }
        }

        class ResultProxy<TItem> : IList<TItem> where TItem : new()
        {
            IList<long> primaryKeys;
            Collection<TItem> source;

            public ResultProxy(Collection<TItem> a_source, IList<long> a_primaryKeys)
            {
                source = a_source;
                primaryKeys = a_primaryKeys;
            }

            #region IList<T> Members

            public int IndexOf(TItem item)
            {
                throw new Exception("The method or operation is not implemented.");
            }

            public void Insert(int index, TItem item)
            {
                throw new Exception("The method or operation is not implemented.");
            }

            public void RemoveAt(int index)
            {
                throw new Exception("The method or operation is not implemented.");
            }

            public TItem this[int index]
            {
                get
                {
                    return source[primaryKeys[index]];
                }
                set
                {
                    throw new Exception("The method or operation is not implemented.");
                }
            }

            #endregion

            #region ICollection<T> Members

            public void Add(TItem item)
            {
                throw new Exception("The method or operation is not implemented.");
            }

            public void Clear()
            {
                primaryKeys.Clear();
            }

            public bool Contains(TItem item)
            {
                throw new Exception("The method or operation is not implemented.");
            }

            public void CopyTo(TItem[] array, int arrayIndex)
            {
                foreach (TItem i in this)
                {
                    array[arrayIndex++] = i;
                }
            }

            public int Count
            {
                get { return primaryKeys.Count; }
            }

            public bool IsReadOnly
            {
                get { return true; }
            }

            public bool Remove(TItem item)
            {
                throw new Exception("The method or operation is not implemented.");
            }

            #endregion

            #region IEnumerable<T> Members

            public IEnumerator<TItem> GetEnumerator()
            {
                foreach (long i in primaryKeys)
                {
                    yield return source[i];
                }
            }

            #endregion

            #region IEnumerable Members

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                foreach (long i in primaryKeys)
                {
                    yield return source[i];
                }
            }

            #endregion
        }

        

        private bool disposed = false;
            
        //Implement IDisposable.
        public void Dispose()
        {
          Dispose(true);
          GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
          if (!disposed)
          {
            if (disposing)
            {
            insert.Dispose();
            select.Dispose();
            getDataByRowId.Dispose();
            updateDataByRowId.Dispose();
            deleteByRowId.Dispose();
            containsQuery.Dispose();
            m_connection.Dispose();
            }
            // Free your own state (unmanaged objects).
            // Set large fields to null.
            disposed = true;
          }
        }

        // Use C# destructor syntax for finalization code.
        ~Collection()
        {
          // Simply call Dispose(false).
          Dispose(false);
        }    
    
    }
}
