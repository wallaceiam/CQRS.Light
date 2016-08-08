using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CQRS.Light.Core;
using System.Threading.Tasks;
using FluentAssertions;
using System.Linq;
using Moq;
using CQRS.Light.Contracts;

namespace DDD.Light.Core.Tests
{
    [TestClass]
    public class CommandHandlersDatabaseTests
    {
        [TestInitialize]
        public void TestInitialize()
        {
            CommandHandlersDatabase<string>.Instance.Clear();
        }

        [TestMethod]
        public void CommandHandlersDatabase_ShouldBeOnlyOneInstance()
        {
            var a = CommandHandlersDatabase<string>.Instance;
            var b = CommandHandlersDatabase<string>.Instance;

            a.Should().Be(b);
        }

        [TestMethod]
        public void CommandHandlersDatabase_FuncAddShouldPersist()
        {
            Func<string, Task> foo = (x) => Task.FromResult<string>(x);

            CommandHandlersDatabase<string>.Instance.Get().Count().Should().Be(0);

            CommandHandlersDatabase<string>.Instance.Add(foo);

            CommandHandlersDatabase<string>.Instance.Get().Count().Should().Be(1);
            CommandHandlersDatabase<string>.Instance.Get().Where(x => x == foo).Count().Should().Be(1);
        }

        [TestMethod]
        public void CommandHandlersDatabase_HandlerAddShouldPersist()
        {
            var handler = new Mock<ICommandHandler<string>>();
            handler.Setup(x => x.HandleAsync(It.IsAny<string>())).Returns(Task.FromResult<string>(null));

            CommandHandlersDatabase<string>.Instance.Get().Count().Should().Be(0);

            CommandHandlersDatabase<string>.Instance.Add(handler.Object);

            CommandHandlersDatabase<string>.Instance.Get().Count().Should().Be(1);
            CommandHandlersDatabase<string>.Instance.Get().Where(x => x == handler.Object.HandleAsync).Count().Should().Be(1);
        }

        [TestMethod]
        public void CommandHandlersDatabase_ClearShouldRemoveAllFuncs()
        {
            Func<string, Task> foo = (x) => Task.FromResult<string>(x);

            CommandHandlersDatabase<string>.Instance.Get().Count().Should().Be(0);
            CommandHandlersDatabase<string>.Instance.Add(foo);
            CommandHandlersDatabase<string>.Instance.Get().Count().Should().Be(1);
            CommandHandlersDatabase<string>.Instance.Clear();
            CommandHandlersDatabase<string>.Instance.Get().Count().Should().Be(0);
        }
    }
}
