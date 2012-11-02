using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sidi.IO
{
    public class PathTypeConverter : System.ComponentModel.TypeConverter
    {
        public override bool CanConvertFrom(System.ComponentModel.ITypeDescriptorContext context, Type sourceType)
        {
            return sourceType.Equals(typeof(string));
        }

        public override object ConvertFrom(System.ComponentModel.ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
        {
            return new LPath((string)value);
        }

        public override bool CanConvertTo(System.ComponentModel.ITypeDescriptorContext context, Type destinationType)
        {
            return destinationType.Equals(typeof(LPath));
        }

        public override object ConvertTo(System.ComponentModel.ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
        {
            return ((LPath)value).ToString();
        }
    }
}
