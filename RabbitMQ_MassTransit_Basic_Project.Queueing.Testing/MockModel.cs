using System;
using System.Collections.Generic;
using System.Linq;
using RabbitMQ_MassTransit_Basic_Project.Queueing.Exceptions;
using Moq;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace RabbitMQ_MassTransit_Basic_Project.Queueing.Testing
{
    public class MockModel : IModel
    {
        public MockModel(TestQueueManager testQueueManager)
        {
            _testQueueManager = testQueueManager;
        }

        private readonly TestQueueManager _testQueueManager;

        private bool _transactionModeEnabled;
        private readonly Queue<KeyValuePair<string, byte[]>> _transactionalMessages = new Queue<KeyValuePair<string, byte[]>>();

        public void BasicAck(ulong deliveryTag, bool multiple)
        {
            _testQueueManager.AckMessage(deliveryTag);
        }

        public string BasicConsume(string queue, bool autoAck, string consumerTag, bool noLocal, bool exclusive,
            IDictionary<string, object> arguments,
            IBasicConsumer consumer)
        {
            if (consumer is AsyncEventingBasicConsumer asyncEventingConsumer)
            {
                _testQueueManager.AddConsumer(queue, asyncEventingConsumer);
                return "MockConsumer";
            }
            else
            {
                throw new QueueingException("Only AsyncEventingBasicConsumers are allowed as consumers");
            }
        }

        public void BasicPublish(string exchange, string routingKey, bool mandatory, IBasicProperties basicProperties, ReadOnlyMemory<byte> body)
        {
            // Assumption is Exchange = QueueName
            if (_transactionModeEnabled)
                _transactionalMessages.Enqueue(new KeyValuePair<string, byte[]>(exchange, body.ToArray()));
            else
                PublishToQueue(exchange, body.ToArray());
        }


        public void BasicReject(ulong deliveryTag, bool requeue)
        {
            _testQueueManager.RejectMessage(deliveryTag, requeue);
        }

        public QueueDeclareOk QueueDeclare(string queue, bool durable, bool exclusive, bool autoDelete,
            IDictionary<string, object> arguments)
        {
            _testQueueManager.AddQueue(queue);
            return new QueueDeclareOk(queue, 0, 0);
        }

        public void TxCommit()
        {
            if (_transactionModeEnabled)
                while (_transactionalMessages.Any())
                {
                    var message = _transactionalMessages.Dequeue();
                    PublishToQueue(message.Key, message.Value);
                }
            else
                throw new QueueingException("Transaction mode not enabled");
        }

        public void TxRollback()
        {
            if (_transactionModeEnabled)
                _transactionalMessages.Clear();
            else
                throw new QueueingException("Transaction mode not enabled");
        }

        public void TxSelect()
        {
            if (!_transactionModeEnabled) _transactionModeEnabled = true;
        }

        private void PublishToQueue(string queueName, byte[] body)
        {
            var container = new QueueMessageContainer { Content = body, QueueName = queueName };
            _testQueueManager.EnqueueMessage(container);
        }


        // Empty implementations
        public void BasicQos(uint prefetchSize, ushort prefetchCount, bool global)
        {
            // Do nothing
        }

        public void Dispose()
        {
            // Do nothing
        }
        public IBasicProperties CreateBasicProperties()
        {
            return new Mock<IBasicProperties>().Object;
        }

        // Not implemented
        public BasicGetResult BasicGet(string queue, bool autoAck)
        {
            throw new NotSupportedException();
        }

        public void BasicNack(ulong deliveryTag, bool multiple, bool requeue)
        {
            throw new NotSupportedException();
        }

        public void Abort()
        {
            throw new NotSupportedException();
        }

        public void Abort(ushort replyCode, string replyText)
        {
            throw new NotSupportedException();
        }

        public void BasicRecover(bool requeue)
        {
            throw new NotSupportedException();
        }

        public void BasicCancel(string consumerTag)
        {
            throw new NotSupportedException();
        }

        public void BasicCancelNoWait(string consumerTag)
        {
            throw new NotSupportedException();
        }

        public void BasicRecoverAsync(bool requeue)
        {
            throw new NotSupportedException();
        }


        public void Close()
        {
            // Do nothing
        }

        public void Close(ushort replyCode, string replyText)
        {
            throw new NotSupportedException();
        }

        public void ConfirmSelect()
        {
            throw new NotSupportedException();
        }

        public IBasicPublishBatch CreateBasicPublishBatch()
        {
            throw new NotSupportedException();
        }


        public void ExchangeBind(string destination, string source, string routingKey,
            IDictionary<string, object> arguments)
        {
            throw new NotSupportedException();
        }

        public void ExchangeBindNoWait(string destination, string source, string routingKey,
            IDictionary<string, object> arguments)
        {
            throw new NotSupportedException();
        }


        public void ExchangeDeclare(string exchange, string type, bool durable, bool autoDelete,
            IDictionary<string, object> arguments)
        {
            // Do nothing;
        }

        public void ExchangeDeclareNoWait(string exchange, string type, bool durable, bool autoDelete,
            IDictionary<string, object> arguments)
        {
            throw new NotSupportedException();
        }

        public void ExchangeDeclarePassive(string exchange)
        {
            throw new NotSupportedException();
        }

        public void ExchangeDelete(string exchange, bool ifUnused)
        {
            throw new NotSupportedException();
        }

        public void ExchangeDeleteNoWait(string exchange, bool ifUnused)
        {
            throw new NotSupportedException();
        }

        public void ExchangeUnbind(string destination, string source, string routingKey,
            IDictionary<string, object> arguments)
        {
            throw new NotSupportedException();
        }

        public void ExchangeUnbindNoWait(string destination, string source, string routingKey,
            IDictionary<string, object> arguments)
        {
            throw new NotSupportedException();
        }

        public void QueueBind(string queue, string exchange, string routingKey, IDictionary<string, object> arguments)
        {
            // Do nothing
        }

        public void QueueBindNoWait(string queue, string exchange, string routingKey,
            IDictionary<string, object> arguments)
        {
            throw new NotSupportedException();
        }


        public void QueueDeclareNoWait(string queue, bool durable, bool exclusive, bool autoDelete,
            IDictionary<string, object> arguments)
        {
            throw new NotSupportedException();
        }

        public QueueDeclareOk QueueDeclarePassive(string queue)
        {
            throw new NotSupportedException();
        }

        public uint MessageCount(string queue)
        {
            return (uint)_testQueueManager.GetQueueMessages(queue).Count;
        }

        public uint ConsumerCount(string queue)
        {
            throw new NotSupportedException();
        }

        public uint QueueDelete(string queue, bool ifUnused, bool ifEmpty)
        {
            throw new NotSupportedException();
        }

        public void QueueDeleteNoWait(string queue, bool ifUnused, bool ifEmpty)
        {
            throw new NotSupportedException();
        }

        public uint QueuePurge(string queue)
        {
            throw new NotSupportedException();
        }

        public void QueueUnbind(string queue, string exchange, string routingKey, IDictionary<string, object> arguments)
        {
            throw new NotSupportedException();
        }


        public bool WaitForConfirms()
        {
            throw new NotSupportedException();
        }

        public bool WaitForConfirms(TimeSpan timeout)
        {
            throw new NotSupportedException();
        }

        public bool WaitForConfirms(TimeSpan timeout, out bool timedOut)
        {
            throw new NotSupportedException();
        }

        public void WaitForConfirmsOrDie()
        {
            throw new NotSupportedException();
        }

        public void WaitForConfirmsOrDie(TimeSpan timeout)
        {
            throw new NotSupportedException();
        }

        public int ChannelNumber { get; }
        public ShutdownEventArgs CloseReason { get; }
        public IBasicConsumer DefaultConsumer { get; set; }
        public bool IsClosed => false;
        public bool IsOpen => true;
        public ulong NextPublishSeqNo { get; }
        public string CurrentQueue { get; }
        public TimeSpan ContinuationTimeout { get; set; }

        public event EventHandler<BasicAckEventArgs> BasicAcks;
        public event EventHandler<BasicNackEventArgs> BasicNacks;
        public event EventHandler<EventArgs> BasicRecoverOk;
        public event EventHandler<BasicReturnEventArgs> BasicReturn;
        public event EventHandler<CallbackExceptionEventArgs> CallbackException;
        public event EventHandler<FlowControlEventArgs> FlowControl;
        public event EventHandler<ShutdownEventArgs> ModelShutdown;
    }
}