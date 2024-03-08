using System.Collections.Concurrent;

namespace Peer2P.Library.Collections;

public class TimeStorage<TKey> : ConcurrentDictionary<TKey, DateTime> where TKey : notnull
{
    public void Store(TKey key)
    {
        AddOrUpdate(key, _ => DateTime.UtcNow, (_, _) => DateTime.UtcNow);
    }

    public double GetTimeDifferenceMilliseconds(TKey key)
    {
        if (TryGetValue(key, out DateTime storedTime))
        {
            TimeSpan elapsedTime = DateTime.UtcNow - storedTime;
            return elapsedTime.TotalMilliseconds;
        }
    
        throw new KeyNotFoundException($"Key '{key}' not found in {nameof(TimeStorage<TKey>)}.");
    }
}