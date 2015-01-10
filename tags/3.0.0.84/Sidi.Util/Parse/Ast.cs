using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sidi.Parse
{
    /// <summary>
    /// Abstract syntax tree (AST).
    /// </summary>
    public class Ast
    {
        public Ast(Text text, IEnumerable<Ast> childs = null)
        {
            if (childs == null)
            {
                Childs = new Ast[] { };
            }
            else
            {
                Childs = childs.ToArray();
            }
            Text = text;
        }

        public Ast[] Childs { get; set; }
        public Text Text { get; set; }
        public object Name { get; set; }

        public override string ToString()
        {
            return Text.ToString();
        }
        
        string Info
        {
            get
            {
                return String.Format("{0}: {1}", Name, Text.Substring(0, Math.Min(32, Text.Length)));
            }
        }

        /// <summary>
        /// Dump the whole abstract syntax tree
        /// </summary>
        /// <param name="o"></param>
        /// <param name="indent"></param>
        public void Dump(TextWriter o, string indent = null)
        {
            if (indent == null)
            {
                indent = String.Empty;
            }

            o.WriteLine("{0}{1}", indent, Info);
            indent = indent + "|";
            foreach (var i in Childs)
            {
                i.Dump(o, indent);
            }
        }

        public string Details
        {
            get
            {
                using (var s = new StringWriter())
                {
                    Dump(s);
                    return s.ToString();
                }
            }
        }

        public Ast this[int index]
        {
            get { return Childs[index]; }
        }

        public Ast this[string name]
        {
            get {
                try
                {
                    return Childs.First(_ => _.Name != null && _.Name.Equals(name));
                }
                catch (InvalidOperationException)
                {
                    throw new ArgumentOutOfRangeException("name", name, Details);
                }
            }
        }
    }
}
