using NUnit.Framework;
using Sidi.Test;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rule = System.Func<Sidi.Parse.Text>;

namespace Sidi.Parse
{
    public class Expression : Parser
    {
        public static Func<Text, Ast> Expr()
        {
            return Enclose(() => Alternative(
                Concatenation(Term(), Plus(), Term()),
                Term()
                ));
        }

        public static Func<Text, Ast> Symbol(string s)
        {
            return Concatenation(OptionalWhitespace(), Expect(s));
        }

        public static Func<Text, Ast> Plus()
        {
            return Collapse(() => Symbol("+"));
        }

        public static Func<Text, Ast> Term()
        {
            return Enclose(() => Alternative(
                Concatenation(Factor(), Asterisk(), Factor()),
                Factor()));
        }

        public static Func<Text, Ast> Factor()
        {
            return Enclose(() => Alternative(
                    Concatenation(Expect("("), Expr(), Expect(")")), 
                    Number()
              ));
        }

        public static Func<Text, Ast> Digits()
        {
            return Enclose(() => Repetition(1, Int32.MaxValue, Digit()));
        }

        public static Func<Text, Ast> Number()
        {
            return Collapse(() => Concatenation(OptionalWhitespace(), Digits(), Optional(Concatenation(Expect("."), Digits()))));
        }
    }

    [TestFixture]
    public class ParserTest : TestBase
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        [Test]
        public void SimpleExpressionParser()
        {
            var input = new Text(@" 1");
            var ast = Expression.Expr()(input);
            log.Info(ast.Details);

            input = new Text(@"1 + 1");
            ast = Expression.Expr()(input);
            log.Info(ast.Details);

            input = new Text(@"1      + 365.1234 * 3");
            ast = Expression.Expr()(input);
            log.Info(ast.Details);

            return;
        }

        [Test]
        public void Text_can_be_concatenated()
        {
            var x = "Hello, World!";
            var texts = new[]
            {
                new Text(x, 0, 5),
                new Text(x, 5, x.Length)
            };

            var a = new Text(texts);
            Assert.AreEqual(x, a.ToString());

            texts = new[]
            {
                new Text("Hello, "),
                new Text("World!")
            };
            var b = new Text(texts);
            Assert.AreEqual(x, b.ToString());

            Assert.IsTrue(object.Equals(a, b));
        }
    }
}
