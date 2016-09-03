using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Newtonsoft.Json;

namespace CQRS.Light.Testing.MSTest
{
    public class BDDTest<TAggregate>
        where TAggregate : CQRS.Light.Contracts.AggregateRoot
    {
        private TAggregate aggregate;
        private List<Exception> exceptions;

        [TestInitialize]
        public void TestInitialize()
        {
            //aggregate = new TAggregate();
            aggregate = (TAggregate)Activator.CreateInstance(typeof(TAggregate), MoqAggregateBus.Instance);
            exceptions = new List<Exception>();
        }

        public BDDTest<TAggregate> Given()
        {
            return this;
        }

        public BDDTest<TAggregate> Given<TEvent>(TEvent @event)
        {
            aggregate.PublishAndApplyEventAsync(@event).ConfigureAwait(true);
            return this;
        }

        public BDDTest<TAggregate> AndGiven<TEvent>(TEvent @event)
        {
            aggregate.PublishAndApplyEventAsync(@event).ConfigureAwait(true);
            return this;
        }

        public BDDTest<TAggregate> When(Func<TAggregate, Task> command)
        {
            MoqAggregateBus.Instance.Reset();
            try
            {
                var t = command(aggregate);
                t.Wait();
            }
            catch (Exception ex)
            {
                this.exceptions.Add(ex);
            }
            return this;
        }

        public BDDTest<TAggregate> When(Action<TAggregate> command)
        {
            MoqAggregateBus.Instance.Reset();
            try
            {
                command(aggregate);
            }
            catch (Exception ex)
            {
                this.exceptions.Add(ex);
            }
            return this;
        }

        public BDDTest<TAggregate> ThenFailWith<TException>()
        {
            if (!this.exceptions.Any())
                Assert.Fail("Expected Exception but non raised");

            var exception = exceptions.First().InnerException;
            if (!(exception.GetType().Name == typeof(TException).Name))
                Assert.Fail(string.Format(
                                    "Incorrect event in results; expected a {0} but got a {1}",
                                   typeof(TException).Name, exception.GetType().Name));

            exceptions.Clear();

            return this;
        }

        public BDDTest<TAggregate> Then<TEvent>(TEvent @event)
        {
            CheckForExceptions();
            CheckForRaisedEvent(@event);

            return this;
        }

        public BDDTest<TAggregate> Then(Action<TAggregate> command)
        {
            command(aggregate);

            return this;
        }

        public BDDTest<TAggregate> Then(Func<TAggregate, bool> func, string message = null)
        {
            if (!string.IsNullOrWhiteSpace(message))
                Assert.IsTrue(func(aggregate), message);
            else
                Assert.IsTrue(func(aggregate));
            return this;
        }

        public BDDTest<TAggregate> AndThen<TEvent>(TEvent @event)
        {
            CheckForExceptions();
            CheckForRaisedEvent(@event);

            return this;
        }

        public BDDTest<TAggregate> AndThen(Action<TAggregate> command)
        {
            command(aggregate);

            return this;
        }

        public BDDTest<TAggregate> AndThen(Func<TAggregate, bool> func, string message = null)
        {
            if (!string.IsNullOrWhiteSpace(message))
                Assert.IsTrue(func(aggregate), message);
            else
                Assert.IsTrue(func(aggregate));
            return this;
        }

        private void CheckForRaisedEvent<TEvent>(TEvent @event)
        {
            var aggregateBus = MoqAggregateBus.Instance as MoqAggregateBus;
            var events = aggregateBus.RaisedEvents;
            var gotEvent = events.Dequeue();

            if (@event.GetType() == gotEvent.GetType())
                Assert.AreEqual(JsonConvert.SerializeObject(gotEvent),
                    JsonConvert.SerializeObject(@event));
            else
                Assert.Fail(string.Format(
                                    "Incorrect event in results; expected a {0} but got a {1}",
                                   @event.GetType().Name, gotEvent.GetType().Name));
        }

        private void CheckForExceptions()
        {
            if (this.exceptions.Any())
                throw this.exceptions.First().InnerException;
        }
    }
}
