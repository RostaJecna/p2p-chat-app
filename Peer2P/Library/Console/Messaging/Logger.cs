using System.Text;

namespace Peer2P.Library.Console.Messaging;

/// <summary>
///     Provides a simple logging utility with support for different log types and protocols.
/// </summary>
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

    /// <summary>
    ///     Initializes a new instance of the <see cref="Logger" /> class with the specified message.
    /// </summary>
    /// <param name="message">The initial log message.</param>
    private Logger(string message)
    {
        _logBuffer = new StringBuilder(message);
    }

    /// <summary>
    ///     Creates a new instance of the <see cref="Logger" /> class with the specified message.
    /// </summary>
    /// <param name="message">The initial log message.</param>
    /// <returns>A new instance of the <see cref="Logger" /> class.</returns>
    public static Logger Log(string message)
    {
        return new Logger(message);
    }

    /// <summary>
    ///     Adds a log type symbol to the log message.
    /// </summary>
    /// <param name="type">The log type.</param>
    /// <returns>The current instance of the <see cref="Logger" />.</returns>
    public Logger Type(LogType type)
    {
        if (LogTypeSymbols.TryGetValue(type, out char symbol))
            _logBuffer.Insert(0, $"[{symbol}] ");

        return this;
    }

    /// <summary>
    ///     Adds a log protocol symbol to the log message.
    /// </summary>
    /// <param name="protocol">The log protocol.</param>
    /// <returns>The current instance of the <see cref="Logger" />.</returns>
    public Logger Protocol(LogProtocol protocol)
    {
        if (LogProtocolSymbols.TryGetValue(protocol, out char symbol))
            _logBuffer.Insert(0, $"[{symbol}] ");

        return this;
    }

    /// <summary>
    ///     Displays the log message by invoking the <see cref="LogDisplayed" /> event.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    ///     Thrown if no subscribers are registered for the <see cref="LogDisplayed" /> event.
    /// </exception>
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