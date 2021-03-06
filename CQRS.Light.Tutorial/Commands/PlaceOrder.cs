﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CQRS.Light.Tutorial.Commands
{
    public class PlaceOrder
    {
        public Guid TabId { get; protected set; }
        public List<OrderedItem> Items { get; protected set; }

        public PlaceOrder(Guid tabId, List<OrderedItem> items)
        {
            this.TabId = tabId;
            this.Items = items;
        }
    }
}
