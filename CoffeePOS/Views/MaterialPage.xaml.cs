﻿using CoffeePOS.ViewModels;

using Microsoft.UI.Xaml.Controls;

namespace CoffeePOS.Views;

// TODO: Change the grid as appropriate for your app. Adjust the column definitions on DataGridPage.xaml.
// For more details, see the documentation at https://docs.microsoft.com/windows/communitytoolkit/controls/datagrid.
public sealed partial class MaterialPage : Page
{
    public MaterialViewModel ViewModel
    {
        get;
    }

    public MaterialPage()
    {
        ViewModel = App.GetService<MaterialViewModel>();
        InitializeComponent();
    }
}
