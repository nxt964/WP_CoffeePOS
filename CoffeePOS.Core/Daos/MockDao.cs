using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoffeePOS.Core.Interfaces;
using CoffeePOS.Core.Models;
using CoffeePOS.Core.Repositories;

namespace CoffeePOS.Core.Daos;
public class MockDao : IDao
{
    public IRepository<User> Users { get; } = new MockRepository<User>(new List<User>() {
        new User { Id = 1, Username = "admin", Password = "admin1234" },
        new User { Id = 2, Username = "employee1", Password = "employee1234" },
        new User { Id = 3, Username = "employee2", Password = "employee1234"}
    });

    public IRepository<Employee> Employees { get; } = new MockRepository<Employee>(new List<Employee>() {
        new Employee { Id = 1, EmployeeName = "John Doe", Email = "john@example.com", Phone = "123-456-7890", Salary = 2000 },
        new Employee { Id = 2, EmployeeName = "Jane Smith", Email = "jane@example.com", Phone = "234-567-8901", Salary = 2200 }
    });

    public IRepository<Category> Categories { get; } = new MockRepository<Category>(new List<Category>() {
        new Category { Id = 1, Name = "Coffee", Description = "Hot and cold coffee" },
        new Category { Id = 2, Name = "Tea", Description = "Various types of tea" },
        new Category { Id = 3, Name = "Pastries", Description = "Baked goods and snacks" },
        new Category { Id = 4, Name = "Juices", Description = "Fresh fruit juices" },
        new Category { Id = 5, Name = "Sandwiches", Description = "Savory sandwiches" },
        new Category { Id = 6, Name = "Milkshakes", Description = "Flavored milk beverages" },
        new Category { Id = 7, Name = "Desserts", Description = "Sweet treats" },
        new Category { Id = 8, Name = "Salads", Description = "Healthy greens" },
        new Category { Id = 9, Name = "Smoothies", Description = "Blended fruit drinks" },
        new Category { Id = 10, Name = "Soft Drinks", Description = "Carbonated drinks" }
    });

