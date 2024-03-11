namespace Peer2P.Library.Console.Messaging;

/// <summary>
///     Represents different types of log messages.
/// </summary>
public enum LogType
{
    /// <summary>
    ///     Indicates an error log message.
    /// </summary>
    Error,

    /// <summary>
    ///     Indicates a warning log message.
    /// </summary>
    Warning,

    /// <summary>
    ///     Indicates a log message for a received item.
    /// </summary>
    Received,

    /// <summary>
    ///     Indicates a log message for an expected item.
    /// </summary>
    Expecting,

    /// <summary>
    ///     Indicates a log message for a sent item.
    /// </summary>
    Sent,

    /// <summary>
    ///     Indicates a successful log message.
    /// </summary>
    Successful
}