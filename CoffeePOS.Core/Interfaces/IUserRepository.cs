// Core/Interfaces/IUserRepository.cs
using CoffeePOS.Core.Models;
using System.Threading.Tasks;

namespace CoffeePOS.Core.Interfaces;

public interface IUserRepository : IRepository<User>
{
    Task<User> Login(string username, string password);

    Task<User> AddTrialUser(User user);

    Task<bool> Signout();
}