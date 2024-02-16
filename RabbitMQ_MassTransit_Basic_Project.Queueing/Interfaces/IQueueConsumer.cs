using System.Threading.Tasks;

namespace RabbitMQ_MassTransit_Basic_Project.Queueing.Interfaces
{
    public interface IQueueConsumer<in TQueueMessage> where TQueueMessage : class, IQueueMessage
    {
        Task ConsumeAsync(TQueueMessage message);
    }
}