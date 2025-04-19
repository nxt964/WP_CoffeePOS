using CommunityToolkit.Mvvm.ComponentModel;
using System;

namespace CoffeePOS.Models;

public partial class OrderDisplay : ObservableObject
{
    [ObservableProperty]
    private int id;

    [ObservableProperty]
    private DateTime orderDate;

    [ObservableProperty]
    private string customerName;

    [ObservableProperty]
    private decimal totalAmount;

    [ObservableProperty]
    private string serviceTypeName;

    [ObservableProperty]
    private string status;
}