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
using System.IO;
using System.Data;
using System.Data.Common;
using NUnit.Framework;
using Sidi.Persistence;
using System.Threading;
using Sidi.IO;
using System.Linq;
using L = Sidi.IO;
using System.Linq.Expressions;
using System.Data.SQLite;
using Sidi.Test;

namespace Sidi.Persistence
{
    [TestFixture]
    public class SqliteTest : TestBase
    {
        [Test]
        public void OpenClose()
        {
            var dbFile = TestFile(@"test.sqlite");
            dbFile.EnsureNotExists();
            var cs = new SQLiteConnectionStringBuilder();
            cs.DataSource = dbFile.StringRepresentation;
            var c = new SQLiteConnection(cs.ConnectionString);
            c.Close();
            dbFile.EnsureNotExists();
        }

        [Test]
        public void OpenCloseCollection()
        {
            for (int i = 0; i < 20; ++i)
            {
                var dbFile = TestFile(@"test.sqlite");
                dbFile.EnsureNotExists();
                using (var c = new Collection<CollectionTest.Address>(dbFile))
                {
                }
                dbFile.EnsureNotExists();
            }
        }
    }
    
    [TestFixture]
    public class CollectionTest : TestBase
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public CollectionTest()
        {
            testFile = TestFile("Sidi.Persistence.Test.sqlite");
        }
        
        class OtherData
        {
            [RowId]
            public long Id = 0;
            [Data]
            public string Name = null;
            [Data]
            public string DateOfBirth = null;
        }

        public class Address
        {
            [RowId]
            public long Id = 0;
            [Data]
            public string Name;
            [Data]
            public string Street;
            [Data]
            public string City;
            [Data, Indexed]
            public long Zip;
            [Data]
            public double Income;
            [Data]
            public DateTime LastContact = DateTime.Now;

            public override string ToString()
            {
                return String.Join(" ", Id, Name, Street, City, Zip, LastContact);
            }
        }

        class AddressSubset
        {
            [RowId]
            public long Id = 0;
            [Data]
            public string Name = null;
            [Data]
            public DateTime LastContact = DateTime.Now;

            public override string ToString()
            {
                return String.Format("{0} {1}", Name, LastContact);
            }
        }

        class AllDataTypes
        {
            [RowId]
            public long Id = 0;
            [Data]
            public bool aBool = true;
            [Data]
            public decimal aDecimal = 0.123M;
            [Data]
            public short aShort = Int16.MaxValue;
            [Data]
            public int aInt = Int32.MaxValue;
            [Data]
            public long aLong = Int64.MaxValue;
            [Data]
            public double aDouble = Double.MaxValue;
            [Data]
            public string aString = "some default value with umlauts ÄÜÖäüöß";
            [Data]
            public DateTime aDateTime = DateTime.Now;
            [Data]
            public Guid aGuid = Guid.NewGuid();
            [Data]
            public byte[] aByteArray = System.Text.ASCIIEncoding.ASCII.GetBytes("Hello, World!");
        }

        L.LPath testFile;
        string table = "a";
        int count = 10;

        Sidi.Persistence.Collection<Address> addressBook;
        
        [SetUp]
        public void Init()
        {
            testFile.EnsureNotExists();
            addressBook = new Sidi.Persistence.Collection<Address>(testFile, table);
            Write();
        }

        [TearDown]
        public void Deinit()
        {
            addressBook.Dispose();
            addressBook = null;
        }

        [Test]
        public void Get()
        {
            var c = addressBook.ToList();
        }

        [Test]
        public void AlterTable()
        {
            string tableName = "alter_table_test";
            using (var a = new Collection<AddressSubset>(addressBook.SharedConnection, tableName))
            {
                a.Add(new AddressSubset());
            }

            using (var b = new Collection<Address>(addressBook.SharedConnection, tableName))
            {
                b.Add(new Address());
            }
        }
        
        [Test]
        public void Count()
        {
            Assert.That(addressBook.Count == count);
        }

        [Test]
        public void Write()
        {
            using (var t = addressBook.BeginTransaction())
            {
                addressBook.Clear();
                Random r = new Random();
                for (int i = 0; i < count; ++i)
                {
                    Address a = new Address();
                    a.Name = "Bert";
                    a.Street = "Some Street";
                    a.City = "Nowhere";
                    a.Zip = i;
                    a.Income = r.NextDouble();
                    addressBook.Add(a);
                }
                t.Commit();
            }
        }

        [Test]
        public void Read()
        {
            int c = 0;
            foreach (Address i in addressBook.Select("1"))
            {
                ++c;
            }
            Assert.That(c == count);
        }

        [Test]
        public void ReadAll()
        {
            int c = 0;
            foreach (Address i in addressBook)
            {
                ++c;
            }
            Assert.That(c == count);
        }

