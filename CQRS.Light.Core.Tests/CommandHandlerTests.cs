using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using FluentAssertions;
using CQRS.Light.Contracts;

namespace DDD.Light.Core.Tests
{
    [TestClass]
    public class CommandHandlerTests
    {
        [TestMethod]
        public void CommandHandler_SubscribeShouldCallCommandBusSubscribe()
        {
            var commandBus = new Mock<ICommandBus>();

            var commandHandler = new TestCommandHandler(commandBus.Object);

            commandBus.Setup(x => x.Subscribe(commandHandler)).Verifiable();

            commandHandler.Subscribe();

            commandBus.Verify(x => x.Subscribe(commandHandler), Times.Once);
        }
    }
}
