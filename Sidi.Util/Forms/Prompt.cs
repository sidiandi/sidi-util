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
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;
using System.Reflection;
using Sidi.Util;
using System.Diagnostics;
using Sidi.IO;
using Sidi.Extensions;

namespace Sidi.Forms
{
    public interface IColumnInfo
    {
        string Name { get; }
        string Value(object item, int index);
    }

    [Serializable]
    public class ColumnInfo<T> : IColumnInfo
    {
        string name;
        Func<T, int, string> stringifier;

        public ColumnInfo(string name, Func<T, int, string> stringifier)
        {
            this.name = name;
            this.stringifier = stringifier;
        }

        public static IEnumerable<IColumnInfo> AllProperties()
        {
            return typeof(T).GetProperties().Select(p =>
            {
                return new ColumnInfo<T>(p.Name, (index, x) => Sidi.Forms.Support.SafeToString(p.GetValue(x, new object[] { })));
            }).Cast<IColumnInfo>();
        }

        public string Name
        {
            get { return name; }
        }

        public string Value(object item, int index)
        {
            try
            {
                return stringifier((T)item, index);
            }
            catch (Exception)
            {
                return "?";
            }
        }
    }

    public static class ColumnInfoExtensions
    {
        public static IEnumerable<IColumnInfo> AllProperties<T>(this IEnumerable<T> e)
        {
            return typeof(T).GetProperties().Select(p =>
            {
                return new ColumnInfo<T>(p.Name, (index, x) => Sidi.Forms.Support.SafeToString(p.GetValue(x, new object[] { })));
            }).Cast<IColumnInfo>();
        }

        public static IEnumerable<IColumnInfo> Properties<T>(this IEnumerable<T> e, params string[] propertyNames)
        {
            return propertyNames.Select(pn =>
            {
                var p = typeof(T).GetProperty(pn);
                if (p == null)
                {
                    throw new Exception("{0} does not have a {1} property".F(typeof(T), pn));
                }
                return new ColumnInfo<T>(p.Name, (index, x) => Sidi.Forms.Support.SafeToString(p.GetValue(x, new object[] { })));
            }).Cast<IColumnInfo>();
        }
    }

    public class Prompt
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static string EditInteractive(string text)
        {
            Sidi.IO.LPath tf = null;
            try
            {
                tf = Sidi.IO.LPath.GetTempFileName();
                tf.WriteAllText(text);

                using (var p = new Process())
                {
                    var fileName = new Sidi.IO.LPath(
                        Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles)).CatDir(
                        "Notepad++", "notepad++.exe");

                    p.StartInfo.FileName = fileName.StringRepresentation;
                    p.StartInfo.Arguments = "-multiInst -nosession " + tf.Quote();

                    if (!fileName.Exists)
                    {
                        p.StartInfo.FileName = "notepad.exe";
                        p.StartInfo.Arguments = tf.Quote();
                    }

                    p.Start();
                    log.Info(p.DetailedInfo());
                    p.WaitForExit();
                    return tf.ReadAllText();
                }
            }
            finally
            {
                try
                {
                    tf.EnsureNotExists();
                }
                catch
                {
                }
            }
        }

        public static void OnActivate<T>(ListFormat<T> list, Action<T> action, string title = null)
        {
            using (var d = new ChooseDialog())
            {
                d.Text = title == null ? "Pick Item" : title;

                d.Objects = list.Data.Cast<object>();
                d.Columns = list.Columns.Select(c => new ColumnInfo<T>(c.Name, c.GetText)).ToList();
                d.ObjectSelected += (s, e) =>
                    {
                        action((T)d.SelectedObject);
                    };

                d.ShowDialog();
            }
        }

        public static T ChooseOne<T>(ListFormat<T> list, string title = null)
        {
            list.DefaultColumns();

            using (var d = new ChooseDialog())
            {
                if (title != null)
                {
                    d.Text = title;
                }
                
                d.Objects = list.Data.Cast<object>();
                d.Columns = list.Columns.Select(c => new ColumnInfo<T>(c.Name, c.GetText)).ToList();
                if (d.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    return (T)d.SelectedObject;
                }
                else
                {
                    throw new Exception("Selection cancelled.");
                }
            }
        }

        public static IList<T> ChooseMany<T>(ListFormat<T> list, string title = null)
        {
            using (var d = new ChooseDialog())
            {
                list.DefaultColumns();
                d.Text = title == null ? "Choose Many" : title;

                d.Objects = list.Data.Cast<object>();
                d.Columns = list.Columns.Select(c => new ColumnInfo<T>(c.Name, c.GetText)).ToList();
                d.listViewObjects.MultiSelect = true;
                if (d.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    return d.listViewObjects.SelectedObjects.Cast<T>().ToList();
                }
                else
                {
                    throw new Exception("Selection cancelled.");
                }
            }
        }

        public static bool AskYesNo(string question)
        {
            return (MessageBox.Show(question, "DevTools", MessageBoxButtons.YesNo) == DialogResult.Yes);
        }

        public static string GetText(string question)
        {
            Console.WriteLine(question);
            return Console.ReadLine();
        }
    }
}
