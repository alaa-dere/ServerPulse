using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using ServerPulse.Collector.Configurations;

namespace ServerPulse.Collector.Services.Messaging;

public class RabbitMqMessagePublisher : IMessagePublisher, IAsyncDisposable
{
    private readonly RabbitMqConfig _config;
    private readonly ILogger<RabbitMqMessagePublisher> _logger;

    private IConnection? _connection;
    private IChannel? _channel;

    public RabbitMqMessagePublisher(IOptions<RabbitMqConfig> options, ILogger<RabbitMqMessagePublisher> logger)
    {
        _config = options.Value;
        _logger = logger;
    }

    public async Task PublishAsync<T>(string routingKey, T message, CancellationToken cancellationToken = default)
    {
        await EnsureConnectionAsync(cancellationToken);
        string json = JsonSerializer.Serialize(message);
        byte[] body = Encoding.UTF8.GetBytes(json);

        var properties = new BasicProperties
        {
            ContentType = "application/json",
            DeliveryMode = DeliveryModes.Persistent
        };

        await _channel!.BasicPublishAsync(
            exchange: _config.ExchangeName,
            routingKey: routingKey,
            mandatory: false,
            basicProperties: properties,
            body: body,
            cancellationToken: cancellationToken);

        _logger.LogInformation("Published message with routing key {RoutingKey}", routingKey);
    }

    private async Task EnsureConnectionAsync(CancellationToken cancellationToken)
    {
        if (_connection is not null && _connection.IsOpen && _channel is not null && _channel.IsOpen)
        {
            return;
        }

        var factory = new ConnectionFactory
        {
            HostName = _config.HostName,
            Port = _config.Port,
            UserName = _config.UserName,
            Password = _config.Password
        };

        _connection = await factory.CreateConnectionAsync(cancellationToken);
        _channel = await _connection.CreateChannelAsync(cancellationToken: cancellationToken);

        await _channel.ExchangeDeclareAsync(
            exchange: _config.ExchangeName,
            type: ExchangeType.Topic,
            durable: true,
            autoDelete: false,
            arguments: null,
            cancellationToken: cancellationToken);
    }

    public async ValueTask DisposeAsync()
    {
        if (_channel is not null)
        {
            await _channel.DisposeAsync();
        }

        if (_connection is not null)
        {
            await _connection.DisposeAsync();
        }
    }
}