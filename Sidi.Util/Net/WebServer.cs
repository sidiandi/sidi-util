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
using System.Threading;
using System.Web;
using System.Web.UI;
using Sidi.IO;
using Sidi.Net;
using Sidi.Extensions;

namespace Sidi.Net
{
    public class WebServer : Sidi.Net.HtmlGenerator
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        HttpListener httpListener = null;
        Thread listenThread;

        public void Start()
        {
            listenThread = new Thread(ServerThread);
            listenThread.Start();
        }
        
        public void Stop()
        {
            httpListener.Stop();
            listenThread.Join();
            listenThread = null;
        }

        public void Run()
        {
            Start();
            listenThread.Join();
        }

        string Prefix = "http://GB/test/";

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

        public Dictionary<string, object> Handlers = new Dictionary<string, object>();

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

        object[] GetParameters(ParameterInfo[] parameters, Uri uri)
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
                        return new object[] { Sidi.CommandLine.Parser.ParseValue(pv, p.ParameterType) };
                    }
                })
                .ToArray();
        }

        public void Handle(HttpListenerContext http)
        {
            using (log4net.NDC.Push(http.Request.RemoteEndPoint.Address.ToString()))
            {
                log.Info(http.Request.Url);
                using (var o = new StreamWriter(http.Response.OutputStream))
                {
                    var relUri = new Uri(Prefix).MakeRelativeUri(http.Request.Url).ToString();
                    var relPath = relUri.Substring(0, relUri.IndexOf('?'));
                    
                    foreach (var k in Handlers.Keys
                        .Where(k => relPath.StartsWith(k))
                        .OrderByDescending(k => k.Length)
                        )
                    {
                        var r = relPath.Substring(k.Length).Split('/');
                        var h = Handlers[k];
                        try
                        {
                            string keyword = r.FirstOrDefault();
                            if (String.IsNullOrEmpty(keyword))
                            {
                                keyword = "Index";
                            }

                            var action = h.GetType().GetMethod(keyword);
                            var html = action.Invoke(h, GetParameters(action.GetParameters(), http.Request.Url));

                            if (html is Action<TextWriter>)
                            {
                                ((Action<TextWriter>)html)(o);
                            }
                            break;
                        }
                        catch
                        {
                        }
                    }

                    http.Response.StatusCode = (int)HttpStatusCode.OK;
                    http.Response.ContentType = "text/html";
                }
                http.Response.Close();
            }
        }
    }
}
