using RabbitMQ.Client;

namespace RabbitMQ_MassTransit_Basic_Project.Queueing.Interfaces
{
    internal interface IConnectionProvider
    {
        IConnection GetConnection();
    }
}