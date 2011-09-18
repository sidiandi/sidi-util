﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using Sidi.Util;
using Sidi.Forms;
using System.Reflection;
using System.Net;
using System.IO;
using System.ComponentModel;
using System.Net.Sockets;

namespace Sidi.CommandLine
{
    public class CodeWriter : TextWriter
    {
        TextWriter o;
        TextWriter ownO;

        public CodeWriter(TextWriter o)
        {
            this.o = o;
            this.o.WriteLine("<code>");
        }

        public CodeWriter(Stream o)
            : this(new StreamWriter(o))
        {
            this.ownO = this.o;
        }

        public override Encoding Encoding
        {
            get { throw new NotImplementedException(); }
        }

        public override void WriteLine(string value)
        {
            foreach (var line in value.Lines())
            {
                o.WriteLine(line);
                o.WriteLine("<br>");
            }
        }

        protected override void Dispose(bool disposing)
        {
            o.WriteLine("</code>");
            if (ownO != null)
            {
                ownO.Close();
            }
            base.Dispose(disposing);
        }
    }

    public class WebServer
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        Parser originalParser;
        Parser parser;

        public WebServer(Parser originalParser)
        {
            this.originalParser = originalParser;
        }

        const string WebServerCategory = "Web Server";

        [Usage("HTTP prefix, e.g. http://*:12345/ . If undefined, a random port will be used.")]
        [Category(WebServerCategory)]
        public string Prefix { set; get; }

        public HttpListener StartHttpListenerOnFreePort()
        {
            for (int port = 49152; port <= 65535; ++port)
            {
                try
                {
                    var listener = new HttpListener();
                    listener.Prefixes.Add("http://*:{0}/".F(port));
                    listener.Start();
                    log.Info(listener.Prefixes);
                    return listener;
                }
                catch (Exception e)
                {
                    log.WarnFormat("port {0} already used", port);
                }
            }
            throw new Exception("no free port for HttpListener");
        }

        [Usage("Offers program functions on an embedded web server.")]
        [Category(WebServerCategory)]
        public void Serve()
        {
            parser = new Parser();
            parser.Applications.AddRange(
                originalParser.Applications
                .Where(a =>
                {
                    var ns = a.GetType().Namespace;
                    return !ns.Equals(this.GetType().Namespace);
                }));


            HttpListener httpListener = null;
            if (Prefix == null)
            {
                httpListener = StartHttpListenerOnFreePort();
            }
            else
            {
                httpListener = new HttpListener(); 
                httpListener.Prefixes.Add(Prefix);
                httpListener.Start();
            }

            for (;;)
            {
                var context = httpListener.GetContext();
                if (context.Request.Url.ToString().Contains("exit"))
                {
                    break;
                }
                Handle(context);
            }
            httpListener.Stop();
        }

