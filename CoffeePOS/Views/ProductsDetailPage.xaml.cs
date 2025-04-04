﻿using System.Diagnostics;
using CoffeePOS.Contracts.Services;
using CoffeePOS.Core.Models;
using CoffeePOS.ViewModels;

using CommunityToolkit.WinUI.UI.Animations;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace CoffeePOS.Views;

public sealed partial class ProductsDetailPage : Page
{
    public ProductsDetailViewModel ViewModel
    {
        get;
    }

    public ProductsDetailPage()
    {
        ViewModel = App.GetService<ProductsDetailViewModel>();
        InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
    }

    protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
    {
        base.OnNavigatingFrom(e);
        if (e.NavigationMode == NavigationMode.Back)
        {
            var navigationService = App.GetService<INavigationService>();

            if (ViewModel.Item != null)
            {
                navigationService.SetListDataItemForNextConnectedAnimation(ViewModel.Item);
            }
        }
    }

    private void OnProductClicked(object sender, RoutedEventArgs e)
    {
        if (sender is Button button)
        {
            if (button.Tag is int productId)
            {
                ViewModel.OnProductClicked(productId);
            }
        }
    }
}
