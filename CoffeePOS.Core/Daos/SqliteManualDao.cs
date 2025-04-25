using CoffeePOS.Core.Data;
using CoffeePOS.Core.Interfaces;
using CoffeePOS.Core.Models;
using CoffeePOS.Core.Repositories;
using Microsoft.Data.Sqlite;
using System;
using System.IO;
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

        // Check if the Users table already exists
        bool needToSeedData = false;
        using (var command = connection.CreateCommand())
        {
            command.CommandText = "SELECT name FROM sqlite_master WHERE type='table' AND name='Users'";
            var result = await command.ExecuteScalarAsync();
            needToSeedData = result == null; // If Users table doesn't exist, we need to seed data
        }

        var tableCreationCommands = new[]
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
                Name TEXT NOT NULL,
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
            
            // Ingredients Table - Moved up since it's referenced by ProductIngredients
            @"CREATE TABLE IF NOT EXISTS Ingredients (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Name TEXT NOT NULL,
                Unit TEXT NOT NULL,
                Quantity INTEGER NOT NULL DEFAULT 0,
                Threshold INTEGER NOT NULL DEFAULT 0
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
            
            // IngredientInventoryTransactions Table
            @"CREATE TABLE IF NOT EXISTS IngredientInventoryTransactions (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                IngredientId INTEGER NOT NULL,
                Timestamp INTEGER NOT NULL,
                Quantity INTEGER NOT NULL,
                Unit TEXT NOT NULL,
                TransactionType TEXT NOT NULL,
                UnitPrice REAL NOT NULL DEFAULT 0,
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
            
            // Vouchers Table
            @"CREATE TABLE IF NOT EXISTS Vouchers (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Code TEXT NOT NULL,
                DiscountPercentage INTEGER
            )",
            
            // PaymentMethods Table
            @"CREATE TABLE IF NOT EXISTS PaymentMethods (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Name TEXT NOT NULL,
                Description TEXT
            )",
            
            // Reservations Table - Moved after Tables and Customers tables
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
            
            // Orders Table - Moved after all referenced tables are created
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
            
            // OrderDetails Table - Placed after Orders and Products tables
            @"CREATE TABLE IF NOT EXISTS OrderDetails (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                OrderId INTEGER,
                ProductId INTEGER,
                Quantity INTEGER,
                Price REAL,
                FOREIGN KEY (OrderId) REFERENCES Orders(Id),
                FOREIGN KEY (ProductId) REFERENCES Products(Id)
            )"
        };

        // Create tables
        foreach (var commandText in tableCreationCommands)
        {
            using var command = connection.CreateCommand();
            command.CommandText = commandText;
            await command.ExecuteNonQueryAsync();
        }


        // Only seed data if necessary
        if (needToSeedData)
        {
            // Double-check to make sure Users doesn't already have data
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "SELECT COUNT(*) FROM Users";
                var count = Convert.ToInt32(await command.ExecuteScalarAsync());
                if (count > 0)
                {
                    // If data already exists, no need to seed
                    needToSeedData = false;
                }
            }

            if (needToSeedData)
            {
                Console.WriteLine("Seeding initial data...");
                var seedCommands = new[]
                {

        // Insert initial data
        // Users
            $"INSERT OR IGNORE INTO Users (Id, Username, Password, ExpireAt) VALUES (1, 'admin', '{BCrypt.Net.BCrypt.HashPassword("admin1234")}', '{DateTime.UtcNow.AddDays(5).ToString("O")}')",
            $"INSERT OR IGNORE INTO Users (Id, Username, Password) VALUES (2, 'employee1', '{BCrypt.Net.BCrypt.HashPassword("employee1234")}')",
            $"INSERT OR IGNORE INTO Users (Id, Username, Password) VALUES (3, 'employee2', '{BCrypt.Net.BCrypt.HashPassword("employee1234")}')",
            
            // Employees
            "INSERT OR IGNORE INTO Employees (Id, Name, Email, Phone, Salary) VALUES (1, 'John Doe', 'john@example.com', '123-456-7890', 2000)",
            "INSERT OR IGNORE INTO Employees (Id, Name, Email, Phone, Salary) VALUES (2, 'Jane Smith', 'jane@example.com', '234-567-8901', 2200)",
            
            // Categories
            "INSERT OR IGNORE INTO Categories (Id, Name, Description) VALUES (1, 'Coffee', 'Hot and cold coffee')",
            "INSERT OR IGNORE INTO Categories (Id, Name, Description) VALUES (2, 'Tea', 'Various types of tea')",
            "INSERT OR IGNORE INTO Categories (Id, Name, Description) VALUES (3, 'Milkshakes', 'Flavored milk beverages')",
            "INSERT OR IGNORE INTO Categories (Id, Name, Description) VALUES (4, 'Juices', 'Fresh fruit juices')",
            "INSERT OR IGNORE INTO Categories (Id, Name, Description) VALUES (5, 'Pastries', 'Baked goods and snacks')",
            "INSERT OR IGNORE INTO Categories (Id, Name, Description) VALUES (6, 'Sandwiches', 'Savory sandwiches')",
            "INSERT OR IGNORE INTO Categories (Id, Name, Description) VALUES (7, 'Desserts', 'Sweet treats')",
            
            // Ingredients - Added before product ingredients
            @"INSERT OR IGNORE INTO Ingredients (Id, Name, Unit, Quantity, Threshold) VALUES
                (1, 'Espresso Coffee Beans', 'kg', 20, 5),
                (2, 'Ground Coffee', 'kg', 15, 4),
                (3, 'Whole Milk', 'liter', 40, 10),
                (4, 'Skim Milk', 'liter', 15, 5),
                (5, 'Almond Milk', 'liter', 10, 3),
                (6, 'Soy Milk', 'liter', 10, 3),
                (7, 'Chocolate Syrup', 'bottle', 8, 2),
                (8, 'Caramel Syrup', 'bottle', 6, 2),
                (9, 'Vanilla Syrup', 'bottle', 7, 2),
                (10, 'Hazelnut Syrup', 'bottle', 5, 2),
                (11, 'Whipped Cream', 'canister', 12, 3),
                (12, 'Chocolate Powder', 'kg', 5, 1),
                (13, 'Cinnamon Powder', 'kg', 2, 0.5),
                (14, 'Chai Tea Bags', 'box', 8, 2),
                (15, 'Green Tea Bags', 'box', 6, 2),
                (16, 'Black Tea Bags', 'box', 10, 3),
                (17, 'Chamomile Tea Bags', 'box', 8, 2),
                (18, 'Mint Tea Bags', 'box', 7, 2),
                (19, 'Oolong Tea Bags', 'box', 5, 1),
                (20, 'Herbal Tea Bags', 'box', 6, 2),
                (21, 'Orange Juice', 'liter', 25, 8),
                (22, 'Apple Juice', 'liter', 20, 8),
                (23, 'Pineapple Juice', 'liter', 15, 5),
                (24, 'Mango Puree', 'kg', 8, 2),
                (25, 'Lemon Juice', 'liter', 10, 3),
                (26, 'Sugar', 'kg', 30, 10),
                (27, 'Flour', 'kg', 25, 8),
                (28, 'Butter', 'kg', 15, 5),
                (29, 'Eggs', 'dozen', 10, 3),
                (30, 'Cocoa Powder', 'kg', 5, 1),
                (31, 'Baking Powder', 'kg', 2, 0.5),
                (32, 'Vanilla Extract', 'bottle', 4, 1),
                (33, 'Blueberries', 'kg', 8, 2),
                (34, 'Strawberries', 'kg', 10, 3),
                (35, 'Bananas', 'kg', 12, 4),
                (36, 'Honey', 'bottle', 6, 2),
                (37, 'Condensed Milk', 'can', 15, 5),
                (38, 'Ice Cream (Vanilla)', 'liter', 10, 3),
                (39, 'Ice Cubes', 'kg', 20, 5),
                (40, 'Chocolate Chips', 'kg', 5, 1),
                (41, 'Oreo Cookies', 'package', 8, 2),
                (42, 'Cream Cheese', 'kg', 6, 2),
                (43, 'Bread', 'loaf', 10, 3),
                (44, 'Sliced Turkey', 'kg', 5, 2),
                (45, 'Sliced Ham', 'kg', 5, 2),
                (46, 'Bacon', 'kg', 8, 3),
                (47, 'Lettuce', 'head', 6, 2),
                (48, 'Tomatoes', 'kg', 8, 3),
                (49, 'Cheese (Cheddar)', 'kg', 7, 2),
                (50, 'Tuna', 'can', 12, 4)",
            
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
                (40, 'Mint Tea','Refreshing mint flavor',2.75,'https://res.cloudinary.com/thanhnguyencloud/image/upload/v1744625628/WP_CoffeePOS/Mint-Tea-SQUARE_awkr06.jpg',1,2)",
            
            // Customers
            "INSERT OR IGNORE INTO Customers (Id, Name, Phone, IsMembership, Points) VALUES (1, 'Alice', '0123456789', 1, 100)",
            "INSERT OR IGNORE INTO Customers (Id, Name, Phone, IsMembership, Points) VALUES (2, 'Bob', '222-333-4444', 0, 50)",
            
            // ServiceTypes
            "INSERT OR IGNORE INTO ServiceTypes (Id, Name) VALUES (1, 'Dine-in')",
            "INSERT OR IGNORE INTO ServiceTypes (Id, Name) VALUES (2, 'Take-away')",
            
            // Tables
            "INSERT OR IGNORE INTO Tables (Id, TableNumber, Status) VALUES (1, 'T01', 'Available')",
            "INSERT OR IGNORE INTO Tables (Id, TableNumber, Status) VALUES (2, 'T02', 'Occupied')",
            
            // Vouchers
            "INSERT OR IGNORE INTO Vouchers (Id, Code, DiscountPercentage) VALUES (1, 'WELCOME10', 10)",
            "INSERT OR IGNORE INTO Vouchers (Id, Code, DiscountPercentage) VALUES (2, 'COFFEE20', 20)",
            "INSERT OR IGNORE INTO Vouchers (Id, Code, DiscountPercentage) VALUES (3, 'PASTRY15', 15)",
            "INSERT OR IGNORE INTO Vouchers (Id, Code, DiscountPercentage) VALUES (4, 'LOYALTY25', 25)",
            "INSERT OR IGNORE INTO Vouchers (Id, Code, DiscountPercentage) VALUES (5, 'SPRING10', 10)",
            "INSERT OR IGNORE INTO Vouchers (Id, Code, DiscountPercentage) VALUES (6, 'SUMMER20', 20)",
            "INSERT OR IGNORE INTO Vouchers (Id, Code, DiscountPercentage) VALUES (7, 'TEA15', 15)",
            "INSERT OR IGNORE INTO Vouchers (Id, Code, DiscountPercentage) VALUES (8, 'FIRSTORDER30', 30)",
            "INSERT OR IGNORE INTO Vouchers (Id, Code, DiscountPercentage) VALUES (9, 'WEEKEND10', 10)",

            
            // PaymentMethods
            "INSERT OR IGNORE INTO PaymentMethods (Id, Name, Description) VALUES (1, 'Cash', 'Payment in cash')",
            "INSERT OR IGNORE INTO PaymentMethods (Id, Name, Description) VALUES (2, 'Card', 'Payment via credit or debit card')",
            
            // Reservations - After Tables and Customers are inserted
            "INSERT OR IGNORE INTO Reservations (Id, CustomerId, TableId, ReservationDate, StartTime, EndTime, Status) VALUES (1, 1, 1, '2025-03-25', '18:00', '20:00', 'Confirmed')",
            
            // Orders - Create some sample orders
            //"INSERT OR IGNORE INTO Orders (Id, CustomerId, OrderDate, PaymentDate, Status, TableId, VoucherId, TotalAmount, PaymentMethodId, ServiceTypeId) VALUES (1, 1, '2025-03-20 10:00:00', '2025-03-20 10:05:00', 'Completed', 1, 1, 15.00, 1, 1)",
            //"INSERT OR IGNORE INTO Orders (Id, CustomerId, OrderDate, PaymentDate, Status, TableId, VoucherId, TotalAmount, PaymentMethodId, ServiceTypeId) VALUES (2, 2, '2025-03-21 12:30:00', NULL, 'Pending', NULL, NULL, 8.50, 2, 2)",
            
            // OrderDetails
            //"INSERT OR IGNORE INTO OrderDetails (Id, OrderId, ProductId, Quantity, Price) VALUES (1, 1, 1, 2, 2.50)",
            //"INSERT OR IGNORE INTO OrderDetails (Id, OrderId, ProductId, Quantity, Price) VALUES (2, 1, 11, 1, 3.00)",
            
            // Initial inventory transactions - After ingredients are inserted
            @"INSERT OR IGNORE INTO IngredientInventoryTransactions (IngredientId, Timestamp, Quantity, Unit, TransactionType, UnitPrice) 
                SELECT Id, strftime('%s','now')*1000, Quantity, Unit, 'IMPORT', 
                    CASE
                        WHEN Unit = 'kg' THEN 15.0
                        WHEN Unit = 'liter' THEN 3.5
                        WHEN Unit = 'bottle' THEN 8.0
                        WHEN Unit = 'box' THEN 12.0
                        WHEN Unit = 'canister' THEN 10.0
                        WHEN Unit = 'dozen' THEN 4.5
                        WHEN Unit = 'can' THEN 2.0
                        WHEN Unit = 'package' THEN 5.0
                        WHEN Unit = 'loaf' THEN 3.0
                        WHEN Unit = 'head' THEN 2.0
                        ELSE 5.0
                    END
                FROM Ingredients WHERE NOT EXISTS (SELECT 1 FROM IngredientInventoryTransactions WHERE IngredientInventoryTransactions.IngredientId = Ingredients.Id)",
            
            // Product Ingredients - After Products and Ingredients are inserted
            // Espresso (ID: 1)
            "INSERT OR IGNORE INTO ProductIngredients (ProductId, IngredientId, QuantityUsed) VALUES (1, 1, 0.02)", // 20g Espresso Coffee Beans
            "INSERT OR IGNORE INTO ProductIngredients (ProductId, IngredientId, QuantityUsed) VALUES (1, 39, 0.05)", // 50g Ice (optional for iced)

            // Cappuccino (ID: 2)
            "INSERT OR IGNORE INTO ProductIngredients (ProductId, IngredientId, QuantityUsed) VALUES (2, 1, 0.02)", // 20g Espresso Coffee Beans
            "INSERT OR IGNORE INTO ProductIngredients (ProductId, IngredientId, QuantityUsed) VALUES (2, 3, 0.12)", // 120ml Whole Milk
            "INSERT OR IGNORE INTO ProductIngredients (ProductId, IngredientId, QuantityUsed) VALUES (2, 11, 0.02)", // 20g Whipped Cream

            // Latte (ID: 3)
            "INSERT OR IGNORE INTO ProductIngredients (ProductId, IngredientId, QuantityUsed) VALUES (3, 1, 0.02)", // 20g Espresso Coffee Beans
            "INSERT OR IGNORE INTO ProductIngredients (ProductId, IngredientId, QuantityUsed) VALUES (3, 3, 0.20)", // 200ml Whole Milk
            "INSERT OR IGNORE INTO ProductIngredients (ProductId, IngredientId, QuantityUsed) VALUES (3, 9, 0.01)", // 10ml Vanilla Syrup (optional)

            // Americano (ID: 11)
            "INSERT OR IGNORE INTO ProductIngredients (ProductId, IngredientId, QuantityUsed) VALUES (11, 1, 0.02)", // 20g Espresso Coffee Beans

            // Mocha (ID: 12)
            "INSERT OR IGNORE INTO ProductIngredients (ProductId, IngredientId, QuantityUsed) VALUES (12, 1, 0.02)", // 20g Espresso Coffee Beans
            "INSERT OR IGNORE INTO ProductIngredients (ProductId, IngredientId, QuantityUsed) VALUES (12, 3, 0.15)", // 150ml Whole Milk
            "INSERT OR IGNORE INTO ProductIngredients (ProductId, IngredientId, QuantityUsed) VALUES (12, 7, 0.02)", // 20ml Chocolate Syrup
            "INSERT OR IGNORE INTO ProductIngredients (ProductId, IngredientId, QuantityUsed) VALUES (12, 11, 0.03)", // 30g Whipped Cream

            // Product Ingredients for Tea Products
            // Chamomile Tea (ID: 4)
            "INSERT OR IGNORE INTO ProductIngredients (ProductId, IngredientId, QuantityUsed) VALUES (4, 17, 0.01)", // 1 Chamomile Tea Bag
            "INSERT OR IGNORE INTO ProductIngredients (ProductId, IngredientId, QuantityUsed) VALUES (4, 36, 0.01)", // 10ml Honey (optional)

            // Peach Tea (ID: 6)
            "INSERT OR IGNORE INTO ProductIngredients (ProductId, IngredientId, QuantityUsed) VALUES (6, 16, 0.01)", // 1 Black Tea Bag
            "INSERT OR IGNORE INTO ProductIngredients (ProductId, IngredientId, QuantityUsed) VALUES (6, 26, 0.01)", // 10g Sugar

            // Oolong Tea (ID: 13)
            "INSERT OR IGNORE INTO ProductIngredients (ProductId, IngredientId, QuantityUsed) VALUES (13, 19, 0.01)", // 1 Oolong Tea Bag
            "INSERT OR IGNORE INTO ProductIngredients (ProductId, IngredientId, QuantityUsed) VALUES (13, 26, 0.01)", // 10g Sugar (optional)

            // Herbal Tea (ID: 14)
            "INSERT OR IGNORE INTO ProductIngredients (ProductId, IngredientId, QuantityUsed) VALUES (14, 20, 0.01)", // 1 Herbal Tea Bag
            "INSERT OR IGNORE INTO ProductIngredients (ProductId, IngredientId, QuantityUsed) VALUES (14, 36, 0.01)", // 10ml Honey (optional)

            // Chai Tea (ID: 15)
            "INSERT OR IGNORE INTO ProductIngredients (ProductId, IngredientId, QuantityUsed) VALUES (15, 14, 0.01)", // 1 Chai Tea Bag
            "INSERT OR IGNORE INTO ProductIngredients (ProductId, IngredientId, QuantityUsed) VALUES (15, 3, 0.15)", // 150ml Whole Milk
            "INSERT OR IGNORE INTO ProductIngredients (ProductId, IngredientId, QuantityUsed) VALUES (15, 13, 0.002)", // 2g Cinnamon Powder

            // Mint Tea (ID: 40)
            "INSERT OR IGNORE INTO ProductIngredients (ProductId, IngredientId, QuantityUsed) VALUES (40, 18, 0.01)", // 1 Mint Tea Bag
            "INSERT OR IGNORE INTO ProductIngredients (ProductId, IngredientId, QuantityUsed) VALUES (40, 26, 0.01)", // 10g Sugar (optional)

            // Product Ingredients for Juices
            // Orange Juice (ID: 7)
            "INSERT OR IGNORE INTO ProductIngredients (ProductId, IngredientId, QuantityUsed) VALUES (7, 21, 0.25)", // 250ml Orange Juice
            "INSERT OR IGNORE INTO ProductIngredients (ProductId, IngredientId, QuantityUsed) VALUES (7, 39, 0.05)", // 50g Ice (optional)

            // Apple Juice (ID: 20)
            "INSERT OR IGNORE INTO ProductIngredients (ProductId, IngredientId, QuantityUsed) VALUES (20, 22, 0.25)", // 250ml Apple Juice

            // Lemonade (ID: 21)
            "INSERT OR IGNORE INTO ProductIngredients (ProductId, IngredientId, QuantityUsed) VALUES (21, 25, 0.05)", // 50ml Lemon Juice
            "INSERT OR IGNORE INTO ProductIngredients (ProductId, IngredientId, QuantityUsed) VALUES (21, 26, 0.02)", // 20g Sugar
            "INSERT OR IGNORE INTO ProductIngredients (ProductId, IngredientId, QuantityUsed) VALUES (21, 39, 0.10)", // 100g Ice

            // Mango Juice (ID: 22)
            "INSERT OR IGNORE INTO ProductIngredients (ProductId, IngredientId, QuantityUsed) VALUES (22, 24, 0.10)", // 100g Mango Puree
            "INSERT OR IGNORE INTO ProductIngredients (ProductId, IngredientId, QuantityUsed) VALUES (22, 26, 0.01)", // 10g Sugar
            "INSERT OR IGNORE INTO ProductIngredients (ProductId, IngredientId, QuantityUsed) VALUES (22, 39, 0.05)", // 50g Ice

            // Grape Juice (ID: 23)
            "INSERT OR IGNORE INTO ProductIngredients (ProductId, IngredientId, QuantityUsed) VALUES (23, 26, 0.01)", // 10g Sugar
            "INSERT OR IGNORE INTO ProductIngredients (ProductId, IngredientId, QuantityUsed) VALUES (23, 39, 0.05)", // 50g Ice

            // Pineapple Juice (ID: 36)
            "INSERT OR IGNORE INTO ProductIngredients (ProductId, IngredientId, QuantityUsed) VALUES (36, 23, 0.25)", // 250ml Pineapple Juice
            "INSERT OR IGNORE INTO ProductIngredients (ProductId, IngredientId, QuantityUsed) VALUES (36, 39, 0.05)", // 50g Ice

            // Carrot Juice (ID: 39)
            "INSERT OR IGNORE INTO ProductIngredients (ProductId, IngredientId, QuantityUsed) VALUES (39, 26, 0.01)", // 10g Sugar (optional)
            "INSERT OR IGNORE INTO ProductIngredients (ProductId, IngredientId, QuantityUsed) VALUES (39, 39, 0.05)", // 50g Ice

            // Product Ingredients for Milkshakes and Smoothies
            // Vanilla Milkshake (ID: 25)
            "INSERT OR IGNORE INTO ProductIngredients (ProductId, IngredientId, QuantityUsed) VALUES (25, 3, 0.15)", // 150ml Whole Milk
            "INSERT OR IGNORE INTO ProductIngredients (ProductId, IngredientId, QuantityUsed) VALUES (25, 38, 0.10)", // 100ml Vanilla Ice Cream
            "INSERT OR IGNORE INTO ProductIngredients (ProductId, IngredientId, QuantityUsed) VALUES (25, 9, 0.02)", // 20ml Vanilla Syrup
            "INSERT OR IGNORE INTO ProductIngredients (ProductId, IngredientId, QuantityUsed) VALUES (25, 39, 0.05)", // 50g Ice

            // Chocolate Milkshake (ID: 26)
            "INSERT OR IGNORE INTO ProductIngredients (ProductId, IngredientId, QuantityUsed) VALUES (26, 3, 0.15)", // 150ml Whole Milk
            "INSERT OR IGNORE INTO ProductIngredients (ProductId, IngredientId, QuantityUsed) VALUES (26, 38, 0.10)", // 100ml Vanilla Ice Cream
            "INSERT OR IGNORE INTO ProductIngredients (ProductId, IngredientId, QuantityUsed) VALUES (26, 7, 0.03)", // 30ml Chocolate Syrup
            "INSERT OR IGNORE INTO ProductIngredients (ProductId, IngredientId, QuantityUsed) VALUES (26, 39, 0.05)", // 50g Ice

            // Strawberry Milkshake (ID: 27)
            "INSERT OR IGNORE INTO ProductIngredients (ProductId, IngredientId, QuantityUsed) VALUES (27, 3, 0.15)", // 150ml Whole Milk
            "INSERT OR IGNORE INTO ProductIngredients (ProductId, IngredientId, QuantityUsed) VALUES (27, 38, 0.10)", // 100ml Vanilla Ice Cream
            "INSERT OR IGNORE INTO ProductIngredients (ProductId, IngredientId, QuantityUsed) VALUES (27, 34, 0.05)", // 50g Strawberries
            "INSERT OR IGNORE INTO ProductIngredients (ProductId, IngredientId, QuantityUsed) VALUES (27, 39, 0.05)", // 50g Ice

            // Oreo Milkshake (ID: 28)
            "INSERT OR IGNORE INTO ProductIngredients (ProductId, IngredientId, QuantityUsed) VALUES (28, 3, 0.15)", // 150ml Whole Milk
            "INSERT OR IGNORE INTO ProductIngredients (ProductId, IngredientId, QuantityUsed) VALUES (28, 38, 0.10)", // 100ml Vanilla Ice Cream
            "INSERT OR IGNORE INTO ProductIngredients (ProductId, IngredientId, QuantityUsed) VALUES (28, 41, 0.05)", // 50g Oreo Cookies
            "INSERT OR IGNORE INTO ProductIngredients (ProductId, IngredientId, QuantityUsed) VALUES (28, 39, 0.05)", // 50g Ice

            // Caramel Milkshake (ID: 29)
            "INSERT OR IGNORE INTO ProductIngredients (ProductId, IngredientId, QuantityUsed) VALUES (29, 3, 0.15)", // 150ml Whole Milk
            "INSERT OR IGNORE INTO ProductIngredients (ProductId, IngredientId, QuantityUsed) VALUES (29, 38, 0.10)", // 100ml Vanilla Ice Cream
            "INSERT OR IGNORE INTO ProductIngredients (ProductId, IngredientId, QuantityUsed) VALUES (29, 8, 0.03)", // 30ml Caramel Syrup
            "INSERT OR IGNORE INTO ProductIngredients (ProductId, IngredientId, QuantityUsed) VALUES (29, 39, 0.05)", // 50g Ice

            // Berry Smoothie (ID: 37)
            "INSERT OR IGNORE INTO ProductIngredients (ProductId, IngredientId, QuantityUsed) VALUES (37, 34, 0.08)", // 80g Strawberries
            "INSERT OR IGNORE INTO ProductIngredients (ProductId, IngredientId, QuantityUsed) VALUES (37, 33, 0.08)", // 80g Blueberries
            "INSERT OR IGNORE INTO ProductIngredients (ProductId, IngredientId, QuantityUsed) VALUES (37, 3, 0.10)", // 100ml Whole Milk
            "INSERT OR IGNORE INTO ProductIngredients (ProductId, IngredientId, QuantityUsed) VALUES (37, 36, 0.01)", // 10ml Honey
            "INSERT OR IGNORE INTO ProductIngredients (ProductId, IngredientId, QuantityUsed) VALUES (37, 39, 0.10)", // 100g Ice

            // Peanut Butter Smoothie (ID: 38)
            "INSERT OR IGNORE INTO ProductIngredients (ProductId, IngredientId, QuantityUsed) VALUES (38, 35, 0.10)", // 100g Bananas
            "INSERT OR IGNORE INTO ProductIngredients (ProductId, IngredientId, QuantityUsed) VALUES (38, 3, 0.15)", // 150ml Whole Milk
            "INSERT OR IGNORE INTO ProductIngredients (ProductId, IngredientId, QuantityUsed) VALUES (38, 36, 0.01)", // 10ml Honey
            "INSERT OR IGNORE INTO ProductIngredients (ProductId, IngredientId, QuantityUsed) VALUES (38, 39, 0.10)", // 100g Ice

            // Product Ingredients for Pastries
            // Blueberry Muffin (ID: 5)
            "INSERT OR IGNORE INTO ProductIngredients (ProductId, IngredientId, QuantityUsed) VALUES (5, 27, 0.05)", // 50g Flour
            "INSERT OR IGNORE INTO ProductIngredients (ProductId, IngredientId, QuantityUsed) VALUES (5, 26, 0.03)", // 30g Sugar
            "INSERT OR IGNORE INTO ProductIngredients (ProductId, IngredientId, QuantityUsed) VALUES (5, 28, 0.02)", // 20g Butter
            "INSERT OR IGNORE INTO ProductIngredients (ProductId, IngredientId, QuantityUsed) VALUES (5, 29, 0.08)", // 1 Egg
            "INSERT OR IGNORE INTO ProductIngredients (ProductId, IngredientId, QuantityUsed) VALUES (5, 33, 0.03)", // 30g Blueberries

            // Croissant (ID: 16)
            "INSERT OR IGNORE INTO ProductIngredients (ProductId, IngredientId, QuantityUsed) VALUES (16, 27, 0.06)", // 60g Flour
            "INSERT OR IGNORE INTO ProductIngredients (ProductId, IngredientId, QuantityUsed) VALUES (16, 28, 0.03)", // 30g Butter

            // Danish Pastry (ID: 17)
            "INSERT OR IGNORE INTO ProductIngredients (ProductId, IngredientId, QuantityUsed) VALUES (17, 27, 0.05)", // 50g Flour
            "INSERT OR IGNORE INTO ProductIngredients (ProductId, IngredientId, QuantityUsed) VALUES (17, 28, 0.03)", // 30g Butter
            "INSERT OR IGNORE INTO ProductIngredients (ProductId, IngredientId, QuantityUsed) VALUES (17, 26, 0.02)", // 20g Sugar
            "INSERT OR IGNORE INTO ProductIngredients (ProductId, IngredientId, QuantityUsed) VALUES (17, 34, 0.02)", // 20g Strawberries or other fruits

            // Chocolate Donut (ID: 18)
            "INSERT OR IGNORE INTO ProductIngredients (ProductId, IngredientId, QuantityUsed) VALUES (18, 27, 0.05)", // 50g Flour
            "INSERT OR IGNORE INTO ProductIngredients (ProductId, IngredientId, QuantityUsed) VALUES (18, 26, 0.02)", // 20g Sugar
            "INSERT OR IGNORE INTO ProductIngredients (ProductId, IngredientId, QuantityUsed) VALUES (18, 30, 0.01)", // 10g Cocoa Powder
            "INSERT OR IGNORE INTO ProductIngredients (ProductId, IngredientId, QuantityUsed) VALUES (18, 7, 0.01)", // 10ml Chocolate Syrup for glaze

            // Apple Turnover (ID: 19)
            "INSERT OR IGNORE INTO ProductIngredients (ProductId, IngredientId, QuantityUsed) VALUES (19, 27, 0.06)", // 60g Flour
            "INSERT OR IGNORE INTO ProductIngredients (ProductId, IngredientId, QuantityUsed) VALUES (19, 28, 0.03)", // 30g Butter
            "INSERT OR IGNORE INTO ProductIngredients (ProductId, IngredientId, QuantityUsed) VALUES (19, 22, 0.05)", // 50ml Apple Juice (concentrated)

            // Product Ingredients for Sandwiches
            // Club Sandwich (ID: 8)
            "INSERT OR IGNORE INTO ProductIngredients (ProductId, IngredientId, QuantityUsed) VALUES (8, 43, 0.15)", // 150g Bread (3 slices)
            "INSERT OR IGNORE INTO ProductIngredients (ProductId, IngredientId, QuantityUsed) VALUES (8, 44, 0.05)", // 50g Sliced Turkey
            "INSERT OR IGNORE INTO ProductIngredients (ProductId, IngredientId, QuantityUsed) VALUES (8, 45, 0.05)", // 50g Sliced Ham
            "INSERT OR IGNORE INTO ProductIngredients (ProductId, IngredientId, QuantityUsed) VALUES (8, 46, 0.03)", // 30g Bacon
            "INSERT OR IGNORE INTO ProductIngredients (ProductId, IngredientId, QuantityUsed) VALUES (8, 47, 0.02)", // 20g Lettuce
            "INSERT OR IGNORE INTO ProductIngredients (ProductId, IngredientId, QuantityUsed) VALUES (8, 48, 0.03)", // 30g Tomatoes

            // BLT Sandwich (ID: 9)
            "INSERT OR IGNORE INTO ProductIngredients (ProductId, IngredientId, QuantityUsed) VALUES (9, 43, 0.10)", // 100g Bread (2 slices)
            "INSERT OR IGNORE INTO ProductIngredients (ProductId, IngredientId, QuantityUsed) VALUES (9, 46, 0.05)", // 50g Bacon
            "INSERT OR IGNORE INTO ProductIngredients (ProductId, IngredientId, QuantityUsed) VALUES (9, 47, 0.03)", // 30g Lettuce
            "INSERT OR IGNORE INTO ProductIngredients (ProductId, IngredientId, QuantityUsed) VALUES (9, 48, 0.04)", // 40g Tomatoes

            // Grilled Cheese (ID: 10)
            "INSERT OR IGNORE INTO ProductIngredients (ProductId, IngredientId, QuantityUsed) VALUES (10, 43, 0.10)", // 100g Bread (2 slices)
            "INSERT OR IGNORE INTO ProductIngredients (ProductId, IngredientId, QuantityUsed) VALUES (10, 49, 0.05)", // 50g Cheese (Cheddar)
            "INSERT OR IGNORE INTO ProductIngredients (ProductId, IngredientId, QuantityUsed) VALUES (10, 28, 0.02)", // 20g Butter for grilling

            // Tuna Sandwich (ID: 24)
            "INSERT OR IGNORE INTO ProductIngredients (ProductId, IngredientId, QuantityUsed) VALUES (24, 43, 0.10)", // 100g Bread (2 slices)
            "INSERT OR IGNORE INTO ProductIngredients (ProductId, IngredientId, QuantityUsed) VALUES (24, 50, 0.08)", // 80g Tuna
            "INSERT OR IGNORE INTO ProductIngredients (ProductId, IngredientId, QuantityUsed) VALUES (24, 47, 0.02)", // 20g Lettuce

            // Product Ingredients for Desserts
            // Chocolate Cake (ID: 30)
            "INSERT OR IGNORE INTO ProductIngredients (ProductId, IngredientId, QuantityUsed) VALUES (30, 27, 0.06)", // 60g Flour
            "INSERT OR IGNORE INTO ProductIngredients (ProductId, IngredientId, QuantityUsed) VALUES (30, 26, 0.04)", // 40g Sugar
            "INSERT OR IGNORE INTO ProductIngredients (ProductId, IngredientId, QuantityUsed) VALUES (30, 30, 0.02)", // 20g Cocoa Powder
            "INSERT OR IGNORE INTO ProductIngredients (ProductId, IngredientId, QuantityUsed) VALUES (30, 28, 0.03)", // 30g Butter
            "INSERT OR IGNORE INTO ProductIngredients (ProductId, IngredientId, QuantityUsed) VALUES (30, 29, 0.08)", // 1 Egg
            "INSERT OR IGNORE INTO ProductIngredients (ProductId, IngredientId, QuantityUsed) VALUES (30, 7, 0.02)", // 20ml Chocolate Syrup for topping

            // Cheesecake (ID: 31)
            "INSERT OR IGNORE INTO ProductIngredients (ProductId, IngredientId, QuantityUsed) VALUES (31, 27, 0.03)", // 30g Flour for base
            "INSERT OR IGNORE INTO ProductIngredients (ProductId, IngredientId, QuantityUsed) VALUES (31, 28, 0.02)", // 20g Butter for base
            "INSERT OR IGNORE INTO ProductIngredients (ProductId, IngredientId, QuantityUsed) VALUES (31, 42, 0.10)", // 100g Cream Cheese
            "INSERT OR IGNORE INTO ProductIngredients (ProductId, IngredientId, QuantityUsed) VALUES (31, 26, 0.03)", // 30g Sugar
            "INSERT OR IGNORE INTO ProductIngredients (ProductId, IngredientId, QuantityUsed) VALUES (31, 29, 0.08)", // 1 Egg

            // Brownie (ID: 32)
            "INSERT OR IGNORE INTO ProductIngredients (ProductId, IngredientId, QuantityUsed) VALUES (32, 27, 0.04)", // 40g Flour
            "INSERT OR IGNORE INTO ProductIngredients (ProductId, IngredientId, QuantityUsed) VALUES (32, 26, 0.05)", // 50g Sugar
            "INSERT OR IGNORE INTO ProductIngredients (ProductId, IngredientId, QuantityUsed) VALUES (32, 30, 0.03)", // 30g Cocoa Powder
            "INSERT OR IGNORE INTO ProductIngredients (ProductId, IngredientId, QuantityUsed) VALUES (32, 28, 0.04)", // 40g Butter
            "INSERT OR IGNORE INTO ProductIngredients (ProductId, IngredientId, QuantityUsed) VALUES (32, 29, 0.16)", // 2 Eggs
            "INSERT OR IGNORE INTO ProductIngredients (ProductId, IngredientId, QuantityUsed) VALUES (32, 40, 0.02)", // 20g Chocolate Chips

            // Apple Pie (ID: 33)
            "INSERT OR IGNORE INTO ProductIngredients (ProductId, IngredientId, QuantityUsed) VALUES (33, 27, 0.08)", // 80g Flour for crust
            "INSERT OR IGNORE INTO ProductIngredients (ProductId, IngredientId, QuantityUsed) VALUES (33, 28, 0.04)", // 40g Butter for crust
            "INSERT OR IGNORE INTO ProductIngredients (ProductId, IngredientId, QuantityUsed) VALUES (33, 22, 0.15)", // 150ml Apple Juice (concentrated)
            "INSERT OR IGNORE INTO ProductIngredients (ProductId, IngredientId, QuantityUsed) VALUES (33, 26, 0.03)", // 30g Sugar
            "INSERT OR IGNORE INTO ProductIngredients (ProductId, IngredientId, QuantityUsed) VALUES (33, 13, 0.002)", // 2g Cinnamon

            // Tiramisu (ID: 34)
            "INSERT OR IGNORE INTO ProductIngredients (ProductId, IngredientId, QuantityUsed) VALUES (34, 2, 0.02)", // 20g Ground Coffee
            "INSERT OR IGNORE INTO ProductIngredients (ProductId, IngredientId, QuantityUsed) VALUES (34, 42, 0.08)", // 80g Cream Cheese
            "INSERT OR IGNORE INTO ProductIngredients (ProductId, IngredientId, QuantityUsed) VALUES (34, 29, 0.08)", // 1 Egg
            "INSERT OR IGNORE INTO ProductIngredients (ProductId, IngredientId, QuantityUsed) VALUES (34, 26, 0.03)", // 30g Sugar
            "INSERT OR IGNORE INTO ProductIngredients (ProductId, IngredientId, QuantityUsed) VALUES (34, 30, 0.01)", // 10g Cocoa Powder for dusting

            // Fruit Tart (ID: 35)
            "INSERT OR IGNORE INTO ProductIngredients (ProductId, IngredientId, QuantityUsed) VALUES (35, 27, 0.05)", // 50g Flour for crust
            "INSERT OR IGNORE INTO ProductIngredients (ProductId, IngredientId, QuantityUsed) VALUES (35, 28, 0.03)", // 30g Butter for crust
            "INSERT OR IGNORE INTO ProductIngredients (ProductId, IngredientId, QuantityUsed) VALUES (35, 34, 0.03)", // 30g Strawberries
            "INSERT OR IGNORE INTO ProductIngredients (ProductId, IngredientId, QuantityUsed) VALUES (35, 33, 0.03)", // 30g Blueberries
            "INSERT OR IGNORE INTO ProductIngredients (ProductId, IngredientId, QuantityUsed) VALUES (35, 42, 0.04)", // 40g Cream Cheese
            "INSERT OR IGNORE INTO ProductIngredients (ProductId, IngredientId, QuantityUsed) VALUES (35, 26, 0.02)",  // 20g Sugar
            @"INSERT OR IGNORE INTO Orders (Id, CustomerId, OrderDate, PaymentDate, Status, TableId, VoucherId, TotalAmount, PaymentMethodId, ServiceTypeId) VALUES 
    (8, 1, '2025-01-03 14:00:00', '2025-01-03 14:10:00', 'Completed', 1, 5, 29.70, 2, 1),
    (9, 2, '2025-01-08 16:30:00', '2025-01-08 16:40:00', 'Completed', NULL, NULL, 35.50, 1, 2),
    (10, 1, '2025-01-12 19:00:00', NULL, 'Pending', 2, NULL, 48.00, NULL, 1),
    (11, 2, '2025-01-18 11:45:00', '2025-01-18 11:50:00', 'Completed', 1, 6, 62.40, 2, 1),
    (12, 1, '2025-01-22 13:20:00', '2025-01-22 13:30:00', 'Completed', NULL, 7, 27.20, 1, 2)",

// February 2025 - Valentine's season
@"INSERT OR IGNORE INTO Orders (Id, CustomerId, OrderDate, PaymentDate, Status, TableId, VoucherId, TotalAmount, PaymentMethodId, ServiceTypeId) VALUES 
    (13, 1, '2025-02-01 18:00:00', '2025-02-01 18:15:00', 'Completed', 2, 8, 76.50, 2, 1),
    (14, 2, '2025-02-07 12:30:00', '2025-02-07 12:35:00', 'Completed', NULL, NULL, 41.25, 1, 2),
    (15, 1, '2025-02-12 20:15:00', '2025-02-12 20:25:00', 'Completed', 1, 9, 53.10, 2, 1),
    (16, 2, '2025-02-16 15:45:00', NULL, 'Pending', NULL, NULL, 39.80, NULL, 2),
    (17, 1, '2025-02-20 17:30:00', '2025-02-20 17:40:00', 'Completed', 2, 1, 67.50, 1, 1)",

// March 2025 - Spring beginning
@"INSERT OR IGNORE INTO Orders (Id, CustomerId, OrderDate, PaymentDate, Status, TableId, VoucherId, TotalAmount, PaymentMethodId, ServiceTypeId) VALUES 
    (18, 2, '2025-03-02 10:30:00', '2025-03-02 10:40:00', 'Completed', 1, 2, 44.80, 2, 1),
    (19, 1, '2025-03-08 13:15:00', '2025-03-08 13:25:00', 'Completed', NULL, NULL, 52.60, 1, 2),
    (20, 2, '2025-03-12 16:00:00', '2025-03-12 16:10:00', 'Completed', 2, 3, 71.40, 2, 1),
    (21, 1, '2025-03-18 19:30:00', NULL, 'Pending', 1, NULL, 33.90, NULL, 1),
    (22, 2, '2025-03-22 11:00:00', '2025-03-22 11:10:00', 'Completed', NULL, 4, 49.50, 1, 2)",

// April 2025 - Spring break
@"INSERT OR IGNORE INTO Orders (Id, CustomerId, OrderDate, PaymentDate, Status, TableId, VoucherId, TotalAmount, PaymentMethodId, ServiceTypeId) VALUES 
    (23, 1, '2025-04-01 12:45:00', '2025-04-01 12:55:00', 'Completed', 2, 5, 88.20, 2, 1),
    (24, 2, '2025-04-05 15:30:00', '2025-04-05 15:40:00', 'Completed', NULL, NULL, 46.75, 1, 2),
    (25, 1, '2025-04-10 18:15:00', '2025-04-10 18:25:00', 'Completed', 1, 6, 64.80, 2, 1),
    (26, 2, '2025-04-15 20:00:00', NULL, 'Pending', NULL, NULL, 42.30, NULL, 2),
    (27, 1, '2025-04-20 14:30:00', '2025-04-20 14:40:00', 'Completed', 2, 7, 73.10, 1, 1)",

// May 2025 - Start of summer
@"INSERT OR IGNORE INTO Orders (Id, CustomerId, OrderDate, PaymentDate, Status, TableId, VoucherId, TotalAmount, PaymentMethodId, ServiceTypeId) VALUES 
    (28, 2, '2025-05-02 11:30:00', '2025-05-02 11:40:00', 'Completed', 1, 8, 55.90, 2, 1),
    (29, 1, '2025-05-08 13:45:00', '2025-05-08 13:55:00', 'Completed', NULL, NULL, 62.40, 1, 2),
    (30, 2, '2025-05-12 16:20:00', '2025-05-12 16:30:00', 'Completed', 2, 9, 79.20, 2, 1),
    (31, 1, '2025-05-18 19:00:00', NULL, 'Pending', 1, NULL, 47.60, NULL, 1),
    (32, 2, '2025-05-22 12:15:00', '2025-05-22 12:25:00', 'Completed', NULL, 1, 51.30, 1, 2)",

// June 2025 - Summer peak
@"INSERT OR IGNORE INTO Orders (Id, CustomerId, OrderDate, PaymentDate, Status, TableId, VoucherId, TotalAmount, PaymentMethodId, ServiceTypeId) VALUES 
    (33, 1, '2025-06-01 14:00:00', '2025-06-01 14:10:00', 'Completed', 2, 2, 92.70, 2, 1),
    (34, 2, '2025-06-05 16:45:00', '2025-06-05 16:55:00', 'Completed', NULL, NULL, 58.90, 1, 2),
    (35, 1, '2025-06-10 19:30:00', '2025-06-10 19:40:00', 'Completed', 1, 3, 66.40, 2, 1),
    (36, 2, '2025-06-15 11:45:00', NULL, 'Pending', NULL, NULL, 44.20, NULL, 2),
    (37, 1, '2025-06-20 13:20:00', '2025-06-20 13:30:00', 'Completed', 2, 4, 81.50, 1, 1)",

// July 2025 - Peak summer
@"INSERT OR IGNORE INTO Orders (Id, CustomerId, OrderDate, PaymentDate, Status, TableId, VoucherId, TotalAmount, PaymentMethodId, ServiceTypeId) VALUES 
    (38, 2, '2025-07-02 15:30:00', '2025-07-02 15:40:00', 'Completed', 1, 5, 77.80, 2, 1),
    (39, 1, '2025-07-08 18:15:00', '2025-07-08 18:25:00', 'Completed', NULL, NULL, 63.20, 1, 2),
    (40, 2, '2025-07-12 20:00:00', '2025-07-12 20:10:00', 'Completed', 2, 6, 85.90, 2, 1),
    (41, 1, '2025-07-18 12:45:00', NULL, 'Pending', 1, NULL, 49.70, NULL, 1),
    (42, 2, '2025-07-22 14:30:00', '2025-07-22 14:40:00', 'Completed', NULL, 7, 72.40, 1, 2)",

// August 2025 - Late summer
@"INSERT OR IGNORE INTO Orders (Id, CustomerId, OrderDate, PaymentDate, Status, TableId, VoucherId, TotalAmount, PaymentMethodId, ServiceTypeId) VALUES 
    (43, 1, '2025-08-01 16:00:00', '2025-08-01 16:10:00', 'Completed', 2, 8, 94.30, 2, 1),
    (44, 2, '2025-08-05 19:30:00', '2025-08-05 19:40:00', 'Completed', NULL, NULL, 57.60, 1, 2),
    (45, 1, '2025-08-10 11:45:00', '2025-08-10 11:55:00', 'Completed', 1, 9, 68.20, 2, 1),
    (46, 2, '2025-08-15 13:20:00', NULL, 'Pending', NULL, NULL, 45.80, NULL, 2),
    (47, 1, '2025-08-20 15:45:00', '2025-08-20 15:55:00', 'Completed', 2, 1, 83.10, 1, 1)",

// September 2025 - Back to school
@"INSERT OR IGNORE INTO Orders (Id, CustomerId, OrderDate, PaymentDate, Status, TableId, VoucherId, TotalAmount, PaymentMethodId, ServiceTypeId) VALUES 
    (48, 2, '2025-09-02 12:15:00', '2025-09-02 12:25:00', 'Completed', 1, 2, 61.40, 2, 1),
    (49, 1, '2025-09-08 14:30:00', '2025-09-08 14:40:00', 'Completed', NULL, NULL, 49.90, 1, 2)",
// September 2025 - Back to school
@"INSERT OR IGNORE INTO Orders (Id, CustomerId, OrderDate, PaymentDate, Status, TableId, VoucherId, TotalAmount, PaymentMethodId, ServiceTypeId) VALUES 
    (50, 2, '2025-09-12 16:20:00', '2025-09-12 16:30:00', 'Completed', 2, 3, 82.80, 2, 1),
    (51, 1, '2025-09-18 18:10:00', NULL, 'Pending', 1, NULL, 56.40, NULL, 1),
    (52, 2, '2025-09-22 20:00:00', '2025-09-22 20:10:00', 'Completed', NULL, 4, 65.70, 1, 2),
    (53, 1, '2025-09-25 11:15:00', '2025-09-25 11:25:00', 'Completed', 2, 5, 74.90, 2, 1),
    (54, 2, '2025-09-28 13:40:00', '2025-09-28 13:50:00', 'Completed', 1, NULL, 43.20, 1, 1)",

// October 2025 - Autumn
@"INSERT OR IGNORE INTO Orders (Id, CustomerId, OrderDate, PaymentDate, Status, TableId, VoucherId, TotalAmount, PaymentMethodId, ServiceTypeId) VALUES 
    (55, 1, '2025-10-02 15:30:00', '2025-10-02 15:40:00', 'Completed', 2, 6, 91.50, 2, 1),
    (56, 2, '2025-10-07 17:45:00', '2025-10-07 17:55:00', 'Completed', NULL, NULL, 59.80, 1, 2),
    (57, 1, '2025-10-12 19:30:00', '2025-10-12 19:40:00', 'Completed', 1, 7, 68.40, 2, 1),
    (58, 2, '2025-10-18 12:15:00', NULL, 'Pending', NULL, NULL, 47.90, NULL, 2),
    (59, 1, '2025-10-22 14:00:00', '2025-10-22 14:10:00', 'Completed', 2, 8, 83.20, 1, 1),
    (60, 2, '2025-10-27 16:30:00', '2025-10-27 16:40:00', 'Completed', 1, 9, 72.60, 2, 1)",

// November 2025 - Pre-holiday
@"INSERT OR IGNORE INTO Orders (Id, CustomerId, OrderDate, PaymentDate, Status, TableId, VoucherId, TotalAmount, PaymentMethodId, ServiceTypeId) VALUES 
    (61, 1, '2025-11-01 11:45:00', '2025-11-01 11:55:00', 'Completed', 2, 1, 88.90, 2, 1),
    (62, 2, '2025-11-05 13:20:00', '2025-11-05 13:30:00', 'Completed', NULL, NULL, 55.30, 1, 2),
    (63, 1, '2025-11-10 15:45:00', '2025-11-10 15:55:00', 'Completed', 1, 2, 76.40, 2, 1),
    (64, 2, '2025-11-15 18:00:00', NULL, 'Pending', NULL, NULL, 62.10, NULL, 2),
    (65, 1, '2025-11-20 20:15:00', '2025-11-20 20:25:00', 'Completed', 2, 3, 94.50, 1, 1),
    (66, 2, '2025-11-25 12:30:00', '2025-11-25 12:40:00', 'Completed', 1, 4, 67.80, 2, 1)",

// December 2025 - Holiday season
@"INSERT OR IGNORE INTO Orders (Id, CustomerId, OrderDate, PaymentDate, Status, TableId, VoucherId, TotalAmount, PaymentMethodId, ServiceTypeId) VALUES 
    (67, 1, '2025-12-02 14:15:00', '2025-12-02 14:25:00', 'Completed', 2, 5, 102.60, 2, 1),
    (68, 2, '2025-12-07 16:45:00', '2025-12-07 16:55:00', 'Completed', NULL, NULL, 71.20, 1, 2),
    (69, 1, '2025-12-12 19:00:00', '2025-12-12 19:10:00', 'Completed', 1, 6, 89.40, 2, 1),
    (70, 2, '2025-12-18 11:30:00', NULL, 'Pending', NULL, NULL, 58.70, NULL, 2),
    (71, 1, '2025-12-22 13:45:00', '2025-12-22 13:55:00', 'Completed', 2, 7, 96.30, 1, 1),
    (72, 2, '2025-12-27 15:20:00', '2025-12-27 15:30:00', 'Completed', 1, 8, 84.50, 2, 1)",

// OrderDetails for September to December 2025
@"INSERT OR IGNORE INTO OrderDetails (Id, OrderId, ProductId, Quantity, Price) VALUES 
    -- September 2025
    (17, 50, 3, 2, 4.00),  -- 2 Latte
    (18, 50, 8, 1, 5.50),  -- 1 Club Sandwich
    (19, 50, 35, 1, 5.00), -- 1 Fruit Tart
    (20, 51, 12, 1, 4.50), -- 1 Mocha
    (21, 51, 24, 2, 5.00), -- 2 Tuna Sandwich
    (22, 52, 7, 2, 3.20),  -- 2 Orange Juice
    (23, 52, 16, 1, 2.50), -- 1 Croissant
    (24, 53, 2, 2, 3.50),  -- 2 Cappuccino
    (25, 53, 30, 1, 4.00), -- 1 Chocolate Cake
    (26, 54, 6, 1, 3.40),  -- 1 Peach Tea
    (27, 54, 9, 1, 4.75),  -- 1 BLT Sandwich
    -- October 2025
    (28, 55, 1, 3, 2.50),  -- 3 Espresso
    (29, 55, 31, 1, 3.75), -- 1 Cheesecake
    (30, 55, 25, 1, 4.50), -- 1 Vanilla Milkshake
    (31, 56, 20, 2, 2.75), -- 2 Apple Juice
    (32, 56, 10, 1, 4.00), -- 1 Grilled Cheese
    (33, 57, 15, 2, 3.50), -- 2 Chai Tea
    (34, 57, 17, 1, 3.25), -- 1 Danish Pastry
    (35, 58, 4, 1, 3.00),  -- 1 Chamomile Tea
    (36, 58, 24, 1, 5.00), -- 1 Tuna Sandwich
    (37, 59, 11, 2, 2.75), -- 2 Americano
    (38, 59, 8, 1, 5.50),  -- 1 Club Sandwich
    (39, 60, 3, 1, 4.00),  -- 1 Latte
    (40, 60, 35, 1, 5.00), -- 1 Fruit Tart
    -- November 2025
    (41, 61, 12, 2, 4.50), -- 2 Mocha
    (42, 61, 30, 1, 4.00), -- 1 Chocolate Cake
    (43, 61, 25, 1, 4.50), -- 1 Vanilla Milkshake
    (44, 62, 7, 2, 3.20),  -- 2 Orange Juice
    (45, 62, 16, 1, 2.50), -- 1 Croissant
    (46, 63, 2, 2, 3.50),  -- 2 Cappuccino
    (47, 63, 9, 1, 4.75),  -- 1 BLT Sandwich
    (48, 64, 6, 1, 3.40),  -- 1 Peach Tea
    (49, 64, 24, 2, 5.00), -- 2 Tuna Sandwich
    (50, 65, 1, 3, 2.50),  -- 3 Espresso
    (51, 65, 31, 1, 3.75), -- 1 Cheesecake
    (52, 65, 35, 1, 5.00), -- 1 Fruit Tart
    (53, 66, 20, 2, 2.75), -- 2 Apple Juice
    (54, 66, 8, 1, 5.50),  -- 1 Club Sandwich
    -- December 2025
    (55, 67, 3, 2, 4.00),  -- 2 Latte
    (56, 67, 30, 1, 4.00), -- 1 Chocolate Cake
    (57, 67, 25, 2, 4.50), -- 2 Vanilla Milkshake
    (58, 68, 15, 2, 3.50), -- 2 Chai Tea
    (59, 68, 17, 1, 3.25), -- 1 Danish Pastry
    (60, 69, 12, 2, 4.50), -- 2 Mocha
    (61, 69, 8, 1, 5.50),  -- 1 Club Sandwich
    (62, 70, 4, 1, 3.00),  -- 1 Chamomile Tea
    (63, 70, 24, 1, 5.00), -- 1 Tuna Sandwich
    (64, 71, 1, 3, 2.50),  -- 3 Espresso
    (65, 71, 31, 1, 3.75), -- 1 Cheesecake
    (66, 71, 35, 1, 5.00), -- 1 Fruit Tart
    (67, 72, 11, 2, 2.75), -- 2 Americano
    (68, 72, 8, 1, 5.50),  -- 1 Club Sandwich
    (69, 72, 30, 1, 4.00)",

        };

                foreach (var commandText in seedCommands)
                {
                    using var command = connection.CreateCommand();
                    command.CommandText = commandText;
                    await command.ExecuteNonQueryAsync();
                }

                Console.WriteLine("Data seeding completed!");
            }
            else
            {
                Console.WriteLine("Data already exists, skipping seed.");
            }
        }
        else
        {
            Console.WriteLine("Database already exists, no need to seed data.");
        }
    }

}