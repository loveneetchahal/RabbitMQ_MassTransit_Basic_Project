using RabbitMQ_MassTransit_Basic_Project.Queueing.Testing;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace RabbitMQ_MassTransit_Basic_Project.Api.Integration.Tests;

public class ApiApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            services.AddMockQueueing(new TestQueueManager());
        });
    }
}