﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace DDD.Light.CQRS
{
    public class Transaction<T>
    {
        public Transaction(){}

        public Transaction(T message, IEnumerable<Func<T, Task>> handlers)
        {
            Message = message;
            Id = Guid.NewGuid();
            ProcessedActions = new List<Func<T, Task>>();
            ErroredOutActions = new Dictionary<Func<T, Task>, Exception>();
            NotProcessedActions = new Queue<Func<T, Task>>(handlers);
        }

        public Guid Id { get; private set; }
        public T Message { get; set; }
        public List<Func<T, Task>> ProcessedActions { get; private set; }
        public IDictionary<Func<T, Task>, Exception> ErroredOutActions { get; private set; }
        public Queue<Func<T, Task>> NotProcessedActions { get; private set; }

        public void Commit()
        {
            while (NotProcessedActions.Count > 0)
            {
                var handler = NotProcessedActions.Dequeue();
                try
                {
                    handler.Invoke(Message);
                    ProcessedActions.Add(handler);
                }
                catch (Exception ex)
                {
                    ErroredOutActions.Add(handler, ex);
                    LogFailedCommit();
                    throw;
                }
            }
        }

        public Task CommitAsync()
        {
            while (NotProcessedActions.Count > 0)
            {
                var handler = NotProcessedActions.Dequeue();
                try
                {
                    handler.Invoke(Message);
                    ProcessedActions.Add(handler);
                }
                catch (Exception ex)
                {
                    ErroredOutActions.Add(handler, ex);
                    LogFailedCommit();
                    throw;
                }
            }

            return Task.FromResult(0);
        }

        //TODO: better logging
        private void LogFailedCommit()
        {
            var errors = (
                from entry  in ErroredOutActions 
                    let eventHandlerType = entry.Key.GetType().ToString() 
                    let errorMessage = entry.Value.Message 
                    let stackTrace = entry.Value.StackTrace 
                    let innerExceptionMessage = entry.Value.InnerException != null ? entry.Value.InnerException.Message : string.Empty 
                    let innerExceptionStackTrace = entry.Value.InnerException != null ? entry.Value.InnerException.StackTrace : string.Empty 
                select string.Format("ACTION HANDLER: {0} ::: ERROR MESSAGE: {1} ::: STACK TRACE: {2} ::: INNER EXCEPTION MESSAGE: {3} ::: INNER EXCEPTION STACK TRACE: {4}", 
                    eventHandlerType, errorMessage, stackTrace, innerExceptionMessage, innerExceptionStackTrace)
                ).ToList();

            var notProcessedEventHandlerTypes = (
                    from h in NotProcessedActions.ToList()
                    select h.GetType()
                ).ToList();
            
            var processedEventHandlerTypes = (
                    from h in ProcessedActions.ToList()
                    select h.GetType()
                ).ToList();

            var message = string.Format(
                "TRANSACTION ERROR [ID: {0}] " +
                "[ACTION TYPE: {1}] " +
                "[ACTION DATA: {2}] " +
                "[ERRORS: {3}] " +
                "[NOT PROCESSED ACTION HANDLERS: {4}] " +
                "[PROCESSED ACTION HANDLERS: {5}]",
                Id, Message.GetType(), Message, string.Join(" ^^^ ", errors), string.Join(", ", notProcessedEventHandlerTypes), string.Join(", ", processedEventHandlerTypes));

            //To Do
        }

    }
}