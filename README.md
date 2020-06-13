# Sekwell
[![NuGet package](http://img.shields.io/nuget/v/Sekwell?style=flat&logo=npm)](https://www.nuget.org/packages/Sekwell/ "View this project on NuGet")
[![Build Status](https://travis-ci.com/Shamus03/Sekwell.svg?token=WuKfy3V3Yw7K95LjG4aM&branch=master)](https://travis-ci.com/Shamus03/Sekwell)

A basic formatter that uses FormattableString to automagically escape query arguments.

# Use

Use the `Statement` class to dynamically build a SQL statement with interpolated arguments without fear of SQL injection.

`Compile` a `Statement` to create a `DbCommand` with the proper statement text and parameters, ready to execute using your preferred pattern.

```c#
var name = "Shamus";
var stmt = new Statement($"SELECT * FROM Users WHERE FirstName={name}")

(string sql, object[] parms) = stmt.ToSql();
// sql = "SELECT * FROM Users WHERE FirstName=?"
// parms = {"Shamus"}

DbCommand cmd = stmt.Compile(connection);
// cmd.StatementText = "SELECT * FROM Users WHERE FirstName=?"
// cmd.Parameters[0] = DbParameter { Value = "Shamus" }
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
