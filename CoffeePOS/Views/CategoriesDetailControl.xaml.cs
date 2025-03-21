﻿using CoffeePOS.Core.Models;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace CoffeePOS.Views;

public sealed partial class CategoriesDetailControl : UserControl
{
    public SampleOrder? ListDetailsMenuItem
    {
        get => GetValue(ListDetailsMenuItemProperty) as SampleOrder;
        set => SetValue(ListDetailsMenuItemProperty, value);
    }

    public static readonly DependencyProperty ListDetailsMenuItemProperty = DependencyProperty.Register("ListDetailsMenuItem", typeof(SampleOrder), typeof(CategoriesDetailControl), new PropertyMetadata(null, OnListDetailsMenuItemPropertyChanged));

    public CategoriesDetailControl()
    {
        InitializeComponent();
    }

    private static void OnListDetailsMenuItemPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is CategoriesDetailControl control)
        {
            control.ForegroundElement.ChangeView(0, 0, 1);
        }
    }
}
