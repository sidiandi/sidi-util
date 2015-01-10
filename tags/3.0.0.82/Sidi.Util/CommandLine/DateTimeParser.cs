using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sidi.Extensions;
using System.Globalization;

namespace Sidi.CommandLine
{
    [Usage("Date/Time value")]
    [Example("now")]
    [Example("tomorrow")]
    [Example("today")]
    [Example("yesterday")]
    [Example("2013-10-01")]
    [Example("2013-10-01T11:59")]
    public class DateTimeParser : ValueContainer<DateTime>, CommandLineHandler
    {
        [Usage("Now")]
        public void Now()
        {
            Value = DateTime.Now;
        }

        [Usage("Tomorrow")]
        public void Tomorrow()
        {
            Value = DateTime.Today.AddDays(1);
        }

        [Usage("Today")]
        public void Today()
        {
            Value = DateTime.Today;
        }

        [Usage("Yesterday")]
        public void Yesterday()
        {
            Value = DateTime.Today.AddDays(-1);
        }

        public void BeforeParse(IList<string> args)
        {
        }

        public void UnknownArgument(IList<string> args)
        {
            var dateTimeRepresentation = args[0];
            DateTime x;
            if (DateTime.TryParse(dateTimeRepresentation, out x) || DateTime.TryParse(dateTimeRepresentation, CultureInfo.InvariantCulture, DateTimeStyles.None, out x))
            {
                Value = x;
                args.PopHead();

            }
        }
    }
}
