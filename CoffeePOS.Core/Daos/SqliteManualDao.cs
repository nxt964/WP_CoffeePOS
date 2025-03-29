using CoffeePOS.Core.Data;
using CoffeePOS.Core.Interfaces;
using CoffeePOS.Core.Models;
using CoffeePOS.Core.Repositories;
using Microsoft.Data.Sqlite;
using System;
using System.Threading.Tasks;

namespace CoffeePOS.Core.Daos;

public class SqliteManualDao : IDao
{
    private readonly SqliteConnectionFactory _connectionFactory;

    public SqliteManualDao(SqliteConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public IRepository<User> Users => new SqliteManualRepository<User>(_connectionFactory);
    public IRepository<Employee> Employees => new SqliteManualRepository<Employee>(_connectionFactory);
    public IRepository<Category> Categories => new SqliteManualRepository<Category>(_connectionFactory);
    public IRepository<Product> Products => new SqliteManualRepository<Product>(_connectionFactory);
    public IRepository<Customer> Customers => new SqliteManualRepository<Customer>(_connectionFactory);
    public IRepository<Ingredient> Ingredients => new SqliteManualRepository<Ingredient>(_connectionFactory);
    public IRepository<ProductIngredient> ProductIngredients => new SqliteManualRepository<ProductIngredient>(_connectionFactory);
    public IRepository<IngredientInventoryTransaction> IngredientInventoryTransactions => new SqliteManualRepository<IngredientInventoryTransaction>(_connectionFactory);
    public IRepository<ServiceType> ServiceTypes => new SqliteManualRepository<ServiceType>(_connectionFactory);
    public IRepository<Table> Tables => new SqliteManualRepository<Table>(_connectionFactory);
    public IRepository<Reservation> Reservations => new SqliteManualRepository<Reservation>(_connectionFactory);
    public IRepository<Voucher> Vouchers => new SqliteManualRepository<Voucher>(_connectionFactory);
    public IRepository<PaymentMethod> PaymentMethods => new SqliteManualRepository<PaymentMethod>(_connectionFactory);
    public IRepository<Order> Orders => new SqliteManualRepository<Order>(_connectionFactory);
    public IRepository<OrderDetail> OrderDetails => new SqliteManualRepository<OrderDetail>(_connectionFactory);

    public async Task SaveChangesAsync()
    {
        // No need for explicit transaction management as each repository operation handles its own
        await Task.CompletedTask;
    }

    public async Task InitializeDatabaseAsync()
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        var commands = new[]
        {
            // Users Table
            @"CREATE TABLE IF NOT EXISTS Users (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Username TEXT NOT NULL,
                Password TEXT NOT NULL
            )",
            // Employees Table
            @"CREATE TABLE IF NOT EXISTS Employees (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                EmployeeName TEXT NOT NULL,
                Email TEXT,
                Phone TEXT,
                Salary REAL
            )",
            // Categories Table
            @"CREATE TABLE IF NOT EXISTS Categories (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Name TEXT NOT NULL,
                Description TEXT
            )",
            // Products Table
            @"CREATE TABLE IF NOT EXISTS Products (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Name TEXT NOT NULL,
                Description TEXT,
                Price REAL,
                Image TEXT,
                IsStocked INTEGER,
                CategoryId INTEGER,
                FOREIGN KEY (CategoryId) REFERENCES Categories(Id)
            )",
            // Customers Table
            @"CREATE TABLE IF NOT EXISTS Customers (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Name TEXT NOT NULL,
                Phone TEXT,
                IsMembership INTEGER,
                Points INTEGER
            )",
            // Ingredients Table
            @"CREATE TABLE IF NOT EXISTS Ingredients (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Name TEXT NOT NULL,
                Unit TEXT NOT NULL
            )",
            // ProductIngredients Table
            @"CREATE TABLE IF NOT EXISTS ProductIngredients (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                ProductId INTEGER,
                IngredientId INTEGER,
                QuantityUsed REAL,
                FOREIGN KEY (ProductId) REFERENCES Products(Id),
                FOREIGN KEY (IngredientId) REFERENCES Ingredients(Id)
            )",
            // IngredientInventoryTransactions Table
            @"CREATE TABLE IF NOT EXISTS IngredientInventoryTransactions (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                IngredientId INTEGER,
                QuantityChange INTEGER,
                TransactionDate TEXT,
                TransactionType TEXT,
                FOREIGN KEY (IngredientId) REFERENCES Ingredients(Id)
            )",
            // ServiceTypes Table
            @"CREATE TABLE IF NOT EXISTS ServiceTypes (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Name TEXT NOT NULL
            )",
            // Tables Table
            @"CREATE TABLE IF NOT EXISTS Tables (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                TableNumber TEXT NOT NULL,
                Status TEXT NOT NULL
            )",
            // Reservations Table
            @"CREATE TABLE IF NOT EXISTS Reservations (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                CustomerId INTEGER,
                TableId INTEGER,
                ReservationDate TEXT,
                StartTime TEXT,
                EndTime TEXT,
                Status TEXT,
                FOREIGN KEY (CustomerId) REFERENCES Customers(Id),
                FOREIGN KEY (TableId) REFERENCES Tables(Id)
            )",
            // Vouchers Table
            @"CREATE TABLE IF NOT EXISTS Vouchers (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Code TEXT NOT NULL,
                DiscountPercentage INTEGER,
                ExpirationDate TEXT,
                IsUsed INTEGER
            )",
            // PaymentMethods Table
            @"CREATE TABLE IF NOT EXISTS PaymentMethods (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Name TEXT NOT NULL,
                Description TEXT
            )",
            // Orders Table
            @"CREATE TABLE IF NOT EXISTS Orders (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                CustomerId INTEGER,
                OrderDate TEXT,
                PaymentDate TEXT,
                Status TEXT,
                TableId INTEGER,
                VoucherId INTEGER,
                TotalAmount REAL,
                PaymentMethodId INTEGER,
                ServiceTypeId INTEGER,
                FOREIGN KEY (CustomerId) REFERENCES Customers(Id),
                FOREIGN KEY (TableId) REFERENCES Tables(Id),
                FOREIGN KEY (VoucherId) REFERENCES Vouchers(Id),
                FOREIGN KEY (PaymentMethodId) REFERENCES PaymentMethods(Id),
                FOREIGN KEY (ServiceTypeId) REFERENCES ServiceTypes(Id)
            )",
            // OrderDetails Table
            @"CREATE TABLE IF NOT EXISTS OrderDetails (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                OrderId INTEGER,
                ProductId INTEGER,
                Quantity INTEGER,
                Price REAL,
                FOREIGN KEY (OrderId) REFERENCES Orders(Id),
                FOREIGN KEY (ProductId) REFERENCES Products(Id)
            )",

            // Insert initial data
            // Users
            "INSERT OR IGNORE INTO Users (Id, Username, Password) VALUES (1, 'admin', 'admin1234')",
            "INSERT OR IGNORE INTO Users (Id, Username, Password) VALUES (2, 'employee1', 'employee1234')",
            "INSERT OR IGNORE INTO Users (Id, Username, Password) VALUES (3, 'employee2', 'employee1234')",
            // Employees
            "INSERT OR IGNORE INTO Employees (Id, EmployeeName, Email, Phone, Salary) VALUES (1, 'John Doe', 'john@example.com', '123-456-7890', 2000)",
            "INSERT OR IGNORE INTO Employees (Id, EmployeeName, Email, Phone, Salary) VALUES (2, 'Jane Smith', 'jane@example.com', '234-567-8901', 2200)",
            // Categories
            "INSERT OR IGNORE INTO Categories (Id, Name, Description) VALUES (1, 'Coffee', 'Hot and cold coffee')",
            "INSERT OR IGNORE INTO Categories (Id, Name, Description) VALUES (2, 'Tea', 'Various types of tea')",
            "INSERT OR IGNORE INTO Categories (Id, Name, Description) VALUES (3, 'Pastries', 'Baked goods and snacks')",
            "INSERT OR IGNORE INTO Categories (Id, Name, Description) VALUES (4, 'Juices', 'Fresh fruit juices')",
            "INSERT OR IGNORE INTO Categories (Id, Name, Description) VALUES (5, 'Sandwiches', 'Savory sandwiches')",
            "INSERT OR IGNORE INTO Categories (Id, Name, Description) VALUES (6, 'Milkshakes', 'Flavored milk beverages')",
            "INSERT OR IGNORE INTO Categories (Id, Name, Description) VALUES (7, 'Desserts', 'Sweet treats')",
            // Products
            "INSERT OR IGNORE INTO Products (Id, Name, Description, Price, Image, IsStocked, CategoryId) VALUES (1, 'Espresso', 'Strong coffee', 2.50, 'ms-appx:///Assets/ProductImage.jpg', 1, 1)",
            "INSERT OR IGNORE INTO Products (Id, Name, Description, Price, Image, IsStocked, CategoryId) VALUES (2, 'Cappuccino', 'Espresso with milk', 3.50, 'ms-appx:///Assets/ProductImage.jpg', 1, 1)",
            "INSERT OR IGNORE INTO Products (Id, Name, Description, Price, Image, IsStocked, CategoryId) VALUES (3, 'Latte', 'Creamy espresso', 4.00, 'ms-appx:///Assets/ProductImage.jpg', 1, 1)",
            "INSERT OR IGNORE INTO Products (Id, Name, Description, Price, Image, IsStocked, CategoryId) VALUES (6, 'Green Tea', 'Healthy green tea', 2.00, 'ms-appx:///Assets/ProductImage.jpg', 0, 2)",
            "INSERT OR IGNORE INTO Products (Id, Name, Description, Price, Image, IsStocked, CategoryId) VALUES (11, 'Blueberry Muffin', 'Soft and sweet muffin', 3.00, 'ms-appx:///Assets/ProductImage.jpg', 1, 3)",
            // Customers
            "INSERT OR IGNORE INTO Customers (Id, Name, Phone, IsMembership, Points) VALUES (1, 'Alice', '0123456789', 1, 100)",
            "INSERT OR IGNORE INTO Customers (Id, Name, Phone, IsMembership, Points) VALUES (2, 'Bob', '222-333-4444', 0, 50)",
            // Ingredients
            "INSERT OR IGNORE INTO Ingredients (Id, Name, Unit) VALUES (1, 'Espresso', 'shot')",
            "INSERT OR IGNORE INTO Ingredients (Id, Name, Unit) VALUES (2, 'Milk', 'cup')",
            "INSERT OR IGNORE INTO Ingredients (Id, Name, Unit) VALUES (3, 'Sugar', 'tsp')",
            // ProductIngredients
            "INSERT OR IGNORE INTO ProductIngredients (Id, ProductId, IngredientId, QuantityUsed) VALUES (1, 1, 1, 1)",
            "INSERT OR IGNORE INTO ProductIngredients (Id, ProductId, IngredientId, QuantityUsed) VALUES (2, 2, 1, 1)",
            "INSERT OR IGNORE INTO ProductIngredients (Id, ProductId, IngredientId, QuantityUsed) VALUES (3, 2, 2, 1)",
            // IngredientInventoryTransactions
            "INSERT OR IGNORE INTO IngredientInventoryTransactions (Id, IngredientId, QuantityChange, TransactionDate, TransactionType) VALUES (1, 1, 50, '2025-03-01 10:00:00', 'Purchase')",
            "INSERT OR IGNORE INTO IngredientInventoryTransactions (Id, IngredientId, QuantityChange, TransactionDate, TransactionType) VALUES (2, 2, -10, '2025-03-02 14:30:00', 'Usage')",
            // ServiceTypes
            "INSERT OR IGNORE INTO ServiceTypes (Id, Name) VALUES (1, 'Dine-in')",
            "INSERT OR IGNORE INTO ServiceTypes (Id, Name) VALUES (2, 'Take-away')",
            // Tables
            "INSERT OR IGNORE INTO Tables (Id, TableNumber, Status) VALUES (1, 'T01', 'Available')",
            "INSERT OR IGNORE INTO Tables (Id, TableNumber, Status) VALUES (2, 'T02', 'Occupied')",
            // Reservations
            "INSERT OR IGNORE INTO Reservations (Id, CustomerId, TableId, ReservationDate, StartTime, EndTime, Status) VALUES (1, 1, 1, '2025-03-25', '18:00', '20:00', 'Confirmed')",
            // Vouchers
            "INSERT OR IGNORE INTO Vouchers (Id, Code, DiscountPercentage, ExpirationDate, IsUsed) VALUES (1, 'WELCOME10', 10, '2025-06-30', 0)",
            "INSERT OR IGNORE INTO Vouchers (Id, Code, DiscountPercentage, ExpirationDate, IsUsed) VALUES (2, 'COFFEE20', 20, '2025-04-15', 1)",
            // PaymentMethods
            "INSERT OR IGNORE INTO PaymentMethods (Id, Name, Description) VALUES (1, 'Cash', 'Payment in cash')",
            "INSERT OR IGNORE INTO PaymentMethods (Id, Name, Description) VALUES (2, 'Card', 'Payment via credit or debit card')",
            // Orders
            "INSERT OR IGNORE INTO Orders (Id, CustomerId, OrderDate, PaymentDate, Status, TableId, VoucherId, TotalAmount, PaymentMethodId, ServiceTypeId) VALUES (1, 1, '2025-03-20 10:00:00', '2025-03-20 10:05:00', 'Completed', 1, 1, 15.00, 1, 1)",
            "INSERT OR IGNORE INTO Orders (Id, CustomerId, OrderDate, PaymentDate, Status, TableId, VoucherId, TotalAmount, PaymentMethodId, ServiceTypeId) VALUES (2, 2, '2025-03-21 12:30:00', NULL, 'Pending', NULL, NULL, 8.50, 2, 2)",
            // OrderDetails
            "INSERT OR IGNORE INTO OrderDetails (Id, OrderId, ProductId, Quantity, Price) VALUES (1, 1, 1, 2, 2.50)",
            "INSERT OR IGNORE INTO OrderDetails (Id, OrderId, ProductId, Quantity, Price) VALUES (2, 1, 11, 1, 3.00)"
        };

        foreach (var commandText in commands)
        {
            using var command = connection.CreateCommand();
            command.CommandText = commandText;
            await command.ExecuteNonQueryAsync();
        }
    }
}