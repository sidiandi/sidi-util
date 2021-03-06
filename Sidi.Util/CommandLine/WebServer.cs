// Copyright (c) 2012, Andreas Grimme (http://andreas-grimme.gmxhome.de/)
// 
// This file is part of sidi-util.
// 
// sidi-util is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// sidi-util is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public License
// along with sidi-util. If not, see <http://www.gnu.org/licenses/>.

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
using System.Web.UI;
using Sidi.IO;
using Sidi.Net;
using Sidi.Extensions;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics;
using System.Security.Principal;

namespace Sidi.CommandLine
{
    [Usage("Stand-alone web server")]
    public class WebServer : Sidi.Net.HtmlGenerator, IDisposable
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        Parser originalParser;
        Parser Parser
        {
            get
            {
                if (_parser == null)
                {
                    _parser = new Parser();
                    _parser.ItemSources.AddRange(
                        originalParser.ItemSources
                        .Where(a =>
                        {
                            var ns = a.GetType().Namespace;
                            return !ns.Equals(this.GetType().Namespace);
                        }));
                }
                return _parser;
            }
        }
        Parser _parser;

        public WebServer(Parser originalParser)
        {
            this.originalParser = originalParser;
        }

        [Usage("URL of web server")]
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
                    return "http://{0}/{1}/".F(System.Environment.MachineName, originalParser.MainSource.Instance.GetType().Name);
                }
                else
                {
                    return prefix;
                }
            }
        }
        string prefix;

        HttpListener httpListener = null;
        Thread listenThread;

        public void StartServer()
        {
            WindowsPrincipal pricipal = new WindowsPrincipal(WindowsIdentity.GetCurrent());
            bool hasAdministrativeRight = pricipal.IsInRole(WindowsBuiltInRole.Administrator);

            if (!hasAdministrativeRight)
            {
                throw new InvalidOperationException("Administrator rights are required to run web server");
            }
            
            listenThread = new Thread(ServerThread);
            listenThread.Start();
        }
        
        public void StopServer()
        {
            httpListener.Stop();
            listenThread.Join();
            listenThread = null;
        }

        [Usage("Run the web server in command line. Press Ctrl+C to stop")]
        public void Run()
        {
            StartServer();
        }

        [Usage("Display web server in the default web browser")]
        public void Browse()
        {
            Process.Start(this.Prefix);
        }

        void ServerThread()
        {
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
                var context = httpListener.GetContext();
                Handle(context);
            }
        }

        Action<TextWriter> StandardPage(Context c, params object[] childs)
        {
            return 
                html(
                head(title(c.Parser.ApplicationName)),
                body(
                    small(BreadCrumbs(c)),
                    hr(),
                    childs
                    ));
        }

        object BreadCrumbs(Context c)
        {
            var cs = new List<Context>();
            for (var i = c; i != null; i = i.Parent)
            {
                cs.Add(i);
            }
            cs.Reverse();
            return cs.Select(x => a(href(x.Base), x.Parser.MainSource.Instance.GetType().Name));
        }

        void Overview(Context c)
        {
            using (var o = new StreamWriter(c.Http.Response.OutputStream))
            {
                    var q = System.Web.HttpUtility.ParseQueryString(c.Http.Request.Url.Query);

                    foreach (string k in q.Keys)
                    {
                        var option = c.Parser.Items.FirstOrDefault(x => x is Option && x.Name.Equals(k));
                        if (option != null)
                        {
                            option.Handle(new List<string>(new string[] { q[k] }), true);
                        }
                    }

                    StandardPage(c,
                        Parser.Categories.Select(category =>
                            div(h2(category),
                                c.Parser.Items
                                    .Where(x => x.Categories.Contains(category))
                                    .Select(item => OverviewItem(c, item))
                                    )
                               )
                            )
                                (o);
            }
        }

        object Form(IParserItem item)
        {
            if (item is Action)
            {
                return Form((Action)item);
            }
            else if (item is Option)
            {
                return Form((Option)item);
            }
            throw new ArgumentException(item.ToString());
        }

        object OverviewItem(Context c, IParserItem item)
        {
            if (item is Action)
            {
                var action = (Action)item;
                return p(a(href(c.Path(action.Name + ".form")), action.Name), " - ", action.Usage);
            }
            else if (item is Option)
            {
                return Form((Option)item);
            }
            else if (item is SubCommand)
            {
                var subCommand = (SubCommand)item;
                return p(a(href(c.Path(subCommand.Name)), subCommand.Name), " - ", subCommand.Usage);
            }
            else
            {
                return p(item.Name, " - ", "not supported");
            }
        }

        object Form(Action a)
        {
            return form(
                action(a.Name + ".html"), 
                method("get"),
                h2(a.Name),
                p(a.Usage),

                // params
                a.MethodInfo.GetParameters().Select(param =>
                    p(
                        param.Name, 
                        input(type("text"), name(param.Name)), 
                        "[", param.ParameterType.GetInfo(), "]"
                        )
                ),

                p(input(type("submit"), value(a.Name)))
            );
        }

        object Form(Option option)
        {
            return form(
                action(String.Empty),
                method("get"),
                p(
                    option.Name,
                    input(
                        type(option.IsPassword ? "password" : "text"),
                        name(option.Name),
                        value(option.GetValue().SafeToString())
                        ),
                    "[", option.Type.GetInfo(), "]",
                    input(type("submit"), value("Set"))
                    )
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
                            return Tokenizer.ToArray(pv);
                        }
                        else
                        {
                            return new string[] { pv };
                        }
                    })
                .ToList();
        }

        [SuppressMessage("Microsoft.Design", "CA1031")]
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
                    var startTime = DateTime.Now;
                    action.Handle(GetParameterList(action.MethodInfo.GetParameters(), c.Request.Url), true);
                    Console.WriteLine("Completed in {0}", DateTime.Now - startTime);                
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

        public bool CanHandle(HttpListenerContext httpContext)
        {
            var parts = Context.SplitUrlPath(httpContext.Request.Url.ToString());
            var baseParts = Context.SplitUrlPath(Prefix);
            return baseParts.SequenceEqual(parts.Take(baseParts.Count()), StringComparer.InvariantCultureIgnoreCase);
        }

        public void Handle(HttpListenerContext http)
        {
            if (!CanHandle(http))
            {
                throw new Exception("Cannot handle {0}".F(http.Request.Url));
            }

            using (log4net.NDC.Push(http.Request.RemoteEndPoint.Address.ToString()))
            {
                log.Info(http.Request.Url);
                var c = new Context();
                c.Http = http;
                var parts = Context.SplitUrlPath(c.Http.Request.Url.AbsolutePath);
                var baseParts = Context.SplitUrlPath(Prefix);
                c.Base = "/" + baseParts.Skip(2).Join("/");
                c.RelPath = parts.Skip(baseParts.Count() - 2).Join("/");
                c.Parser = Parser;
                Handle(c);
                log.InfoFormat("{0} {1}", http.Response.StatusCode, http.Request.Url);
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1031")]
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
                        cs.Parser = subCommand.Parser;
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
                                        StandardPage(c, Form(item))(o);
                                    }
                                }
                                break;
                            case "html":
                                {
                                    using (var o = new StreamWriter(c.Http.Response.OutputStream))
                                    {
                                        StandardPage(c, Form(item), Verbose(cw => Run(item, c.Http, cw)))(o);
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
                using (var cw = new HtmlCodeWriter(c.Http.Response.OutputStream))
                {
                    cw.WriteLine(e);
                }
            }
            c.Http.Response.Close();
        }
        
        

        private bool disposed = false;
            
        //Implement IDisposable.
        public void Dispose()
        {
          Dispose(true);
          GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
          if (!disposed)
          {
            if (disposing)
            {
                this.httpListener.Close();
            }
            // Free your own state (unmanaged objects).
            // Set large fields to null.
            disposed = true;
          }
        }

        // Use C# destructor syntax for finalization code.
        ~WebServer()
        {
          // Simply call Dispose(false).
          Dispose(false);
        }    
    
    }

    public class ShowWebServer : IDisposable
    {
        public ShowWebServer(Parser parser)
        {
            this.parser = parser;
        }

        Parser parser;

        [Category(Parser.categoryUserInterface)]
        [SubCommand]
        public WebServer WebServer
        {
            get
            {
                if (_WebServer == null)
                {
                    _WebServer = new WebServer(parser);
                }
                return _WebServer;
            }
        }
        WebServer _WebServer;

        private bool disposed = false;
            
        //Implement IDisposable.
        public void Dispose()
        {
          Dispose(true);
          GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
          if (!disposed)
          {
            if (disposing)
            {
                _WebServer.Dispose();
            }
            // Free your own state (unmanaged objects).
            // Set large fields to null.
            disposed = true;
          }
        }

        // Use C# destructor syntax for finalization code.
        ~ShowWebServer()
        {
          // Simply call Dispose(false).
          Dispose(false);
        }    
    }
}
