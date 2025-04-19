using CoffeePOS.Activation;
using CoffeePOS.Contracts.Services;
using CoffeePOS.Core.Contracts.Services;
using CoffeePOS.Core.Daos;
using CoffeePOS.Core.Data;
using CoffeePOS.Core.Interfaces;
using CoffeePOS.Core.Services;
using CoffeePOS.Helpers;
using CoffeePOS.Models;
using CoffeePOS.Notifications;
using CoffeePOS.Services;
using CoffeePOS.ViewModels;
using CoffeePOS.Views;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.UI.Xaml;

namespace CoffeePOS;

// To learn more about WinUI 3, see https://docs.microsoft.com/windows/apps/winui/winui3/.
public partial class App : Application
{
    // The .NET Generic Host provides dependency injection, configuration, logging, and other services.
    // https://docs.microsoft.com/dotnet/core/extensions/generic-host
    // https://docs.microsoft.com/dotnet/core/extensions/dependency-injection
    // https://docs.microsoft.com/dotnet/core/extensions/configuration
    // https://docs.microsoft.com/dotnet/core/extensions/logging
    public IHost Host
    {
        get;
    }

    public static T GetService<T>()
        where T : class
    {
        if ((App.Current as App)!.Host.Services.GetService(typeof(T)) is not T service)
        {
            throw new ArgumentException($"{typeof(T)} needs to be registered in ConfigureServices within App.xaml.cs.");
        }

        return service; 
    }

    public static WindowEx MainWindow { get; } = new MainWindow();

    public static UIElement? AppTitlebar { get; set; }

    public App()
    {
        InitializeComponent();

        Host = Microsoft.Extensions.Hosting.Host.
        CreateDefaultBuilder().
        UseContentRoot(AppContext.BaseDirectory).
        ConfigureServices((context, services) =>
        {
            // Default Activation Handler
            services.AddTransient<ActivationHandler<LaunchActivatedEventArgs>, DefaultActivationHandler>();

            // Other Activation Handlers
            services.AddTransient<IActivationHandler, AppNotificationActivationHandler>();

            // Services
            services.AddSingleton<IAppNotificationService, AppNotificationService>();
            services.AddSingleton<ILocalSettingsService, LocalSettingsService>();
            services.AddSingleton<IThemeSelectorService, ThemeSelectorService>();
            services.AddTransient<INavigationViewService, NavigationViewService>();

            services.AddSingleton<IActivationService, ActivationService>();
            services.AddSingleton<IPageService, PageService>();
            services.AddSingleton<INavigationService, NavigationService>();


            services.AddSingleton<ContentDialogHelper>();
            services.AddSingleton<CloudinaryService>();
            services.AddSingleton<SqliteConnectionFactory>();
            // Core Dao
            //services.AddSingleton<IDao, MockDao>();
            services.AddSingleton<IDao, SqliteManualDao>();

            // Core Services
            services.AddSingleton<ISampleDataService, SampleDataService>();
            services.AddSingleton<IFileService, FileService>();

            // Views and ViewModels
            services.AddTransient<SettingsViewModel>();
            services.AddTransient<SettingsPage>();
            services.AddTransient<StatisticsViewModel>();
            services.AddTransient<StatisticsPage>();
            services.AddTransient<TableDetailViewModel>();
            services.AddTransient<TableDetailPage>();
            services.AddTransient<TableViewModel>();
            services.AddTransient<TablePage>();
            services.AddTransient<MaterialViewModel>();
            services.AddTransient<MaterialPage>();
            services.AddTransient<InventoryViewModel>();
            services.AddTransient<InventoryPage>();
            services.AddTransient<CustomersViewModel>();
            services.AddTransient<CustomersPage>();
            services.AddTransient<ProductsDetailViewModel>();
            services.AddTransient<ProductsDetailPage>();
            services.AddTransient<ProductsViewModel>();
            services.AddTransient<ProductsPage>();
            services.AddTransient<CategoriesViewModel>();
            services.AddTransient<CategoriesPage>();
            services.AddTransient<DashboardViewModel>();
            services.AddTransient<DashboardPage>();
            services.AddTransient<ShellPage>();
            services.AddTransient<ShellViewModel>();
            services.AddTransient<ProductViewModel>();
            services.AddTransient<ProductPage>();
            services.AddTransient<AddProductViewModel>();
            services.AddTransient<AddProductPage>();
            services.AddTransient<UpdateProductViewModel>();
            services.AddTransient<UpdateProductPage>();
            services.AddTransient<OrderPage>();
            services.AddTransient<OrderViewModel>();
            services.AddTransient<AddOrderPage>();
            services.AddTransient<AddOrderViewModel>();
            services.AddTransient<DetailOrderPage>();
            services.AddTransient<DetailOrderViewModel>();
            services.AddTransient<AddProductToOrderDetailPage>();
            services.AddTransient<AddProductToOrderDetailViewModel>();
            services.AddTransient<LoginViewModel>();
            services.AddTransient<LoginPage>();
            services.AddTransient<EmployeesPage>();
            services.AddTransient<EmployeesViewModel>();
            services.AddTransient<AddEmployeeViewModel>();
            services.AddTransient<AddEmployeePage>();
            services.AddTransient<EditEmployeePage>();
            services.AddTransient<EditEmployeeViewModel>();
            services.AddTransient<AddCustomerPage>();
            services.AddTransient<AddCustomerViewModel>();
            services.AddTransient<EditCustomerPage>();
            services.AddTransient<EditCustomerViewModel>();





            // Configuration
            services.Configure<LocalSettingsOptions>(context.Configuration.GetSection(nameof(LocalSettingsOptions)));
            services.Configure<CloudinarySettings>(context.Configuration.GetSection("Cloudinary"));
        }).
        Build();

        App.GetService<IAppNotificationService>().Initialize();

        UnhandledException += App_UnhandledException;
    }

    private void App_UnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
    {
        // TODO: Log and handle exceptions as appropriate.
        // https://docs.microsoft.com/windows/windows-app-sdk/api/winrt/microsoft.ui.xaml.application.unhandledexception.
    }

    protected async override void OnLaunched(LaunchActivatedEventArgs args)
    {
        base.OnLaunched(args);

        // Khởi tạo database
        using (var scope = Host.Services.CreateScope())
        {
            var dao = scope.ServiceProvider.GetRequiredService<IDao>();
            if (dao is SqliteManualDao sqliteManualDao)
            {
                await sqliteManualDao.InitializeDatabaseAsync();
            }
        }

        App.GetService<IAppNotificationService>().Show(string.Format("AppNotificationSamplePayload".GetLocalized(), AppContext.BaseDirectory));

        await App.GetService<IActivationService>().ActivateAsync(args);
    }
}
