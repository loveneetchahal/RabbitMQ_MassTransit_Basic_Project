using System;
using System.Threading.Tasks;
using RabbitMQ_MassTransit_Basic_Project.Api.Models;
using RabbitMQ_MassTransit_Basic_Project.Queueing.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace RabbitMQ_MassTransit_Basic_Project.Api.Controllers;

[ApiController]
[Route("[controller]/[action]")]
public class QueueController : ControllerBase
{
    private readonly IQueueProducer<SentenceQueueMessage> _queueProducer;

    public QueueController(IQueueProducer<SentenceQueueMessage> queueProducer)
    {
        _queueProducer = queueProducer;
    }

    /// <summary>
    /// Places a DemoQueueMessage on the queue 
    /// </summary>
    /// <param name="fail">Indicates if the processing of this message fail, thereby triggering a Deadlettering of the message</param>
    /// <param name="timeToLive">TimeToLive of the created queue message, in milliseconds</param>
    /// <param name="numberOfSecondMessages">Number of messages that will be created in the second step of processing</param>
    /// <returns></returns>
    [HttpGet()]
    public async Task<ActionResult> SplitSentenceAsync(string sentence)
    {
        var message = new SentenceQueueMessage
        {
            TimeToLive = TimeSpan.FromMinutes(1),
            Sentence = sentence
        };

        var ss= await _queueProducer.PublishMessage(message);

        return Ok($"Message enqueued");
    }
}