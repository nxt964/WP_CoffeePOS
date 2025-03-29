using System.Diagnostics;
using System.Numerics;
using CoffeePOS.Contracts.Services;
using CoffeePOS.Contracts.ViewModels;
using CoffeePOS.Core.Models;
using CoffeePOS.Models;
using CoffeePOS.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Hosting;

namespace CoffeePOS.Views;

public sealed partial class CategoriesDetailControl : UserControl
{
    private readonly INavigationService _navigationService = App.GetService<INavigationService>();
    public CategoryProducts? CategoryWithProducts
    {
        get => GetValue(CategoryWithProductsProperty) as CategoryProducts;
        set => SetValue(CategoryWithProductsProperty, value);
    }

    public static readonly DependencyProperty CategoryWithProductsProperty = DependencyProperty.Register("CategoryWithProducts", typeof(CategoryProducts), typeof(CategoriesDetailControl), new PropertyMetadata(null, OnCategoryWithProductsPropertyChanged));

    public CategoriesDetailControl()
    {
        InitializeComponent();

        var visual = ElementCompositionPreview.GetElementVisual(this);
        visual.Offset = new Vector3(0, 100, 0); 
        visual.Opacity = 0;

    }

    private static void OnCategoryWithProductsPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is CategoriesDetailControl control)
        {
            control.StartEnterAnimation();
        }
    }

    private void StartEnterAnimation()
    {
        var compositor = ElementCompositionPreview.GetElementVisual(this).Compositor;
        var visual = ElementCompositionPreview.GetElementVisual(this);

        var offsetAnimation = compositor.CreateVector3KeyFrameAnimation();
        offsetAnimation.InsertKeyFrame(0.0f, new Vector3(0, 100, 0));
        offsetAnimation.InsertKeyFrame(1.0f, new Vector3(0, 0, 0));
        offsetAnimation.Duration = TimeSpan.FromMilliseconds(500);

        var fadeAnimation = compositor.CreateScalarKeyFrameAnimation();
        fadeAnimation.InsertKeyFrame(0.0f, 0f);
        fadeAnimation.InsertKeyFrame(1.0f, 1f);
        fadeAnimation.Duration = TimeSpan.FromMilliseconds(500);

        visual.StartAnimation("Offset", offsetAnimation);
        visual.StartAnimation("Opacity", fadeAnimation);
    }

    private void OnProductClicked(object sender, RoutedEventArgs e)
    {
        if (sender is Button button)
        {
            if (button.Tag is int productId)
            {
                var parameter = (productId, true);
                _navigationService.NavigateTo(typeof(ProductsDetailViewModel).FullName, parameter);
            }
        }
    }

}
