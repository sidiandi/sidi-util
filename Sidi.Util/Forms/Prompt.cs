using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;
using System.Reflection;
using Sidi.Util;

namespace Sidi.Forms
{
    public interface IColumnInfo
    {
        string Name { get; }
        string Value(object item);
    }

    public class ColumnInfo<T> : IColumnInfo
    {
        string name;
        Func<T, string> stringifier;

        public ColumnInfo(string name, Func<T, string> stringifier)
        {
            this.name = name;
            this.stringifier = stringifier;
        }

        public static IEnumerable<IColumnInfo> AllProperties()
        {
            return typeof(T).GetProperties().Select(p =>
            {
                return new ColumnInfo<T>(p.Name, x => Sidi.Forms.Support.SafeToString(p.GetValue(x, new object[] { })));
            }).Cast<IColumnInfo>();
        }

        public string Name
        {
            get { return name; }
        }

        public string Value(object item)
        {
            try
            {
                return stringifier((T)item);
            }
            catch (Exception)
            {
                return "?";
            }
        }
    }

    public static class ColumnInfoEx
    {
        public static IEnumerable<IColumnInfo> AllProperties<T>(this IEnumerable<T> e)
        {
            return typeof(T).GetProperties().Select(p =>
            {
                return new ColumnInfo<T>(p.Name, x => Sidi.Forms.Support.SafeToString(p.GetValue(x, new object[] { })));
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
                return new ColumnInfo<T>(p.Name, x => Sidi.Forms.Support.SafeToString(p.GetValue(x, new object[] { })));
            }).Cast<IColumnInfo>();
        }
    }

    public class Prompt
    {
        public static T ChooseOne<T>(IEnumerable<T> list)
        {
            var d = new ChooseOneDialog();
            d.Objects = list.Cast<object>();
            if (d.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                return (T) d.SelectedObject;
            }
            else
            {
                return default(T);
            }
        }

        public static void Choose<T>(IEnumerable<T> list, IEnumerable<IColumnInfo> columns, Action<T> action)
        {
            var d = new ChooseOneDialog();
            d.Columns = columns;
            d.Objects = list.Cast<object>();
            d.ObjectSelected += new EventHandler((o, e) =>
            {
                var item = (T)d.SelectedObject;
                action(item);
            });
            d.ShowDialog();
        }
        
        public static T ChooseOne<T>(IEnumerable<T> list, IEnumerable<IColumnInfo> columns)
        {
            var d = new ChooseOneDialog();
            d.Columns = columns;
            d.Objects = list.Cast<object>();
            if (d.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                return (T)d.SelectedObject;
            }
            else
            {
                return default(T);
            }
        }

        public static IEnumerable<T> SelectObjects<T>(IEnumerable<T> objects, string caption)
        {
            return SelectObjects(objects.Cast<object>(), caption, x => x.ToString()).Cast<T>();
        }
        
        public static IEnumerable<object> SelectObjects(IEnumerable<object> objects, string caption, Func<object, string> stringifier)
        {
            SelectObjectsDialog d = new SelectObjectsDialog();
            d.Text = caption;
            d.Columns = new[] { new ColumnInfo<object>("Item", x => Sidi.Forms.Support.SafeToString(x)) };
            d.Objects = objects;
            if (d.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                return d.SelectedObjects;
            }
            else
            {
                return new List<object>();
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
