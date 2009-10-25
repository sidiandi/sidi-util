// Copyright (c) 2009, Andreas Grimme (http://andreas-grimme.gmxhome.de/)
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

namespace Sidi.Persistence
{
    public static class MemberInfoEx
    {
        public static void SetValue(this MemberInfo member, object target, object value)
        {
            if (member is FieldInfo)
            {
                ((FieldInfo)member).SetValue(target, value);
            }
            else if (member is PropertyInfo)
            {
                ((PropertyInfo)member).SetValue(target, value, new object[]{});
            }
            else
            {
                throw new InvalidOperationException();
            }
        }

        public static Type FieldType(this MemberInfo member)
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

        public static object GetValue(this MemberInfo member, object item)
        {
            if (member is FieldInfo)
            {
                return ((FieldInfo)member).GetValue(item);
            }
            else if (member is PropertyInfo)
            {
                return ((PropertyInfo)member).GetValue(item, new object[] { });
            }
            else
            {
                throw new InvalidOperationException();
            }
        }
    }
    
    public class Collection<T> : ICollection<T> where T : new()
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        string path;
        string table;
        SQLiteConnection connection;
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

        public DbConnection Connection
        {
            get { return connection; }
        }

        public string Table
        {
            get { return table; }
        }

        public void Close()
        {
            connection.Close();
            connection = null;
        }

