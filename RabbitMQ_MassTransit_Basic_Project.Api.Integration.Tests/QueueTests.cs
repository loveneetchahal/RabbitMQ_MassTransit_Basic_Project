using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using RabbitMQ_MassTransit_Basic_Project.Api.Models;
using RabbitMQ_MassTransit_Basic_Project.Queueing.Testing;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace RabbitMQ_MassTransit_Basic_Project.Api.Integration.Tests;

public class QueueTests
{
    private ApiApplicationFactory _factory;
    private HttpClient _client;
    private TestQueueManager _testQueueManager;

    [SetUp]
    public void Setup()
    {
        _factory = new ApiApplicationFactory();
        _client = _factory.CreateClient();
        _testQueueManager = _factory.Services.GetRequiredService<TestQueueManager>();
    }

    [Test]
    public async Task Enqueue_Call_Enqueues_One_Message()
    {
        // Act
        var response = await _client.GetAsync("/Queue/SplitSentence?sentence=Count my words");

        // Assert
        Assert.Multiple(() =>
        {
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual(1, _testQueueManager.GetQueueMessages(nameof(SentenceQueueMessage)).Count, "There should be exactly one queue message");
            Assert.AreEqual(1, _testQueueManager.GetInProcessMessages().Count());
        });
    }

    [Test]
    public async Task Processing_Sentence_Creates_Multiple_Words()
    {
        // Act
        var response = await _client.GetAsync("/Queue/SplitSentence?sentence=Count my words");

        await _testQueueManager.ConsumeMessagesAsync();

        // Assert
        Assert.Multiple(() =>
        {
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual(1, _testQueueManager.GetAckedMessages(nameof(SentenceQueueMessage)).Count());
            Assert.AreEqual(3, _testQueueManager.GetQueueMessages(nameof(WordQueueMessage)).Count());
        });
    }

    [Test]
    public async Task Processing_All_Messages_Results_In_Acked_Sentence_And_Words()
    {
        // Act
        var response = await _client.GetAsync("/Queue/SplitSentence?sentence=Count my words");

        while (_testQueueManager.GetInProcessMessages().Any()) await _testQueueManager.ConsumeMessagesAsync();

        // Assert
        Assert.Multiple(() =>
        {
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual(1, _testQueueManager.GetAckedMessages(nameof(SentenceQueueMessage)).Count());
            Assert.AreEqual(3, _testQueueManager.GetAckedMessages(nameof(WordQueueMessage)).Count());

            // There should be a message with the word 'my' 
            var wordQueueMessages = _testQueueManager.GetAckedMessages(nameof(WordQueueMessage)).Select(x => x.ReadContent<WordQueueMessage>());

            Assert.AreEqual(1, wordQueueMessages.Count(x => x.Word == "my"), "There should be a message with the word 'my'");
        });
    }

    [Test]
    public async Task Sentence_Without_Words_Results_In_Deadletter()
    {
        // Act
        var response = await _client.GetAsync("/Queue/SplitSentence?sentence=");

        await _testQueueManager.ConsumeMessagesAsync();

        // Assert
        Assert.Multiple(() =>
        {
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual(1, _testQueueManager.GetDeadLetterMessages(nameof(SentenceQueueMessage)).Count());
        });
    }
}