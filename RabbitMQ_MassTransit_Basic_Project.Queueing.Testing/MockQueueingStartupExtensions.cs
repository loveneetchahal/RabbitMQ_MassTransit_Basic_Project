using Microsoft.Extensions.DependencyInjection;
using Moq;
using RabbitMQ.Client;

namespace RabbitMQ_MassTransit_Basic_Project.Queueing.Testing
{
    public static class MockQueueingStartupExtensions
    {
        public static void AddMockQueueing(this IServiceCollection services, TestQueueManager testQueueManager)
        {
            var mockConnectionFactory = new Mock<IAsyncConnectionFactory>();
            var mockConnection = new Mock<IConnection>();
            mockConnectionFactory.Setup(x => x.CreateConnection()).Returns(mockConnection.Object);
            mockConnection.Setup(x => x.CreateModel()).Returns(new MockModel(testQueueManager));

            services.AddSingleton(mockConnectionFactory.Object);
            services.AddSingleton(testQueueManager);
        }
    }

}
