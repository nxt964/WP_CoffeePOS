// Core/Repositories/UserRepository.cs
using CoffeePOS.Core.Data;
using CoffeePOS.Core.Interfaces;
using CoffeePOS.Core.Models;
using Microsoft.Data.Sqlite;
using System.Threading.Tasks;

namespace CoffeePOS.Core.Repositories;

public class UserRepository : SqliteManualRepository<User>, IUserRepository
{
    private readonly SqliteConnectionFactory _connectionFactory;
    private User _currentUser;

    public UserRepository(SqliteConnectionFactory connectionFactory)
        : base(connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<User> Login(string username, string password)
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        var command = connection.CreateCommand();
        command.CommandText = "SELECT * FROM Users WHERE Username = @Username AND Password = @Password";
        command.Parameters.AddWithValue("@Username", username);
        command.Parameters.AddWithValue("@Password", password);

        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            _currentUser = MapToEntity(reader);
            return _currentUser;
        }

        return null;
    }

    public Task<bool> Signout()
    {
        _currentUser = null;
        return Task.FromResult(true);
    }
}