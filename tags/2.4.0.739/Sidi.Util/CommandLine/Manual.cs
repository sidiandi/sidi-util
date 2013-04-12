using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sidi.Net;
using Sidi.IO;
using Sidi.Util;
using System.Diagnostics;
using Sidi.Extensions;

namespace Sidi.CommandLine
{
    public class Manual : HtmlGenerator
    {
        public static void Show(Parser p)
        {
            var file = Paths.Temp.CatDir(LPath.GetValidFilename(p.ApplicationName)).CatName(".html");
            var m = new Manual();
            using (var w = LFile.StreamWriter(file))
            {
                m.Write(w, () => m.Page(p));
            }
            Process.Start(file);
        }

        object ToDt(IParserItem item)
        {
            if (item is Option)
            {
                return ToDt((Option)item);
            }
            else if (item is SubCommand)
            {
                return ToDt((SubCommand)item);
            }
            else
            {
                return new[]
                {
                    dt(item.Syntax),
                    dd(p(item.Usage))
                };
            }
        }

        object ToDt(SubCommand subCommand)
        {
            return new[]
            {
                dt(a(href("#" + subCommand.Name), subCommand.Syntax)),
                dd(p(subCommand.Usage))
            };
        }

        object ToDt(Option option)
        {
            return new[]
            {
                dt(option.Syntax),
                dd(p("default: ", option.DisplayValue, option.IsPersistent ? " (persistent)" : null),
                    p(option.Usage))
            };
        }

        object Page(Parser parser)
        {
            return html
            (
                head
                (
                    title(new object[]{parser.ApplicationName})
                ),
                body
                (
                    h1(parser.ApplicationName),
                    p(parser.VersionInfo),
                    p(Usage.Get(parser.MainApplication.GetType())),
                    CommandList(parser),
                    parser.SubCommands.Select(subCommand =>
                        new[]
                        {
                            h2(a(name(subCommand.Name), "Subcommand ", subCommand.Name)),
                            CommandList(new Parser
                                {
                                    Applications = { subCommand.CommandInstance }
                                })
                        })
                )
            );
        }

        object CommandList(Parser parser)
        {
            return dl
            (
                parser.GroupByCategory(parser.Items)
                .Select(cat => div(
                    h2(cat.Key),
                    dl(cat.Value.Select(item => ToDt(item))
                    )))
            );
        }
    }
}
