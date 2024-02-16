using System;

namespace RabbitMQ_MassTransit_Basic_Project.Queueing.Interfaces
{
    public interface IQueueMessage
    {
        Guid MessageId { get; set; }
        TimeSpan TimeToLive { get; set; }
    }
}