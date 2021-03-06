﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sidi.Util;
using Sidi.Extensions;

namespace Sidi.CommandLine
{
    class ShellSupport
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        [Usage("Exit interactive shell")]
        [System.ComponentModel.Category(Parser.categoryUserInterface)]
        public void Exit()
        {
            mustExit = true;
            throw new CommandLineException("exit");
        }

        bool mustExit = false;

        List<Parser> Parsers = new List<Parser>();

        public void Shell(Parser parser)
        {
            Parsers.Add(parser);
            var p = new Parser();
            p.ItemSources.AddRange(parser.ItemSources);
            p.ItemSources.Add(new ItemSource(this));

            for (; ; )
            {
                Console.Write("{0}>", Parsers.Select(x => x.MainSource.Instance.GetType().Name).Join(">"));
                var input = Console.ReadLine();
                var args = Tokenizer.ToArray(input);

                // special treatment for subcommands
                // if the only arg is a subcommand, we "change dirctory" into this subcommand.
                if (args.Length == 1)
                {
                    var subCommand = (SubCommand)p.LookupParserItem(args[0], p.SubCommands);
                    if (subCommand != null)
                    {
                        Shell(subCommand.Parser);
                        continue;
                    }
                }

                try
                {
                    p.Parse(args);
                }
                catch (Exception ex)
                {
                    if (mustExit)
                    {
                        break;
                    }

                    log.Error(ex);
                    Console.WriteLine(ex.Message);
                }
            }

            Parsers.Pop();
        }
    }
}
