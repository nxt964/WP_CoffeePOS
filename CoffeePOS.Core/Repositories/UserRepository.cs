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

    public async Task<User?> AddTrialUser(User user)
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        // Check if the user already exists
        var checkCmd = connection.CreateCommand();
        checkCmd.CommandText = "SELECT COUNT(*) FROM Users WHERE Username = @Username";
        checkCmd.Parameters.AddWithValue("@Username", user.Username);

        var count = (long)await checkCmd.ExecuteScalarAsync();
        if (count > 0)
        {
            return null;
        }

        // Insert the new user
        var command = connection.CreateCommand();
        command.CommandText = "INSERT INTO Users (Username, Password, ExpireAt) VALUES (@Username, @Password, @ExpireAt)";
        command.Parameters.AddWithValue("@Username", user.Username);
        command.Parameters.AddWithValue("@Password", BCrypt.Net.BCrypt.HashPassword(user.Password));
        command.Parameters.AddWithValue("@ExpireAt", user.ExpireAt?.ToString("O"));
        await command.ExecuteNonQueryAsync();

        return user;
    }

    public async Task<User> Login(string username, string password)
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        var command = connection.CreateCommand();
        //command.CommandText = "SELECT * FROM Users WHERE Username = @Username AND Password = @Password";
        //command.Parameters.AddWithValue("@Username", username);
        //command.Parameters.AddWithValue("@Password", password);

        //using var reader = await command.ExecuteReaderAsync();
        //if (await reader.ReadAsync())
        //{
        //    _currentUser = MapToEntity(reader);
        //    return _currentUser;
        //}
        command.CommandText = "SELECT * FROM Users WHERE Username = @Username";
        command.Parameters.AddWithValue("@Username", username);

        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            var storedHash = reader.GetString(reader.GetOrdinal("Password"));
            if (BCrypt.Net.BCrypt.Verify(password, storedHash))
            {
                _currentUser = MapToEntity(reader);
                return _currentUser;
            }
        }

        return null;
    }

    public Task<bool> Signout()
    {
        _currentUser = null;
        return Task.FromResult(true);
    }
}