        bool IsPrimaryKeyField(MemberInfo f)
        {
            object[] a = f.GetCustomAttributes(typeof(PrimaryKey), true);
            return a.Length > 0;
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

        bool IsUnique(MemberInfo f)
        {
            object[] a = f.GetCustomAttributes(typeof(Unique), true);
            return a.Length > 0;
        }

        public Collection(string a_path)
        {
            Init(a_path, typeof(T).Name);
        }

        public Collection(string a_path, string a_table)
        {
            Init(a_path, a_table);
        }

        public Collection(DbConnection connection)
        {
            Init((SQLiteConnection) connection, typeof(T).Name);
        }

        public Collection(DbConnection connection, string a_table)
        {
            Init((SQLiteConnection) connection, a_table);
        }

        public void SetParameters(DbCommand dbCommand, T parameters)
        {
            SQLiteCommand command = (SQLiteCommand)dbCommand;
            SetFieldParams(command, parameters);
            SetRowIdParam(command, GetRowId(parameters));
        }

        public DbCommand CreateCommand(string sql)
        {
            SQLiteCommand command = connection.CreateCommand();
            AddFieldParams(command);
            AddRowIdParam(command);
            sql = sql.Replace("@table", table);
            command.CommandText = sql;
            return command;
        }

        void Init(string a_path, string a_table)
        {
            path = a_path;
            SQLiteConnectionStringBuilder b = new SQLiteConnectionStringBuilder();
            b.DataSource = path;
            b.DateTimeFormat = SQLiteDateFormats.ISO8601;
            b.UseUTF16Encoding = true;
            bool create = !File.Exists(path);
            if (create)
            {
                Directory.CreateDirectory(Path.GetDirectoryName(path));
            }
            SQLiteConnection connection = new SQLiteConnection(b.ToString());
            connection.Open();
            Init(connection, a_table);
        }

        void Init(SQLiteConnection a_connection, string a_table)
        {
            connection = a_connection;

            rowIdMember = typeof(T).GetMembers().FirstOrDefault(x => IsRowId(x));
            if (rowIdMember == null)
            {
                throw new InvalidDataException("Need [RowId] attribute.");
            }
            rowIdColumn = "oid";

            members = typeof(T).GetMembers().Where(x => IsDataField(x)).ToArray();

            table = a_table;

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

            insert = connection.CreateCommand();
            insert.CommandText = String.Format("insert or replace into {0} values({1})",
                table,
                Members.Select(x => x.Name)).Join(", "),
                Members.Select(x => String.Format("@{0}", x.Name)).Join(", ")
                );
            AddFieldParams(insert);

            select = connection.CreateCommand();
            select.CommandText = String.Format("select {0} from {1} where @query", rowIdColumn, table);
            select.Parameters.Add(new SQLiteParameter("query"));

            containsQuery = connection.CreateCommand();
            containsQuery.CommandText = String.Format("select {0} from {1} where {2}",
                rowIdColumn, table,
                String.Join(" AND ", Members.Select(x => String.Format("{0} = @{0}", x.Name)).ToArray())
                );
            AddFieldParams(containsQuery);

            getDataByRowId = connection.CreateCommand();
            getDataByRowId.CommandText =
                String.Format("select {0} from {1} where {2} = @{2}",
                SelectFieldList, table, rowIdColumn);
            AddRowIdParam(getDataByRowId);

            updateDataByRowId = connection.CreateCommand();
            updateDataByRowId.CommandText = String.Format(
                "update {0} set {1} where {2} = @{2}",
                table,
                FieldAssignments,
                rowIdColumn);
            AddFieldParams(updateDataByRowId);
            AddRowIdParam(updateDataByRowId);

            deleteByRowId = connection.CreateCommand();
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

        void CreateTable()
        {
            string sql = String.Format("create table {0} ({1})", table, FieldDefinition);
            SQLiteCommand c = connection.CreateCommand();
            c.CommandText = sql;
            c.ExecuteNonQuery();
            CreateIndex();
        }

        void CreateIndex()
        {
            foreach (MemberInfo i in Members)
            {
                if (IsIndexed(i))
                {
                    SQLiteCommand c = connection.CreateCommand();
                    string fields = i.Name;
                    Indexed indexed = (Indexed)i.GetCustomAttributes(typeof(Indexed), false)[0];
                    if (indexed.Fields != null)
                    {
                        fields = indexed.Fields;
                    }
                    c.CommandText = String.Format(
                        "create {2} index if not exists {0}_{1} on {0} ({3})",
                        table, 
                        i.Name, 
                        IsUnique(i) ? "unique" : String.Empty, 
                        fields);
                    c.ExecuteNonQuery();

                    
                    c.CommandText = String.Format("create index if not exists {0}_{1} on {0} ({1})",
                        table, i.Name);
                    c.ExecuteNonQuery();
                }
            }
        }

        bool TableExists(string table)
        {
            try
            {
                SQLiteCommand c = connection.CreateCommand();
                c.CommandText = String.Format("select * from {0} limit 1", table);
                c.ExecuteScalar();
                return true;
            }
            catch (SQLiteException)
            {
                return false;
            }
        }

        bool CheckTableSchema()
        {
            foreach (MemberInfo i in Members)
            {
                SQLiteCommand c = connection.CreateCommand();
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
                string[] names = Members.Select(x => x.Name).ToArray();
                return String.Join(", ", names);
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

        public DbTransaction BeginTransaction()
        {
            return connection.BeginTransaction();
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
        public IList<T> Query(string query)
        {
            SQLiteCommand command = connection.CreateCommand();
            command.CommandText = query;
            return DoSelect(command);
        }

        public T Find(string query)
        {
            List<long> ids = new List<long>();
            select = connection.CreateCommand();
            select.CommandText = String.Format("select {0} from {1} where {2}", rowIdColumn, table, query);
            SQLiteDataReader reader = select.ExecuteReader();
            try
            {
                while (reader.Read())
                {
                    return this[(long)reader[0]];
                }
                return default(T);
            }
            finally
            {
                reader.Close();
            }
        }

        T Find(DbCommand select)
        {
            SQLiteDataReader reader = (SQLiteDataReader)select.ExecuteReader();
            try
            {
                while (reader.Read())
                {
                    return this[(long)reader[0]];
                }
                return default(T);
            }
            finally
            {
                reader.Close();
            }
        }

        public T Find(string query, string paramName, object param)
        {
            DbCommand select = CreateCommand(String.Format("select {0} from @table where {1}", rowIdColumn, query));
            SQLiteParameter p = new SQLiteParameter(paramName);
            select.Parameters.Add(p);
            select.Parameters[paramName].Value = param;
            return Find(select);
        }

        ResultProxy<T> DoSelect(string query)
        {
            List<long> ids = new List<long>();

            select = connection.CreateCommand();
            select.CommandText = String.Format("select {0} from {1} where {2}", "oid", table, query);
            SQLiteDataReader reader = select.ExecuteReader();
            while (reader.Read())
            {
                ids.Add((long)reader[0]);
            }
            reader.Close();
            return new ResultProxy<T>(this, ids);
        }

        ResultProxy<T> DoSelect(DbCommand select)
        {
            List<long> ids = new List<long>();
            SQLiteDataReader reader = ((SQLiteCommand)select).ExecuteReader();
            while (reader.Read())
            {
                ids.Add((long)reader[0]);
            }
            reader.Close();
            return new ResultProxy<T>(this, ids);
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
            SQLiteDataReader reader = getDataByRowId.ExecuteReader();
            try
            {
                reader.Read();
                T result = FromReader(reader);
                return result;
            }
            finally
            {
                reader.Close();
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
                if (reader.IsDBNull(index))
                {
                    i.SetValue(item, null);
                }
                else if (i.FieldType() == typeof(DateTime))
                {
                    i.SetValue(item, reader.GetDateTime(index));
                }
                else if (i.FieldType() == typeof(bool))
                {
                    i.SetValue(item, reader.GetBoolean(index));
                }
                else if (i.FieldType() == typeof(decimal))
                {
                    i.SetValue(item, reader.GetDecimal(index));
                }
                else if (i.FieldType() == typeof(short))
                {
                    i.SetValue(item, reader.GetInt16(index));
                }
                else if (i.FieldType() == typeof(int))
                {
                    i.SetValue(item, reader.GetInt32(index));
                }
                else if (i.FieldType() == typeof(long))
                {
                    i.SetValue(item, reader.GetInt64(index));
                }
                else if (i.FieldType() == typeof(Guid))
                {
                    i.SetValue(item, reader.GetGuid(index));
                }
                else
                {
                    i.SetValue(item, reader[index]);
                }
            }
            catch (Exception)
            {
            }
        }

        #region ICollection<T> Members

        public void Add(T item)
        {
            SetFieldParams(insert, item);
            insert.ExecuteNonQuery();
        }

        public void Clear()
        {
            DbCommand clear = connection.CreateCommand();
            clear.CommandText = String.Format("delete from {0}", table);
            clear.ExecuteNonQuery();
        }

        public bool Contains(T item)
        {
            SetFieldParams(containsQuery, item);
            IList<T> result = DoSelect(containsQuery);
            return result.Count > 0;
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            foreach (T i in this)
            {
                array[arrayIndex++] = i;
            }
        }

        public int Count
        {
            get
            {
                DbCommand c = connection.CreateCommand();
                c.CommandText = String.Format("select count(*) from {0}", table);
                object result = c.ExecuteScalar();
                return (int)(long)result;
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

        public IEnumerator<T> GetEnumerator()
        {
            SQLiteCommand all = connection.CreateCommand();
            all.CommandText = String.Format("select {0} from {1};",
                SelectFieldList, table);
            SQLiteDataReader reader = all.ExecuteReader();
            while (reader.Read())
            {
                yield return FromReader(reader);
            }
            reader.Close();
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
            string path = System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData);
            string an = typeof(T).Assembly.GetName().Name;
            path = Path.Combine(path, an);
            path = Path.Combine(path, an + ".sqlite");
            return new Collection<T>(path);
        }

        private void Sql(string sql)
        {
            DbCommand c = connection.CreateCommand();
            c.CommandText = sql;
            c.ExecuteNonQuery();
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

    }
}
