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


    //public IRepository<User> Users { get; } = new MockRepository<User>(new List<User>() {
    //    new User { Id = 1, Username = "admin", Password = "admin1234" },
    //    new User { Id = 2, Username = "employee1", Password = "employee1234" },
    //    new User { Id = 3, Username = "employee2", Password = "employee1234"}
    //});
    public IUserRepository Users => throw new NotImplementedException("MockDao.Users is not implemented.");
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
        new Product { Id = 1, Name = "Espresso", Description = "Strong coffee", Price = 2.50, Image = "ms-appx:///Assets/ProductImage.jpg", IsStocked = true, CategoryId = 1 },
        new Product { Id = 2, Name = "Cappuccino", Description = "Espresso with milk", Price = 3.50, Image = "ms-appx:///Assets/ProductImage.jpg", IsStocked = true, CategoryId = 1 },
        new Product { Id = 3, Name = "Latte", Description = "Creamy espresso", Price = 4.00, Image = "ms-appx:///Assets/ProductImage.jpg", IsStocked = true, CategoryId = 1 },
        new Product { Id = 4, Name = "Americano", Description = "Espresso with water", Price = 2.75, Image = "ms-appx:///Assets/ProductImage.jpg", IsStocked = true, CategoryId = 1 },
        new Product { Id = 5, Name = "Mocha", Description = "Chocolate-flavored coffee", Price = 4.50, Image = "ms-appx:///Assets/ProductImage.jpg", IsStocked = false, CategoryId = 1 },

        // Tea (CategoryId = 2)
        new Product { Id = 6, Name = "Green Tea", Description = "Healthy green tea", Price = 2.00, Image = "ms-appx:///Assets/ProductImage.jpg", IsStocked = false, CategoryId = 2 },
        new Product { Id = 7, Name = "Black Tea", Description = "Strong tea", Price = 2.50, Image = "ms-appx:///Assets/ProductImage.jpg", IsStocked = true, CategoryId = 2 },
        new Product { Id = 8, Name = "Oolong Tea", Description = "Smooth flavor", Price = 3.00, Image = "ms-appx:///Assets/ProductImage.jpg", IsStocked = true, CategoryId = 2 },
        new Product { Id = 9, Name = "Herbal Tea", Description = "Caffeine-free tea", Price = 2.75, Image = "ms-appx:///Assets/ProductImage.jpg", IsStocked = false, CategoryId = 2 },
        new Product { Id = 10, Name = "Chai Tea", Description = "Spiced milk tea", Price = 3.50, Image = "ms-appx:///Assets/ProductImage.jpg", IsStocked = true, CategoryId = 2 },

        // Pastries (CategoryId = 3)
        new Product { Id = 11, Name = "Blueberry Muffin", Description = "Soft and sweet muffin", Price = 3.00, Image = "ms-appx:///Assets/ProductImage.jpg", IsStocked = true, CategoryId = 3 },
        new Product { Id = 12, Name = "Croissant", Description = "Buttery and flaky", Price = 2.50, Image = "ms-appx:///Assets/ProductImage.jpg", IsStocked = true, CategoryId = 3 },
        new Product { Id = 13, Name = "Danish Pastry", Description = "Sweet and fruity", Price = 3.25, Image = "ms-appx:///Assets/ProductImage.jpg", IsStocked = true, CategoryId = 3 },
        new Product { Id = 14, Name = "Chocolate Donut", Description = "Glazed donut", Price = 2.75, Image = "ms-appx:///Assets/ProductImage.jpg", IsStocked = true, CategoryId = 3 },
        new Product { Id = 15, Name = "Apple Turnover", Description = "Crispy apple filling", Price = 3.50, Image = "ms-appx:///Assets/ProductImage.jpg", IsStocked = true, CategoryId = 3 },

        // Juices (CategoryId = 4)
        new Product { Id = 16, Name = "Orange Juice", Description = "Fresh squeezed", Price = 3.00, Image = "ms-appx:///Assets/ProductImage.jpg", IsStocked = true, CategoryId = 4 },
        new Product { Id = 17, Name = "Apple Juice", Description = "Sweet and refreshing", Price = 2.75, Image = "ms-appx:///Assets/ProductImage.jpg", IsStocked = true, CategoryId = 4 },
        new Product { Id = 18, Name = "Lemonade", Description = "Tart and sweet", Price = 2.50, Image = "ms-appx:///Assets/ProductImage.jpg", IsStocked = true, CategoryId = 4 },
        new Product { Id = 19, Name = "Mango Juice", Description = "Rich mango flavor", Price = 3.25, Image = "ms-appx:///Assets/ProductImage.jpg", IsStocked = true, CategoryId = 4 },
        new Product { Id = 20, Name = "Grape Juice", Description = "Sweet grape flavor", Price = 3.00, Image = "ms-appx:///Assets/ProductImage.jpg", IsStocked = true, CategoryId = 4 },
   
        // Sandwiches (CategoryId = 5)
        new Product { Id = 21, Name = "Club Sandwich", Description = "Turkey, ham, bacon", Price = 5.50, Image = "ms-appx:///Assets/ProductImage.jpg", IsStocked = true, CategoryId = 5 },
        new Product { Id = 22, Name = "BLT Sandwich", Description = "Bacon, lettuce, tomato", Price = 4.75, Image = "ms-appx:///Assets/ProductImage.jpg", IsStocked = true, CategoryId = 5 },
        new Product { Id = 23, Name = "Grilled Cheese", Description = "Cheese & toast", Price = 4.00, Image = "ms-appx:///Assets/ProductImage.jpg", IsStocked = true, CategoryId = 5 },
        new Product { Id = 24, Name = "Tuna Sandwich", Description = "Tuna & mayo", Price = 5.00, Image = "ms-appx:///Assets/ProductImage.jpg", IsStocked = true, CategoryId = 5 },
        new Product { Id = 25, Name = "Chicken Avocado Sandwich", Description = "Chicken & avocado", Price = 5.75, Image = "ms-appx:///Assets/ProductImage.jpg", IsStocked = true, CategoryId = 5 },

        // Milkshakes (CategoryId = 6)
        new Product { Id = 26, Name = "Vanilla Milkshake", Description = "Classic vanilla", Price = 4.50, Image = "ms-appx:///Assets/ProductImage.jpg", IsStocked = true, CategoryId = 6 },
        new Product { Id = 27, Name = "Chocolate Milkshake", Description = "Rich chocolate", Price = 4.75, Image = "ms-appx:///Assets/ProductImage.jpg", IsStocked = true, CategoryId = 6 },
        new Product { Id = 28, Name = "Strawberry Milkshake", Description = "Fruity and creamy", Price = 5.00, Image = "ms-appx:///Assets/ProductImage.jpg", IsStocked = true, CategoryId = 6 },
        new Product { Id = 29, Name = "Oreo Milkshake", Description = "Cookies & cream", Price = 5.25, Image = "ms-appx:///Assets/ProductImage.jpg", IsStocked = true, CategoryId = 6 },
        new Product { Id = 30, Name = "Caramel Milkshake", Description = "Sweet caramel flavor", Price = 5.50, Image = "ms-appx:///Assets/ProductImage.jpg", IsStocked = true, CategoryId = 6 },

        // Desserts (CategoryId = 7)
        new Product { Id = 31, Name = "Chocolate Cake", Description = "Rich chocolate layers", Price = 6.00, Image = "ms-appx:///Assets/ProductImage.jpg", IsStocked = true, CategoryId = 7 },
        new Product { Id = 32, Name = "Cheesecake", Description = "Creamy and sweet", Price = 5.75, Image = "ms-appx:///Assets/ProductImage.jpg", IsStocked = true, CategoryId = 7 },
        new Product { Id = 33, Name = "Brownie", Description = "Dense & fudgy", Price = 4.50, Image = "ms-appx:///Assets/ProductImage.jpg", IsStocked = true, CategoryId = 7 },
        new Product { Id = 34, Name = "Apple Pie", Description = "Classic American dessert", Price = 5.25, Image = "ms-appx:///Assets/ProductImage.jpg", IsStocked = true, CategoryId = 7 },
        new Product { Id = 35, Name = "Tiramisu", Description = "Coffee-flavored dessert", Price = 6.50, Image = "ms-appx:///Assets/ProductImage.jpg", IsStocked = true, CategoryId = 7 },
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

   

    public IRepository<IngredientInventoryTransaction> IngredientInventoryTransactions
    {
        get;
    } = new MockRepository<IngredientInventoryTransaction>(new List<IngredientInventoryTransaction>() {
    new IngredientInventoryTransaction { Id = 1, IngredientId = 1, Timestamp = 1672531200000, Quantity = 50, Unit = "shot", TransactionType = "IMPORT", UnitPrice = 2.50 },
    new IngredientInventoryTransaction { Id = 2, IngredientId = 2, Timestamp = 1672617600000, Quantity = 100, Unit = "cup", TransactionType = "IMPORT", UnitPrice = 1.00 },
    new IngredientInventoryTransaction { Id = 3, IngredientId = 3, Timestamp = 1672704000000, Quantity = 200, Unit = "tsp", TransactionType = "IMPORT", UnitPrice = 0.10 },
    new IngredientInventoryTransaction { Id = 4, IngredientId = 4, Timestamp = 1672790400000, Quantity = 30, Unit = "oz", TransactionType = "IMPORT", UnitPrice = 3.00 },
    new IngredientInventoryTransaction { Id = 5, IngredientId = 5, Timestamp = 1672876800000, Quantity = 150, Unit = "oz", TransactionType = "IMPORT", UnitPrice = 2.00 },
    new IngredientInventoryTransaction { Id = 6, IngredientId = 6, Timestamp = 1672963200000, Quantity = 500, Unit = "cup", TransactionType = "IMPORT", UnitPrice = 0.05 },
    new IngredientInventoryTransaction { Id = 7, IngredientId = 7, Timestamp = 1673049600000, Quantity = 200, Unit = "cup", TransactionType = "IMPORT", UnitPrice = 0.50 },
    new IngredientInventoryTransaction { Id = 8, IngredientId = 8, Timestamp = 1673136000000, Quantity = 100, Unit = "tbsp", TransactionType = "IMPORT", UnitPrice = 0.20 },
});

    public IRepository<Customer> Customers { get; } = new MockRepository<Customer>(new List<Customer>() {
        new Customer { Id = 1, Name = "Alice", Phone = "0123456789", IsMembership = true, Points = 100 },
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
    public IRepository<ServiceType> ServiceTypes
    {
        get;
    } = new MockRepository<ServiceType>(new List<ServiceType>()
{
    new ServiceType { Id = 1, Name = "Dine-in" },
    new ServiceType { Id = 2, Name = "Take-away" }
});

    public IRepository<Table> Tables
    {
        get;
    } = new MockRepository<Table>(new List<Table>() {
    new Table { Id = 1, TableNumber = "T01", Status = "Available" },
    new Table { Id = 2, TableNumber = "T02", Status = "Occupied" },
    new Table { Id = 3, TableNumber = "T03", Status = "Available" },
    new Table { Id = 4, TableNumber = "T04", Status = "Reserved" },
    new Table { Id = 5, TableNumber = "T05", Status = "Available" },
    new Table { Id = 6, TableNumber = "T06", Status = "Occupied" },
    new Table { Id = 7, TableNumber = "T07", Status = "Available" },
    new Table { Id = 8, TableNumber = "T08", Status = "Reserved" },
    new Table { Id = 9, TableNumber = "T09", Status = "Available" },
    new Table { Id = 10, TableNumber = "T10", Status = "Occupied" }
});

    public IRepository<Reservation> Reservations
    {
        get;
    } = new MockRepository<Reservation>(new List<Reservation>() {
    new Reservation { Id = 1, CustomerId = 1, TableId = 4, ReservationDate = DateTime.Parse("2025-03-25"), StartTime = TimeSpan.Parse("18:00"), EndTime = TimeSpan.Parse("20:00"), Status = "Confirmed" },
    new Reservation { Id = 2, CustomerId = 3, TableId = 8, ReservationDate = DateTime.Parse("2025-03-26"), StartTime = TimeSpan.Parse("12:00"), EndTime = TimeSpan.Parse("14:00"), Status = "Pending" },
    new Reservation { Id = 3, CustomerId = 5, TableId = 1, ReservationDate = DateTime.Parse("2025-03-27"), StartTime = TimeSpan.Parse("19:00"), EndTime = TimeSpan.Parse("21:00"), Status = "Confirmed" },
    new Reservation { Id = 4, CustomerId = 7, TableId = 5, ReservationDate = DateTime.Parse("2025-03-28"), StartTime = TimeSpan.Parse("17:00"), EndTime = TimeSpan.Parse("19:00"), Status = "Pending" },
    new Reservation { Id = 5, CustomerId = 9, TableId = 9, ReservationDate = DateTime.Parse("2025-03-29"), StartTime = TimeSpan.Parse("13:00"), EndTime = TimeSpan.Parse("15:00"), Status = "Confirmed" },
    new Reservation { Id = 6, CustomerId = 2, TableId = 3, ReservationDate = DateTime.Parse("2025-03-30"), StartTime = TimeSpan.Parse("11:00"), EndTime = TimeSpan.Parse("13:00"), Status = "Pending" },
    new Reservation { Id = 7, CustomerId = 4, TableId = 7, ReservationDate = DateTime.Parse("2025-03-31"), StartTime = TimeSpan.Parse("20:00"), EndTime = TimeSpan.Parse("22:00"), Status = "Confirmed" },
    new Reservation { Id = 8, CustomerId = 6, TableId = 2, ReservationDate = DateTime.Parse("2025-04-01"), StartTime = TimeSpan.Parse("16:00"), EndTime = TimeSpan.Parse("18:00"), Status = "Pending" },
    new Reservation { Id = 9, CustomerId = 8, TableId = 6, ReservationDate = DateTime.Parse("2025-04-02"), StartTime = TimeSpan.Parse("14:00"), EndTime = TimeSpan.Parse("16:00"), Status = "Confirmed" },
    new Reservation { Id = 10, CustomerId = 10, TableId = 10, ReservationDate = DateTime.Parse("2025-04-03"), StartTime = TimeSpan.Parse("18:30"), EndTime = TimeSpan.Parse("20:30"), Status = "Pending" }
});

    public IRepository<Voucher> Vouchers
    {
        get;
    } = new MockRepository<Voucher>(new List<Voucher>() {
    new Voucher { Id = 1, Code = "WELCOME10", DiscountPercentage = 10, ExpirationDate = DateTime.Parse("2025-06-30"), IsUsed = false },
    new Voucher { Id = 2, Code = "COFFEE20", DiscountPercentage = 20, ExpirationDate = DateTime.Parse("2025-04-15"), IsUsed = true },
    new Voucher { Id = 3, Code = "PASTRY15", DiscountPercentage = 15, ExpirationDate = DateTime.Parse("2025-05-31"), IsUsed = false },
    new Voucher { Id = 4, Code = "LOYALTY25", DiscountPercentage = 25, ExpirationDate = DateTime.Parse("2025-12-31"), IsUsed = false },
    new Voucher { Id = 5, Code = "SPRING10", DiscountPercentage = 10, ExpirationDate = DateTime.Parse("2025-03-31"), IsUsed = true },
    new Voucher { Id = 6, Code = "SUMMER20", DiscountPercentage = 20, ExpirationDate = DateTime.Parse("2025-08-31"), IsUsed = false },
    new Voucher { Id = 7, Code = "TEA15", DiscountPercentage = 15, ExpirationDate = DateTime.Parse("2025-07-15"), IsUsed = false },
    new Voucher { Id = 8, Code = "FIRSTORDER30", DiscountPercentage = 30, ExpirationDate = DateTime.Parse("2025-04-30"), IsUsed = true },
    new Voucher { Id = 9, Code = "WEEKEND10", DiscountPercentage = 10, ExpirationDate = DateTime.Parse("2025-05-15"), IsUsed = false },
    new Voucher { Id = 10, Code = "BIRTHDAY50", DiscountPercentage = 50, ExpirationDate = DateTime.Parse("2025-03-25"), IsUsed = true }
});

    public IRepository<PaymentMethod> PaymentMethods
    {
        get;
    } = new MockRepository<PaymentMethod>(new List<PaymentMethod>() {
    new PaymentMethod { Id = 1, Name = "Cash", Description = "Payment in cash" },
    new PaymentMethod { Id = 2, Name = "Card", Description = "Payment via credit or debit card" },
    new PaymentMethod { Id = 3, Name = "Online Transfer", Description = "Direct online bank transfer" }
});

    public IRepository<Order> Orders
    {
        get;
    } = new MockRepository<Order>(new List<Order>() {
    new Order { Id = 1, CustomerId = 1, OrderDate = DateTime.Parse("2025-03-20 10:00:00"), PaymentDate = DateTime.Parse("2025-03-20 10:05:00"), Status = "Completed", TableId = 1, VoucherId = 1, TotalAmount = 15.00m, PaymentMethodId = 1, ServiceTypeId = 1 },
    new Order { Id = 2, CustomerId = 2, OrderDate = DateTime.Parse("2025-03-21 12:30:00"), PaymentDate = null, Status = "Pending", TableId = null, VoucherId = null, TotalAmount = 8.50m, PaymentMethodId = 2, ServiceTypeId = 1 },
    new Order { Id = 3, CustomerId = 3, OrderDate = DateTime.Parse("2025-03-22 15:00:00"), PaymentDate = DateTime.Parse("2025-03-22 15:10:00"), Status = "Completed", TableId = 2, VoucherId = 2, TotalAmount = 20.00m, PaymentMethodId = 3, ServiceTypeId = 1 },
    new Order { Id = 4, CustomerId = 4, OrderDate = DateTime.Parse("2025-03-23 09:15:00"), PaymentDate = null, Status = "Pending", TableId = null, VoucherId = null, TotalAmount = 12.75m, PaymentMethodId = 4, ServiceTypeId = 2 },
    new Order { Id = 5, CustomerId = 5, OrderDate = DateTime.Parse("2025-03-24 17:45:00"), PaymentDate = DateTime.Parse("2025-03-24 17:50:00"), Status = "Completed", TableId = 3, VoucherId = 3, TotalAmount = 25.50m, PaymentMethodId = 5, ServiceTypeId = 1 },
    new Order { Id = 6, CustomerId = 6, OrderDate = DateTime.Parse("2025-03-25 11:20:00"), PaymentDate = null, Status = "Pending", TableId = null, VoucherId = null, TotalAmount = 9.25m, PaymentMethodId = 6, ServiceTypeId = 2 },
    new Order { Id = 7, CustomerId = 7, OrderDate = DateTime.Parse("2025-03-26 19:00:00"), PaymentDate = DateTime.Parse("2025-03-26 19:05:00"), Status = "Completed", TableId = 4, VoucherId = 4, TotalAmount = 30.00m, PaymentMethodId = 7, ServiceTypeId = 1 },
    new Order { Id = 8, CustomerId = 8, OrderDate = DateTime.Parse("2025-03-27 14:30:00"), PaymentDate = null, Status = "Pending", TableId = null, VoucherId = null, TotalAmount = 7.50m, PaymentMethodId = 8, ServiceTypeId = 2 },
    new Order { Id = 9, CustomerId = 9, OrderDate = DateTime.Parse("2025-03-28 16:15:00"), PaymentDate = DateTime.Parse("2025-03-28 16:20:00"), Status = "Completed", TableId = 5, VoucherId = 5, TotalAmount = 18.00m, PaymentMethodId = 9, ServiceTypeId = 1 },
    new Order { Id = 10, CustomerId = 10, OrderDate = DateTime.Parse("2025-03-29 13:00:00"), PaymentDate = null, Status = "Pending", TableId = null, VoucherId = null, TotalAmount = 10.00m, PaymentMethodId = 10, ServiceTypeId = 2 }
});

    public IRepository<OrderDetail> OrderDetails
    {
        get;
    } = new MockRepository<OrderDetail>(new List<OrderDetail>() {
    new OrderDetail { Id = 1, OrderId = 1, ProductId = 1, Quantity = 2, Price = 2.50m },
    new OrderDetail { Id = 2, OrderId = 1, ProductId = 11, Quantity = 1, Price = 3.00m },
    new OrderDetail { Id = 3, OrderId = 2, ProductId = 6, Quantity = 1, Price = 2.00m },
    new OrderDetail { Id = 4, OrderId = 3, ProductId = 21, Quantity = 1, Price = 5.50m },
    new OrderDetail { Id = 5, OrderId = 3, ProductId = 26, Quantity = 1, Price = 4.50m },
    new OrderDetail { Id = 6, OrderId = 4, ProductId = 16, Quantity = 2, Price = 3.00m },
    new OrderDetail { Id = 7, OrderId = 5, ProductId = 31, Quantity = 1, Price = 6.00m },
    new OrderDetail { Id = 8, OrderId = 5, ProductId = 2, Quantity = 2, Price = 3.50m },
    new OrderDetail { Id = 9, OrderId = 7, ProductId = 35, Quantity = 1, Price = 6.50m },
    new OrderDetail { Id = 10, OrderId = 9, ProductId = 3, Quantity = 2, Price = 4.00m }
});

    public Task SaveChangesAsync() => Task.CompletedTask;
}
