using System;
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
using System.Threading;
using System.Web;

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
        public string Prefix
        {
            set
            {
                prefix = value;
            }
            get
            {
                if (prefix == null)
                {
                    return "http://{0}/{1}/".F(System.Environment.MachineName, parser.ApplicationName);
                }
                else
                {
                    return prefix;
                }
            }
        }
        string prefix;

        public HttpListener StartHttpListenerOnFreePort()
        {
            for (int port = 49152; port <= 65535; ++port)
            {
                try
                {
                    var listener = new HttpListener();
                    listener.Prefixes.Add("http://*:{0}/".F(port));
                    listener.Start();
                    return listener;
                }
                catch (HttpListenerException e)
                {
                    if (e.ErrorCode == 183)
                    {
                        log.Warn("port {0} already used".F(port), e);
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            throw new Exception("no free port for HttpListener");
        }

        HttpListener httpListener = null;
        Thread listenThread;

        public void StartServer()
        {
            listenThread = new Thread(new ThreadStart(() =>
                {
                    this.Serve();
                }));

            listenThread.Start();
        }

        public void StopServer()
        {
            httpListener.Stop();
            listenThread.Join();
            listenThread = null;
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


            try
            {
                httpListener = new HttpListener();
                httpListener.Prefixes.Add(Prefix);
                httpListener.Start();
            }
            catch (HttpListenerException e)
            {
                if (e.ErrorCode == 5)
                {
                    log.Warn("Run with administrator rights to use the web server.", e);
                }
                throw;
            }

            log.InfoFormat("Web server running on {0}", httpListener.Prefixes.First());
            
            for (; ; )
            {
                try
                {
                    var context = httpListener.GetContext();
                    Handle(context);
                }
                catch
                {
                    break;
                }
            }
        }

        void HtmlHeader(Context c, TextWriter o)
        {
            o.WriteLine(@"<!DOCTYPE HTML PUBLIC ""-//W3C//DTD HTML 4.01//EN"" ""http://www.w3.org/TR/html4/strict.dtd"">
<html>
<head>
<title>{0}</title>
</head>
<body>
<small>{1}</small>
<hr />", c.Parser.ApplicationName, BreadCrumbs(c)
       );

        }

        string BreadCrumbs(Context c)
        {
            var cs = new List<Context>();
            for (var i = c; i != null; i = i.Parent)
            {
                cs.Add(i);
            }
            cs.Reverse();
            return cs
                .Select(x => @"<a href=""{0}"">{1}</a>".F(x.Base, x.Parser.MainApplication.GetType().Name))
                .Join(" &gt ");
        }

        void HtmlFooter(Context c, TextWriter o)
        {
            o.WriteLine(@"</body>
</html>");
        }

        void Overview(Context c)
        {
            using (var o = new StreamWriter(c.Http.Response.OutputStream))
            {
                HtmlHeader(c, o);
                var q = System.Web.HttpUtility.ParseQueryString(c.Http.Request.Url.Query);

                foreach (string k in q.Keys)
                {
                    var option = c.Parser.Items.FirstOrDefault(x => x is Option && x.Name.Equals(k));
                    if (option != null)
                    {
                        option.Handle(new List<string>(new string[] { q[k] }), true);
                    }
                }

                foreach (var category in parser.Categories)
                {
                    var items = c.Parser.Items.Where(x => x.Categories.Contains(category));
                    if (items.Any())
                    {
                        o.WriteLine("<h2>{0}</h2>", category);
                        foreach (var item in items)
                        {
                            OverviewItem(c, o, item);
                        }
                    }
                }

                HtmlFooter(c, o);
            }
        }

        void Form(TextWriter o, IParserItem item)
        {
            if (item is Action)
            {
                Form(o, (Action)item);
            }
            else if (item is Option)
            {
                Form(o, (Option)item);
            }
        }

        void OverviewItem(Context c, TextWriter o, IParserItem item)
        {
            if (item is Action)
            {
                var action = (Action)item;
                o.WriteLine(@"<p><a href=""{0}.form"">{2}</a> - {1}", c.Path(action.Name), action.Usage, action.Name);
            }
            else if (item is Option)
            {
                Form(o, (Option)item);
            }
            else if (item is SubCommand)
            {
                var subCommand = (SubCommand)item;
                o.WriteLine(@"<p><a href=""{0}"">{1}</a> - {2}", c.Path(subCommand.Name), subCommand.Name, subCommand.Usage);
            }
            else
            {
                o.WriteLine("<p>{0}</p> - not supported", item.Name);
            }
        }

        void Form(TextWriter o, Action a)
        {
            o.WriteLine(@"<form action=""{0}.html"" method=""get"">
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
            o.WriteLine(@"<form action="""" method=""get"">
<p>{0} <input type=""{4}"" name=""{0}"" value=""{3}"" /> [{1}] - {2}<input type=""submit"" value=""Set"" /></p>
</form>",
                option.Name,
                option.Type.GetInfo(),
                option.Usage,
                option.GetValue().SafeToString(),
                option.IsPassword ? "password" : "text"
                );
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

        void Run(IParserItem item, HttpListenerContext c, TextWriter cw)
        {
            TextWriter oldOut = Console.Out;
            TextWriter oldError = Console.Error;
            try
            {
                Console.SetOut(cw);
                Console.SetError(cw);
                if (item is Action)
                {
                    var action = (Action)item;
                    action.Handle(GetParameterList(action.MethodInfo.GetParameters(), c.Request.Url), true);
                }
                else if (item is Option)
                {
                    var option = (Option)item;
                    Console.WriteLine(option.GetValue());
                }
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

        //else if (item is Option)
        //{
        //    var option = (Option) item;
        //    using (var o = new StreamWriter(c.Response.OutputStream))
        //    {
        //        o.WriteLine(option.GetValue().SafeToString());
        //    }
        //}
        //else
        //{
        //    c.Response.StatusCode = (int) HttpStatusCode.NotFound;
        //}


        class Context
        {
            public HttpListenerContext Http;
            public string Base;
            public string RelPath;
            public Parser Parser;
            public Context Parent;

            public IEnumerable<string> RelPathParts
            {
                get
                {
                    return SplitUrlPath(RelPath);
                }
            }

            public static IEnumerable<string> SplitUrlPath(string p)
            {
                return p.Split(new string[] { PartSep }, StringSplitOptions.RemoveEmptyEntries);
            }

            const string PartSep = "/";

            public string Path(params string[] parts)
            {
                return PartSep + SplitUrlPath(Base).Concat(parts).Join(PartSep);
            }
        }

        void Handle(HttpListenerContext http)
        {
            log.Info(http.Request.Url);
            var c = new Context();
            c.Http = http;
            var parts = c.Http.Request.Url.AbsolutePath.Split(new string[] { "/" }, StringSplitOptions.RemoveEmptyEntries);
            var baseParts = Context.SplitUrlPath(httpListener.Prefixes.First());
            c.Base = "/" + baseParts.Skip(2).Join("/");
            c.RelPath = parts.Skip(baseParts.Count()-2).Join("/");
            c.Parser = parser;
            Handle(c);
            log.InfoFormat("{0} {1}", http.Response.StatusCode, http.Request.Url);
        }

        void Handle(Context c)
        {
            try
            {
                if (!String.IsNullOrEmpty(c.RelPath))
                {
                    var page = c.RelPathParts.First();
                    var dotParts = page.Split('.');
                    var itemName = dotParts.First();
                    var mode = dotParts.Last();
                    var item = c.Parser.Items.FirstOrDefault(x => c.Parser.IsExactMatch(itemName, x.Name));
                    var query = HttpUtility.ParseQueryString(c.Http.Request.Url.Query);

                    if (item is SubCommand)
                    {
                        var subCommand = (SubCommand)item;
                        var cs = new Context();
                        cs.Http = c.Http;
                        cs.Base = c.Path(subCommand.Name);
                        cs.Parser = new Parser();
                        cs.Parser.Applications.Add(subCommand.CommandInstance);
                        cs.RelPath = c.RelPathParts.Skip(1).Join("/");
                        cs.Parent = c;
                        Handle(cs);
                        return;
                    }

                    if (item != null)
                    {
                        switch (mode)
                        {
                            case "form":
                                {
                                    using (var o = new StreamWriter(c.Http.Response.OutputStream))
                                    {
                                        HtmlHeader(c, o);
                                        Form(o, item);
                                        HtmlFooter(c, o);
                                    }
                                }
                                break;
                            case "html":
                                {
                                    using (var o = new StreamWriter(c.Http.Response.OutputStream))
                                    {
                                        HtmlHeader(c, o);
                                        using (var cw = new CodeWriter(o))
                                        {
                                            if (item is Action)
                                            {
                                                Run(item, c.Http, cw);
                                            }
                                        }
                                        HtmlFooter(c, o);
                                    }
                                }
                                break;
                            default:
                                {
                                    using (var o = new StreamWriter(c.Http.Response.OutputStream))
                                    {
                                        if (item is Action)
                                        {
                                            Run((Action)item, c.Http, o);
                                        }
                                    }
                                }
                                break;
                        }
                    }
                }
                else
                {
                    Overview(c);
                }
                c.Http.Response.StatusCode = (int)HttpStatusCode.OK;
            }
            catch (Exception e)
            {
                c.Http.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                using (var cw = new CodeWriter(c.Http.Response.OutputStream))
                {
                    cw.WriteLine(e);
                }
            }
            c.Http.Response.Close();
        }
    }
}
