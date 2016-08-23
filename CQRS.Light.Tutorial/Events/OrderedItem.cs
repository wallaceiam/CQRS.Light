using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CQRS.Light.Tutorial
{
    public class OrderedItem
    {
        public int MenuNumber { get; set; }
        public string Description { get; set; }
        public bool IsDrink { get; set; }
        public decimal Price { get; set; }
    }
}
