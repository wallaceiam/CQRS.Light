using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using FluentAssertions;
using CQRS.Light.Core;
using System.Threading.Tasks;
using System.Linq;
using CQRS.Light.Contracts;

namespace DDD.Light.Core.Tests
{
    [TestClass]
    public class CommandBusTests
    {
        [TestInitialize]
        public void TestInitialize()
        {
            CommandHandlersDatabase<string>.Instance.Clear();
        }

        [TestMethod]
        public void CommandBus_ShouldBeOnlyOneInstance()
        {
            var a = CommandBus.Instance;
            var b = CommandBus.Instance;

            a.Should().Be(b);
        }

        [TestMethod]
        public void CommandBus_FuncSubscribeShouldAddToCommandHandlersDatabase()
        {
            Func<string, Task> foo = (x) => Task.FromResult<string>(x);

            CommandHandlersDatabase<string>.Instance.Get().Count().Should().Be(0);

            CommandBus.Instance.Subscribe<string>(foo);

            CommandHandlersDatabase<string>.Instance.Get().Count().Should().Be(1);
            CommandHandlersDatabase<string>.Instance.Get().Where(x => x == foo).Count().Should().Be(1);
        }

        [TestMethod]
        public void CommandBus_HandlerSubscribeShouldAddToCommandHandlersDatabase()
        {
            var commandHandler = new Mock<ICommandHandler<string>>();
            commandHandler.Setup(x => x.HandleAsync(It.IsAny<string>())).Returns(Task.FromResult<string>(null));

            CommandHandlersDatabase<string>.Instance.Get().Count().Should().Be(0);

            CommandBus.Instance.Subscribe<string>(commandHandler.Object);

            CommandHandlersDatabase<string>.Instance.Get().Count().Should().Be(1);
            CommandHandlersDatabase<string>.Instance.Get().Where(x => x == commandHandler.Object.HandleAsync).Count().Should().Be(1);
        }

        [TestMethod]
        public void CommandBus_ImplicitFuncSubscribeShouldAddToCommandHandlersDatabase()
        {
            Func<string, Task> foo = (x) => Task.FromResult<string>(x);

            CommandHandlersDatabase<string>.Instance.Get().Count().Should().Be(0);

            CommandBus.Instance.Subscribe(foo);

            CommandHandlersDatabase<string>.Instance.Get().Count().Should().Be(1);
            CommandHandlersDatabase<string>.Instance.Get().Where(x => x == foo).Count().Should().Be(1);
        }

        [TestMethod]
        public void CommandBus_ImplicitHandlerSubscribeShouldAddToCommandHandlersDatabase()
        {
            var commandHandler = new Mock<ICommandHandler<string>>();
            commandHandler.Setup(x => x.HandleAsync(It.IsAny<string>())).Returns(Task.FromResult<string>(null));

            CommandHandlersDatabase<string>.Instance.Get().Count().Should().Be(0);

            CommandBus.Instance.Subscribe(commandHandler.Object);

            CommandHandlersDatabase<string>.Instance.Get().Count().Should().Be(1);
            CommandHandlersDatabase<string>.Instance.Get().Where(x => x == commandHandler.Object.HandleAsync).Count().Should().Be(1);
        }

        [TestMethod]
        public void CommandBus_DispatchWillExecuteTheCommands()
        {
            var commandHandler = new Mock<ICommandHandler<string>>();
            commandHandler.Setup(x => x.HandleAsync(It.IsAny<string>())).Returns(Task.FromResult<string>(null)).Verifiable();

            CommandBus.Instance.Subscribe(commandHandler.Object);

            CommandBus.Instance.DispatchAsync<string>("Test Command").ConfigureAwait(true);

            commandHandler.Verify(x => x.HandleAsync(It.IsAny<string>()), Times.Once);
        }

        [TestMethod]
        public void CommandBus_DispatchBubbleExceptions()
        {
            var commandHandler = new Mock<ICommandHandler<string>>();
            commandHandler.Setup(x => x.HandleAsync(It.IsAny<string>())).Throws(new ApplicationException()).Verifiable();

            CommandBus.Instance.Subscribe(commandHandler.Object);

            CommandBus.Instance.Awaiting(x => x.DispatchAsync<string>("Test Command")).ShouldThrow<ApplicationException>();

            commandHandler.Verify(x => x.HandleAsync(It.IsAny<string>()), Times.Once);
        }
    }
}
