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

namespace Sidi.Persistence
{
    [TestFixture, Ignore("does not yet work")]
    public class CollectionTest
    {
        class OtherData
        {
            [RowId]
            public long Id = 0;
            [Data]
            public string Name = null;
            [Data]
            public string DateOfBirth = null;
        }

        class Address
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
                return String.Format("{0} {1} {2} {3} {4}",
                    Id, Name, Street, City, Zip, LastContact);
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

        string path = Sidi.IO.FileUtil.BinFile(@"test-data\Sidi.Persistence.Test.sqlite");
        string table = "a";
        int count = 10;

        Sidi.Persistence.Collection<Address> addressBook;
        
        [SetUp]
        public void Init()
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
            addressBook = new Sidi.Persistence.Collection<Address>(path, table);
            Write();
        }

        [TearDown]
        public void Deinit()
        {
            addressBook.Close();
        }

        [Test]
        public void AlterTable()
        {
            string tableName = "alter_table_test";
            var a = new Sidi.Persistence.Collection<AddressSubset>(path, tableName);

            a.Add(new AddressSubset());

            var b = new Sidi.Persistence.Collection<Address>(path, tableName);

            b.Add(new Address());
        }
        
        [Test]
        public void Create()
        {
            Sidi.Persistence.Collection<Address> addressBook = new Sidi.Persistence.Collection<Address>(path, table);
        }

        [Test]
        public void Count()
        {
            Assert.That(addressBook.Count == count);
        }

        [Test]
        public void Write()
        {
            DbTransaction t = addressBook.BeginTransaction();
            addressBook.Clear();
            Random r = new Random();
            for (int i = 0; i < count; ++i)
            {
                Address a = new Address();
                a.Name = "Andreas";
                a.Street = "Some Street";
                a.City = "Nowhere";
                a.Zip = i;
                a.Income = r.NextDouble();
                addressBook.Add(a);
            }
            t.Commit();
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
            Address a = addressBook.Find("Name = 'Andreas'");
            a.Name = a.Name + " (updated)";
            addressBook.Update(a);

            Address b = addressBook.Find("Name like '%(updated)%'");
            Assert.That(b != null);
        }

        [Test]
        public void AutoTableName()
        {
            Sidi.Persistence.Collection<OtherData> otherData = new Sidi.Persistence.Collection<OtherData>(path);
        }

        [Test]
        public void Schema()
        {
            Sidi.Persistence.Collection<OtherData> otherData = new Sidi.Persistence.Collection<OtherData>(path);
            DbCommand c = otherData.Connection.CreateCommand();
            c.CommandText = "select * from sqlite_master;";
            DbDataReader reader = c.ExecuteReader();
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

        [Test]
        public void Subset()
        {
            Sidi.Persistence.Collection<AddressSubset> subset = new Sidi.Persistence.Collection<AddressSubset>(path, table);
            foreach (AddressSubset i in subset)
            {
                Console.WriteLine(i);
            }
        }

        [Test]
        public void DataTypes()
        {
            Sidi.Persistence.Collection<AllDataTypes> c = new Sidi.Persistence.Collection<AllDataTypes>(path);
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
            DbCommand c = addressBook.CreateCommand("update @table set Zip = @Zip");
            addressBook.SetParameters(c, a);
            c.ExecuteNonQuery();
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
    }
}
