using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sidi.Util
{
    interface ITaggable
    {
        TTag Tag<TTag>() where TTag : new();
    }

    public class Taggable : ITaggable
    {
        public TTag Tag<TTag>() where TTag : new()
        {
            if (tags == null)
            {
                tags = new List<object>();
            }

            var t = tags.OfType<TTag>().FirstOrDefault();
            if (t == null)
            {
                t = new TTag();
                tags.Add(t);
            }
            return t;
        }

        IList<object> tags;
    }
}
