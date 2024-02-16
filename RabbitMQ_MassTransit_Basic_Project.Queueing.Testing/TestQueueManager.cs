using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RabbitMQ_MassTransit_Basic_Project.Queueing.Contstants;
using RabbitMQ_MassTransit_Basic_Project.Queueing.Exceptions;
using Moq;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace RabbitMQ_MassTransit_Basic_Project.Queueing.Testing
{
    public class TestQueueManager
    {
        private readonly ConcurrentDictionary<string, ConcurrentQueue<QueueMessageContainer>> _queues =
            new ConcurrentDictionary<string, ConcurrentQueue<QueueMessageContainer>>();

        private readonly ConcurrentDictionary<string, AsyncEventingBasicConsumer> _consumers =
            new ConcurrentDictionary<string, AsyncEventingBasicConsumer>();

        private readonly ConcurrentDictionary<ulong, QueueMessageContainer> _inprocessMessages =
            new ConcurrentDictionary<ulong, QueueMessageContainer>();

        private readonly ConcurrentBag<QueueMessageContainer> _ackedMessages = new ConcurrentBag<QueueMessageContainer>();

        private readonly Random _random = new Random();


        public void AddConsumer(string queue, AsyncEventingBasicConsumer consumer)
        {
            _consumers.TryAdd(queue, consumer);
        }

        public void AddQueue(string queue)
        {
            _queues.TryAdd(queue, new ConcurrentQueue<QueueMessageContainer>());
        }

        public void EnqueueMessage(QueueMessageContainer message)
        {
            if (message.QueueName.EndsWith("-delay", StringComparison.OrdinalIgnoreCase)) message.QueueName = message.QueueName.Substring(0, message.QueueName.LastIndexOf("-"));
            if (_queues.TryGetValue(message.QueueName, out var queue))
                queue.Enqueue(message);
            else
                throw new QueueingException($"Queue {message.QueueName} not declared, assumption is Exchange = QueueName");
        }

        public void AckMessage(ulong deliveryTag)
        {
            if (_inprocessMessages.TryRemove(deliveryTag, out var message))
                _ackedMessages.Add(message);
            else
                throw new QueueingException($"Could not Ack message {deliveryTag}");
        }

        public void RejectMessage(ulong deliveryTag, bool requeue)
        {
            if (_inprocessMessages.TryRemove(deliveryTag, out var message))
            {
                if (!requeue)
                    message.QueueName += QueueingConstants.DeadletterAddition;
                else
                    message.Requeued = true;

                EnqueueMessage(message);
            }
            else
            {
                throw new QueueingException($"Could not Reject message {deliveryTag}");
            }
        }
        public IEnumerable<QueueMessageContainer> GetInProcessMessages()
        {
            return new List<QueueMessageContainer>(_inprocessMessages.Values);
        }

        public IEnumerable<QueueMessageContainer> GetInProcessMessages(string queueName)
        {
            return GetInProcessMessages().Where(x => x.QueueName == queueName);
        }

        public IEnumerable<QueueMessageContainer> GetAckedMessages()
        {
            return new List<QueueMessageContainer>(_ackedMessages);
        }

        public IEnumerable<QueueMessageContainer> GetAckedMessages(string queueName)
        {
            return GetAckedMessages().Where(x => x.QueueName == queueName);
        }

        public IEnumerable<QueueMessageContainer> GetDeadLetterMessages()
        {
            var deadLetterMessages = new List<QueueMessageContainer>();
            foreach (var queue in _queues.Where(x => x.Key.EndsWith(QueueingConstants.DeadletterAddition)))
            {
                if (queue.Key.EndsWith(QueueingConstants.DeadletterAddition))
                {
                    deadLetterMessages.AddRange(queue.Value);
                }
            }

            return deadLetterMessages;
        }

        public IEnumerable<QueueMessageContainer> GetDeadLetterMessages(string queueName)
        {
            return GetDeadLetterMessages().Where(x => x.QueueName == queueName + QueueingConstants.DeadletterAddition);
        }

        /// <summary>
        /// Returns all messages that are not delivered to a consumer
        /// </summary>
        /// <returns></returns>
        public List<QueueMessageContainer> GetQueueMessages()
        {
            var messages = new List<QueueMessageContainer>();
            foreach (var queue in _queues)
            {
                messages.AddRange(queue.Value);
            }

            return messages;
        }

        public List<QueueMessageContainer> GetQueueMessages(string queueName)
        {
            return new List<QueueMessageContainer>(GetQueue(queueName));
        }



        /// <summary>
        /// Runs one iteration of the consumption process
        /// </summary>
        public async Task ConsumeMessagesAsync(bool ignoreDeadletterQueues = true)
        {
            // Build a list of messages to consume
            var messages = new List<QueueMessageContainer>();
            foreach (var queueEntry in _queues)
                while (!queueEntry.Value.IsEmpty)
                {
                    queueEntry.Value.TryDequeue(out var messageContainer);
                    messages.Add(messageContainer);
                }

            // Deliver messages to consumers
            foreach (var message in messages) await DeliverMessageAsync(message, ignoreDeadletterQueues);
        }


        private uint GenerateDeliveryTag()
        {
            return (uint)_random.Next(0, int.MaxValue);
        }

        private async Task DeliverMessageAsync(QueueMessageContainer message, bool ignoreDeadletterQueues)
        {
            if (_consumers.TryGetValue(message.QueueName, out var consumer))
            {
                var _deliveryTag = GenerateDeliveryTag();
                _inprocessMessages.TryAdd(_deliveryTag, message);

                await consumer.HandleBasicDeliver("MockConsumer", _deliveryTag, false, message.QueueName, message.QueueName,
                    new Mock<IBasicProperties>().Object, new ReadOnlyMemory<byte>(message.Content));
            }
            else if (ignoreDeadletterQueues && message.QueueName.EndsWith(QueueingConstants.DeadletterAddition))
            {
                // Do nothing
            }
            else
            {
                throw new QueueingException($"No Consumer declared for '{message.QueueName}' message");
            }
        }

        private ConcurrentQueue<QueueMessageContainer> GetQueue(string queue)
        {
            if (_queues.TryGetValue(queue, out var queueInstance))
                return queueInstance;
            else
                throw new QueueingException($"Queue {queue} not found");
        }
    }
}