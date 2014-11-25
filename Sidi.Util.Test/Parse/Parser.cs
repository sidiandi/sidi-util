using NUnit.Framework;
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
        public static double? Add(Text text)
        {
            var t = text.Copy();
            var a = Multiply(t);
            if (a == null) return null;
            var p = Plus(t);
            if (p == null) return a;
            var b = Add(t);
            if (b == null) return null;

            text.Set(t);
            return a + b;
        }

        public static Text Plus(Text text)
        {
            return Expect(text, "+");
        }

        public static double? Multiply(Text text)
        {
            var t = text.Copy();
            var a = Number(t);
            if (a == null) return null;
            var p = Asterisk(t);
            if (p == null)
            {
                text.Set(t);
                return a;
            }

            var b = Number(t);
            if (b == null) return null;

            text.Set(t);
            return a * b;
        }

        static Tree Digits(Text text)
        {
            return Repetition(text, 1, Int32.MaxValue, Digit);
        }

        public static double? Number(Text text)
        {
            var r = Concatenation(text, Digits, t1 => Optional(t1, t2 => Concatenation(t2, t3 => Expect(t3 , "."), Digits)));
            if (r == null)
            {
                return null;
            }
            else
            {
                return Double.Parse(r.ToString());
            }
        }
    }

    [TestFixture]
    public class ParserTest
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        [Test]
        public void SimpleExpressionParser()
        {
            var input = new Text(@"1");
            Assert.AreEqual(1, Expression.Number(input));

            input = new Text(@"1+1");
            Assert.AreEqual(2, Expression.Add(input));

            input = new Text(@"1+2*2");
            Assert.AreEqual(5, Expression.Add(input));
        }
    }
}
