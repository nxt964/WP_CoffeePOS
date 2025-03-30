using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoffeePOS.Core.Models;

namespace CoffeePOS.Core.Interfaces;
public interface IDao
{
    IUserRepository Users { get; }
    IRepository<Employee> Employees { get; }
    IRepository<Category> Categories { get; }
    IRepository<Product> Products { get; }
    IRepository<Ingredient> Ingredients { get; }
    IRepository<ProductIngredient> ProductIngredients { get; }
    IRepository<IngredientInventoryTransaction> IngredientInventoryTransactions { get; }
    IRepository<Customer> Customers { get; }
    IRepository<ServiceType> ServiceTypes { get; }
    IRepository<Table> Tables { get; }
    IRepository<Reservation> Reservations { get; }
    IRepository<Voucher> Vouchers { get; }
    IRepository<PaymentMethod> PaymentMethods { get; }
    IRepository<Order> Orders { get; }
    IRepository<OrderDetail> OrderDetails { get; }

    Task SaveChangesAsync(); // Ensures changes are committed in Unit of Work pattern
}
