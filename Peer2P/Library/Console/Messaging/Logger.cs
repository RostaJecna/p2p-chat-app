using System.Text;

namespace Peer2P.Library.Console.Messaging;

public class Logger
{
    public static event Action<string>? LogDisplayed;

    private readonly StringBuilder _logBuffer;

    private static readonly Dictionary<LogType, char> LogTypeSymbols = new()
    {
        { LogType.Successful, '~' },
        { LogType.Error, 'X' },
        { LogType.Warning, '!' },
        { LogType.Expecting, '?' },
        { LogType.Received, '*' },
        { LogType.Sent, '^' }
    };

    private static readonly Dictionary<LogProtocol, char> LogProtocolSymbols = new()
    {
        { LogProtocol.Tcp, 'T' },
        { LogProtocol.Udp, 'U' },
        { LogProtocol.Http, 'H' }
    };
    
    private Logger(string message)
    {
        _logBuffer = new StringBuilder(message);
    }
    
    public static Logger Log(string message)
    {
        return new Logger(message);
    }
    
    public Logger Type(LogType type)
    {
        if (LogTypeSymbols.TryGetValue(type, out char symbol))
            _logBuffer.Insert(0, $"[{symbol}] ");

        return this;
    }
    
    public Logger Protocol(LogProtocol protocol)
    {
        if (LogProtocolSymbols.TryGetValue(protocol, out char symbol))
            _logBuffer.Insert(0, $"[{symbol}] ");

        return this;
    }

    public void Display()
    {
        if (LogDisplayed == null)
            throw new InvalidOperationException($"No subscribers to the {nameof(LogDisplayed)} event.");

        LogDisplayed.Invoke(_logBuffer.ToString());
    }
    
    public override string ToString()
    {
        return _logBuffer.ToString();
    }
}