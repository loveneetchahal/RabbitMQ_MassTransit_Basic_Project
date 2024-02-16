using System;
using System.Globalization;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;
using System.Threading;
using RabbitMQ_MassTransit_Basic_Project.Queueing.Exceptions;
using RabbitMQ_MassTransit_Basic_Project.Queueing.Extensions;
using RabbitMQ_MassTransit_Basic_Project.Queueing.Interfaces;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RabbitMQ.Client;
using System.Collections.Concurrent;

namespace RabbitMQ_MassTransit_Basic_Project.Queueing.Implementation
{
    internal class QueueProducer<TQueueMessage> : IQueueProducer<TQueueMessage> where TQueueMessage : IQueueMessage
    {
        private readonly ILogger<QueueProducer<TQueueMessage>> _logger;
        private readonly string _queueName;
        private readonly IModel _channel;
        private readonly ConcurrentDictionary<string, TaskCompletionSource<string>> callbackMapper;

        public QueueProducer(IQueueChannelProvider<TQueueMessage> channelProvider, ILogger<QueueProducer<TQueueMessage>> logger)
        {
            _logger = logger;
            _channel = channelProvider.GetChannel();
            _queueName = typeof(TQueueMessage).Name;
            callbackMapper = new ConcurrentDictionary<string, TaskCompletionSource<string>>();
        }

        public Task<string> PublishMessage(TQueueMessage message,CancellationToken cancellationToken = default)
        {
            if (Equals(message, default(TQueueMessage))) throw new ArgumentNullException(nameof(message));

            if (message.TimeToLive.Ticks <= 0) throw new QueueingException($"{nameof(message.TimeToLive)} cannot be zero or negative");

            // Set message ID
            message.MessageId = Guid.NewGuid();

            try
            {
                _logger.LogInformation($"Publising message to Queue '{_queueName}' with TTL {message.TimeToLive.TotalMilliseconds}");

                var serializedMessage = SerializeMessage(message);
                var properties = _channel.CreateBasicProperties();
                var correlationId = Guid.NewGuid().ToString();
                properties.CorrelationId = correlationId;
                properties.ReplyTo= _queueName;
                properties.Persistent = true;
                properties.Type = _queueName;
                properties.Expiration = message.TimeToLive.TotalMilliseconds.ToString(CultureInfo.InvariantCulture);

                var tcs = new TaskCompletionSource<string>();
                callbackMapper.TryAdd(correlationId, tcs);

                _channel.BasicPublish(exchange: _queueName,
                                     routingKey: _queueName,
                                     basicProperties: properties,
                                     body: serializedMessage);


                cancellationToken.Register(() => callbackMapper.TryRemove(correlationId, out _));
                return tcs.Task;
            }
            catch (Exception ex)
            {
                var msg = $"Cannot publish message to Queue '{_queueName}'";
                _logger.LogError(ex, msg);
                throw new QueueingException(msg);
            }
        }
        private static byte[] SerializeMessage(TQueueMessage message)
        {
            var stringContent = JsonConvert.SerializeObject(message);
            return Encoding.UTF8.GetBytes(stringContent);
        }
    }
}