    public IRepository<Product> Products { get; } = new MockRepository<Product>(new List<Product>() { 

        // Coffee (CategoryId = 1)
        new Product { Id = 1, Name = "Espresso", Description = "Strong coffee", Price = 2.50, ImageURL = "ProductImage.jpg", IsStocked = true, CategoryId = 1 },
        new Product { Id = 2, Name = "Cappuccino", Description = "Espresso with milk", Price = 3.50, ImageURL = "ProductImage.jpg", IsStocked = true, CategoryId = 1 },
        new Product { Id = 3, Name = "Latte", Description = "Creamy espresso", Price = 4.00, ImageURL = "ProductImage.jpg", IsStocked = true, CategoryId = 1 },
        new Product { Id = 4, Name = "Americano", Description = "Espresso with water", Price = 2.75, ImageURL = "ProductImage.jpg", IsStocked = true, CategoryId = 1 },
        new Product { Id = 5, Name = "Mocha", Description = "Chocolate-flavored coffee", Price = 4.50, ImageURL = "ProductImage.jpg", IsStocked = true, CategoryId = 1 },

        // Tea (CategoryId = 2)
        new Product { Id = 6, Name = "Green Tea", Description = "Healthy green tea", Price = 2.00, ImageURL = "ProductImage.jpg", IsStocked = true, CategoryId = 2 },
        new Product { Id = 7, Name = "Black Tea", Description = "Strong tea", Price = 2.50, ImageURL = "ProductImage.jpg", IsStocked = true, CategoryId = 2 },
        new Product { Id = 8, Name = "Oolong Tea", Description = "Smooth flavor", Price = 3.00, ImageURL = "ProductImage.jpg", IsStocked = true, CategoryId = 2 },
        new Product { Id = 9, Name = "Herbal Tea", Description = "Caffeine-free tea", Price = 2.75, ImageURL = "ProductImage.jpg", IsStocked = true, CategoryId = 2 },
        new Product { Id = 10, Name = "Chai Tea", Description = "Spiced milk tea", Price = 3.50, ImageURL = "ProductImage.jpg", IsStocked = true, CategoryId = 2 },

        // Pastries (CategoryId = 3)
        new Product { Id = 11, Name = "Blueberry Muffin", Description = "Soft and sweet muffin", Price = 3.00, ImageURL = "ProductImage.jpg", IsStocked = true, CategoryId = 3 },
        new Product { Id = 12, Name = "Croissant", Description = "Buttery and flaky", Price = 2.50, ImageURL = "ProductImage.jpg", IsStocked = true, CategoryId = 3 },
        new Product { Id = 13, Name = "Danish Pastry", Description = "Sweet and fruity", Price = 3.25, ImageURL = "ProductImage.jpg", IsStocked = true, CategoryId = 3 },
        new Product { Id = 14, Name = "Chocolate Donut", Description = "Glazed donut", Price = 2.75, ImageURL = "ProductImage.jpg", IsStocked = true, CategoryId = 3 },
        new Product { Id = 15, Name = "Apple Turnover", Description = "Crispy apple filling", Price = 3.50, ImageURL = "ProductImage.jpg", IsStocked = true, CategoryId = 3 },

        // Juices (CategoryId = 4)
        new Product { Id = 16, Name = "Orange Juice", Description = "Fresh squeezed", Price = 3.00, ImageURL = "ProductImage.jpg", IsStocked = true, CategoryId = 4 },
        new Product { Id = 17, Name = "Apple Juice", Description = "Sweet and refreshing", Price = 2.75, ImageURL = "ProductImage.jpg", IsStocked = true, CategoryId = 4 },
        new Product { Id = 18, Name = "Lemonade", Description = "Tart and sweet", Price = 2.50, ImageURL = "ProductImage.jpg", IsStocked = true, CategoryId = 4 },
        new Product { Id = 19, Name = "Mango Juice", Description = "Rich mango flavor", Price = 3.25, ImageURL = "ProductImage.jpg", IsStocked = true, CategoryId = 4 },
        new Product { Id = 20, Name = "Grape Juice", Description = "Sweet grape flavor", Price = 3.00, ImageURL = "ProductImage.jpg", IsStocked = true, CategoryId = 4 },
   
        // Sandwiches (CategoryId = 5)
        new Product { Id = 21, Name = "Club Sandwich", Description = "Turkey, ham, bacon", Price = 5.50, ImageURL = "ProductImage.jpg", IsStocked = true, CategoryId = 5 },
        new Product { Id = 22, Name = "BLT Sandwich", Description = "Bacon, lettuce, tomato", Price = 4.75, ImageURL = "ProductImage.jpg", IsStocked = true, CategoryId = 5 },
        new Product { Id = 23, Name = "Grilled Cheese", Description = "Cheese & toast", Price = 4.00, ImageURL = "ProductImage.jpg", IsStocked = true, CategoryId = 5 },
        new Product { Id = 24, Name = "Tuna Sandwich", Description = "Tuna & mayo", Price = 5.00, ImageURL = "ProductImage.jpg", IsStocked = true, CategoryId = 5 },
        new Product { Id = 25, Name = "Chicken Avocado Sandwich", Description = "Chicken & avocado", Price = 5.75, ImageURL = "ProductImage.jpg", IsStocked = true, CategoryId = 5 },

        // Milkshakes (CategoryId = 6)
        new Product { Id = 26, Name = "Vanilla Milkshake", Description = "Classic vanilla", Price = 4.50, ImageURL = "ProductImage.jpg", IsStocked = true, CategoryId = 6 },
        new Product { Id = 27, Name = "Chocolate Milkshake", Description = "Rich chocolate", Price = 4.75, ImageURL = "ProductImage.jpg", IsStocked = true, CategoryId = 6 },
        new Product { Id = 28, Name = "Strawberry Milkshake", Description = "Fruity and creamy", Price = 5.00, ImageURL = "ProductImage.jpg", IsStocked = true, CategoryId = 6 },
        new Product { Id = 29, Name = "Oreo Milkshake", Description = "Cookies & cream", Price = 5.25, ImageURL = "ProductImage.jpg", IsStocked = true, CategoryId = 6 },
        new Product { Id = 30, Name = "Caramel Milkshake", Description = "Sweet caramel flavor", Price = 5.50, ImageURL = "ProductImage.jpg", IsStocked = true, CategoryId = 6 },

        // Desserts (CategoryId = 7)
        new Product { Id = 31, Name = "Chocolate Cake", Description = "Rich chocolate layers", Price = 6.00, ImageURL = "ProductImage.jpg", IsStocked = true, CategoryId = 7 },
        new Product { Id = 32, Name = "Cheesecake", Description = "Creamy and sweet", Price = 5.75, ImageURL = "ProductImage.jpg", IsStocked = true, CategoryId = 7 },
        new Product { Id = 33, Name = "Brownie", Description = "Dense & fudgy", Price = 4.50, ImageURL = "ProductImage.jpg", IsStocked = true, CategoryId = 7 },
        new Product { Id = 34, Name = "Apple Pie", Description = "Classic American dessert", Price = 5.25, ImageURL = "ProductImage.jpg", IsStocked = true, CategoryId = 7 },
        new Product { Id = 35, Name = "Tiramisu", Description = "Coffee-flavored dessert", Price = 6.50, ImageURL = "ProductImage.jpg", IsStocked = true, CategoryId = 7 },
    });