        void HtmlHeader(TextWriter o)
        {
            o.WriteLine(@"<!DOCTYPE HTML PUBLIC ""-//W3C//DTD HTML 4.01//EN"" ""http://www.w3.org/TR/html4/strict.dtd"">
<html>
<head>
<title>{0}</title>
</head>
<body>
<small>{1}</small>
<hr />", parser.ApplicationName, parser.Info.Lines().Join("<br>"));

        }

        void HtmlFooter(TextWriter o)
        {
            o.WriteLine(@"</body>
</html>");
        }

        void Overview(HttpListenerContext c)
        {
            using (var o = new StreamWriter(c.Response.OutputStream))
            {
                HtmlHeader(o);
                var q = System.Web.HttpUtility.ParseQueryString(c.Request.Url.Query);
                
                foreach (string k in q.Keys)
                {
                    var option = parser.Items.FirstOrDefault(x => x is Option && x.Name.Equals(k));
                    if (option != null)
                    {
                        option.Handle(new List<string>(new string[] { q[k] }), true);
                    }
                }

                foreach (var category in parser.Categories)
                {
                    var items = parser.Items.Where(x => x.Categories.Contains(category));
                    if (items.Any())
                    {
                        o.WriteLine("<h2>{0}</h2>", category);
                        foreach (var item in items)
                        {
                            OverviewItem(o, item);
                        }
                    }
                }

                HtmlFooter(o);
            }
        }

        void OverviewItem(TextWriter o, IParserItem item)
        {
            if (item is Action)
            {
                var action = (Action) item;
                o.WriteLine(@"<p><a href=""/{0}?form"">{0}</a> - {1}", action.Name, action.Usage);
            }
            else if (item is Option)
            {
                Form(o, (Option)item);
            }
            else
            {
                o.WriteLine("<p>{0}</p> - not supported", item.Name);
            }
        }

        void Form(TextWriter o, Action a)
        {
            o.WriteLine(@"<form action=""/{0}"" method=""get"">
<h2>{0}</h2>
<p>{2}</p>
{1}
<p><input type=""submit"" value=""{0}"" /></p>
</form>",
                a.Name,
                a.MethodInfo.GetParameters().Select(p => @"<p>{0} <input type=""text"" name=""{0}"" /> [{1}]</p>".F(
                    p.Name, p.ParameterType.GetInfo())).Join(),
                a.Usage);
        }

        void Form(TextWriter o, Option option)
        {
            o.WriteLine(@"<form action=""/"" method=""get"">
<p>{0} <input type=""text"" name=""{0}"" value=""{3}"" /> [{1}] - {2}<input type=""submit"" value=""Set"" /></p>
</form>",
                option.Name,
                option.Type.GetInfo(),
                option.Usage,
                option.GetValue().SafeToString());
        }

        IList<string> GetParameterList(ParameterInfo[] parameters, Uri uri)
        {
            var v = System.Web.HttpUtility.ParseQueryString(uri.Query);
            return parameters
                .SelectMany(p => 
                    {
                        var pv = v[p.Name];
                        if (pv == null)
                        {
                            throw new Exception("parameter {0} not supplied".F(p.Name));
                        }
                        if (p.ParameterType == typeof(List<string>))
                        {
                            return new Tokenizer(new StringReader(pv)).Tokens.ToArray();
                        }
                        else
                        {
                            return new string[] { pv };
                        }
                    })
                .ToList();
        }

        void Handle(HttpListenerContext c)
        {
            try
            {
                var parts = c.Request.Url.AbsolutePath.Split(new char[] { '/' });
                if (parts.Length > 1 && !String.IsNullOrEmpty(parts[1]))
                {
                    var item = parser.Items.FirstOrDefault(x => parser.IsExactMatch(parts[1], x.Name));
                    if (item is Action)
                    {
                        var action = (Action)item;
                        using (var o = new StreamWriter(c.Response.OutputStream))
                        {
                            HtmlHeader(o);

                            if (c.Request.Url.Query == "?form")
                            {
                                Form(o, action);
                            }
                            else
                            {
                                TextWriter oldOut = Console.Out;
                                TextWriter oldError = Console.Error;
                                using (var cw = new CodeWriter(o))
                                {
                                    try
                                    {
                                        Console.SetOut(cw);
                                        Console.SetError(cw);
                                        action.Handle(GetParameterList(action.MethodInfo.GetParameters(), c.Request.Url), true);
                                    }
                                    catch (Exception e)
                                    {
                                        cw.WriteLine(e.ToString());
                                    }
                                    finally
                                    {
                                        Console.SetOut(oldOut);
                                        Console.SetError(oldError);
                                    }
                                }
                            }

                            HtmlFooter(o);
                        }
                    }
                    else if (item is Option)
                    {
                        var option = (Option) item;
                        using (var o = new StreamWriter(c.Response.OutputStream))
                        {
                            o.WriteLine(option.GetValue().SafeToString());
                        }
                    }
                    else
                    {
                        c.Response.StatusCode = (int) HttpStatusCode.NotFound;
                    }
                }
                else
                {
                    Overview(c);
                }
                c.Response.StatusCode = (int) HttpStatusCode.OK;
            }
            catch (Exception e)
            {
                c.Response.StatusCode = (int) HttpStatusCode.InternalServerError;
                using (var cw = new CodeWriter(c.Response.OutputStream))
                {
                    cw.WriteLine(e);
                }
            }
            c.Response.Close();
        }
    }
}
