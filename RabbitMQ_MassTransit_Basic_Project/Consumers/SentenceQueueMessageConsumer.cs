using System.Linq;
using System.Threading.Tasks;
using RabbitMQ_MassTransit_Basic_Project.Api.Models;
using RabbitMQ_MassTransit_Basic_Project.Queueing.Exceptions;
using RabbitMQ_MassTransit_Basic_Project.Queueing.Interfaces;

namespace RabbitMQ_MassTransit_Basic_Project.Api.Consumers;

public class SentenceQueueMessageConsumer : IQueueConsumer<SentenceQueueMessage>
{
    private readonly IQueueProducer<WordQueueMessage> _wordQueueProducer;

    public SentenceQueueMessageConsumer(IQueueProducer<WordQueueMessage> wordQueueProducer)
    {
        _wordQueueProducer = wordQueueProducer;
    }

    public Task ConsumeAsync(SentenceQueueMessage message)
    {
        var words = message.Sentence.Split(" ");

        if (words.Length < 2)
            throw new QueueingException("Sentence should contain multiple words");

        foreach (var word in words)
        {
            var wordMessage = new WordQueueMessage
            {
                Word = word,
                TimeToLive = message.TimeToLive
            };

            _wordQueueProducer.PublishMessage(wordMessage);
        }

        return Task.CompletedTask;
    }
}