using NUnit.Framework;

namespace Sekwell.Tests
{
    [TestFixture]
    public class StatementTests
    {
        [Test]
        public void BasicQuery()
        {
            string str = "some thing";
            int num1 = 123;
            int num2 = 456;
            Statement query = new Statement($"SELECT * FROM SOMETABLE WHERE SOME={str} AND OTHER={num1}")
                .Append($"AND ANOTHER={num2}");
            (string sql, object[] parms) = query.ToSql();

            Assert.That(sql, Is.EqualTo("SELECT * FROM SOMETABLE WHERE SOME=? AND OTHER=? AND ANOTHER=?"));
            Assert.That(parms, Has.Length.EqualTo(3));
            Assert.Multiple(() =>
            {
                Assert.That(parms[0], Is.EqualTo(str));
                Assert.That(parms[1], Is.EqualTo(num1));
                Assert.That(parms[2], Is.EqualTo(num2));
            });
        }

        [Test]
        public void In()
        {
            int[] someArr = new[] { 1, 2, 3, 4 };
            string str = "some thing";
            Statement query = new Statement($"SELECT * FROM SOMETABLE WHERE MYTHING IN {Statement.In(someArr)} AND SOME={str}");
            (string sql, object[] parms) = query.ToSql();

            Assert.That(sql, Is.EqualTo("SELECT * FROM SOMETABLE WHERE MYTHING IN (?,?,?,?) AND SOME=?"));
            Assert.That(parms, Has.Length.EqualTo(5));
            Assert.Multiple(() =>
            {
                Assert.That(parms[0], Is.EqualTo(1));
                Assert.That(parms[1], Is.EqualTo(2));
                Assert.That(parms[2], Is.EqualTo(3));
                Assert.That(parms[3], Is.EqualTo(4));
                Assert.That(parms[4], Is.EqualTo(str));
            });
        }

        [Test]
        public void Subquery()
        {
            Statement someSubquery = new Statement($"SELECT * FROM WHATEVER WHERE DELETED={0}");
            Statement query = new Statement($"SELECT * FROM SOMETABLE WHERE MYTHING IN ({someSubquery})");
            (string sql, object[] parms) = query.ToSql();

            Assert.That(sql, Is.EqualTo("SELECT * FROM SOMETABLE WHERE MYTHING IN (SELECT * FROM WHATEVER WHERE DELETED=?)"));
            Assert.That(parms, Has.Length.EqualTo(1));
            Assert.Multiple(() =>
            {
                Assert.That(parms[0], Is.EqualTo(0));
            });
        }

        [Test]
        public void Append_Empty()
        {
            Statement query;
            query = query.Append($"SOME APPENDED VALUE");
            (string sql, object[] parms) = query.ToSql();

            Assert.That(sql, Is.EqualTo(" SOME APPENDED VALUE"));
            Assert.That(parms, Is.Empty);
        }

        [Test]
        public void AppendRaw_RetainsArguments()
        {
            Statement query = new Statement($"SELECT * FROM SOME WHERE A={3}");
            query = query.AppendRaw("AND B=2");
            (string sql, object[] parms) = query.ToSql();

            Assert.That(sql, Is.EqualTo("SELECT * FROM SOME WHERE A=? AND B=2"));
            Assert.That(parms, Has.Length.EqualTo(1));
            Assert.Multiple(() =>
            {
                Assert.That(parms[0], Is.EqualTo(3));
            });
        }

        [Test]
        public void AppendRaw_AppendArgument()
        {
            Statement query = new Statement($"SELECT * FROM SOME WHERE A={3}");
            query = query.AppendRaw("AND B=?", 2);
            (string sql, object[] parms) = query.ToSql();

            Assert.That(sql, Is.EqualTo("SELECT * FROM SOME WHERE A=? AND B=?"));
            Assert.That(parms, Has.Length.EqualTo(2));
            Assert.Multiple(() =>
            {
                Assert.That(parms[0], Is.EqualTo(3));
                Assert.That(parms[1], Is.EqualTo(2));
            });
        }
    }
}