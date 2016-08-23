using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CQRS.Light.Testing.MSTest;
using CQRS.Light.Tutorial.Commands;
using CQRS.Light.Tutorial.Events;
using System.Collections.Generic;

namespace CQRS.Light.Tutorial.Tests
{
    [TestClass]
    public class TabTests : BDDTest<TabAggregate>
    {
        [TestMethod]
        public void CanOpenANewTab()
        {
            var testId = Guid.NewGuid();
            var testTable = 42;
            var testWaiter = "Derek";

            Given()
            .When(x => x.OpenTab(testId, testTable, testWaiter))
            .Then(new TabOpened(testId, testTable, testWaiter));
        }

        [TestMethod]
        public void CanNotOrderWithUnopenedTab()
        {
            var testDrink = new OrderedItem() { Description = "Test Drink", IsDrink = true, MenuNumber = 4, Price = 5.0M };
            Given()
            .When(x => x.PlaceOrder(new List<OrderedItem> { testDrink }))
            .ThenFailWith<TabNotOpen>();
        }


        [TestMethod]
        public void CanPlaceDrinksOrder()
        {
            var testId = Guid.NewGuid();
            var testTable = 42;
            var testWaiter = "Derek";
            var testDrink1 = new OrderedItem() { Description = "Test Drink 1", IsDrink = true, MenuNumber = 4, Price = 5.00M };
            var testDrink2 = new OrderedItem() { Description = "Test Drink 2", IsDrink = true, MenuNumber = 5, Price = 5.95M };

            Given(new TabOpened(testId, testTable, testWaiter))
            .When(x => x.PlaceOrder(new List<OrderedItem> { testDrink1, testDrink2 }))
            .Then(new DrinksOrdered(testId, new List<OrderedItem> { testDrink1, testDrink2 }));
        }

        [TestMethod]
        public void CanPlaceFoodOrder()
        {
            var testId = Guid.NewGuid();
            var testTable = 42;
            var testWaiter = "Derek";
            var testFood1 = new OrderedItem() { Description = "Test Food 1", IsDrink = false, MenuNumber = 8, Price = 10.95M };
            var testFood2 = new OrderedItem() { Description = "Test Food 2", IsDrink = false, MenuNumber = 9, Price = 15.95M };

            Given(new TabOpened(testId, testTable, testWaiter))
            .When(x => x.PlaceOrder(new List<OrderedItem>() { testFood1, testFood2 }))
            .Then(new FoodOrdered(testId, new List<OrderedItem>() { testFood1, testFood2 }));
        }

        [TestMethod]
        public void CanPlaceFoodAndDrinkOrder()
        {
            var testId = Guid.NewGuid();
            var testTable = 42;
            var testWaiter = "Derek";
            var testFood1 = new OrderedItem() { Description = "Test Food 1", IsDrink = false, MenuNumber = 8, Price = 10.95M };
            var testDrink2 = new OrderedItem() { Description = "Test Drink 2", IsDrink = true, MenuNumber = 5, Price = 5.95M };

            Given(new TabOpened(testId, testTable, testWaiter))
            .When(x => x.PlaceOrder(new List<OrderedItem> { testFood1, testDrink2 }))
            .Then(new DrinksOrdered(testId, new List<OrderedItem>() { testDrink2 }))
            .AndThen(new FoodOrdered(testId, new List<OrderedItem>() { testFood1 }));
        }
    }
}
