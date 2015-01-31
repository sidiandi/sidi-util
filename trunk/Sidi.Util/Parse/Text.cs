using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sidi.Parse
{
    /// <summary>
    /// Section of a string.
    /// </summary>
    public class Text : IEquatable<Text>
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public Text(IEnumerable<Text> texts)
        {
            foreach (var i in texts)
            {
                if (i.IsEmpty)
                {
                    continue;
                }

                if (this.text == null)
                {
                    this.text = i.text;
                    this.begin = i.begin;
                    this.end = i.end;
                }
                else
                {
                    if (this.text == i.text && this.end == i.begin)
                    {
                        this.end = i.end;
                    }
                    else
                    {
                        this.text = this.AsString() + i.AsString();
                        this.begin = 0;
                        this.end = text.Length;
                    }
                }
            }
        }

        public Text(string text)
        {
            this.text = text;
            this.begin = 0;
            this.end = text.Length;
        }

        public Text(string text, int begin, int end)
        {
            this.text = text;
            this.begin = begin;
            this.end = end;
        }

        string text;
        int begin;
        int end;

        public override string ToString()
        {
            return AsString();
        }

        public string AsString()
        {
            return text.Substring(begin, end - begin);
        }

        public Text Part(int begin)
        {
            return new Text(this.text, this.begin + begin, this.end);
        }

        public Text Remove(Text subText)
        {
            return new Text(text, begin, subText.begin);
        }

        public string Substring(int begin, int length)
        {
            return text.Substring(this.begin + begin, length);
        }

        public Text Part(int begin, int end)
        {
            return new Text(this.text, this.begin + begin, this.begin + end);
        }

        public Text Copy()
        {
            return new Text(text, begin, end);
        }

        public int Length
        {
            get { return end - begin; }
        }

        public bool IsEmpty
        {
            get { return Length == 0; }
        }

        public char this[int index]
        {
            get
            {
                return text[begin + index];
            }
        }

        public void Set(Text r)
        {
            this.text = r.text;
            this.begin = r.begin;
            this.end = r.end;
        }

        public static Text Union(IEnumerable<Text> p)
        {
            if (p == null)
            {
                return null;
            }

            var s = p.First().text;
            var begin = s.Length;
            var end = 0;

            foreach (var i in p)
            {
                begin = Math.Min(begin, i.begin);
                end = Math.Max(end, i.end);
                if (i.text != s)
                {
                    throw new ArgumentOutOfRangeException("must have the same base string");
                }
            }
            return new Text(s, begin, end);
        }

        public bool Equals(Text other)
        {
            if (this.text == other.text)
            {
                if (this.begin == other.begin && this.end == other.end)
                {
                    return true;
                }
            }

            return this.AsString().Equals(other.AsString());
        }

        public override bool Equals(object obj)
        {
            var other = obj as Text;
            if (other == null)
            {
                return false;
            }
            return this.Equals(other);
        }

        public override int GetHashCode()
        {
            return AsString().GetHashCode();
        }
    }
}
