using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CQRS.Light.Tutorial.Events
{
    public class FoodOrdered
    {
        public Guid TabId { get; protected set; }
        public List<OrderedItem> Items { get; protected set; }

        public FoodOrdered(Guid tabId, List<OrderedItem> items)
        {
            this.TabId = tabId;
            this.Items = items;
        }
    }
}
