using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading.Tasks;

namespace Sekwell
{
    public static class DbConnectionExtensions
    {
        public static async Task<T[]> QueryAsync<T>(this DbConnection conn, FormattableString stmt, Func<DbDataReader, T> map)
        {
            return await conn.QueryAsync(new Statement(stmt), map);
        }

        public static async Task<T[]> QueryAsync<T>(this DbConnection conn, Statement stmt, Func<DbDataReader, T> map)
        {
            List<T> results = new List<T>();
            using (DbCommand cmd = stmt.Compile(conn))
            using (DbDataReader reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    results.Add(map(reader));
                }
            }
            return results.ToArray();

        }

        public static async Task<T> FirstOrDefaultAsync<T>(this DbConnection conn, FormattableString stmt, Func<DbDataReader, T> map)
        {
            return await conn.FirstOrDefaultAsync(new Statement(stmt), map);
        }

        public static async Task<T> FirstOrDefaultAsync<T>(this DbConnection conn, Statement stmt, Func<DbDataReader, T> map)
        {
            using (DbCommand cmd = stmt.Compile(conn))
            using (DbDataReader reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    return map(reader);
                }
            }
            return default;
        }

        public static async Task<T> SingleOrDefaultAsync<T>(this DbConnection conn, FormattableString stmt, Func<DbDataReader, T> map)
        {
            return await conn.SingleOrDefaultAsync(new Statement(stmt), map);
        }

        public static async Task<T> SingleOrDefaultAsync<T>(this DbConnection conn, Statement stmt, Func<DbDataReader, T> map)
        {
            using (DbCommand cmd = stmt.Compile(conn))
            using (DbDataReader reader = await cmd.ExecuteReaderAsync())
            {
                if (!await reader.ReadAsync())
                {
                    return default;
                }

                T result = map(reader);

                if (await reader.ReadAsync())
                {
                    throw new InvalidOperationException("The query contains more than one row");
                }

                return result;
            }
        }

        public static async Task<T> FirstAsync<T>(this DbConnection conn, FormattableString stmt, Func<DbDataReader, T> map)
        {
            return await conn.FirstAsync(new Statement(stmt), map);
        }

        public static async Task<T> FirstAsync<T>(this DbConnection conn, Statement stmt, Func<DbDataReader, T> map)
        {
            using (DbCommand cmd = stmt.Compile(conn))
            using (DbDataReader reader = await cmd.ExecuteReaderAsync())
            {
                if (!await reader.ReadAsync())
                {
                    throw new InvalidOperationException("The query returned no rows");
                }

                return map(reader);
            }
        }

        public static async Task<T> SingleAsync<T>(this DbConnection conn, FormattableString stmt, Func<DbDataReader, T> map)
        {
            return await conn.SingleAsync(new Statement(stmt), map);
        }

        public static async Task<T> SingleAsync<T>(this DbConnection conn, Statement stmt, Func<DbDataReader, T> map)
        {
            using (DbCommand cmd = stmt.Compile(conn))
            using (DbDataReader reader = await cmd.ExecuteReaderAsync())
            {
                if (!await reader.ReadAsync())
                {
                    throw new InvalidOperationException("The query returned no rows");
                }

                T result = map(reader);

                if (await reader.ReadAsync())
                {
                    throw new InvalidOperationException("The query contains more than one row");
                }

                return result;
            }
        }

        public static async Task<int> ExecuteNonQueryAsync(this DbConnection conn, FormattableString stmt)
        {
            return await conn.ExecuteNonQueryAsync(new Statement(stmt));
        }

        public static async Task<int> ExecuteNonQueryAsync(this DbConnection conn, Statement stmt)
        {
            using (DbCommand cmd = stmt.Compile(conn))
            {
                return await cmd.ExecuteNonQueryAsync();
            }
        }

        public static async Task<object> ExecuteScalarAsync(this DbConnection conn, FormattableString stmt)
        {
            return await conn.ExecuteScalarAsync(new Statement(stmt));
        }

        public static async Task<object> ExecuteScalarAsync(this DbConnection conn, Statement stmt)
        {
            using (DbCommand cmd = stmt.Compile(conn))
            {
                return await cmd.ExecuteScalarAsync();
            }
        }
    }
}
