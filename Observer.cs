using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Web.Script.Serialization;

/// <summary>
/// Interact with the Observer logging utility.
/// </summary>
public class Observer
{
    /// <summary>
    /// List of all queued entries.
    /// </summary>
    private static List<ObserverEntry> QueuedEntries = new List<ObserverEntry>();

    /// <summary>
    /// The URL endpoint for the logging site.
    /// </summary>
    public static string LoggerURLEndpoint = "https://obsr.us/log";

    /// <summary>
    /// The log identifier token.
    /// </summary>
    public static string LoggerToken = null;

    /// <summary>
    /// Parse payload and add it to transfer queue for logging.
    /// </summary>
    /// <param name="payload">Variable to log.</param>
    public static void Log<T>(T payload)
    {
        Observer.Log(payload, Observer.LoggerToken);
    }

    /// <summary>
    /// Parse payload and add it to transfer queue for logging.
    /// </summary>
    /// <param name="payload">Variable to log.</param>
    /// <param name="token">The log identifier token.</param>
    public static void Log<T>(T payload, string token)
    {
        Observer.Log(payload, token, 10, false, false);
    }

    /// <summary>
    /// Parse payload and add it to transfer queue for logging.
    /// </summary>
    /// <param name="payload">Variable to log.</param>
    /// <param name="token">The log identifier token.</param>
    /// <param name="maxAttempts">The maximum number of attempts to try and transfer this log entry.</param>
    /// <param name="throwOnFailed">Indicate to throw an exception if transfer for this log entry fails.</param>
    /// <param name="throwOnlyAfterMaxAttempts">Indicating to throw an exception only after the maximum number of attempts has been made.</param>
    public static void Log<T>(T payload, string token, int maxAttempts, bool throwOnFailed, bool throwOnlyAfterMaxAttempts)
    {
        // Verify that the token passed is not null or empty.
        if (string.IsNullOrWhiteSpace(token))
            throw new Exception("Token cannot be null or empty.");

        // Add the parsed entry to the queue for transfer.
        Observer.QueuedEntries.Add(
            new ObserverEntry(
                new JavaScriptSerializer().Serialize(payload),
                typeof(T).ToString(),
                token,
                maxAttempts,
                throwOnFailed,
                throwOnlyAfterMaxAttempts));

        // TODO: Check if an instance of the transfer-engine is running, if
        //       not, spawn a new instance in a separate thread.
    }
}

/// <summary>
/// A single Observer log entry.
/// </summary>
public class ObserverEntry
{
    /// <summary>
    /// Initiate a new instance of a log entry.
    /// </summary>
    /// <param name="text">The text to log.</param>
    /// <param name="type">The type of variable passed.</param>
    /// <param name="token">The log identifier token.</param>
    /// <param name="maxAttempts">The maximum number of attempts to try and transfer this log entry.</param>
    /// <param name="throwOnFailed">Indicate to throw an exception if transfer for this log entry fails.</param>
    /// <param name="throwOnlyAfterMaxAttempts">Indicating to throw an exception only after the maximum number of attempts has been made.</param>
    public ObserverEntry(string text, string type, string token, int maxAttempts = 10, bool throwOnFailed = false, bool throwOnlyAfterMaxAttempts = false)
    {
        this.DateTime = DateTime.Now;
        this.Length = text.Length;
        this.Text = text;
        this.Type = type;
        this.Token = token;
        this.MaxAttempts = maxAttempts;
        this.ThrowExceptionOnFailedTransfer = throwOnFailed;
        this.ThrowOnlyAfterMaxAttempts = throwOnlyAfterMaxAttempts;
    }

    /// <summary>
    /// Current attempts made to transfer the log entry.
    /// </summary>
    public int Attempts = 0;

    /// <summary>
    /// The maximum number of attempts to try and transfer this log entry.
    /// </summary>
    public int MaxAttempts;

    /// <summary>
    /// Exact date and time the entry was logged.
    /// </summary>
    public DateTime DateTime;

    /// <summary>
    /// Total length of the logged text entry.
    /// </summary>
    public long Length;

    /// <summary>
    /// The parsed text to log.
    /// </summary>
    public string Text;

    /// <summary>
    /// The type of variable passed.
    /// </summary>
    public string Type;

    /// <summary>
    /// The log identifier token.
    /// </summary>
    public string Token;

    /// <summary>
    /// Indicate to throw an exception if transfer for this log entry fails.
    /// </summary>
    public bool ThrowExceptionOnFailedTransfer;

    /// <summary>
    /// Indicating to throw an exception only after the maximum number of attempts has been made.
    /// </summary>
    public bool ThrowOnlyAfterMaxAttempts;
}