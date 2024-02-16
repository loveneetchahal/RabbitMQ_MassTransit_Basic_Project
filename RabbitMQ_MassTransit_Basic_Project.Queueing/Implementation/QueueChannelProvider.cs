using System.Collections.Generic;
using RabbitMQ_MassTransit_Basic_Project.Queueing.Contstants;
using RabbitMQ_MassTransit_Basic_Project.Queueing.Interfaces;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace RabbitMQ_MassTransit_Basic_Project.Queueing.Implementation
{
    internal class QueueChannelProvider<TQueueMessage> : IQueueChannelProvider<TQueueMessage> where TQueueMessage : IQueueMessage
    {
        private readonly IChannelProvider _channelProvider;
        private IModel _channel;
        private readonly string _queueName;

        public QueueChannelProvider(IChannelProvider channelProvider)
        {
            _channelProvider = channelProvider;
            _queueName = typeof(TQueueMessage).Name;
        }

        public IModel GetChannel()
        {
            _channel = _channelProvider.GetChannel();
            DeclareQueueAndDeadLetter();
            return _channel;
        }

        private void DeclareQueueAndDeadLetter()
        {
            var deadLetterQueueName = $"{_queueName}{QueueingConstants.DeadletterAddition}";

            // Declare the DeadLetter Queue
            var deadLetterQueueArgs = new Dictionary<string, object>
            {
                { "x-queue-type", "quorum" }, 
                { "overflow", "reject-publish" } // If the queue is full, reject the publish
            };

            _channel.ExchangeDeclare(deadLetterQueueName, ExchangeType.Direct);
            _channel.QueueDeclare(deadLetterQueueName, true, false, false, deadLetterQueueArgs);
            _channel.QueueBind(deadLetterQueueName, deadLetterQueueName, deadLetterQueueName, null);

            // Declare the Queue
            var queueArgs = new Dictionary<string, object>
            {
                { "x-dead-letter-exchange", deadLetterQueueName },
                { "x-dead-letter-routing-key", deadLetterQueueName },
                { "x-queue-type", "quorum" },
                { "x-dead-letter-strategy", "at-least-once" }, // Ensure that deadletter messages are delivered in any case see: https://www.rabbitmq.com/quorum-queues.html#dead-lettering
                { "overflow", "reject-publish" } // If the queue is full, reject the publish
            };

            _channel.ExchangeDeclare(_queueName, ExchangeType.Direct);

            _channel.QueueDeclare(_queueName, true, false, false, queueArgs);
            _channel.QueueBind(_queueName, _queueName, _queueName, null);
        }
    }
}