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
    private readonly IUserRepository _userRepository;


    public SqliteManualDao(SqliteConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
        _userRepository = new UserRepository(_connectionFactory);
    }

    public IUserRepository Users => _userRepository;
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
                Username TEXT NOT NULL UNIQUE,
                Password TEXT NOT NULL,
                ExpireAt TEXT
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
            // ProductIngredients Table
            @"CREATE TABLE IF NOT EXISTS ProductIngredients (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                ProductId INTEGER,
                IngredientId INTEGER,
                QuantityUsed REAL,
                FOREIGN KEY (ProductId) REFERENCES Products(Id),
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
            @"CREATE TABLE IF NOT EXISTS Ingredients (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Name TEXT NOT NULL,
                Unit TEXT NOT NULL,
                Quantity INTEGER NOT NULL DEFAULT 0,
                Threshold INTEGER NOT NULL DEFAULT 0
            );",

        // Create IngredientInventoryTransactions table
            @"CREATE TABLE IF NOT EXISTS IngredientInventoryTransactions (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                IngredientId INTEGER NOT NULL,
                Timestamp INTEGER NOT NULL,
                Quantity INTEGER NOT NULL,
                Unit TEXT NOT NULL,
                TransactionType TEXT NOT NULL,
                UnitPrice REAL NOT NULL DEFAULT 0,
                FOREIGN KEY (IngredientId) REFERENCES Ingredients(Id)
            );",


        // Insert initial data
        // Users
        // Insert initial data with hashed passwords
        $"INSERT OR IGNORE INTO Users (Id, Username, Password) VALUES (1, 'admin', '{BCrypt.Net.BCrypt.HashPassword("admin1234")}')",
        $"INSERT OR IGNORE INTO Users (Id, Username, Password) VALUES (2, 'employee1', '{BCrypt.Net.BCrypt.HashPassword("employee1234")}')",
        $"INSERT OR IGNORE INTO Users (Id, Username, Password) VALUES (3, 'employee2', '{BCrypt.Net.BCrypt.HashPassword("employee1234")}')",
            // Employees
            "INSERT OR IGNORE INTO Employees (Id, EmployeeName, Email, Phone, Salary) VALUES (1, 'John Doe', 'john@example.com', '123-456-7890', 2000)",
            "INSERT OR IGNORE INTO Employees (Id, EmployeeName, Email, Phone, Salary) VALUES (2, 'Jane Smith', 'jane@example.com', '234-567-8901', 2200)",
            // Categories
            "INSERT OR IGNORE INTO Categories (Id, Name, Description) VALUES (1, 'Coffee', 'Hot and cold coffee')",
            "INSERT OR IGNORE INTO Categories (Id, Name, Description) VALUES (2, 'Tea', 'Various types of tea')",
            "INSERT OR IGNORE INTO Categories (Id, Name, Description) VALUES (3, 'Milkshakes', 'Flavored milk beverages')",
            "INSERT OR IGNORE INTO Categories (Id, Name, Description) VALUES (4, 'Juices', 'Fresh fruit juices')",
            "INSERT OR IGNORE INTO Categories (Id, Name, Description) VALUES (5, 'Pastries', 'Baked goods and snacks')",
            "INSERT OR IGNORE INTO Categories (Id, Name, Description) VALUES (6, 'Sandwiches', 'Savory sandwiches')",
            "INSERT OR IGNORE INTO Categories (Id, Name, Description) VALUES (7, 'Desserts', 'Sweet treats')",
            // Products
            @"INSERT OR IGNORE INTO Products (Id, Name, Description, Price, Image, IsStocked, CategoryId) VALUES
                (1, 'Espresso','Strong coffee',2.5,'https://res.cloudinary.com/thanhnguyencloud/image/upload/v1744388923/WP_CoffeePOS/Screenshot_2025-04-11_232824_lfaefl.png',1,1),
                (2, 'Cappuccino','Espresso with milk',3.5,'https://res.cloudinary.com/thanhnguyencloud/image/upload/v1744388310/WP_CoffeePOS/cafe-cappuccino-3_uvnysk.jpg',0,1),
                (3, 'Latte','Creamy espresso',4.0,'https://res.cloudinary.com/thanhnguyencloud/image/upload/v1744278708/Cardamom-latte-square_va6w6q.jpg',1,1),
                (4, 'Chamomile Tea','Relaxing chamomile flavor',3.0,'https://res.cloudinary.com/thanhnguyencloud/image/upload/v1744625269/WP_CoffeePOS/Chamomile-tea-in-a-tea-cup-61063da_qbs6ov.jpg',1,2),
                (5, 'Blueberry Muffin','Soft and sweet muffin',3.0,'https://res.cloudinary.com/thanhnguyencloud/image/upload/v1744279549/blueberry-muffin-recipe_fh11yi.jpg',1,5),
                (6, 'Peach Tea','Peach tea',3.4,'https://res.cloudinary.com/thanhnguyencloud/image/upload/v1744279008/images_urobbj.jpg',1,2),
                (7, 'Orange Juice','Fresh Squeezed Orange Juice',3.2,'https://res.cloudinary.com/thanhnguyencloud/image/upload/v1744278971/Fresh-squeezed-orange-juice-featured_etov3t.jpg',1,4),
                (8, 'Club Sandwich','Turkey, ham, bacon',5.5,'https://res.cloudinary.com/thanhnguyencloud/image/upload/v1744280009/WP_CoffeePOS/chicken-club-sandwich-ft-500x500_t5rojn.jpg',1,6),
                (9, 'BLT Sandwich','Bacon, lettuce, tomato',4.75,'https://res.cloudinary.com/thanhnguyencloud/image/upload/v1744280080/WP_CoffeePOS/avocado-blt-thumb_u0xs46.jpg',1,6),
                (10, 'Grilled Cheese','Cheese & toast',4.0,'https://res.cloudinary.com/thanhnguyencloud/image/upload/v1744280125/WP_CoffeePOS/images_1_d8d7xm.jpg',1,6),
                (11, 'Americano','Espresso with water',2.75,'https://res.cloudinary.com/thanhnguyencloud/image/upload/v1744280277/WP_CoffeePOS/Americano-Da_Nong-scaled_xmsjgf.jpg',1,1),
                (12, 'Mocha','Chocolate-flavored coffee',4.5,'https://res.cloudinary.com/thanhnguyencloud/image/upload/v1744280399/WP_CoffeePOS/mocha-latte-13_fpqomd.jpg',1,1),
                (13, 'Oolong Tea','Smooth flavor',3.0,'https://res.cloudinary.com/thanhnguyencloud/image/upload/v1744280640/WP_CoffeePOS/Screenshot_2025-04-10_172347_jrn6rw.png',1,2),
                (14, 'Herbal Tea','Caffeine-free tea',2.75,'https://res.cloudinary.com/thanhnguyencloud/image/upload/v1744280740/WP_CoffeePOS/Screenshot_2025-04-10_172533_kogkzx.png',1,2),
                (15, 'Chai Tea','Spiced milk tea',3.5,'https://res.cloudinary.com/thanhnguyencloud/image/upload/v1744280809/WP_CoffeePOS/images_2_jtuehw.jpg',1,2),
                (16, 'Croissant','Buttery and flaky',2.5,'https://res.cloudinary.com/thanhnguyencloud/image/upload/v1744280859/WP_CoffeePOS/French-Croissants-SM-2363_kt7fsu.jpg',1,5),
                (17, 'Danish Pastry','Sweet and fruity',3.25,'https://res.cloudinary.com/thanhnguyencloud/image/upload/v1744280904/WP_CoffeePOS/Danish-pastry-thumbnail_nf5csq.jpg',1,5),
                (18, 'Chocolate Donut','Glazed donut',2.75,'https://res.cloudinary.com/thanhnguyencloud/image/upload/v1744280940/WP_CoffeePOS/glazed-chocolate-donuts-thumbnail_llvla6.jpg',1,5),
                (19, 'Apple Turnover','Crispy apple filling',3.5,'https://res.cloudinary.com/thanhnguyencloud/image/upload/v1744281002/WP_CoffeePOS/images_3_xapraa.jpg',1,5),
                (20, 'Apple Juice','Sweet and refreshing',2.75,'https://res.cloudinary.com/thanhnguyencloud/image/upload/v1744281060/WP_CoffeePOS/images_4_tmsuuq.jpg',1,4),
                (21, 'Lemonade','Tart and sweet',2.5,'https://res.cloudinary.com/thanhnguyencloud/image/upload/v1744281116/WP_CoffeePOS/DSC_3444-3_cctl4y.jpg',1,4),
                (22, 'Mango Juice','Rich mango flavor',3.25,'https://res.cloudinary.com/thanhnguyencloud/image/upload/v1744281155/WP_CoffeePOS/mango-juice-recipe_sqz0lz.jpg',0,4),
                (23, 'Grape Juice','Sweet grape flavor',3.0,'https://res.cloudinary.com/thanhnguyencloud/image/upload/v1744281205/WP_CoffeePOS/images_5_o2v1ix.jpg',0,4),
                (24, 'Tuna Sandwich','Tuna & mayo',5.0,'https://res.cloudinary.com/thanhnguyencloud/image/upload/v1744281274/WP_CoffeePOS/images_6_k6lcbh.jpg',1,6),
                (25, 'Vanilla Milkshake','Classic vanilla',4.5,'https://res.cloudinary.com/thanhnguyencloud/image/upload/v1744281349/WP_CoffeePOS/vanilla-milkshake-recipe_dkqsad.jpg',1,3),
                (26, 'Chocolate Milkshake','Rich chocolate',4.75,'https://res.cloudinary.com/thanhnguyencloud/image/upload/v1744281456/WP_CoffeePOS/The-Best-Chocolate-Milkshake-Featured-Image_jbnfjk.jpg',1,3),
                (27, 'Strawberry Milkshake','Fruity and creamy',4.75,'https://res.cloudinary.com/thanhnguyencloud/image/upload/v1744281494/WP_CoffeePOS/Strawberry-milkshake-frappuccino-featured_cohzlu.jpg',0,3),
                (28, 'Oreo Milkshake','Cookies & cream',5.25,'https://res.cloudinary.com/thanhnguyencloud/image/upload/v1744281527/WP_CoffeePOS/images_7_qgegud.jpg',1,3),
                (29, 'Caramel Milkshake','Sweet caramel flavor',5.5,'https://res.cloudinary.com/thanhnguyencloud/image/upload/v1744281597/WP_CoffeePOS/2K3A4574-720x720_pnuxdi.jpg',0,3),
                (30, 'Chocolate Cake','Rich chocolate layers',4.0,'https://res.cloudinary.com/thanhnguyencloud/image/upload/v1744281652/WP_CoffeePOS/images_8_iy7kex.jpg',1,7),
                (31, 'Cheesecake','Creamy and sweet',3.75,'https://res.cloudinary.com/thanhnguyencloud/image/upload/v1744281705/WP_CoffeePOS/IMG_7944-vanilla-cheesecake-feature_ispzch.jpg',1,7),
                (32, 'Brownie','Dense & fudgy',4.25,'https://res.cloudinary.com/thanhnguyencloud/image/upload/v1744281749/WP_CoffeePOS/unnamed_sqkmzm.jpg',1,7),
                (33, 'Apple Pie','Classic American dessert',4.5,'https://res.cloudinary.com/thanhnguyencloud/image/upload/v1744281788/WP_CoffeePOS/images_9_p3r1g8.jpg',1,7),
                (34, 'Tiramisu','Coffee-flavored dessert',4.75,'https://res.cloudinary.com/thanhnguyencloud/image/upload/v1744281832/WP_CoffeePOS/Template-Size-for-Blog-7-500x500_mnv8e2.jpg',1,7),
                (35, 'Fruit Tart','Fresh fruit & cream',5.0,'https://res.cloudinary.com/thanhnguyencloud/image/upload/v1744625350/WP_CoffeePOS/mini-fruit-tarts-featured_gbp6ia.jpg',1,7),
                (36, 'Pineapple Juice','Tropical pineapple flavor',3.0,'https://res.cloudinary.com/thanhnguyencloud/image/upload/v1744625413/WP_CoffeePOS/highly-nutrition-enriched-sweet-100-fresh-yellow-pineapple-juice--265_eib8ta.jpg',0,4),
                (37, 'Berry Smoothie','Mixed berry smoothie',4.0,'https://res.cloudinary.com/thanhnguyencloud/image/upload/v1744625472/WP_CoffeePOS/images_10_mzuojq.jpg',1,3),
                (38, 'Peanut Butter Smoothie','Nutty and creamy',4.5,'https://res.cloudinary.com/thanhnguyencloud/image/upload/v1744625574/WP_CoffeePOS/images_11_revjss.jpg',0,3),
                (39, 'Carrot Juice','Fresh carrot juice',3.5,'https://res.cloudinary.com/thanhnguyencloud/image/upload/v1744625606/WP_CoffeePOS/carrot-juice-recipe_fciexl.jpg',1,4),
                (40, 'Mint Tea','Refreshing mint flavor',2.75,'https://res.cloudinary.com/thanhnguyencloud/image/upload/v1744625628/WP_CoffeePOS/Mint-Tea-SQUARE_awkr06.jpg',1,2);",
            // Customers
            "INSERT OR IGNORE INTO Customers (Id, Name, Phone, IsMembership, Points) VALUES (1, 'Alice', '0123456789', 1, 100)",
            "INSERT OR IGNORE INTO Customers (Id, Name, Phone, IsMembership, Points) VALUES (2, 'Bob', '222-333-4444', 0, 50)",
            // Ingredients
            "INSERT OR IGNORE INTO Ingredients (Id, Name, Unit, Quantity, Threshold) VALUES (1, 'Espresso Coffee', 'bag', 50, 10)",
            "INSERT OR IGNORE INTO Ingredients (Id, Name, Unit, Quantity, Threshold) VALUES (2, 'Milk', 'cup', 60, 15)",
            "INSERT OR IGNORE INTO Ingredients (Id, Name, Unit, Quantity, Threshold) VALUES (3, 'Sugar', 'tsp', 100, 20)",
            "INSERT OR IGNORE INTO Ingredients (Id, Name, Unit, Quantity, Threshold) VALUES (4, 'Ground Coffee', 'bag', 40, 10)",
            "INSERT OR IGNORE INTO Ingredients (Id, Name, Unit, Quantity, Threshold) VALUES (5, 'Steamed Milk', 'liter', 30, 10)",
            "INSERT OR IGNORE INTO Ingredients (Id, Name, Unit, Quantity, Threshold) VALUES (6, 'Milk Foam', 'liter', 20, 5)",
            // ProductIngredients
            "INSERT OR IGNORE INTO ProductIngredients (Id, ProductId, IngredientId, QuantityUsed) VALUES (1, 1, 1, 1)",
            "INSERT OR IGNORE INTO ProductIngredients (Id, ProductId, IngredientId, QuantityUsed) VALUES (2, 2, 1, 1)",
            "INSERT OR IGNORE INTO ProductIngredients (Id, ProductId, IngredientId, QuantityUsed) VALUES (3, 2, 2, 1)",
            // IngredientInventoryTransactions
            "INSERT OR IGNORE INTO IngredientInventoryTransactions (Id, IngredientId, Timestamp, Quantity, Unit, TransactionType, UnitPrice) VALUES (1, 1, 1672531199000, 50, 'bag', 'IMPORT', 10.00)",
            "INSERT OR IGNORE INTO IngredientInventoryTransactions (Id, IngredientId, Timestamp, Quantity, Unit, TransactionType, UnitPrice) VALUES (2, 2, 1672531199000, -10, 'cup', 'EXPORT', 0.50)",
            "INSERT OR IGNORE INTO IngredientInventoryTransactions (Id, IngredientId, Timestamp, Quantity, Unit, TransactionType, UnitPrice) VALUES (3, 3, 1672531199000, 100, 'tsp', 'IMPORT', 0.05)",
            "INSERT OR IGNORE INTO IngredientInventoryTransactions (Id, IngredientId, Timestamp, Quantity, Unit, TransactionType, UnitPrice) VALUES (4, 4, 1672531199000, -20, 'bag', 'EXPORT', 5.00)",
            "INSERT OR IGNORE INTO IngredientInventoryTransactions (Id, IngredientId, Timestamp, Quantity, Unit, TransactionType, UnitPrice) VALUES (5, 5, 1672531199000, 30, 'liter', 'IMPORT', 2.00)",
            "INSERT OR IGNORE INTO IngredientInventoryTransactions (Id, IngredientId, Timestamp, Quantity, Unit, TransactionType, UnitPrice) VALUES (6, 6, 1672531199000, -5, 'liter', 'EXPORT', 1.00)",
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
            //"INSERT OR IGNORE INTO Orders (Id, CustomerId, OrderDate, PaymentDate, Status, TableId, VoucherId, TotalAmount, PaymentMethodId, ServiceTypeId) VALUES (1, 1, '2025-03-20 10:00:00', '2025-03-20 10:05:00', 'Completed', 1, 1, 15.00, 1, 1)",
            //"INSERT OR IGNORE INTO Orders (Id, CustomerId, OrderDate, PaymentDate, Status, TableId, VoucherId, TotalAmount, PaymentMethodId, ServiceTypeId) VALUES (2, 2, '2025-03-21 12:30:00', NULL, 'Pending', NULL, NULL, 8.50, 2, 2)",
            // OrderDetails
            //"INSERT OR IGNORE INTO OrderDetails (Id, OrderId, ProductId, Quantity, Price) VALUES (1, 1, 1, 2, 2.50)",
            //"INSERT OR IGNORE INTO OrderDetails (Id, OrderId, ProductId, Quantity, Price) VALUES (2, 1, 11, 1, 3.00)"
        };



        foreach (var commandText in commands)
        {
            using var command = connection.CreateCommand();
            command.CommandText = commandText;
            await command.ExecuteNonQueryAsync();
        }
    }
}