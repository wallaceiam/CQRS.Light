using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CQRS.Light.Core;
using FluentAssertions;
using System.Linq;
using System.Threading.Tasks;
using CQRS.Light.Contracts;
using Moq;

namespace DDD.Light.Core.Tests
{
    [TestClass]
    public class EventBusTests
    {
        [TestInitialize]
        public void TestInitialize()
        {
            EventBus.Instance.Reset();
            EventHandlersDatabase<string>.Instance.Clear();
        }

        [TestMethod]
        public void EventBus_ShouldBeOnlyOneInstance()
        {
            var a = EventBus.Instance;
            var b = EventBus.Instance;

            a.Should().Be(b);
        }

        [TestMethod]
        public void EventBus_FuncSubscribeShouldAddToEventHandlersDatabase()
        {
            Func<string, Task> foo = (x) => Task.FromResult<string>(x);

            EventHandlersDatabase<string>.Instance.Get().Count().Should().Be(0);

            EventBus.Instance.Subscribe<string>(foo);

            EventHandlersDatabase<string>.Instance.Get().Count().Should().Be(1);
            EventHandlersDatabase<string>.Instance.Get().Where(x => x == foo).Count().Should().Be(1);
        }

        [TestMethod]
        public void EventBus_HandlerSubscribeShouldAddToEventHandlersDatabase()
        {
            var EventHandler = new Mock<IEventHandler<string>>();
            EventHandler.Setup(x => x.HandleAsync(It.IsAny<string>())).Returns(Task.FromResult<string>(null));

            EventHandlersDatabase<string>.Instance.Get().Count().Should().Be(0);

            EventBus.Instance.Subscribe<string>(EventHandler.Object);

            EventHandlersDatabase<string>.Instance.Get().Count().Should().Be(1);
            EventHandlersDatabase<string>.Instance.Get().Where(x => x == EventHandler.Object.HandleAsync).Count().Should().Be(1);
        }

        [TestMethod]
        public void EventBus_ImplicitFuncSubscribeShouldAddToEventHandlersDatabase()
        {
            Func<string, Task> foo = (x) => Task.FromResult<string>(x);

            EventHandlersDatabase<string>.Instance.Get().Count().Should().Be(0);

            EventBus.Instance.Subscribe(foo);

            EventHandlersDatabase<string>.Instance.Get().Count().Should().Be(1);
            EventHandlersDatabase<string>.Instance.Get().Where(x => x == foo).Count().Should().Be(1);
        }

        [TestMethod]
        public void EventBus_ImplicitHandlerSubscribeShouldAddToEventHandlersDatabase()
        {
            var EventHandler = new Mock<IEventHandler<string>>();
            EventHandler.Setup(x => x.HandleAsync(It.IsAny<string>())).Returns(Task.FromResult<string>(null));

            EventHandlersDatabase<string>.Instance.Get().Count().Should().Be(0);

            EventBus.Instance.Subscribe(EventHandler.Object);

            EventHandlersDatabase<string>.Instance.Get().Count().Should().Be(1);
            EventHandlersDatabase<string>.Instance.Get().Where(x => x == EventHandler.Object.HandleAsync).Count().Should().Be(1);
        }

        [TestMethod]
        public void EventBus_ConfigureShouldAssignValues()
        {
            var eventStore = new Mock<IEventStore>();
            var eventSerializationStrat = new Mock<IEventSerializationStrategy>();

            EventBus.Instance.Configure(eventStore.Object, eventSerializationStrat.Object, true);

            EventBus.Instance.GetEventStore().Should().Be(eventStore.Object);
        }

        [TestMethod]
        public void EventBus_ConfigureShouldNotAllowNulls()
        {

            EventBus.Instance.Invoking(x => x.Configure(null, null, false)).ShouldThrow<ArgumentNullException>();

        }
    }
}
