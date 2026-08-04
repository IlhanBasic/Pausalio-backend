using System.Collections.Concurrent;

namespace Pausalio.API.Hubs
{
    public interface IChatConnectionManager
    {
        void AddConnection(string userId, string connectionId);
        void RemoveConnection(string userId, string connectionId);
        bool IsOnline(string userId);
        bool ShouldSendOfflineEmailNotification(string userId);
    }

    public sealed class ChatConnectionManager : IChatConnectionManager
    {
        private static readonly TimeSpan OfflineEmailCooldown = TimeSpan.FromMinutes(15);
        private readonly ConcurrentDictionary<string, HashSet<string>> _connections = new(StringComparer.OrdinalIgnoreCase);
        private readonly ConcurrentDictionary<string, DateTime> _lastEmailSentTime = new(StringComparer.OrdinalIgnoreCase);

        public void AddConnection(string userId, string connectionId)
        {
            _connections.AddOrUpdate(
                userId,
                _ => new HashSet<string>(StringComparer.OrdinalIgnoreCase) { connectionId },
                (_, connectionIds) =>
                {
                    lock (connectionIds)
                    {
                        connectionIds.Add(connectionId);
                    }

                    return connectionIds;
                });
        }

        public void RemoveConnection(string userId, string connectionId)
        {
            if (!_connections.TryGetValue(userId, out var connectionIds))
                return;

            lock (connectionIds)
            {
                connectionIds.Remove(connectionId);
            }

            if (connectionIds.Count == 0)
            {
                _connections.TryRemove(userId, out _);
            }
        }

        public bool IsOnline(string userId)
        {
            return _connections.TryGetValue(userId, out var connectionIds) && connectionIds.Count > 0;
        }

        public bool ShouldSendOfflineEmailNotification(string userId)
        {
            var now = DateTime.UtcNow;
            if (_lastEmailSentTime.TryGetValue(userId, out var lastSentAt) &&
                now - lastSentAt < OfflineEmailCooldown)
            {
                return false;
            }

            _lastEmailSentTime[userId] = now;
            return true;
        }
    }
}
