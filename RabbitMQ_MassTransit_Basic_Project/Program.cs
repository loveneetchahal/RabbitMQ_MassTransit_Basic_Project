using RabbitMQ_MassTransit_Basic_Project.Api.Consumers;
using RabbitMQ_MassTransit_Basic_Project.Api.Models;
using RabbitMQ_MassTransit_Basic_Project.Queueing.Extensions;
using RabbitMQ_MassTransit_Basic_Project.Queueing.Implementation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;


namespace RabbitMQ_MassTransit_Basic_Project.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            // Run locally with
            // docker run -it --rm --name rabbitmq -p 5672:5672 -p 15672:15672 rabbitmq:3.12-management
            builder.Services.AddQueueing(new QueueingConfigurationSettings
            {
                RabbitMqConsumerConcurrency = 5,
                RabbitMqHostname = "localhost",
                RabbitMqPort = 5672,
                RabbitMqPassword = "guest",
                RabbitMqUsername = "guest"
            });

            builder.Services.AddQueueMessageConsumer<SentenceQueueMessageConsumer, SentenceQueueMessage>();
            builder.Services.AddQueueMessageConsumer<WordQueueMessageConsumer, WordQueueMessage>();


            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}



