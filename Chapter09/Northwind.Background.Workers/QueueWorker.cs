using Microsoft.EntityFrameworkCore.Metadata;
using Northwind.Queue.Models; // To use ProductQueueMessage.
using RabbitMQ.Client; // To use ConnectionFactory.
using RabbitMQ.Client.Events; // To use EventingBasicConsumer.
using System.Text.Json; // To use JsonSerializer.

namespace Northwind.Background.Workers;

public class QueueWorker : BackgroundService
{
    private readonly ILogger<QueueWorker> _logger;

    private const string queueName = "product";
    private ConnectionFactory _factory = new() { HostName = "localhost" };
    private IConnection? _connection;
    private IChannel? _channel;
    private AsyncEventingBasicConsumer? _consumer;

    public QueueWorker(ILogger<QueueWorker> logger)
    {
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _connection = await _factory.CreateConnectionAsync();
        _channel = await _connection.CreateChannelAsync(cancellationToken: stoppingToken);
        _consumer = new AsyncEventingBasicConsumer(_channel);

        await _channel.QueueDeclareAsync(queue: queueName, durable: false, exclusive: false,
                                         autoDelete: false, arguments: null);

        _consumer.ReceivedAsync += (sender, args) =>
        {
            byte[] body = args.Body.ToArray();
            ProductQueueMessage? message = JsonSerializer.Deserialize<ProductQueueMessage>(body);
            if (message is not null)
            {
                _logger.LogInformation("Received product: {message}", message.Text);
            }
            else
            {
                _logger.LogWarning("Received unknown: {0}.", args.Body.ToArray());
            }

            // Note: BasicConsumeAsync should NOT be called here - it's moved outside the handler
            return Task.CompletedTask;
        };

        // Start consuming messages - this should be outside the message handler
        await _channel.BasicConsumeAsync(queue: queueName, autoAck: true, consumer: _consumer);

        while (!stoppingToken.IsCancellationRequested)
        {
            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
            }
            await Task.Delay(1000, stoppingToken);
        }
    }
}
