// Core/Repositories/SqliteManualRepository.cs
using CoffeePOS.Core.Data;
using CoffeePOS.Core.Interfaces;
using Microsoft.Data.Sqlite;
using Humanizer;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace CoffeePOS.Core.Repositories;

public class SqliteManualRepository<T> : IRepository<T> where T : class
{
    private readonly SqliteConnectionFactory _connectionFactory;

    public SqliteManualRepository(SqliteConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IEnumerable<T>> GetAll()
    {
        var result = new List<T>();
        var tableName = typeof(T).Name.Pluralize();

        using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        var command = connection.CreateCommand();
        command.CommandText = $"SELECT * FROM {tableName}";

        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            var entity = MapToEntity(reader);
            result.Add(entity);
        }

        return result;
    }

    public async Task<T> GetById(int id)
    {
        var tableName = typeof(T).Name.Pluralize();

        using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        var command = connection.CreateCommand();
        command.CommandText = $"SELECT * FROM {tableName} WHERE Id = @Id";
        command.Parameters.AddWithValue("@Id", id);

        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return MapToEntity(reader);
        }
        return null;
    }

    public async Task<T> Add(T entity)
    {
        var tableName = typeof(T).Name.Pluralize();
        var properties = typeof(T).GetProperties().Where(p => p.Name != "Id"); // Bỏ Id nếu tự tăng
        var columnNames = string.Join(", ", properties.Select(p => p.Name));
        var paramNames = string.Join(", ", properties.Select(p => $"@{p.Name}"));

        using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        var command = connection.CreateCommand();
        command.CommandText = $"INSERT INTO {tableName} ({columnNames}) VALUES ({paramNames}) RETURNING *";

        foreach (var prop in properties)
        {
            command.Parameters.AddWithValue($"@{prop.Name}", prop.GetValue(entity) ?? DBNull.Value);
        }

        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return MapToEntity(reader);
        }
        return entity;
    }

    public async Task<T> Update(T entity)
    {
        var tableName = typeof(T).Name.Pluralize();
        var properties = typeof(T).GetProperties().Where(p => p.Name != "Id");
        var setClause = string.Join(", ", properties.Select(p => $"{p.Name} = @{p.Name}"));
        var id = (int)typeof(T).GetProperty("Id").GetValue(entity);

        using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        var command = connection.CreateCommand();
        command.CommandText = $"UPDATE {tableName} SET {setClause} WHERE Id = @Id RETURNING *";
        command.Parameters.AddWithValue("@Id", id);

        foreach (var prop in properties)
        {
            command.Parameters.AddWithValue($"@{prop.Name}", prop.GetValue(entity) ?? DBNull.Value);
        }

        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return MapToEntity(reader);
        }
        return entity;
    }

    public async Task<T> Delete(int id)
    {
        var tableName = typeof(T).Name.Pluralize();

        using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        var command = connection.CreateCommand();
        command.CommandText = $"DELETE FROM {tableName} WHERE Id = @Id RETURNING *";
        command.Parameters.AddWithValue("@Id", id);

        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return MapToEntity(reader);
        }
        return null;
    }

    private T MapToEntity(SqliteDataReader reader)
    {
        var entity = Activator.CreateInstance<T>();
        var properties = typeof(T).GetProperties();

        foreach (var prop in properties)
        {
            if (reader.HasColumn(prop.Name) && !reader.IsDBNull(reader.GetOrdinal(prop.Name)))
            {
                var value = reader.GetValue(reader.GetOrdinal(prop.Name));

                // Nếu thuộc tính là Nullable<T>, ta cần lấy kiểu thực tế bên trong
                var targetType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;

                // Chuyển đổi giá trị sang kiểu phù hợp
                var convertedValue = Convert.ChangeType(value, targetType);

                prop.SetValue(entity, convertedValue);
            }
        }

        return entity;
    }

}

public static class SqliteDataReaderExtensions
{
    public static bool HasColumn(this SqliteDataReader reader, string columnName)
    {
        for (int i = 0; i < reader.FieldCount; i++)
        {
            if (reader.GetName(i).Equals(columnName, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }
        return false;
    }
}