    public IRepository<Ingredient> Ingredients { get; } = new MockRepository<Ingredient>(new List<Ingredient>() {
        new Ingredient { Id = 1, Name = "Espresso", Unit = "shot"},
        new Ingredient { Id = 2, Name = "Milk", Unit = "cup" },
        new Ingredient { Id = 3, Name = "Sugar", Unit = "tsp" },
        new Ingredient { Id = 4, Name = "Chocolate", Unit = "oz" },
        new Ingredient { Id = 5, Name = "Tea Leaves", Unit = "oz" },
        new Ingredient { Id = 6, Name = "Water", Unit = "cup" },
        new Ingredient { Id = 7, Name = "Flour", Unit = "cup" },
        new Ingredient { Id = 8, Name = "Butter", Unit = "tbsp" },
        new Ingredient { Id = 9, Name = "Eggs", Unit = "each" },
        new Ingredient { Id = 10, Name = "Blueberries", Unit = "cup" },
        new Ingredient { Id = 11, Name = "Apples", Unit = "each" },
        new Ingredient { Id = 12, Name = "Oranges", Unit = "each" },
        new Ingredient { Id = 13, Name = "Lemons", Unit = "each" },
        new Ingredient { Id = 14, Name = "Mangoes", Unit = "each" },
        new Ingredient { Id = 15, Name = "Grapes", Unit = "cup" },
        new Ingredient { Id = 16, Name = "Turkey", Unit = "oz" },
        new Ingredient { Id = 17, Name = "Ham", Unit = "oz" },
        new Ingredient { Id = 18, Name = "Bacon", Unit = "oz" },
        new Ingredient { Id = 19, Name = "Lettuce", Unit = "cup" },
        new Ingredient { Id = 20, Name = "Tomatoes", Unit = "each"}
    });

