using CQRS.Light.Contracts;
using CQRS.Light.Tutorial.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CQRS.Light.Tutorial
{
    public class TabAggregate : Aggregate
    {
        private bool isOpen = false;

        public TabAggregate(IEventBus eventBus)
            :base(eventBus)
        {
            this.AllItemsOrdered = new List<OrderedItem>();
        }

        public int TableNumber { get; private set; }
        public string Waiter { get; private set; }
        public List<OrderedItem> AllItemsOrdered { get; private set; }


        public async Task OpenTab(Guid id, int tableNumber, string waiter)
        {
            await this.PublishAndApplyEventAsync<TabOpened>(
                new TabOpened(id, tableNumber, waiter));
        }

        public async Task PlaceOrder(List<OrderedItem> items)
        {
            if (!this.isOpen)
                throw new TabNotOpen();

            var drinks = items.Where(i => i.IsDrink).ToList();
            if (drinks.Any())
                await this.PublishAndApplyEventAsync(new DrinksOrdered(this.Id, drinks));

            var food = items.Where(i => !i.IsDrink).ToList();
            if (food.Any())
                await this.PublishAndApplyEventAsync(new FoodOrdered(this.Id, food));
        }

        private void ApplyEvent(TabOpened @event)
        {
            this.Id = @event.TabId;
            this.TableNumber = @event.TableNumber;
            this.Waiter = @event.Waiter;
            this.isOpen = true;
        }

        private void ApplyEvent(DrinksOrdered @event)
        {
            this.AllItemsOrdered.AddRange(@event.Items);
        }

        private void ApplyEvent(FoodOrdered @event)
        {
            this.AllItemsOrdered.AddRange(@event.Items);
        }
    }
}
