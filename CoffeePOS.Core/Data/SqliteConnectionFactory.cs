// Core/Data/SqliteConnectionFactory.cs
using CoffeePOS.Core.Models;
using Microsoft.Data.Sqlite;
using System;

namespace CoffeePOS.Core.Data;

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