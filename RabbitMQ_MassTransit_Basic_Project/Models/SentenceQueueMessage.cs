using System;
using RabbitMQ_MassTransit_Basic_Project.Queueing.Interfaces;

namespace RabbitMQ_MassTransit_Basic_Project.Api.Models;

public class SentenceQueueMessage : IQueueMessage
{
    public Guid MessageId { get; set; }
    public TimeSpan TimeToLive { get; set; }
    public string Sentence { get; set; }
}