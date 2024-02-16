using System;

namespace RabbitMQ_MassTransit_Basic_Project.Queueing.Interfaces
{
    internal interface IQueueConsumerHandler<TMessageConsumer, TQueueMessage> where TMessageConsumer : IQueueConsumer<TQueueMessage> where TQueueMessage : class, IQueueMessage
    {
        void RegisterQueueConsumer();

        void CancelQueueConsumer();
    }
}