namespace TravelNow.Domain.Exceptions;

/// <summary>
/// Represents an exception that is thrown when a request is malformed or contains invalid data.
/// This exception typically corresponds to HTTP 400 Bad Request status codes.
/// </summary>
[Serializable]
public class BadRequestException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BadRequestException"/> class with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public BadRequestException(string message) : base(message) { }
}