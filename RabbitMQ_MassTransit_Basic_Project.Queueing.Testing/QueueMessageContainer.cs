namespace RabbitMQ_MassTransit_Basic_Project.Queueing.Testing
{
    public class QueueMessageContainer
    {
        public string QueueName { get; set; }
        public byte[] Content { get; set; }
        public bool Requeued { get; set; }
    }
}