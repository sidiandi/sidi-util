﻿using System;
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
            using (var w = file.WriteText())
            {
                m.Write(w, () => m.Page(p));
            }
            Process.Start(file.StringRepresentation);
        }

        object ToDt(IParserItem item)
        {
            if (item is Option)
            {
                return ToDt((Option)item);
            }
            else if (item is Action)
            {
                return ToDt((Action)item);
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

        object ToDt(IValueParser vp)
        {
            return new[]
            {
                dt(a(name(vp.Name), vp.Name)),
                dd(p(vp.UsageText)),
            };
        }

        object Format(IValueParser p)
        {
            return a(href("#" + p.Name), p.Syntax);
        }

        object ToDt(Option option)
        {
            return new[]
            {
                dt(option.Name, " [ ", Format(option.ValueParser), " ] "),
                dd(p("default: ", option.DisplayValue, option.IsPersistent ? " (persistent)" : null),
                    p(option.Usage))
            };
        }

        object ToDt(Action action)
        {
            return new[]
            {
                dt(action.Name, action.Parameters.Select(p => new object[]{ " [ ", p.ParameterInfo.Name, ": ", Format(p.ValueParser), " ] " })),
                dd(this.p(action.Usage))
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
                    p(Usage.Get(parser.MainSource.Instance.GetType())),
                    p("All commands can be abbreviated as long as they are unique, e.g. \"Manual\" can be writen as \"m\" "),
                    CommandList(parser),
                    parser.SubCommands.Select(subCommand =>
                        new[]
                        {
                            h2(a(name(subCommand.Name), "Subcommand ", subCommand.Name)),
                            CommandList(subCommand.Parser)
                        }),
                    h2("Value Syntax"),
                    dl(parser.AvailableValueParsers.Select(item => ToDt(item)))
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
