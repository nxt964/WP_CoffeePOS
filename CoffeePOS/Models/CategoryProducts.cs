using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoffeePOS.Core.Models;

namespace CoffeePOS.Models;
public class CategoryProducts : Category
{
    public List<CoffeePOS.Core.Models.Product> Products { get; set; } = new List<CoffeePOS.Core.Models.Product>();
}
