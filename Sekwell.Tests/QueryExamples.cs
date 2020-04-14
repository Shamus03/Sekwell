using System.Data.Common;
using System.Threading.Tasks;

namespace Sekwell.Tests
{
    public class QueryExamples
    {
        public async Task Example_Query()
        {
            DbConnection conn = GetDbConnection();

            string id = "abc-def";
            User[] users = await conn.QueryAsync($"SELECT * FROM Users WHERE ID={id}", ReadUser);
        }

        public async Task Example_FirstOrDefault()
        {
            DbConnection conn = GetDbConnection();

            string firstName = "Shamus";
            User user = await conn.FirstOrDefaultAsync($"SELECT * FROM Users WHERE FirstName={firstName}", ReadUser);
        }

        public static User ReadUser(DbDataReader row)
        {
            return new User
            {
                ID = row["ID"].ToString(),
                FirstName = row["FirstName"].ToString(),
                LastName = row["LastName"].ToString(),
            };
        }

        public class User
        {
            public string ID { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
        }

        private DbConnection GetDbConnection()
        {
            // Just for examples
            return null;
        }
    }
}
