using System.Collections.Concurrent;
using System.Threading.Channels;
using Domain.Shared.Interfaces;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Notifications;

public class NotificationService : INotificationService
{
    private readonly ConcurrentDictionary<string, ConcurrentBag<ChannelWriter<string>>> _topics = new();
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(ILogger<NotificationService> logger)
    {
        _logger = logger;
    }

    public Task<ChannelReader<string>> SubscribeAsync(string topic, CancellationToken cancellationToken)
    {
        var channel = Channel.CreateBounded<string>(new BoundedChannelOptions(100)
        {
            FullMode = BoundedChannelFullMode.DropOldest
        });

        var subscribers = _topics.GetOrAdd(topic, _ => new ConcurrentBag<ChannelWriter<string>>());
        subscribers.Add(channel.Writer);

        cancellationToken.Register(() =>
        {
            channel.Writer.TryComplete();
            _logger.LogInformation("Cliente desconectado do tópico {Topic}", topic);
        });

        _logger.LogInformation("Novo cliente conectado ao tópico {Topic}", topic);
        return Task.FromResult(channel.Reader);
    }

    public async Task PublishAsync(string topic, string message)
    {
        if (_topics.TryGetValue(topic, out var subscribers))
        {
            _logger.LogInformation("Publicando mensagem no tópico {Topic} para {Count} clientes", topic, subscribers.Count);
            
            foreach (var writer in subscribers)
            {
                if (!writer.TryWrite(message))
                {
                    // Clean up could be implemented here
                }
            }
        }
        await Task.CompletedTask;
    }
}