    public IRepository<ProductIngredient> ProductIngredients { get; } = new MockRepository<ProductIngredient>(new List<ProductIngredient>()
    {
        new ProductIngredient { Id = 1, ProductId = 1, IngredientId = 1, QuantityUsed = 1 },
        new ProductIngredient { Id = 2, ProductId = 2, IngredientId = 1, QuantityUsed = 1 },
        new ProductIngredient { Id = 3, ProductId = 2, IngredientId = 2, QuantityUsed = 1 },
        new ProductIngredient { Id = 4, ProductId = 3, IngredientId = 1, QuantityUsed = 1 },
        new ProductIngredient { Id = 5, ProductId = 3, IngredientId = 2, QuantityUsed = 1 },
        new ProductIngredient { Id = 6, ProductId = 3, IngredientId = 3, QuantityUsed = 1 },
        new ProductIngredient { Id = 7, ProductId = 4, IngredientId = 1, QuantityUsed = 1 },
        new ProductIngredient { Id = 8, ProductId = 4, IngredientId = 6, QuantityUsed = 1 },
        new ProductIngredient { Id = 9, ProductId = 5, IngredientId = 1, QuantityUsed = 1 },
        new ProductIngredient { Id = 10, ProductId = 5, IngredientId = 4, QuantityUsed = 1 },
        new ProductIngredient { Id = 11, ProductId = 6, IngredientId = 5, QuantityUsed = 1 },
        new ProductIngredient { Id = 12, ProductId = 7, IngredientId = 5, QuantityUsed = 1 },
        new ProductIngredient { Id = 13, ProductId = 8, IngredientId = 5, QuantityUsed = 1 },
        new ProductIngredient { Id = 14, ProductId = 9, IngredientId = 5, QuantityUsed = 1 },
        new ProductIngredient { Id = 15, ProductId = 10, IngredientId = 5, QuantityUsed = 1 },
        new ProductIngredient { Id = 16, ProductId = 11, IngredientId = 7, QuantityUsed = 1 },
        new ProductIngredient { Id = 17, ProductId = 12, IngredientId = 7, QuantityUsed = 1 },
        new ProductIngredient { Id = 18, ProductId = 13, IngredientId = 7, QuantityUsed = 1 },
    });

    public IRepository<IngredientInventoryTransaction> IngredientInventoryTransactions { get; } = new MockRepository<IngredientInventoryTransaction>( new List<IngredientInventoryTransaction>() { });

    public IRepository<Customer> Customers { get; } = new MockRepository<Customer>(new List<Customer>() {
        new Customer { Id = 1, Name = "Alice", Phone = "111-222-3333", IsMembership = true, Points = 100 },
        new Customer { Id = 2, Name = "Bob", Phone = "222-333-4444", IsMembership = false, Points = 50 },
        new Customer { Id = 3, Name = "Charlie", Phone = "333-444-5555", IsMembership = true, Points = 200 },
        new Customer { Id = 4, Name = "David", Phone = "444-555-6666", IsMembership = false, Points = 30 },
        new Customer { Id = 5, Name = "Emma", Phone = "555-666-7777", IsMembership = true, Points = 150 },
        new Customer { Id = 6, Name = "Frank", Phone = "666-777-8888", IsMembership = false, Points = 75 },
        new Customer { Id = 7, Name = "Grace", Phone = "777-888-9999", IsMembership = true, Points = 250 },
        new Customer { Id = 8, Name = "Henry", Phone = "888-999-0000", IsMembership = false, Points = 40 },
        new Customer { Id = 9, Name = "Ivy", Phone = "999-000-1111", IsMembership = true, Points = 180 },
        new Customer { Id = 10, Name = "Jack", Phone = "000-111-2222", IsMembership = false, Points = 60 },
    });
    public IRepository<ServiceType> ServiceTypes { get; } = new MockRepository<ServiceType>(new List<ServiceType>() { });

    public IRepository<Table> Tables { get; } = new MockRepository<Table>(new List<Table>() { } );

    public IRepository<Reservation> Reservations { get; } = new MockRepository<Reservation>(new List<Reservation>() { });

    public IRepository<Voucher> Vouchers { get; } = new MockRepository<Voucher>(new List<Voucher>() { });

    public IRepository<PaymentMethod> PaymentMethods { get; } = new MockRepository<PaymentMethod>(new List<PaymentMethod>() { });

    public IRepository<Order> Orders { get; } = new MockRepository<Order>(new List<Order>() { });

    public IRepository<OrderDetail> OrderDetails { get; } = new MockRepository<OrderDetail>(new List<OrderDetail>() { });

    public Task SaveChangesAsync() => Task.CompletedTask;
}
