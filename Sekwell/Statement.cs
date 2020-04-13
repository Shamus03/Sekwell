using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;

namespace Sekwell
{
    public struct Statement
    {
        private readonly string _statementFormat;
        private string StatementFormat => _statementFormat ?? "";

        private readonly object[] _arguments;
        private object[] Arguments => _arguments ?? new object[] { };

        public Statement(FormattableString str)
        {
            _statementFormat = str.Format;
            _arguments = str.GetArguments();
        }

        private Statement(string fmt, object[] args)
        {
            _statementFormat = fmt;
            _arguments = args;
        }

        public Statement Append(FormattableString str)
        {
            string fmt = StatementFormat + " " + str.Format;
            object[] args = Arguments.Concat(str.GetArguments()).ToArray();
            return new Statement(fmt, args);
        }

        public Statement AppendRaw(string str)
        {
            string fmt = StatementFormat + " " + str;
            return new Statement(fmt, new object[] { });
        }

        public static Statement In<T>(IEnumerable<T> args)
        {
            object[] argsArr = args.Select(a => a as object).ToArray();
            if (argsArr.Length == 0)
            {
                throw new ArgumentException("arguments to an 'IN' expression cannot be empty");
            }

            string fmt = "(" + string.Join(",", Enumerable.Repeat("?", argsArr.Length)) + ")";
            return new Statement(fmt, argsArr);
        }

        public static Statement Raw(string rawSql)
        {
            return new Statement(rawSql, new object[] { });
        }

        public (string sql, object[] parameters) ToSql()
        {
            List<string> formatArgs = new List<string>();
            List<object> parameters = new List<object>();

            foreach (object arg in Arguments)
            {
                if (arg is Statement inner)
                {
                    (string nestedSql, object[] nestedParams) = inner.ToSql();
                    formatArgs.Add(nestedSql);
                    foreach (object parm in nestedParams)
                    {
                        parameters.Add(parm);
                    }
                }
                else
                {
                    parameters.Add(arg);
                    formatArgs.Add("?");
                }
            }

            return (string.Format(StatementFormat, formatArgs.ToArray()), parameters.ToArray());
        }

        public DbCommand Compile(DbConnection conn)
        {
            DbCommand cmd = conn.CreateCommand();
            (string stmtText, object[] parameters) = ToSql();
            foreach (object parm in parameters)
            {
                if (parm is DbParameter)
                {
                    cmd.Parameters.Add(parm);
                }
                else
                {
                    DbParameter dbParm = cmd.CreateParameter();
                    dbParm.Value = parm;
                    cmd.Parameters.Add(dbParm);
                }
            }
            cmd.CommandText = stmtText;
            return cmd;
        }

        public async Task<T[]> QueryAsync<T>(DbConnection conn, Func<DbDataReader, T> map)
        {
            List<T> results = new List<T>();
            using (DbCommand cmd = Compile(conn))
            using (DbDataReader reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    results.Add(map(reader));
                }
            }
            return results.ToArray();
        }

        public async Task<int> ExecuteNonQueryAsync(DbConnection conn)
        {
            using (DbCommand cmd = Compile(conn))
            {
                return await cmd.ExecuteNonQueryAsync();
            }
        }

        public async Task<object> ExecuteScalarAsync(DbConnection conn)
        {
            using (DbCommand cmd = Compile(conn))
            {
                return await cmd.ExecuteScalarAsync();
            }
        }
    }
}
