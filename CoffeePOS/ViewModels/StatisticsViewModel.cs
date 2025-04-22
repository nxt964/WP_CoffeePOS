using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using CoffeePOS.Core.Interfaces;
using CoffeePOS.Core.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.WinUI.UI.Controls;

namespace CoffeePOS.ViewModels;

public partial class StatisticsViewModel : ObservableRecipient
{
    private readonly IDao _dao;

    [ObservableProperty]
    private DateTime startDate = DateTime.Today.AddDays(-30);

    [ObservableProperty]
    private DateTime endDate = DateTime.Today;

    [ObservableProperty]
    private string selectedDateRange = "Last 30 Days";

    [ObservableProperty]
    private bool isCustomDateRange;

    [ObservableProperty]
    private bool isLoading;

    [ObservableProperty]
    private bool isDataLoaded;

    [ObservableProperty]
    private decimal totalRevenue;

    [ObservableProperty]
    private decimal totalCost;

    [ObservableProperty]
    private decimal totalProfit;

    [ObservableProperty]
    private int totalOrders;

    [ObservableProperty]
    private decimal revenueChangePercentage;

    [ObservableProperty]
    private decimal costChangePercentage;

    [ObservableProperty]
    private decimal profitChangePercentage;

    [ObservableProperty]
    private decimal ordersChangePercentage;

    [ObservableProperty]
    private string searchText = string.Empty;

    // Collections for data display
    [ObservableProperty]
    private ObservableCollection<OrderSummary> orderSummaries = new();

    [ObservableProperty]
    private ObservableCollection<CategoryRevenue> categoryRevenues = new();

    [ObservableProperty]
    private ObservableCollection<ProductSales> topSellingProducts = new();

    [ObservableProperty]
    private ObservableCollection<CustomerSummary> topCustomers = new();

    [ObservableProperty]
    private ObservableCollection<InventoryStatus> inventoryStatus = new();

    [ObservableProperty]
    private ObservableCollection<RevenueTimeSeries> revenueTimeSeries = new();

    [ObservableProperty]
    private ObservableCollection<InventoryTransaction> inventoryTransactions = new();

    public List<string> DateRanges
    {
        get;
    } = new List<string>
    {
        "Today",
        "This Week",
        "This Month",
        "This Year",
        "Last 7 Days",
        "Last 30 Days",
        "Last Year",
        "Custom Range"
    };

    public List<string> OrderFilterOptions
    {
        get;
    } = new List<string>
    {
        "All",
        "Completed",
        "Pending"
    };

    [ObservableProperty]
    private string selectedOrderFilter = "All";

    [ObservableProperty]
    private string selectedSortOption = "Date (Newest First)";

    public List<string> SortOptions
    {
        get;
    } = new List<string>
    {
        "Date (Newest First)",
        "Date (Oldest First)",
        "Amount (Highest First)",
        "Amount (Lowest First)"
    };

    // Business insights
    [ObservableProperty]
    private string highestRevenueDate;

    [ObservableProperty]
    private decimal averageDailyRevenue;

    [ObservableProperty]
    private string topPerformingCategory;

    [ObservableProperty]
    private string topSellingProductName;

    [ObservableProperty]
    private string lowStockWarning;

    [ObservableProperty]
    private decimal profitMargin;

    [ObservableProperty]
    private string mostValuableCustomer;

    // Membership vs Regular
    [ObservableProperty]
    private decimal membershipRevenue;

    [ObservableProperty]
    private decimal regularCustomerRevenue;

    [ObservableProperty]
    private double membershipPercentage;

    [ObservableProperty]
    private double regularCustomerPercentage;

    public StatisticsViewModel(IDao dao)
    {
        _dao = dao;
        LoadInitialDataAsync();
    }

    private async void LoadInitialDataAsync()
    {
        await UpdateDateRangeAsync();
        await LoadDataAsync();
    }

