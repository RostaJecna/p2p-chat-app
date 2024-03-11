using System.Collections.Concurrent;

namespace Peer2P.Library.Collections;

/// <summary>
///     Represents a concurrent dictionary for storing timestamps associated with keys.
/// </summary>
/// <typeparam name="TKey">The type of keys in the dictionary.</typeparam>
public class TimeStorage<TKey> : ConcurrentDictionary<TKey, DateTime> where TKey : notnull
{
    /// <summary>
    ///     Stores the current UTC time associated with the specified key.
    /// </summary>
    /// <param name="key">The key to associate with the current UTC time.</param>
    public void Store(TKey key)
    {
        AddOrUpdate(key, _ => DateTime.UtcNow, (_, _) => DateTime.UtcNow);
    }

    /// <summary>
    ///     Gets the time difference in milliseconds between the current UTC time and the stored time associated with the
    ///     specified key.
    /// </summary>
    /// <param name="key">The key for which to retrieve the time difference.</param>
    /// <returns>The time difference in milliseconds.</returns>
    /// <exception cref="KeyNotFoundException">Thrown if the specified key is not found in the dictionary.</exception>
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