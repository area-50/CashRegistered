using System.Threading.Channels;

namespace Domain.Shared.Interfaces;

public interface INotificationService
{
    Task<ChannelReader<string>> SubscribeAsync(string topic, CancellationToken cancellationToken);
    
    Task PublishAsync(string topic, string message);
}
