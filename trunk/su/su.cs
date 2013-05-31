using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sidi.CommandLine;
using Sidi.IO;
using Sidi.Util;
using NUnit.Framework;
using Microsoft.CSharp;
using System.CodeDom.Compiler;
using Sidi.Tool;

namespace su
{
    [Usage("sidi-util command line utilities")]
    public class su
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        static int Main(string[] args)
        {
            return Parser.Run(new su(), args);
        }

        [Usage("Run a C# script")]
        public void RunScript(LPath csFile, string[] args)
        {
            using (new LogScope(log.Info, "{0}", csFile))
            {
                var source = @"using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sidi.CommandLine;
using Sidi.IO;
using Sidi.Util;
using Sidi.Extensions;

[assembly: AssemblyCompany(""Andreas Grimme"")]
[assembly: AssemblyProduct(""su"")]


    public class TestScript
    {"
                    + LFile.ReadAllText(csFile) +
@" }
";
                var references = new[] { 
                    typeof(Sidi.CommandLine.Parser),
                    typeof(System.Linq.Enumerable),
                    typeof(System.Xml.Serialization.IXmlSerializable),
                };
                var options = new CompilerParameters(references.Select(a => a.Assembly.Location).ToArray())
                {
                    GenerateInMemory = true
                };

                var compiler = new CSharpCodeProvider(new Dictionary<string, string> { { "CompilerVersion", "v4.0" } });

                var results = compiler.CompileAssemblyFromSource(options, source);

                if (results.Errors.Count > 0)
                    throw new InvalidOperationException(String.Join(
                        Environment.NewLine,
                        results.Errors.Cast<CompilerError>().Select(ce => ce.ErrorText)
                    ));

                var application = Activator.CreateInstance(results.CompiledAssembly.GetType("TestScript"));
                Parser.Run(application, args);
            }
        }

        [SubCommand]
        public Files File;

        [SubCommand]
        public Backup Backup;

        [TestFixture]
        public class Test
        {
            [Test]
            public void Compile()
            {
                var su = new su();
                su.RunScript(@"C:\temp\test.cs", new[]{"Add", "1", "1"});
            }
        }
    }
}
