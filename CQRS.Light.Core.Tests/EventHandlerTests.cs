using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using FluentAssertions;
using CQRS.Light.Contracts;
using CQRS.Light.Core;
using System.Threading.Tasks;

namespace DDD.Light.Core.Tests
{
    [TestClass]
    public class EventHandlerTests
    {
        [TestMethod]
        public void EventHandler_SubscribeShouldSubscribeOnTheEventBus()
        {
            var eventBus = new Mock<IEventBus>();
            var eventHandler = new TestEventHandler(eventBus.Object);
            eventBus.Setup(x => x.Subscribe(eventHandler)).Verifiable();

            eventHandler.Subscribe();

            eventBus.Verify(x => x.Subscribe(eventHandler), Times.Once);
        }
    }

   
}