        [Test]
        public void QueryByLambdaExpressions()
        {
            var r = addressBook.Query(a => a.Zip == 3);
            Assert.AreEqual(1, r.Count());

            r = addressBook.Query(a => a.Name == "Bert");
            Assert.AreEqual(count, r.Count);

            r = addressBook.Query(a => a.Name == "Bert" && a.Zip == 3);
            Assert.AreEqual(1, r.Count);

            var entry = addressBook.Find(a => a.Zip == 3);
            Assert.AreEqual(3, entry.Zip);
        }

        [Test]
        public void Select()
        {
            IList<Address> searchResult = addressBook.Select(String.Format("Zip > {0} order by Zip", count - 5));

            for (int n = 0; n < searchResult.Count; ++n)
            {
                Console.WriteLine(searchResult[n]);
            }

            foreach (Address i in searchResult)
            {
                Console.WriteLine(i);
            }
        }

        [Test]
        public void Clear()
        {
            addressBook.Clear();
            Assert.That(addressBook.Count == 0);
        }

        [Test]
        public void CopyTo()
        {
            Address[] a = new Address[addressBook.Count];
            addressBook.CopyTo(a, 0);
        }

        [Test]
        public void Update()
        {
            Address a = addressBook.Find("Name = 'Bert'");
            a.Name = a.Name + " (updated)";
            addressBook.Update(a);

            Address b = addressBook.Find("Name like '%(updated)%'");
            Assert.That(b != null);
        }

        [Test]
        public void AutoTableName()
        {
            using (Sidi.Persistence.Collection<OtherData> otherData = new Sidi.Persistence.Collection<OtherData>(addressBook.SharedConnection))
            {
            }
        }

