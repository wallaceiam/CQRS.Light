using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CQRS.Light.Core;
using System.Threading.Tasks;
using FluentAssertions;
using System.Linq;
using Moq;
using CQRS.Light.Contracts;

namespace CQRS.Light.Core.Tests
{
    [TestClass]
    public class EventHandlersDatabaseTests
    {
        [TestInitialize]
        public void TestInitialize()
        {
            EventHandlersDatabase<string>.Instance.Clear();
        }

        [TestMethod]
        public void EventHandlersDatabase_ShouldBeOnlyOneInstance()
        {
            var a = EventHandlersDatabase<string>.Instance;
            var b = EventHandlersDatabase<string>.Instance;

            a.Should().Be(b);
        }

        [TestMethod]
        public void EventHandlersDatabase_FuncAddShouldPersist()
        {
            Func<string, Task> foo = (x) => Task.FromResult<string>(x);

            EventHandlersDatabase<string>.Instance.Get().Count().Should().Be(0);

            EventHandlersDatabase<string>.Instance.Add(foo);

            EventHandlersDatabase<string>.Instance.Get().Count().Should().Be(1);
            EventHandlersDatabase<string>.Instance.Get().Where(x => x == foo).Count().Should().Be(1);
        }

        [TestMethod]
        public void EventHandlersDatabase_HandlerAddShouldPersist()
        {
            var handler = new Mock<IEventHandler<string>>();
            handler.Setup(x => x.HandleAsync(It.IsAny<string>())).Returns(Task.FromResult<string>(null));

            EventHandlersDatabase<string>.Instance.Get().Count().Should().Be(0);

            EventHandlersDatabase<string>.Instance.Add(handler.Object);

            EventHandlersDatabase<string>.Instance.Get().Count().Should().Be(1);
            EventHandlersDatabase<string>.Instance.Get().Where(x => x == handler.Object.HandleAsync).Count().Should().Be(1);
        }

        [TestMethod]
        public void EventHandlersDatabase_ClearShouldRemoveAllFuncs()
        {
            Func<string, Task> foo = (x) => Task.FromResult<string>(x);

            EventHandlersDatabase<string>.Instance.Get().Count().Should().Be(0);
            EventHandlersDatabase<string>.Instance.Add(foo);
            EventHandlersDatabase<string>.Instance.Get().Count().Should().Be(1);
            EventHandlersDatabase<string>.Instance.Clear();
            EventHandlersDatabase<string>.Instance.Get().Count().Should().Be(0);
        }
    }
}
