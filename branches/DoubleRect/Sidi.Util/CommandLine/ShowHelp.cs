using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.ComponentModel;

namespace Sidi.CommandLine
{
    public class ShowHelp
    {
        Parser parser;

        public ShowHelp(Parser parser)
        {
            this.parser = parser;
        }

        [Usage("Shows help for all options and actions that match searchString")]
        [Category(Parser.categoryUserInterface)]
        public void Help(string searchString)
        {
            foreach (var i in parser.Items)
            {
                if (Regex.IsMatch(i.UsageText, searchString, RegexOptions.IgnoreCase))
                {
                    Console.WriteLine();
                    Console.WriteLine(i.UsageText);
                }
            }
        }
    }

}