        [Test]
        public void Schema()
        {
            using (var otherData = new Sidi.Persistence.Collection<OtherData>(addressBook.SharedConnection))
            {
                using (var c = otherData.Connection.CreateCommand())
                {
                    c.CommandText = "select * from sqlite_master;";
                    using (var reader = c.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            for (int i = 0; i < reader.FieldCount; ++i)
                            {
                                Console.WriteLine(String.Format("{0}: {1}",
                                    reader.GetName(i),
                                    reader[i]));
                            }
                        }
                    }
                }
            }
        }

        [Test]
        public void Subset()
        {
            using (var subset = new Sidi.Persistence.Collection<AddressSubset>(addressBook.SharedConnection, table))
            {
                foreach (AddressSubset i in subset)
                {
                    Console.WriteLine(i);
                }
            }
        }

        [Test]
        public void DataTypes()
        {
            using (Sidi.Persistence.Collection<AllDataTypes> c = new Sidi.Persistence.Collection<AllDataTypes>(addressBook.SharedConnection))
            {
                c.Clear();
                using (DbTransaction t = c.BeginTransaction())
                {
                    for (int i = 0; i < count; ++i)
                    {
                        AllDataTypes newItem = new AllDataTypes();
                        c.Add(newItem);
                    }
                    t.Commit();
                }

                AllDataTypes def = new AllDataTypes();
                foreach (AllDataTypes item in c)
                {
                    Assert.That(item.aDecimal == def.aDecimal);

                    Assert.That(item.aBool == def.aBool);
                    Assert.That(item.aDecimal == def.aDecimal);
                    Assert.That(item.aShort == def.aShort);
                    Assert.That(item.aInt == def.aInt);
                    Assert.That(item.aLong == def.aLong);
                    Assert.That(item.aDouble == def.aDouble);
                    Assert.AreEqual(item.aString, def.aString);
                }
            }
        }

        [Test]
        public void UserSetting()
        {
            Sidi.Persistence.Collection<Address> a = Sidi.Persistence.Collection<Address>.UserSetting();
        }

        [Test]
        public void Execute()
        {
            Address a = new Address();
            a.Zip = 123456;
            using (var c = addressBook.CreateCommand("update @table set Zip = @Zip"))
            {
                addressBook.SetParameters(c, a);
                c.ExecuteNonQuery();
            }
            IList<Address> s = addressBook.Select("Zip = " + a.Zip);
            Assert.AreEqual(s.Count, count);
        }

        [Test]
        public void Find()
        {
            string city = "Nowhere";
            Address a = addressBook.Find("City = @City", "City", city);
            Assert.AreEqual(a.City, city);
        }

        public class WithPrimaryKey
        {
            [Data, RowId]
            public long Id;

            [Data]
            public string Name;
        }

        [Test]
        public void PrimaryKeyData()
        {
            var path = TestFile(@"test-data\primarykey.sqlite");
            path.EnsureNotExists();
            var c = new Collection<WithPrimaryKey>(path);

            var d = new WithPrimaryKey();
            d.Id = 123;
            d.Name = "sidi";
            c.Add(d);

            d.Id = 456;
            d.Name = "util";
            c.Add(d);

            d.Id = 456;
            d.Name = "util2";
            c.Add(d);

            Assert.AreEqual(2, c.Count);
            c.Remove(d);
            Assert.AreEqual(1, c.Count);

            foreach (var i in c)
            {
                log.InfoFormat("{0} {1}", i.Id, i.Name);
            }

            var name = "sidi";
            var cmd = c.CreateCommand("select oid from @table where Name = {0}", name);
            Assert.AreEqual(name, c.Query(cmd)[0].Name);
        }

        [Test]
        public void ThreadSafety()
        {
            var p = TestFile("thread_safety.sqlite");
            p.EnsureNotExists();
            using (var addresses = new Collection<Address>(p))
            {
                var threadCount = 10;
                var insertCount = 10;
                var threads = Enumerable.Range(0, threadCount)
                    .Select(n =>
                        {
                            var t = new Thread(() =>
                                {
                                    for (int i = 0; i < insertCount; ++i)
                                    {
                                        lock (addresses)
                                        {
                                            log.InfoFormat("thread {0}: insert", Thread.CurrentThread.ManagedThreadId);
                                            addresses.Add(new Address()
                                            {
                                                Name = "test",
                                            });
                                        }
                                    }
                                });
                            t.Start();
                            return t;
                        })
                        .ToList();

                threads.ForEach(t => t.Join());

                Assert.AreEqual(threadCount * insertCount, addresses.Count);
            }
        }

        [Test]
        public void ThreadSafety2()
        {
            var p = TestFile("thread_safety.sqlite");
            p.EnsureNotExists();

            var threadCount = 10;
            var insertCount = 10;
            var threads = Enumerable.Range(0, threadCount)
                .Select(n =>
                {
                    var t = new Thread(() =>
                    {
                        using (var addresses = new Collection<Address>(p))
                        {
                            for (int i = 0; i < insertCount; ++i)
                            {
                                log.InfoFormat("Thread {0}: add", Thread.CurrentThread.ManagedThreadId);
                                addresses.Add(new Address()
                                {
                                    Name = "test",
                                });
                            }
                        }
                    });
                    t.Start();
                    return t;
                })
                    .ToList();

            threads.ForEach(t => t.Join());

            var db = new Collection<Address>(p);
            Assert.AreEqual(threadCount * insertCount, db.Count);
        }

        public class FileInfo
        {
            [RowId]
            public long Id;

            [Data]
            public L.LPath Path;
        }

        [Test]
        public void Path()
        {
            var p = TestFile("path.sqlite");
            p.EnsureNotExists();
            var db = new Collection<FileInfo>(p);
            db.Add(new FileInfo()
            {
                Path = new L.LPath(@"C:\temp")
            });

            Assert.AreEqual(new L.LPath(@"C:\temp"), db.First().Path);
        }

        [Test]
        public void SqlExpression()
        {
            var a = new Address();
            Expression<Func<Address, bool>> f = i => i.Zip == 3;
            Assert.AreEqual("(Zip = 3)", Collection<Address>.SqlPredicate(f.Body));

            f = i => i.Zip == 3 && i.Name == "Bert";
            Assert.AreEqual("((Zip = 3) AND (Name = \"Bert\"))", Collection<Address>.SqlPredicate(f.Body));
        }

        [Test]
        public void SqlExpressionResolve()
        {
            var bert = "Bert";
            Expression<Func<Address, bool>> f = i => i.Zip == 3 && i.Name == bert;
            Assert.AreEqual("((Zip = 3) AND (Name = \"Bert\"))", Collection<Address>.SqlPredicate(f.Body));

            f = i => i.Zip == (1 + 2);
            Assert.AreEqual("(Zip = 3)", Collection<Address>.SqlPredicate(f.Body));

            f = i => i.Name == bert + bert;
            Assert.AreEqual("(Name = \"BertBert\")", Collection<Address>.SqlPredicate(f.Body));

            f = i => i.Name != bert;
            Assert.AreEqual("(Name != \"Bert\")", Collection<Address>.SqlPredicate(f.Body));
        }

        [Test, Explicit("todo")]
        public void QueryDateTime()
        {
            var x = DateTime.Now;
            using (var c = new Collection<AllDataTypes>(testFile))
            {
                c.Add(new AllDataTypes() { aDateTime = x });

                var r = c.Query(dt => dt.aDateTime == x);
                Assert.AreEqual(1, r.Count);
            }
        }

        [Indexed(Fields = new []{"A", "B", "C"}, Unique=true) ]
        class IndexedData
        {
            [RowId]
            public long oid = 0;

            [Data]
            public string A;

            [Data]
            public string B;

            [Data]
            public string C;
        }

        [Test]
        public void Index()
        {
            using (var c = new Collection<IndexedData>(testFile))
            {
                c.Add(new IndexedData() { A = "A", B = "B", C = "C" });
                c.Add(new IndexedData() { A = "A", B = "B", C = "C" });
                Assert.AreEqual(1, c.Count);
            }
        }
    }
}
