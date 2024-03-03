namespace Peer2P.Library.Collections;

public class TimeStorage<TKey> : Dictionary<TKey, DateTime> where TKey : notnull
{
    private readonly object _lockObject = new();

    public void Add(TKey key)
    {
        lock (_lockObject)
        {
            this[key] = DateTime.Now;
        }
    }

    public double GetTimeDifference(TKey key)
    {
        lock (_lockObject)
        {
            return (DateTime.Now - this[key]).TotalMilliseconds;
        }
    }
}