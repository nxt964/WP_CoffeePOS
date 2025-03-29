// Core/Data/SqliteConnectionFactory.cs
using Microsoft.Data.Sqlite;
using System;

namespace CoffeePOS.Data;

public class SqliteConnectionFactory
{
    private readonly string _connectionString;

    public SqliteConnectionFactory()
    {
        _connectionString = $"Data Source={AppContext.BaseDirectory}CoffeePOS.db";
    }

    public SqliteConnection CreateConnection()
    {
        return new SqliteConnection(_connectionString);
    }
}