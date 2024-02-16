using System.Threading.Tasks;
using RabbitMQ_MassTransit_Basic_Project.Api.Models;
using RabbitMQ_MassTransit_Basic_Project.Queueing.Interfaces;
using Microsoft.Extensions.Logging;

namespace RabbitMQ_MassTransit_Basic_Project.Api.Consumers;

public class WordQueueMessageConsumer : IQueueConsumer<WordQueueMessage>
{
    private readonly ILogger<WordQueueMessageConsumer> _logger;

    public WordQueueMessageConsumer(ILogger<WordQueueMessageConsumer> logger)
    {
        _logger = logger;
    }
    public Task ConsumeAsync(WordQueueMessage message)
    {
        _logger.LogInformation($"Word: {message.Word}");

        return Task.CompletedTask;
    }
}