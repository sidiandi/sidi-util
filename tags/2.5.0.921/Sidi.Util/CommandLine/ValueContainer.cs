using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sidi.CommandLine
{
    public interface IValueContainer
    {
        object Value { get; }
        Type ValueType { get; }
    }

    public class ValueContainer<T> : IValueContainer
    {
        public T Value { get; protected set; }

        object IValueContainer.Value
        {
            get { return Value; }
        }

        Type IValueContainer.ValueType
        {
            get { return typeof(T); }
        }
    }
}
