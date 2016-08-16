using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CQRS.Light.Core;
using System.Threading.Tasks;
using System.Collections.Generic;
using FluentAssertions;
using Moq;

namespace CQRS.Light.Core.Tests
{
    [TestClass]
    public class TransactionTests
    {
        [TestMethod]
        public void Transaction_ConstructorShouldAssignValues()
        {
            var message = "Message";
            var handlers = new List<Func<string, Task>>() { (m) => Task.FromResult<object>(null) };
            var transaction = new Transaction<string>(message, handlers);

            transaction.Id.Should().NotBeEmpty();
            transaction.Id.Should().NotBe(Guid.Empty);

            transaction.Message.Should().Be("Message");
            transaction.ProcessedActions.Should().BeEmpty();

            transaction.NotProcessedActions.Should().NotBeEmpty();
            transaction.NotProcessedActions.Count.Should().Be(1);
        }

        [TestMethod]
        public void Transaction_CommitAsyncShouldInvokeTheHandlers()
        {
            var message = "Message";
            var handlerWasCalled = false;
            var handlers = new List<Func<string, Task>>() { (m) => { handlerWasCalled = true; return Task.FromResult<object>(null); } };
            var transaction = new Transaction<string>(message, handlers);

            transaction.NotProcessedActions.Count.Should().Be(1);
            transaction.ProcessedActions.Count.Should().Be(0);

            transaction.CommitAsync().ConfigureAwait(true);

            handlerWasCalled.Should().BeTrue();

            transaction.NotProcessedActions.Count.Should().Be(0);
            transaction.ProcessedActions.Count.Should().Be(1);
        }

        [TestMethod]
        public void Transaction_CommitAsyncShouldInvokeAllTheHandlers()
        {
            var message = "Message";
            var firstHandlerWasCalled = false;
            var secondHandlerWasCalled = false;
            var handlers = new List<Func<string, Task>>() {
                (m) => { firstHandlerWasCalled = true; return Task.FromResult<object>(null); },
                (m) => { secondHandlerWasCalled = true; return Task.FromResult<object>(null); }
            };
            var transaction = new Transaction<string>(message, handlers);

            transaction.NotProcessedActions.Count.Should().Be(2);
            transaction.ProcessedActions.Count.Should().Be(0);

            transaction.CommitAsync().ConfigureAwait(true);

            firstHandlerWasCalled.Should().BeTrue();
            secondHandlerWasCalled.Should().BeTrue();

            transaction.NotProcessedActions.Count.Should().Be(0);
            transaction.ProcessedActions.Count.Should().Be(2);
        }

        [TestMethod]
        public void Transaction_CommitAsyncShouldBubbleExceptions()
        {
            var message = "Message";
            var handlerWasCalled = false;
            var handlers = new List<Func<string, Task>>() { (m) => { handlerWasCalled = true; throw new Exception("This is an exception. There have been others but this one is mine."); } };
            var transaction = new Transaction<string>(message, handlers);

            transaction.NotProcessedActions.Count.Should().Be(1);
            transaction.ProcessedActions.Count.Should().Be(0);

            transaction.Awaiting(x => x.CommitAsync()).ShouldThrow<Exception>();

            handlerWasCalled.Should().BeTrue();

            transaction.NotProcessedActions.Count.Should().Be(1);
            transaction.ProcessedActions.Count.Should().Be(0);
        }
    }
}
