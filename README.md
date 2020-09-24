# Sekwell
[![NuGet package](http://img.shields.io/nuget/v/Sekwell?style=flat&logo=nuget)](https://www.nuget.org/packages/Sekwell/ "View this project on NuGet")
[![Build Status](https://travis-ci.com/Shamus03/Sekwell.svg?token=WuKfy3V3Yw7K95LjG4aM&branch=master)](https://travis-ci.com/Shamus03/Sekwell)

A basic formatter that uses FormattableString to automagically escape query arguments.

# Use

Use the `Statement` class to dynamically build a SQL statement with interpolated arguments without fear of SQL injection.

`Compile` a `Statement` to create a `DbCommand` with the proper statement text and parameters, ready to execute using your preferred pattern, or call `ToSql` to get the raw statement and list of arguments passed in.

```c#
var name = "Shamus";
var stmt = new Statement($"SELECT * FROM Users")
    .Append($"WHERE FirstName={name}")

(string sql, object[] parms) = stmt.ToSql();
// sql = "SELECT * FROM Users WHERE FirstName=?"
// parms = {"Shamus"}

DbCommand cmd = stmt.Compile(connection);
// cmd.StatementText = "SELECT * FROM Users WHERE FirstName=?"
// cmd.Parameters[0] = DbParameter { Value = "Shamus" }
```

A statement's constructor only accepts a `FormattableString` as an argument to prevent accidental SQL injection.  To pass in a raw string, you must explicitly use one of the functions with `Raw` in its name.

```c#
var name = "Shamus";
var query = $"SELECT * FROM Users WHERE NAME={name}"

// Will not compile!  `query` is a `string`, not `FormattableString`!
var stmt = new Statement(query);

// This will compile, but will be vulnerable to attacks. Use `Raw` sparingly!
var stmt = Statement.Raw(query);
```

For situations where you must manually create `DbParameters` (like "InOut" parameters), you can pass a parameter instance into the statement.

```c#
var param = new OdbcParameter("myParam", "");
using(var cmd = new Statement($"CALL SomeProcedure({param})").Compile(conn))
{
    await cmd.ExecuteNonQueryAsync();
}
// read param.Value
```

This package also provides various helper functions for easily quering domain objects in a `System.Linq`-style syntax.

```c#
var name = "Shamus";
User user = await conn.FirstOrDefaultAsync($"SELECT * FROM Users WHERE FirstName={name}",
    async reader =>
    {
        return new User
        {
            ID = (string)reader["ID"],
            FirstName = (string)reader["FirstName"],
            LastName = (string)reader["LastName"],
        };
    });
```
