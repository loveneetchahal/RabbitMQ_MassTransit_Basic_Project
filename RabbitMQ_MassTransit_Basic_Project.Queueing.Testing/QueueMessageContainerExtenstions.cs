using System.Text;
using Newtonsoft.Json;

namespace RabbitMQ_MassTransit_Basic_Project.Queueing.Testing
{
    public static class QueueMessageContainerExtenstions
    {
        public static TQueueMessage ReadContent<TQueueMessage>(this QueueMessageContainer container)
        {
            var json = Encoding.UTF8.GetString(container.Content);
            return JsonConvert.DeserializeObject<TQueueMessage>(json);
        }
    }
}