    [RelayCommand]
    private async Task DateRangeChangedAsync()
    {
        await UpdateDateRangeAsync();
        await LoadDataAsync();
    }

    private async Task UpdateDateRangeAsync()
    {
        IsCustomDateRange = SelectedDateRange == "Custom Range";

        switch (SelectedDateRange)
        {
            case "Today":
                StartDate = DateTime.Today;
                EndDate = DateTime.Today;
                break;
            case "This Week":
                StartDate = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek);
                EndDate = DateTime.Today;
                break;
            case "This Month":
                StartDate = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
                EndDate = DateTime.Today;
                break;
            case "This Year":
                StartDate = new DateTime(DateTime.Today.Year, 1, 1);
                EndDate = DateTime.Today;
                break;
            case "Last 7 Days":
                StartDate = DateTime.Today.AddDays(-6);
                EndDate = DateTime.Today;
                break;
            case "Last 30 Days":
                StartDate = DateTime.Today.AddDays(-29);
                EndDate = DateTime.Today;
                break;
            case "Last Year":
                StartDate = DateTime.Today.AddYears(-1);
                EndDate = DateTime.Today;
                break;
            case "Custom Range":
                // Keep existing dates
                break;
        }

        await Task.CompletedTask;
    }

    [RelayCommand]
    private async Task CustomDateChangedAsync()
    {
        await LoadDataAsync();
    }

    [RelayCommand]
    private async Task RefreshDataAsync()
    {
        await LoadDataAsync();
    }

    [RelayCommand]
    private async Task ExportDataAsync()
    {
        // Implementation of export functionality
        // We'll create CSV files from the current data
        await Task.CompletedTask;
    }

    [RelayCommand]
    private async Task SearchOrdersAsync()
    {
        await FilterOrdersAsync();
    }

    [RelayCommand]
    private async Task OrderFilterChangedAsync()
    {
        await FilterOrdersAsync();
    }

    [RelayCommand]
    private async Task SortOptionChangedAsync()
    {
        await FilterOrdersAsync();
    }

    private async Task FilterOrdersAsync()
    {
        IsLoading = true;

        try
        {
            var allOrders = await GetOrdersInDateRangeAsync();

            // Filter by search text
            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                allOrders = allOrders.Where(o =>
                    o.OrderId.ToString().Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                    (o.CustomerName != null && o.CustomerName.Contains(SearchText, StringComparison.OrdinalIgnoreCase)) ||
                    o.Status.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                    o.PaymentMethod.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                    o.ServiceType.Contains(SearchText, StringComparison.OrdinalIgnoreCase)
                ).ToList();
            }

            // Filter by order status
            if (SelectedOrderFilter != "All")
            {
                allOrders = allOrders.Where(o => o.Status == SelectedOrderFilter).ToList();
            }

            // Sort the orders
            allOrders = SortOrders(allOrders);

            OrderSummaries = new ObservableCollection<OrderSummary>(allOrders);
        }
        catch (Exception ex)
        {
            // Log error or show message
            System.Diagnostics.Debug.WriteLine($"Error filtering orders: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private List<OrderSummary> SortOrders(List<OrderSummary> orders)
    {
        return SelectedSortOption switch
        {
            "Date (Newest First)" => orders.OrderByDescending(o => o.OrderDate).ToList(),
            "Date (Oldest First)" => orders.OrderBy(o => o.OrderDate).ToList(),
            "Amount (Highest First)" => orders.OrderByDescending(o => o.TotalAmount).ToList(),
            "Amount (Lowest First)" => orders.OrderBy(o => o.TotalAmount).ToList(),
            _ => orders.OrderByDescending(o => o.OrderDate).ToList()
        };
    }

    private async Task LoadDataAsync()
    {
        IsLoading = true;
        IsDataLoaded = false;

        try
        {
            // Load orders in date range
            var ordersInRange = await GetOrdersInDateRangeAsync();
            OrderSummaries = new ObservableCollection<OrderSummary>(ordersInRange);

            // Calculate previous period for comparison
            var currentPeriodDays = (EndDate - StartDate).Days + 1;
            var previousPeriodStart = StartDate.AddDays(-currentPeriodDays);
            var previousPeriodEnd = StartDate.AddDays(-1);

            // Get orders from previous period for comparison
            var previousPeriodOrders = await GetOrdersInDateRangeAsync(previousPeriodStart, previousPeriodEnd);

            // Calculate KPIs
            await CalculateKPIsAsync(ordersInRange, previousPeriodOrders);

            // Load category revenue data
            await LoadCategoryRevenueAsync(ordersInRange);

            // Load top selling products
            await LoadTopSellingProductsAsync(ordersInRange);

            // Load top customers
            await LoadTopCustomersAsync(ordersInRange);

            // Load inventory status
            await LoadInventoryStatusAsync();

            // Load inventory transactions
            await LoadInventoryTransactionsAsync();

            // Load time series data
            await LoadRevenueTimeSeriesAsync(ordersInRange);

            // Calculate membership vs regular customer stats
            await CalculateMembershipStatsAsync(ordersInRange);

            // Generate business insights
            GenerateBusinessInsights(ordersInRange);

            IsDataLoaded = true;
        }
        catch (Exception ex)
        {
            // Log error or show message
            System.Diagnostics.Debug.WriteLine($"Error loading data: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task<List<OrderSummary>> GetOrdersInDateRangeAsync(DateTime? start = null, DateTime? end = null)
    {
        var startDate = start ?? StartDate;
        var endDate = end ?? EndDate;

        // Include the entire end date by setting it to the end of the day
        var endDateAdjusted = endDate.Date.AddDays(1).AddTicks(-1);

        var allOrders = await _dao.Orders.GetAll();
        var filteredOrders = allOrders.Where(o =>
            o.OrderDate >= startDate &&
            o.OrderDate <= endDateAdjusted).ToList();

        var orderSummaries = new List<OrderSummary>();

        foreach (var order in filteredOrders)
        {
            var orderDetails = (await _dao.OrderDetails.GetAll())
                .Where(od => od.OrderId == order.Id).ToList();

            var totalAmount = orderDetails.Sum(od => od.Price * od.Quantity);

            // Get customer name if available
            string customerName = null;
            if (order.CustomerId.HasValue)
            {
                var customer = await _dao.Customers.GetById(order.CustomerId.Value);
                customerName = customer?.Name;
            }

            // Get payment method if available
            string paymentMethod = "Cash";
            if (order.PaymentMethodId.HasValue)
            {
                var payment = await _dao.PaymentMethods.GetById(order.PaymentMethodId.Value);
                paymentMethod = payment?.Name ?? "Cash";
            }

            // Get service type if available
            string serviceType = "Dine-in";
            if (order.ServiceTypeId.HasValue)
            {
                var service = await _dao.ServiceTypes.GetById(order.ServiceTypeId.Value);
                serviceType = service?.Name ?? "Dine-in";
            }

            // Get order details with product names
            var detailsWithProducts = new List<OrderDetailSummary>();
            foreach (var detail in orderDetails)
            {
                var product = await _dao.Products.GetById(detail.ProductId);
                detailsWithProducts.Add(new OrderDetailSummary
                {
                    ProductId = detail.ProductId,
                    ProductName = product?.Name ?? "Unknown Product",
                    Quantity = detail.Quantity,
                    Price = detail.Price,
                    Subtotal = detail.Price * detail.Quantity
                });
            }

            orderSummaries.Add(new OrderSummary
            {
                OrderId = order.Id,
                OrderDate = order.OrderDate,
                PaymentDate = order.PaymentDate,
                Status = order.Status,
                TotalAmount = totalAmount,
                CustomerName = customerName,
                TableId = order.TableId,
                CustomerIsMember = order.CustomerId.HasValue ?
                    (await _dao.Customers.GetById(order.CustomerId.Value))?.IsMembership ?? false : false,
                PaymentMethod = paymentMethod,
                ServiceType = serviceType,
                OrderDetails = detailsWithProducts
            });
        }

        return orderSummaries;
    }

    private async Task CalculateKPIsAsync(List<OrderSummary> currentPeriodOrders, List<OrderSummary> previousPeriodOrders)
    {
        // Calculate current period metrics
        TotalRevenue = currentPeriodOrders.Sum(o => o.TotalAmount);
        TotalOrders = currentPeriodOrders.Count;

        // Calculate ingredient costs for orders in current period
        TotalCost = await CalculateIngredientCostsAsync(currentPeriodOrders);

        // Calculate profit
        TotalProfit = TotalRevenue - TotalCost;

        // Calculate previous period metrics for comparison
        var previousRevenue = previousPeriodOrders.Sum(o => o.TotalAmount);
        var previousOrders = previousPeriodOrders.Count;
        var previousCost = await CalculateIngredientCostsAsync(previousPeriodOrders);
        var previousProfit = previousRevenue - previousCost;

        // Calculate percentage changes
        RevenueChangePercentage = CalculatePercentageChange(previousRevenue, TotalRevenue);
        OrdersChangePercentage = CalculatePercentageChange(previousOrders, TotalOrders);
        CostChangePercentage = CalculatePercentageChange(previousCost, TotalCost);
        ProfitChangePercentage = CalculatePercentageChange(previousProfit, TotalProfit);
    }

    private decimal CalculatePercentageChange(decimal previous, decimal current)
    {
        if (previous == 0)
        {
            return current > 0 ? 100 : 0;
        }

        return Math.Round(((current - previous) / Math.Abs(previous)) * 100, 2);
    }

    private async Task<decimal> CalculateIngredientCostsAsync(List<OrderSummary> orders)
    {
        decimal totalCost = 0;

        foreach (var order in orders)
        {
            foreach (var detail in order.OrderDetails)
            {
                // Get product ingredients
                var productIngredients = (await _dao.ProductIngredients.GetAll())
                    .Where(pi => pi.ProductId == detail.ProductId)
                    .ToList();

                foreach (var pi in productIngredients)
                {
                    // Get the ingredient
                    var ingredient = await _dao.Ingredients.GetById(pi.IngredientId);
                    if (ingredient == null) continue;

                    // Get the most recent ingredient transaction to determine cost
                    var transactions = (await _dao.IngredientInventoryTransactions.GetAll())
                        .Where(t => t.IngredientId == pi.IngredientId && t.TransactionType == "IMPORT")
                        .OrderByDescending(t => t.Timestamp)
                        .ToList();

                    if (!transactions.Any()) continue;

                    var unitPrice = (decimal)transactions.First().UnitPrice;
                    var quantityUsed = pi.QuantityUsed * detail.Quantity;

                    totalCost += unitPrice * quantityUsed;
                }
            }
        }

        return totalCost;
    }

    private async Task LoadCategoryRevenueAsync(List<OrderSummary> orders)
    {
        var categoryDict = new Dictionary<int, CategoryRevenue>();

        foreach (var order in orders)
        {
            foreach (var detail in order.OrderDetails)
            {
                var product = await _dao.Products.GetById(detail.ProductId);
                if (product == null || product.CategoryId == 0) continue;

                var category = await _dao.Categories.GetById(product.CategoryId);
                if (category == null) continue;

                var revenue = detail.Price * detail.Quantity;

                if (!categoryDict.ContainsKey(category.Id))
                {
                    categoryDict[category.Id] = new CategoryRevenue
                    {
                        CategoryId = category.Id,
                        CategoryName = category.Name,
                        Revenue = 0
                    };
                }

                categoryDict[category.Id].Revenue += revenue;
            }
        }

        // Calculate percentages
        var totalRevenue = categoryDict.Values.Sum(c => c.Revenue);
        foreach (var category in categoryDict.Values)
        {
            category.Percentage = (double)totalRevenue > 0
                ? (double)Math.Round((category.Revenue / totalRevenue) * 100, 2)
                : 0;
        }

        // Sort by revenue (highest first)
        var sortedCategories = categoryDict.Values
            .OrderByDescending(c => c.Revenue)
            .ToList();

        CategoryRevenues = new ObservableCollection<CategoryRevenue>(sortedCategories);
    }

    private async Task LoadTopSellingProductsAsync(List<OrderSummary> orders)
    {
        var productDict = new Dictionary<int, ProductSales>();

        foreach (var order in orders)
        {
            foreach (var detail in order.OrderDetails)
            {
                if (!productDict.ContainsKey(detail.ProductId))
                {
                    var product = await _dao.Products.GetById(detail.ProductId);
                    if (product == null) continue;

                    productDict[detail.ProductId] = new ProductSales
                    {
                        ProductId = product.Id,
                        ProductName = product.Name,
                        QuantitySold = 0,
                        Revenue = 0
                    };
                }

                productDict[detail.ProductId].QuantitySold += detail.Quantity;
                productDict[detail.ProductId].Revenue += detail.Quantity * detail.Price;
            }
        }

        // Sort by quantity sold (highest first)
        var topProducts = productDict.Values
            .OrderByDescending(p => p.QuantitySold)
            .Take(10)
            .ToList();

        TopSellingProducts = new ObservableCollection<ProductSales>(topProducts);
    }

    private async Task LoadTopCustomersAsync(List<OrderSummary> orders)
    {
        var customerOrders = orders
            .Where(o => o.CustomerName != null)
            .GroupBy(o => o.CustomerName)
            .Select(g => new CustomerSummary
            {
                CustomerName = g.Key,
                OrderCount = g.Count(),
                TotalSpent = g.Sum(o => o.TotalAmount)
            })
            .OrderByDescending(c => c.TotalSpent)
            .Take(10)
            .ToList();

        TopCustomers = new ObservableCollection<CustomerSummary>(customerOrders);
    }

    private async Task LoadInventoryStatusAsync()
    {
        var ingredients = await _dao.Ingredients.GetAll();
        var inventoryItems = new List<InventoryStatus>();

        foreach (var ingredient in ingredients)
        {
            inventoryItems.Add(new InventoryStatus
            {
                IngredientId = ingredient.Id,
                IngredientName = ingredient.Name,
                CurrentQuantity = ingredient.Quantity,
                ThresholdQuantity = ingredient.Threshold,
                Unit = ingredient.Unit,
                IsLowStock = ingredient.Quantity <= ingredient.Threshold
            });
        }

        // Sort with low stock items first, then by quantity relative to threshold
        var sortedItems = inventoryItems
            .OrderBy(i => i.IsLowStock ? 0 : 1)
            .ThenBy(i => (double)i.CurrentQuantity / Math.Max(1, i.ThresholdQuantity))
            .Take(15)
            .ToList();

        InventoryStatus = new ObservableCollection<InventoryStatus>(sortedItems);
    }

    private async Task LoadInventoryTransactionsAsync()
    {
        try
        {
            var allTransactions = await _dao.IngredientInventoryTransactions.GetAll();

            // Filter transactions by date range
            var startTimestamp = new DateTimeOffset(StartDate).ToUnixTimeMilliseconds();
            var endTimestamp = new DateTimeOffset(EndDate.AddDays(1).AddTicks(-1)).ToUnixTimeMilliseconds();

            var filteredTransactions = allTransactions
                .Where(t => t.Timestamp >= startTimestamp && t.Timestamp <= endTimestamp)
                .OrderBy(t => t.Timestamp)
                .ToList();

            var transactionsList = new List<InventoryTransaction>();

            foreach (var transaction in filteredTransactions)
            {
                var ingredient = await _dao.Ingredients.GetById(transaction.IngredientId);
                var ingredientName = ingredient?.Name ?? "Unknown";

                var dateTime = DateTimeOffset.FromUnixTimeMilliseconds(transaction.Timestamp).DateTime;

                transactionsList.Add(new InventoryTransaction
                {
                    TransactionId = transaction.Id,
                    IngredientId = transaction.IngredientId,
                    IngredientName = ingredientName,
                    Date = dateTime,
                    Quantity = transaction.Quantity,
                    Unit = transaction.Unit,
                    TransactionType = transaction.TransactionType,
                    UnitPrice = (decimal)transaction.UnitPrice,
                    TotalCost = (decimal)transaction.UnitPrice * transaction.Quantity
                });
            }

            InventoryTransactions = new ObservableCollection<InventoryTransaction>(transactionsList);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading inventory transactions: {ex.Message}");
        }
    }

    private async Task LoadRevenueTimeSeriesAsync(List<OrderSummary> orders)
    {
        var timeSeriesData = new List<RevenueTimeSeries>();

        // Determine appropriate grouping based on date range
        var dateDiff = (EndDate - StartDate).TotalDays;

        if (dateDiff <= 7)
        {
            // Group by hour for very short ranges
            var hourlyData = orders
                .GroupBy(o => new DateTime(o.OrderDate.Year, o.OrderDate.Month, o.OrderDate.Day, o.OrderDate.Hour, 0, 0))
                .Select(g => new RevenueTimeSeries
                {
                    Date = g.Key,
                    Revenue = g.Sum(o => o.TotalAmount)
                })
                .OrderBy(d => d.Date)
                .ToList();

            // Fill in missing hours
            var currentHour = new DateTime(StartDate.Year, StartDate.Month, StartDate.Day, 0, 0, 0);
            var endHour = new DateTime(EndDate.Year, EndDate.Month, EndDate.Day, 23, 0, 0);

            while (currentHour <= endHour)
            {
                if (!hourlyData.Any(d => d.Date == currentHour))
                {
                    timeSeriesData.Add(new RevenueTimeSeries
                    {
                        Date = currentHour,
                        Revenue = 0
                    });
                }
                else
                {
                    timeSeriesData.Add(hourlyData.First(d => d.Date == currentHour));
                }

                currentHour = currentHour.AddHours(1);
            }
        }
        else if (dateDiff <= 31)
        {
            // Group by day for short ranges
            var dailyData = orders
                .GroupBy(o => o.OrderDate.Date)
                .Select(g => new RevenueTimeSeries
                {
                    Date = g.Key,
                    Revenue = g.Sum(o => o.TotalAmount)
                })
                .OrderBy(d => d.Date)
                .ToList();

            // Fill in missing days
            var currentDay = StartDate.Date;

            while (currentDay <= EndDate.Date)
            {
                if (!dailyData.Any(d => d.Date.Date == currentDay))
                {
                    timeSeriesData.Add(new RevenueTimeSeries
                    {
                        Date = currentDay,
                        Revenue = 0
                    });
                }
                else
                {
                    timeSeriesData.Add(dailyData.First(d => d.Date.Date == currentDay));
                }

                currentDay = currentDay.AddDays(1);
            }
        }
        else if (dateDiff <= 90)
        {
            // Group by week for medium ranges
            var weeklyData = orders
                .GroupBy(o => CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(
                    o.OrderDate,
                    CalendarWeekRule.FirstDay,
                    DayOfWeek.Monday))
                .Select(g => new
                {
                    Week = g.Key,
                    // Using the first day of the week for display
                    FirstDay = g.Min(o => o.OrderDate),
                    Revenue = g.Sum(o => o.TotalAmount)
                })
                .OrderBy(d => d.Week)
                .ToList();

            foreach (var week in weeklyData)
            {
                timeSeriesData.Add(new RevenueTimeSeries
                {
                    Date = week.FirstDay,
                    Revenue = week.Revenue
                });
            }
        }
        else
        {
            // Group by month for long ranges
            var monthlyData = orders
                .GroupBy(o => new DateTime(o.OrderDate.Year, o.OrderDate.Month, 1))
                .Select(g => new RevenueTimeSeries
                {
                    Date = g.Key,
                    Revenue = g.Sum(o => o.TotalAmount)
                })
                .OrderBy(d => d.Date)
                .ToList();

            // Fill in missing months
            var currentMonth = new DateTime(StartDate.Year, StartDate.Month, 1);
            var endMonth = new DateTime(EndDate.Year, EndDate.Month, 1);

            while (currentMonth <= endMonth)
            {
                if (!monthlyData.Any(d => d.Date.Year == currentMonth.Year && d.Date.Month == currentMonth.Month))
                {
                    timeSeriesData.Add(new RevenueTimeSeries
                    {
                        Date = currentMonth,
                        Revenue = 0
                    });
                }
                else
                {
                    timeSeriesData.Add(monthlyData.First(d =>
                        d.Date.Year == currentMonth.Year && d.Date.Month == currentMonth.Month));
                }

                currentMonth = currentMonth.AddMonths(1);
            }
        }

        // Sort by date
        timeSeriesData = timeSeriesData.OrderBy(d => d.Date).ToList();

        RevenueTimeSeries = new ObservableCollection<RevenueTimeSeries>(timeSeriesData);
    }

    private async Task CalculateMembershipStatsAsync(List<OrderSummary> orders)
    {
        MembershipRevenue = orders
            .Where(o => o.CustomerIsMember)
            .Sum(o => o.TotalAmount);

        RegularCustomerRevenue = orders
            .Where(o => !o.CustomerIsMember)
            .Sum(o => o.TotalAmount);

        var totalRevenue = MembershipRevenue + RegularCustomerRevenue;

        if (totalRevenue > 0)
        {
            MembershipPercentage = Math.Round((double)(MembershipRevenue / totalRevenue) * 100, 2);
            RegularCustomerPercentage = Math.Round((double)(RegularCustomerRevenue / totalRevenue) * 100, 2);
        }
        else
        {
            MembershipPercentage = 0;
            RegularCustomerPercentage = 0;
        }
    }

    private void GenerateBusinessInsights(List<OrderSummary> orders)
    {
        if (!orders.Any())
        {
            HighestRevenueDate = "No data available";
            AverageDailyRevenue = 0;
            TopPerformingCategory = "No data available";
            TopSellingProductName = "No data available";
            LowStockWarning = "No data available";
            ProfitMargin = 0;
            MostValuableCustomer = "No data available";
            return;
        }

        // Highest revenue date
        var dailyRevenue = orders
            .GroupBy(o => o.OrderDate.Date)
            .Select(g => new { Date = g.Key, Revenue = g.Sum(o => o.TotalAmount) })
            .OrderByDescending(d => d.Revenue)
            .FirstOrDefault();

        HighestRevenueDate = dailyRevenue != null
            ? $"{dailyRevenue.Date.ToShortDateString()} (${dailyRevenue.Revenue:N2})"
            : "No data available";

        // Average daily revenue
        var uniqueDays = orders.Select(o => o.OrderDate.Date).Distinct().Count();
        AverageDailyRevenue = uniqueDays > 0
            ? Math.Round(TotalRevenue / uniqueDays, 2)
            : 0;

        // Top performing category
        var topCategory = CategoryRevenues.OrderByDescending(c => c.Revenue).FirstOrDefault();
        TopPerformingCategory = topCategory != null
            ? $"{topCategory.CategoryName} ({topCategory.Percentage}%)"
            : "No data available";

        // Top selling product
        var topProduct = TopSellingProducts.OrderByDescending(p => p.QuantitySold).FirstOrDefault();
        TopSellingProductName = topProduct != null
            ? $"{topProduct.ProductName} ({topProduct.QuantitySold} units, ${topProduct.Revenue:N2})"
            : "No data available";

        // Low stock warnings
        var lowStockItems = InventoryStatus.Where(i => i.IsLowStock).ToList();
        if (lowStockItems.Any())
        {
            LowStockWarning = $"{lowStockItems.Count} ingredients below threshold: {string.Join(", ", lowStockItems.Take(3).Select(i => i.IngredientName))}";
            if (lowStockItems.Count > 3)
            {
                LowStockWarning += $" and {lowStockItems.Count - 3} more";
            }
        }
        else
        {
            LowStockWarning = "All ingredients are well-stocked";
        }

        // Overall profit margin
        ProfitMargin = TotalRevenue > 0
            ? Math.Round((TotalProfit / TotalRevenue) * 100, 2)
            : 0;

        // Most valuable customer
        var topCustomer = TopCustomers.OrderByDescending(c => c.TotalSpent).FirstOrDefault();
        MostValuableCustomer = topCustomer != null
            ? $"{topCustomer.CustomerName} ({topCustomer.OrderCount} orders, ${topCustomer.TotalSpent:N2})"
            : "No data available";
    }
}

// Helper classes for data display
public class OrderSummary
{
    public int OrderId
    {
        get; set;
    }
    public DateTime OrderDate
    {
        get; set;
    }
    public DateTime? PaymentDate
    {
        get; set;
    }
    public string Status
    {
        get; set;
    }
    public decimal TotalAmount
    {
        get; set;
    }
    public string? CustomerName
    {
        get; set;
    }
    public int? TableId
    {
        get; set;
    }
    public bool CustomerIsMember
    {
        get; set;
    }
    public string PaymentMethod
    {
        get; set;
    }
    public string ServiceType
    {
        get; set;
    }
    public List<OrderDetailSummary> OrderDetails { get; set; } = new();
}

public class OrderDetailSummary
{
    public int ProductId
    {
        get; set;
    }
    public string ProductName
    {
        get; set;
    }
    public int Quantity
    {
        get; set;
    }
    public decimal Price
    {
        get; set;
    }
    public decimal Subtotal
    {
        get; set;
    }
}

public class CategoryRevenue
{
    public int CategoryId
    {
        get; set;
    }
    public string CategoryName
    {
        get; set;
    }
    public decimal Revenue
    {
        get; set;
    }
    public double Percentage
    {
        get; set;
    }
}

public class ProductSales
{
    public int ProductId
    {
        get; set;
    }
    public string ProductName
    {
        get; set;
    }
    public int QuantitySold
    {
        get; set;
    }
    public decimal Revenue
    {
        get; set;
    }
}

public class CustomerSummary
{
    public string CustomerName
    {
        get; set;
    }
    public int OrderCount
    {
        get; set;
    }
    public decimal TotalSpent
    {
        get; set;
    }
}

public class InventoryStatus
{
    public int IngredientId
    {
        get; set;
    }
    public string IngredientName
    {
        get; set;
    }
    public int CurrentQuantity
    {
        get; set;
    }
    public int ThresholdQuantity
    {
        get; set;
    }
    public string Unit
    {
        get; set;
    }
    public bool IsLowStock
    {
        get; set;
    }
}

public class InventoryTransaction
{
    public int TransactionId
    {
        get; set;
    }
    public int IngredientId
    {
        get; set;
    }
    public string IngredientName
    {
        get; set;
    }
    public DateTime Date
    {
        get; set;
    }
    public int Quantity
    {
        get; set;
    }
    public string Unit
    {
        get; set;
    }
    public string TransactionType
    {
        get; set;
    }
    public decimal UnitPrice
    {
        get; set;
    }
    public decimal TotalCost
    {
        get; set;
    }
}

public class RevenueTimeSeries
{
    public DateTime Date
    {
        get; set;
    }
    public decimal Revenue
    {
        get; set;
    }
}