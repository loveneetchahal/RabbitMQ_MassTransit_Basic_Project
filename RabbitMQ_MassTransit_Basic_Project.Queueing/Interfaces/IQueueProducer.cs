using System.Threading;
using System.Threading.Tasks;

namespace RabbitMQ_MassTransit_Basic_Project.Queueing.Interfaces
{
    public interface IQueueProducer<in TQueueMessage> where TQueueMessage : IQueueMessage
    {
        Task<string> PublishMessage(TQueueMessage message, CancellationToken cancellationToken = default);
    }
}