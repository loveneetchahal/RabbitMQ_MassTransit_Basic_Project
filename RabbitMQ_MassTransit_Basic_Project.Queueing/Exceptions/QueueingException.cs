using System;

namespace RabbitMQ_MassTransit_Basic_Project.Queueing.Exceptions
{
    public class QueueingException : Exception
    {
        public QueueingException(string message, Exception ex) : base(message, ex)
        {
        }

        public QueueingException(string message) : base(message)
        {
        }
    }
}